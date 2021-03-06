﻿using Common.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;

namespace Common.Utilities
{
    /// <summary>
    /// Can tell if an instance of T has been seen within a sliding window of time
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReplayDetector<T>
    {
        /// <summary>
        /// The amount of time to keep something in _contents
        /// </summary>
        private readonly TimeSpan _window;

        private readonly ConcurrentDictionary<string, Wrapper<int>> _counts;

        /// <summary>
        /// Something that helps pin the lifetimes of other objects to ReplayDetectors
        /// </summary>
        private static readonly LifetimePinner<AutoFinalizer, ReplayDetector<T>> LifetimePinner = new LifetimePinner<AutoFinalizer, ReplayDetector<T>>();

        /// <summary>
        /// Creates a new replay detector, which can tell if an instance of T has been seen by the "InNew" function within the past "window" timeframe
        /// </summary>
        /// <param name="window"></param>
        public ReplayDetector(TimeSpan window)
        {
            _window = window;
            _counts = new ConcurrentDictionary<string, Wrapper<int>>();
        }

        /// <summary>
        /// Returns true if the given thing has not been seen by this function within the past "window" timeframe.
        /// Returns false otherwise
        /// </summary>
        /// <param name="thing"></param>
        /// <returns></returns>
        public bool IsNew(T thing)
        {
            var result = false;
            var serialized = JsonConvert.SerializeObject(thing); // Serializing something ensures that .Equals doesn't have to be implemented correctly on T
            RaceConditionBreaker.BreakRaceCondition(() =>
            {
                // ReSharper disable once InconsistentlySynchronizedField
                var count = _counts.GetOrAdd(serialized, new Wrapper<int>());
                lock (count)
                {
                    // It's a race condition between grabbing the thing out of the dictionary and acquiring a lock on it.
                    // In that time someone could have removed it from the dictionary (meaning the count went down to zero for them)
                    // so we need to double-check that this exact thing is still in the dictionary
                    if (!_counts.TryGetValue(serialized, out Wrapper<int> check) || !ReferenceEquals(check, count))
                    {
                        return true; // Try again
                    }

                    // Now that we know that "count" is the official count to use for this thing...

                    // We know whether the thing is new or not
                    result = count.Value++ == 0;

                    // ...and we can set up a timer so that the count is decremented after so long
                    IDisposable subscription = null;
                    subscription = Observable.Timer(_window).Subscribe(x =>
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        using (subscription)
                        {
                            lock (count)
                            {
                                if (--count.Value <= 0)
                                {
                                    // This count is through... we can remove it from memory
                                    // Note that this is where the race condition above came from
                                    _counts.TryRemove(serialized);
                                }
                            }
                        }
                    });
                    // Pin the created subscription to the lifetime of this ReplayDetector so that it's disposed when we are finalized
                    LifetimePinner.Pin(new AutoFinalizer(subscription.Dispose), this);
                    
                    return false; // Don't loop again
                }
            });
            return result;
        }
    }
}