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
using UoFiddler.Plugin.MultiEditor.House;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    internal abstract partial class WingDraftTool
    {
        // The copied wing and what the tool keeps of it besides (a house's hand edits inside it).
        private RoofWing _clipWing;
        private object _clipExtra;

        // A pasted wing on the mouse: the wing it was copied from, as it stood, and what came with it.
        private RoofWing _floatSource;
        private object _floatExtra;

        /// <summary>Runs <paramref name="remap"/> on what copied wings took along besides themselves.</summary>
        protected void RemapCopies(Action<object> remap)
        {
            if (_clipExtra != null)
            {
                remap(_clipExtra);
            }

            if (_floatExtra != null)
            {
                remap(_floatExtra);
            }
        }

        /// <summary>True while a pasted wing follows the mouse.</summary>
        public bool FloatingWing => _drag == DragKind.Float;

        /// <summary>True when a copied wing can be pasted into the draft.</summary>
        public bool CanPasteWing => _clipWing != null && HasPending && Ready;

        /// <summary>Copies the selected wing, with what the tool keeps of it besides, for <see cref="PasteWing"/>.</summary>
        public bool CopyWing()
        {
            if (SelectedWing is not { } wing || IsDragging)
            {
                return false;
            }

            _clipWing = wing.Clone();
            _clipExtra = CopyWingExtra(wing);
            return true;
        }

        /// <summary>Puts a copy of the copied wing on the mouse; a click drops it, Esc takes it away.</summary>
        public void PasteWing()
        {
            if (CanPasteWing)
            {
                StartFloat(_clipWing, _clipExtra);
            }
        }

        /// <summary>Puts a copy of the selected wing on the mouse, leaving the copied wing alone.</summary>
        public void DuplicateWing()
        {
            if (SelectedWing is { } wing && !IsDragging && Ready)
            {
                StartFloat(wing.Clone(), CopyWingExtra(wing));
            }
        }

        private void StartFloat(RoofWing source, object extra)
        {
            CancelDrag();
            _dragStart = Capture();

            RoofWing wing = source.Clone();
            wing.EaveZ = PastedWingZ(source);
            Context.PlaneCell(Context.MouseLoc, GripZ(wing), out int cx, out int cy);

            // The mouse holds the wing by its middle.
            int width = wing.X1 - wing.X0, height = wing.Y1 - wing.Y0;
            wing.X0 = Math.Max(0, cx - (width / 2));
            wing.Y0 = Math.Max(0, cy - (height / 2));
            (wing.X1, wing.Y1) = (wing.X0 + width, wing.Y0 + height);

            _wings.Add(wing);
            _selected = _wings.Count - 1;
            _before = wing.Clone();
            (_grabX, _grabY) = (cx, cy);
            _floatSource = source.Clone();
            _floatExtra = extra;
            _beforeAll = null;
            _drag = DragKind.Float;
            DragWing();
            RaiseChanged();
        }

        /// <summary>Drops the pasted wing where it is, as one step of the draft's history.</summary>
        private void DropFloat()
        {
            DragWing();
            Push(_dragStart);
            EndFloat();
        }

        private void EndFloat()
        {
            _drag = DragKind.None;
            _before = null;
            _dragStart = null;
            _floatSource = null;
            _floatExtra = null;
        }
    }
}
