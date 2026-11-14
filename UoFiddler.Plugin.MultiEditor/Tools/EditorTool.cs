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

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Where the draw tools put a new tile.
    /// </summary>
    internal enum PlacementMode
    {
        /// <summary>On top of the hovered tile, or on an empty cell on the work plane (Z 0 with all floors and the plane not moved).</summary>
        Stack,

        /// <summary>At the hovered tile's own Z, replacing tiles of the same kind (flat or tall) there.</summary>
        Replace,

        /// <summary>At the work plane's Z, in the cell under the cursor on that plane.</summary>
        FixedZ
    }

    /// <summary>
    /// What a tool may see and do in the editor. Implemented by the editor control.
    /// </summary>
    internal interface IEditorContext
    {
        MultiEditorComponentList CompList { get; }

        /// <summary>Mouse position in scrolled (and unzoomed) canvas space.</summary>
        Point MouseLoc { get; }

        MultiTile HoverTile { get; }

        IReadOnlyCollection<MultiTile> Selection { get; }

        ushort DrawTileId { get; }

        int BrushSize { get; }

        PlacementMode Placement { get; }

        /// <summary>The client's house design tables (walls, floors, doors, stairs).</summary>
        HouseStyles HouseStyles { get; }

        /// <summary>The house part style chosen in the House tab, or null.</summary>
        HouseStyle HouseStyle { get; }

        /// <summary>Amount the altitude tool adds to Z.</summary>
        int ZStep { get; }

        /// <summary>The work plane's Z once a floor is chosen or the plane was moved, where house parts go; otherwise null.</summary>
        int? WorkZ { get; }

        /// <summary>Highest Z drawn (the height slider).</summary>
        int MaxHeight { get; }

        int ScrollX { get; }

        int ScrollY { get; }

        /// <summary>The cell under a canvas point and the Z a new tile there would get.</summary>
        void CellAt(Point canvasLoc, out int x, out int y, out int z);

        /// <summary>The cell under a canvas point on the flat plane at height <paramref name="z"/>.</summary>
        void PlaneCell(Point canvasLoc, int z, out int x, out int y);

        /// <summary>The tiles you can work on inside a box drawn on the canvas (screen space).</summary>
        List<MultiTile> TilesInRect(Rectangle screenRect);

        void SetSelection(IEnumerable<MultiTile> tiles, bool add);

        void ToggleSelected(MultiTile tile);

        void SetDrawTile(ushort id);

        /// <summary>
        /// Call after an edit: refreshes the height slider, minimap and hover. With
        /// <paramref name="raiseTo"/> the slider is raised to show tiles placed at that Z.
        /// </summary>
        void AfterEdit(int? raiseTo = null);

        void RecordRecentTile(ushort id);

        /// <summary>Leaves a temporary tool (paste) for the tool chosen before it.</summary>
        void EndTemporaryTool();
    }

    /// <summary>
    /// How a tool marks an existing tile.
    /// </summary>
    internal enum TileHighlight
    {
        /// <summary>The tile would be removed.</summary>
        Remove
    }

    /// <summary>
    /// What a tool draws over the multi. The canvas decides how it looks.
    /// </summary>
    internal interface IToolOverlay
    {
        /// <summary>A translucent tile where a tool would place it.</summary>
        void Ghost(ushort id, int x, int y, int z);

        /// <summary>An existing tile, marked.</summary>
        void Highlight(MultiTile tile, TileHighlight kind);

        /// <summary>A selection box, in the canvas space of <see cref="IEditorContext.TilesInRect"/>.</summary>
        void Marquee(Rectangle rect);

        /// <summary>A line through canvas points, joined back to the start when closed.</summary>
        void Polyline(IReadOnlyList<Point> points, bool closed);
    }

    /// <summary>
    /// A canvas tool. The editor forwards left mouse button events and paints the overlay after
    /// the multi. Right button pans and cancels; middle button selects the hovered tile.
    /// </summary>
    internal abstract class EditorTool
    {
        protected EditorTool(IEditorContext context)
        {
            Context = context;
        }

        protected IEditorContext Context { get; }

        /// <summary>True while a drag is in progress, so every mouse move repaints.</summary>
        public virtual bool IsDragging => false;

        /// <summary>True when the overlay follows the cell under the cursor.</summary>
        public virtual bool TracksCursorCell => false;

        public virtual void OnMouseDown(MouseEventArgs e)
        {
        }

        public virtual void OnMouseMove(MouseEventArgs e)
        {
        }

        public virtual void OnMouseUp(MouseEventArgs e)
        {
        }

        public virtual void PaintOverlay(IToolOverlay overlay)
        {
        }

        /// <summary>Drops a drag or pending action (Esc, right click, switching tools).</summary>
        public virtual void Cancel()
        {
        }

        /// <summary>True when new tiles replace tiles of the same kind at their Z.</summary>
        protected bool Replacing => Context.Placement == PlacementMode.Replace;

        protected static bool CtrlHeld => (Control.ModifierKeys & Keys.Control) != 0;

        protected static bool ShiftHeld => (Control.ModifierKeys & Keys.Shift) != 0;
    }

    /// <summary>
    /// Shared drag state for tools that span two cells (rectangle, line, area remove).
    /// </summary>
    internal abstract class CellDragTool : EditorTool
    {
        private bool _dragging;

        protected CellDragTool(IEditorContext context) : base(context)
        {
        }

        protected int StartX { get; private set; }

        protected int StartY { get; private set; }

        protected int EndX { get; private set; }

        protected int EndY { get; private set; }

        public override bool IsDragging => _dragging;

        protected bool Dragging => _dragging;

        public override void OnMouseDown(MouseEventArgs e)
        {
            Context.CellAt(Context.MouseLoc, out int x, out int y, out _);
            StartX = EndX = x;
            StartY = EndY = y;
            _dragging = true;
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (!_dragging)
            {
                return;
            }

            Context.CellAt(Context.MouseLoc, out int x, out int y, out _);
            EndX = x;
            EndY = y;
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (!_dragging)
            {
                return;
            }

            _dragging = false;
            Context.CellAt(Context.MouseLoc, out int x, out int y, out int z);
            EndX = x;
            EndY = y;
            Commit(z);
        }

        public override void Cancel()
        {
            _dragging = false;
        }

        protected abstract void Commit(int z);

        protected (int X0, int Y0, int X1, int Y1) Bounds =>
            (System.Math.Min(StartX, EndX), System.Math.Min(StartY, EndY), System.Math.Max(StartX, EndX), System.Math.Max(StartY, EndY));
    }
}
