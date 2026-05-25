// /***************************************************************************
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;

namespace Ultima.Caching
{
    /// <summary>
    /// Bounded LRU cache for decoded bitmaps, replacing the unbounded
    /// <c>Bitmap[0x14000]</c> / <c>Bitmap[0xFFFF]</c> arrays that previously
    /// pinned every decoded item for the lifetime of the process.
    ///
    /// Eviction policy: when <c>Set</c> would push <c>Count</c> past
    /// <see cref="Capacity"/>, the least-recently-used entry is removed.
    /// By default the evicted <see cref="Bitmap"/> is NOT disposed — the SDK
    /// has no way to know whether the UI is still holding a reference to it,
    /// and disposing an in-use GDI handle crashes the renderer. Consumers
    /// that own the bitmap lifecycle exclusively can opt in via
    /// <see cref="DisposeOnEvict"/>. <see cref="Dispose"/> always disposes
    /// every entry — call it only on shutdown or when the consumer guarantees
    /// no stale references survive.
    ///
    /// Thread safety: every public member is guarded by a single lock. The
    /// previous array-backed cache was lock-free and racy; the lock cost
    /// (~tens of ns per op) is dwarfed by decode cost on a miss, and on a
    /// hit it preserves SDK behavior that consumers already accept.
    /// </summary>
    public sealed class LruBitmapCache : IDisposable
    {
        private readonly object _lock = new object();
        private readonly LinkedList<KeyValuePair<int, Bitmap>> _list =
            new LinkedList<KeyValuePair<int, Bitmap>>();
        private readonly Dictionary<int, LinkedListNode<KeyValuePair<int, Bitmap>>> _map;

        private int _capacity;
        private int _evictedCount;
        private int _disposedCount;
        private bool _disposed;

        public LruBitmapCache(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be non-negative.");
            }
            _capacity = capacity;
            _map = new Dictionary<int, LinkedListNode<KeyValuePair<int, Bitmap>>>(Math.Min(capacity, 4096));
        }

        /// <summary>
        /// Maximum number of bitmaps held by the cache. Setting this lower
        /// than the current Count evicts down to the new cap immediately.
        /// </summary>
        public int Capacity
        {
            get { lock (_lock) { return _capacity; } }
        }

        public int Count
        {
            get { lock (_lock) { return _map.Count; } }
        }

        /// <summary>
        /// If true, bitmaps evicted by the LRU policy or by <see cref="Clear"/>
        /// are <see cref="IDisposable.Dispose"/>'d before being dropped. Off
        /// by default — see class remarks. <see cref="Dispose"/> ignores this
        /// flag and always disposes everything it owns.
        /// </summary>
        public bool DisposeOnEvict { get; set; }

        /// <summary>
        /// Diagnostic counter — total bitmaps evicted by LRU policy since
        /// the cache was constructed. Useful in tests/benchmarks to assert
        /// that bounding is actually happening.
        /// </summary>
        public int EvictedCount
        {
            get { lock (_lock) { return _evictedCount; } }
        }

        /// <summary>
        /// Diagnostic counter — total bitmaps that were <c>Dispose</c>'d via
        /// either <see cref="DisposeOnEvict"/> or <see cref="Dispose"/> /
        /// <see cref="Clear"/>.
        /// </summary>
        public int DisposedCount
        {
            get { lock (_lock) { return _disposedCount; } }
        }

        public bool TryGet(int key, out Bitmap value)
        {
            lock (_lock)
            {
                if (_disposed || _capacity == 0)
                {
                    value = null;
                    return false;
                }
                if (_map.TryGetValue(key, out var node))
                {
                    _list.Remove(node);
                    _list.AddFirst(node);
                    value = node.Value.Value;
                    return true;
                }
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Insert or update an entry. If the key already exists, updates the
        /// existing entry and moves it to MRU; the displaced previous bitmap
        /// is disposed iff <see cref="DisposeOnEvict"/> is true. Capacity 0
        /// is a no-op (the bitmap is not retained and not disposed).
        /// </summary>
        public void Set(int key, Bitmap value)
        {
            if (value == null)
            {
                Remove(key);
                return;
            }
            lock (_lock)
            {
                if (_disposed || _capacity == 0)
                {
                    return;
                }

                if (_map.TryGetValue(key, out var existing))
                {
                    Bitmap previous = existing.Value.Value;
                    _list.Remove(existing);
                    var replacement = new LinkedListNode<KeyValuePair<int, Bitmap>>(new KeyValuePair<int, Bitmap>(key, value));
                    _list.AddFirst(replacement);
                    _map[key] = replacement;

                    if (DisposeOnEvict && !ReferenceEquals(previous, value))
                    {
                        previous?.Dispose();
                        _disposedCount++;
                    }
                    return;
                }

                var node = new LinkedListNode<KeyValuePair<int, Bitmap>>(new KeyValuePair<int, Bitmap>(key, value));
                _list.AddFirst(node);
                _map[key] = node;
                EvictWhileOverCapacityNoLock();
            }
        }

        public bool Remove(int key)
        {
            lock (_lock)
            {
                if (_map.TryGetValue(key, out var node))
                {
                    Bitmap bmp = node.Value.Value;
                    _list.Remove(node);
                    _map.Remove(key);
                    if (DisposeOnEvict)
                    {
                        bmp?.Dispose();
                        _disposedCount++;
                    }
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Drops every entry. Disposes the bitmaps iff
        /// <see cref="DisposeOnEvict"/> is true. Use this for soft resets
        /// where consumers may still hold references; use <see cref="Dispose"/>
        /// when you own the lifecycle outright.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                if (DisposeOnEvict)
                {
                    foreach (var kvp in _list)
                    {
                        kvp.Value?.Dispose();
                        _disposedCount++;
                    }
                }
                _list.Clear();
                _map.Clear();
            }
        }

        public void SetCapacity(int newCapacity)
        {
            if (newCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newCapacity));
            }
            lock (_lock)
            {
                _capacity = newCapacity;
                EvictWhileOverCapacityNoLock();
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    return;
                }
                _disposed = true;
                foreach (var kvp in _list)
                {
                    kvp.Value?.Dispose();
                    _disposedCount++;
                }
                _list.Clear();
                _map.Clear();
            }
        }

        private void EvictWhileOverCapacityNoLock()
        {
            while (_map.Count > _capacity)
            {
                var lru = _list.Last;
                if (lru == null)
                {
                    break;
                }
                _list.RemoveLast();
                _map.Remove(lru.Value.Key);
                _evictedCount++;
                if (DisposeOnEvict)
                {
                    lru.Value.Value?.Dispose();
                    _disposedCount++;
                }
            }
        }
    }
}
