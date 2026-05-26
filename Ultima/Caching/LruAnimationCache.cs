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
using System.Threading;

namespace Ultima.Caching
{
    /// <summary>
    /// Bounded LRU cache for decoded animation frame sets. The sibling of
    /// <see cref="LruBitmapCache"/>, but keyed by a packed <see cref="long"/>
    /// (body/action/direction/hue/firstFrame) and storing whole
    /// <see cref="AnimationFrame"/> arrays instead of a single bitmap, since
    /// <c>Animations.GetAnimation</c> returns all frames of a direction at once.
    ///
    /// Eviction policy mirrors <see cref="LruBitmapCache"/>: by default the
    /// evicted array's bitmaps are NOT disposed — the SDK cannot know whether
    /// the UI still holds a reference, and disposing an in-use GDI handle
    /// crashes the renderer. Consumers borrow the returned bitmaps and must not
    /// dispose them. <see cref="Dispose"/> always disposes every owned bitmap;
    /// <see cref="AnimationFrame.Empty"/> is never disposed (shared singleton).
    ///
    /// Thread safety: every public member is guarded by a single lock.
    /// </summary>
    public sealed class LruAnimationCache : IDisposable
    {
        private readonly Lock _lock = new();
        private readonly LinkedList<KeyValuePair<long, AnimationFrame[]>> _list =
            new LinkedList<KeyValuePair<long, AnimationFrame[]>>();
        private readonly Dictionary<long, LinkedListNode<KeyValuePair<long, AnimationFrame[]>>> _map;

        private int _capacity;
        private int _evictedCount;
        private bool _disposed;

        public LruAnimationCache(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be non-negative.");
            }
            _capacity = capacity;
            _map = new Dictionary<long, LinkedListNode<KeyValuePair<long, AnimationFrame[]>>>(Math.Min(capacity, 4096));
        }

        public int Capacity
        {
            get { lock (_lock) { return _capacity; } }
        }

        public int Count
        {
            get { lock (_lock) { return _map.Count; } }
        }

        /// <summary>
        /// If true, frame arrays evicted by the LRU policy or by <see cref="Clear"/>
        /// have their bitmaps disposed before being dropped. Off by default —
        /// see class remarks.
        /// </summary>
        public bool DisposeOnEvict { get; set; }

        public int EvictedCount
        {
            get { lock (_lock) { return _evictedCount; } }
        }

        public bool TryGet(long key, out AnimationFrame[] value)
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

        public void Set(long key, AnimationFrame[] value)
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
                    AnimationFrame[] previous = existing.Value.Value;
                    _list.Remove(existing);
                    var replacement = new LinkedListNode<KeyValuePair<long, AnimationFrame[]>>(new KeyValuePair<long, AnimationFrame[]>(key, value));
                    _list.AddFirst(replacement);
                    _map[key] = replacement;

                    if (DisposeOnEvict && !ReferenceEquals(previous, value))
                    {
                        DisposeFrames(previous);
                    }
                    return;
                }

                var node = new LinkedListNode<KeyValuePair<long, AnimationFrame[]>>(new KeyValuePair<long, AnimationFrame[]>(key, value));
                _list.AddFirst(node);
                _map[key] = node;
                EvictWhileOverCapacityNoLock();
            }
        }

        public bool Remove(long key)
        {
            lock (_lock)
            {
                if (_map.TryGetValue(key, out var node))
                {
                    AnimationFrame[] frames = node.Value.Value;
                    _list.Remove(node);
                    _map.Remove(key);
                    if (DisposeOnEvict)
                    {
                        DisposeFrames(frames);
                    }
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Drops every entry. Disposes the bitmaps iff <see cref="DisposeOnEvict"/>
        /// is true. Use for soft resets where consumers may still hold references.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                if (DisposeOnEvict)
                {
                    foreach (var kvp in _list)
                    {
                        DisposeFrames(kvp.Value);
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
                    DisposeFrames(kvp.Value);
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
                    DisposeFrames(lru.Value.Value);
                }
            }
        }

        private static void DisposeFrames(AnimationFrame[] frames)
        {
            if (frames == null)
            {
                return;
            }
            foreach (var frame in frames)
            {
                if (frame != null && !ReferenceEquals(frame, AnimationFrame.Empty))
                {
                    frame.Bitmap?.Dispose();
                }
            }
        }
    }
}
