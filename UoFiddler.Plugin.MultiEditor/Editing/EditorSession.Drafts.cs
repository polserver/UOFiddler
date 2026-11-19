/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UoFiddler.Plugin.MultiEditor.Tools;
using UoFiddler.Plugin.MultiEditor.House;
using UoFiddler.Plugin.MultiEditor.Model;
using UoFiddler.Plugin.MultiEditor.Storage;

namespace UoFiddler.Plugin.MultiEditor.Editing
{
    internal sealed partial class EditorSession
    {
        // ---------------------------------------------------------------- placed houses

        private readonly List<System.Xml.Linq.XElement> _placedHouses = new List<System.Xml.Linq.XElement>();

        /// <summary>
        /// The houses placed with the House tool, each as the draft it was placed from. They are
        /// kept with the design (design file, layers file), so a placed house can be opened as a
        /// draft again.
        /// </summary>
        public IReadOnlyList<System.Xml.Linq.XElement> PlacedHouses => _placedHouses;

        private string FreeHouseName()
        {
            for (int n = _placedHouses.Count + 1; ; ++n)
            {
                string name = $"House {n}";
                if (_placedHouses.All(h => (string)h.Attribute("name") != name) && CompList.Layers.All(l => l.Name != name))
                {
                    return name;
                }
            }
        }

        /// <summary>Sets the placed houses of a design just opened, moved by an offset (the layers file counts cells from the save origin).</summary>
        public void SetPlacedHouses(IEnumerable<System.Xml.Linq.XElement> houses, int dx = 0, int dy = 0)
        {
            _placedHouses.Clear();
            foreach (System.Xml.Linq.XElement house in houses ?? Enumerable.Empty<System.Xml.Linq.XElement>())
            {
                try
                {
                    _placedHouses.Add(dx == 0 && dy == 0 ? new System.Xml.Linq.XElement(house) : HouseDraftFile.Shifted(house, dx, dy, HouseStyles));
                }
                catch (System.IO.InvalidDataException)
                {
                    // A house that cannot be read is only lost as a draft: its tiles are in the multi.
                }
            }

            DraftChanged?.Invoke();
        }

        /// <summary>The placed houses moved by an offset, for a file that counts cells from another origin.</summary>
        public List<System.Xml.Linq.XElement> PlacedHousesShifted(int dx, int dy)
        {
            return _placedHouses.Select(h => HouseDraftFile.Shifted(h, dx, dy, HouseStyles)).ToList();
        }

        /// <summary>
        /// Opens a placed house as a draft again: the tiles it was placed with that are still in
        /// the multi are taken out (one undo step) and the House tool gets its draft back. Tiles
        /// changed by hand since stay where they are. Returns what is wrong, or null.
        /// </summary>
        public string ReopenHouse(System.Xml.Linq.XElement house)
        {
            if (House.HasPending)
            {
                return "Place or drop the house being designed first.";
            }

            List<(ushort Id, int X, int Y, int Z, string Layer)> built;
            try
            {
                built = House.TilesOf(house);
            }
            catch (System.IO.InvalidDataException ex)
            {
                return ex.Message;
            }

            // The house is on the layer named after it, or on a layer per part.
            string name = (string)house.Attribute("name");
            var remove = new List<MultiTile>();
            foreach ((ushort id, int x, int y, int z, string layer) in built)
            {
                MultiTile tile = CompList.GetTilesAt(x, y).FirstOrDefault(t =>
                    t.Id == id && t.Z == z && (t.Layer.Name == name || t.Layer.Name == layer) && !remove.Contains(t));
                if (tile != null)
                {
                    remove.Add(tile);
                }
            }

            _placedHouses.Remove(house);
            if (remove.Count * 2 < built.Count)
            {
                DraftChanged?.Invoke();
                return "This house is no longer in the multi: most of its tiles were changed or removed.";
            }

            CompList.ReplaceTilesOnLayers(remove, new List<(ushort, int, int, int, string)>(), "Edit house", HouseDraft.WallsLayer);
            House.OpenXml(house, keepName: true);
            AfterEdit();
            LayersChanged?.Invoke();
            return null;
        }

        /// <summary>The house tool, for its draft and options.</summary>
        public HouseTool House => (HouseTool)_tools[ToolKind.House];

        /// <summary>Raised when a roof or house draft starts, changes or ends.</summary>
        public event Action DraftChanged;

        /// <summary>The roof or house tool when it is the tool in use, else null.</summary>
        public WingDraftTool WingTool => _temporaryTool == null ? _tools[_toolKind] as WingDraftTool : null;

        /// <summary>
        /// The draft that Enter places, Esc drops and undo steps through: the one of the wing
        /// tool in use, else an open house draft (which stays open under the other tools).
        /// </summary>
        public WingDraftTool OpenDraft => WingTool is { HasPending: true } tool ? tool : House.HasPending ? House : null;

        /// <summary>
        /// The open draft when the tool in use works on it (a wing tool, or a tool that edits the
        /// house draft): the one Enter places and Esc drops.
        /// </summary>
        public WingDraftTool DraftInFocus => _temporaryTool != null ? null
            : WingTool is { HasPending: true } tool ? tool
            : House.HasPending && ActiveTool is DraftEditTool ? House : null;

        /// <summary>True while a wing tool is in use and holds a draft that is not placed yet.</summary>
        public bool ShapingRoof => WingTool is { HasPending: true };

        /// <summary>The storey of an open house draft that the chosen floor means.</summary>
        public int DraftStorey => House.StoreyFor(WorkZ);

        /// <summary>Draws an open house draft while a tool that does not work on it is in use.</summary>
        public void PaintIdleDraft(IToolOverlay overlay)
        {
            if (House.HasPending && _temporaryTool == null && ActiveTool is not (HouseTool or DraftEditTool))
            {
                House.PaintDraft(overlay, active: false);
            }
        }
    }
}
