using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace GPS_Logger.Utilities
{
    /// <summary>
    /// Can tell if an instance of T has been seen within a sliding window of time
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReplayDetector<T>
    {
        /// <summary>
        /// Holds all the things that have been seen within the past "window" timeframe
        /// </summary>
        private readonly HashSet<T> _contents;

        /// <summary>
        /// Something to synchronize threads
        /// </summary>
        private readonly object _lockObject = new object();

        /// <summary>
        /// Holds all active timers, which will remove things from _contents as "window" time passes
        /// </summary>
        private readonly ConcurrentDictionary<T, IDisposable> _subscriptions;

        /// <summary>
        /// The amount of time to keep something in _contents
        /// </summary>
        private readonly TimeSpan _window;

        /// <summary>
        /// Creates a new replay detector, which can tell if an instance of T has been seen by the "InNew" function within the past "window" timeframe
        /// </summary>
        /// <param name="window"></param>
        public ReplayDetector(TimeSpan window)
        {
            _window = window;
            _contents = new HashSet<T>();
            _subscriptions = new ConcurrentDictionary<T, IDisposable>();
        }

        /// <summary>
        /// Returns true if the given thing has not been seen by this function within the past "window" timeframe.
        /// Returns false otherwise
        /// </summary>
        /// <param name="thing"></param>
        /// <returns></returns>
        public bool IsNew(T thing)
        {
            // Set up a function to end the subscription for the current thing and remove it from _contents, if it exists in those places
            var endSubscription = new Action(() =>
            {
                IDisposable subscription;
                if (_subscriptions.TryRemove(thing, out subscription))
                    subscription.Dispose();
            });

            // Set up a function to remove the given thing from _contents
            var removeFromContents = new Action(() =>
            {
                lock (_lockObject)
                    _contents.Remove(thing);
            });

            // Sets up a new subscription for the given thing so that "endSubscription" will be called after "window" time
            var createSubscription = new Func<IDisposable>(() =>
                Observable
                    .Timer(_window)
                    .Subscribe(x =>
                    {
                        endSubscription();
                        removeFromContents();
                    })
            );

            lock (_lockObject)
            {
                // Add a new subscription, or end the current one and create a new one in its place
                _subscriptions.AddOrUpdate(
                    thing,
                    t => createSubscription(), // Create a new subscription
                    (t, oldSubscription) =>
                    {
                        // End the current one and create a new one in its place.
                        // This is equivalent to resetting the expiration timer for thing
                        endSubscription();
                        return createSubscription();
                    }
                );

                // Return true if the given thing is in _contents
                return _contents.Add(thing);
            }
        }
    }
}