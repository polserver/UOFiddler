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
using UoFiddler.Plugin.MultiEditor.Model;
using UoFiddler.Plugin.MultiEditor.Styles;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
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

        /// <summary>Makes a tile picked on the canvas the draw tile; the pick tool then hands over to the draw tool used last.</summary>
        void PickDrawTile(ushort id);

        /// <summary>
        /// Call after an edit: refreshes the height slider, minimap and hover. With
        /// <paramref name="raiseTo"/> the slider is raised to show tiles placed at that Z.
        /// </summary>
        void AfterEdit(int? raiseTo = null);

        void RecordRecentTile(ushort id);
    }
}
