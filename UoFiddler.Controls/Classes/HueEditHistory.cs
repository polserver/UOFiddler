/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;

namespace UoFiddler.Controls.Classes
{
    /// <summary>One state of the hue being edited.</summary>
    public sealed class HueEditSnapshot
    {
        public HueEditSnapshot(ushort[] colors, string name, string label, string mergeKey)
        {
            Colors = (ushort[])colors.Clone();
            Name = name;
            Label = label;
            MergeKey = mergeKey;
        }

        public ushort[] Colors { get; }

        public string Name { get; }

        /// <summary>What produced this state, shown in the history list.</summary>
        public string Label { get; }

        /// <summary>Consecutive records with the same non-null key collapse into one entry.</summary>
        public string MergeKey { get; }

        public override string ToString() => Label;
    }

    /// <summary>
    /// The hue editor's undo history: a list of whole snapshots with a cursor. Entry 0 is the hue as
    /// it was opened. Recording after an undo drops the entries past the cursor.
    /// </summary>
    public sealed class HueEditHistory
    {
        private readonly List<HueEditSnapshot> _entries = [];
        private readonly int _capacity;

        public HueEditHistory(ushort[] colors, string name, int capacity = 200)
        {
            _capacity = Math.Max(2, capacity);
            _entries.Add(new HueEditSnapshot(colors, name, "Original", null));
            Position = 0;
        }

        public IReadOnlyList<HueEditSnapshot> Entries => _entries;

        /// <summary>The index of the current state in <see cref="Entries"/>.</summary>
        public int Position { get; private set; }

        public HueEditSnapshot Current => _entries[Position];

        public bool CanUndo => Position > 0;

        public bool CanRedo => Position < _entries.Count - 1;

        /// <summary>Raised whenever the entries or the position change.</summary>
        public event EventHandler Changed;

        /// <summary>
        /// Records a new state. With a <paramref name="mergeKey"/> equal to the current entry's,
        /// the current entry is replaced instead, so one slider drag is one step.
        /// Returns false when nothing changed.
        /// </summary>
        public bool Record(ushort[] colors, string name, string label, string mergeKey = null)
        {
            HueEditSnapshot current = Current;
            if (name == current.Name && colors.AsSpan().SequenceEqual(current.Colors))
            {
                return false;
            }

            if (Position < _entries.Count - 1)
            {
                _entries.RemoveRange(Position + 1, _entries.Count - Position - 1);
            }

            var snapshot = new HueEditSnapshot(colors, name, label, mergeKey);
            if (mergeKey != null && Position > 0 && current.MergeKey == mergeKey)
            {
                _entries[Position] = snapshot;
            }
            else
            {
                _entries.Add(snapshot);
                if (_entries.Count > _capacity)
                {
                    // Keep the original; drop the oldest edit after it.
                    _entries.RemoveAt(1);
                }

                Position = _entries.Count - 1;
            }

            Changed?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <summary>Ends merging, so the next record starts a new entry even with the same key.</summary>
        public void BreakMerge()
        {
            HueEditSnapshot current = Current;
            if (current.MergeKey != null)
            {
                _entries[Position] = new HueEditSnapshot(current.Colors, current.Name, current.Label, null);
            }
        }

        public HueEditSnapshot Undo() => CanUndo ? JumpTo(Position - 1) : null;

        public HueEditSnapshot Redo() => CanRedo ? JumpTo(Position + 1) : null;

        public HueEditSnapshot JumpTo(int index)
        {
            Position = Math.Clamp(index, 0, _entries.Count - 1);
            BreakMerge();
            Changed?.Invoke(this, EventArgs.Empty);
            return Current;
        }
    }
}
