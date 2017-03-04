using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Common.Utilities
{
    /// <summary>
    /// Executes an action within a lock that is associated with a key
    /// </summary>
    public class Locker<TKey>
    {
        /// <summary>
        /// The number of rooms. Increasing this number reduces locking contention, but increases memory use
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static readonly int NumRooms = Environment.ProcessorCount * 20;

        /// <summary>
        /// Some entropy so that keys don't land in the same room for each instance
        /// </summary>
        private readonly long _salt = DateTime.Now.Ticks;

        /// <summary>
        /// The rooms. Each room maps keys to rings
        /// </summary>
        private readonly Dictionary<TKey, ConcurrentBag<object>>[] _rooms = Enumerable.Repeat(0, NumRooms).Select(x => new Dictionary<TKey, ConcurrentBag<object>>()).ToArray();

        /// <summary>
        /// A collection of unused hats that can be tossed into rings.
        /// The count will be O(simultaneous nested lock depth)
        /// </summary>
        private readonly ConcurrentBag<object> _hatRack = new ConcurrentBag<object>();

        /// <summary>
        /// A collection of unused rings.
        /// The count will be O(simultaneous distinct lock keys)
        /// </summary>
        private readonly ConcurrentBag<ConcurrentBag<object>> _unusedRings = new ConcurrentBag<ConcurrentBag<object>>();

        /// <summary>
        /// Executes an action within a lock that is associated with the given key.
        /// Using a null key will not lock against any other calls with with a null key--in other words null keys are considered distinct
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public void DoLocked(TKey key, Action action)
        {
            if (action == null) return;
            if (ReferenceEquals(key, null))
            {
                action();
                return;
            }

            var roomToUse = Math.Abs(key.GetHashCode() * _salt) % NumRooms;

            ConcurrentBag<object> ring;

            // Grab a hat from the hat rack
            if (!_hatRack.TryTake(out object aHat))
                aHat = new object(); // (or create a new one)

            lock (_rooms[roomToUse])
            {
                // Find a ring that's in use for the given key
                if (!_rooms[roomToUse].TryGetValue(key, out ring))
                {
                    // No rings are yet in use, so let's grab an unused one
                    if (!_unusedRings.TryTake(out ring))
                        ring = new ConcurrentBag<object>(); // All the rings are used, so let's create a new one
                    // Mark this ring as being in use for this key
                    _rooms[roomToUse][key] = ring;
                }

                // Toss our hat into the ring
                ring.Add(aHat);
            }

            try
            {

                lock (ring)
                {
                    // Do our stuff
                    action();
                }
            }
            finally
            {
                // Take out a hat from the ring
                ring.TryTake(out aHat);
                // Put that hat back on the hat rack
                _hatRack.Add(aHat);

                // See if we're the last one to pull our hat out of the ring
                lock (_rooms[roomToUse])
                {
                    if (!ring.TryPeek(out aHat))
                    {
                        // We're the last one to use this ring.

                        // So let's unmark it from being in use for this key
                        if (!_rooms[roomToUse].Remove(key))
                            throw new Exception("Someone else already took our ring out");
                        // ...and put it back into the set of unused rings
                        _unusedRings.Add(ring);
                    }
                }
            }
        }
    }
}
