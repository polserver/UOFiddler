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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UoFiddler.Plugin.MultiEditor.Tools;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>
    /// A house draft as XML: one <c>House</c> element with its wings, the materials of each storey
    /// and everything changed by hand. It is what a house draft file (*.uohouse) holds, and what
    /// a design file and the layers file keep of each house placed in a multi, so the house can
    /// be opened as a draft again. Styles are kept by kind, by a piece that tells them apart
    /// (<see cref="HouseStyle.Anchor"/>) and by name; one the client does not have is left to
    /// the draft's defaults.
    /// </summary>
    internal static class HouseDraftFile
    {
        public const string Extension = "uohouse";

        public const string Filter = "House draft (*.uohouse)|*.uohouse|All files (*.*)|*.*";

        private const string _root = "House";

        /// <summary>Format version of the element: raise it when an older editor would misread the content.</summary>
        private const int _version = 1;

        public static XElement ToXml(IReadOnlyList<RoofWing> wings, HouseDraft draft, string name)
        {
            var root = new XElement(_root,
                new XAttribute("version", _version),
                new XAttribute("name", LayerStore.XmlSafe(name ?? string.Empty, "House")),
                new XAttribute("baseZ", wings.Count > 0 ? wings[0].EaveZ : draft.BaseZ),
                new XAttribute("foundation", draft.Foundation));
            if (draft.Gables != RoofGables.Front)
            {
                root.Add(new XAttribute("gables", draft.Gables));
            }

            if (draft.GableFill != RoofGableFill.Caps)
            {
                root.Add(new XAttribute("gableFill", draft.GableFill));
            }

            if (draft.Parapet)
            {
                root.Add(new XAttribute("battlements", true));
            }

            if (draft.AutoDoors)
            {
                root.Add(new XAttribute("roomDoors", true));
            }

            if (draft.FrontDoor != FrontDoorSide.None)
            {
                root.Add(new XAttribute("frontDoor", draft.FrontDoor));
            }

            if (draft.WindowSpacing > 0)
            {
                root.Add(new XAttribute("windowSpacing", draft.WindowSpacing));
            }

            AddStyle(root, "roof", draft.RoofStyle);
            AddStyle(root, "flatRoof", draft.FlatRoofStyle);
            AddStyle(root, "door", draft.DoorStyle);
            AddStyle(root, "stairs", draft.StairStyle);
            AddStyle(root, "foundationStyle", draft.FoundationStyle);

            foreach (RoofWing wing in wings)
            {
                var element = new XElement("Wing",
                    new XAttribute("x0", wing.X0), new XAttribute("y0", wing.Y0), new XAttribute("x1", wing.X1), new XAttribute("y1", wing.Y1),
                    new XAttribute("storeys", wing.Storeys),
                    new XAttribute("rooms", wing.Rooms),
                    new XAttribute("roof", wing.NoRoof ? "None" : wing.Shape.ToString()),
                    new XAttribute("ridge", wing.RidgeAuto ? "Auto" : wing.RidgeAlongY ? "Y" : "X"));
                if (wing.Layout != RoomLayout.Row)
                {
                    element.Add(new XAttribute("layout", wing.Layout));
                }

                if (wing.Walled)
                {
                    element.Add(new XAttribute("walled", true));
                }

                // Storeys with rooms of their own, as "storey:rooms:layout".
                string above = string.Join(" ", Enumerable.Range(1, HouseDraft.MaxStoreys - 1).Where(k => wing.RoomsAbove[k] != null)
                    .Select(k => $"{k.ToString(CultureInfo.InvariantCulture)}:{wing.RoomsAbove[k].Value.Rooms.ToString(CultureInfo.InvariantCulture)}:{wing.RoomsAbove[k].Value.Layout}"));
                if (above.Length > 0)
                {
                    element.Add(new XAttribute("roomsAbove", above));
                }

                root.Add(element);
            }

            for (int k = 0; k < HouseDraft.MaxStoreys; ++k)
            {
                var storey = new XElement("Storey", new XAttribute("index", k));
                AddStyle(storey, "walls", draft.WallStyles[k]);
                AddStyle(storey, "floor", draft.FloorStyles[k]);

                AddCells(storey, "addedSouth", draft.AddedSouth[k]);
                AddCells(storey, "addedEast", draft.AddedEast[k]);
                AddCells(storey, "erasedSouth", draft.ErasedSouth[k]);
                AddCells(storey, "erasedEast", draft.ErasedEast[k]);
                AddCells(storey, "windows", draft.Windows.Where(w => w.Storey == k).Select(w => (w.X, w.Y)));
                AddCells(storey, "setBack", draft.Suppressed.Where(w => w.Storey == k).Select(w => (w.X, w.Y)));
                root.Add(storey);
            }

            foreach (var door in draft.Doors)
            {
                var element = new XElement("Door", new XAttribute("storey", door.Key.Storey), new XAttribute("x", door.Key.X), new XAttribute("y", door.Key.Y));
                AddStyle(element, "style", door.Value.Style);
                element.Add(new XAttribute("piece", door.Value.Piece));
                root.Add(element);
            }

            foreach ((int storey, int x0, int y0, int x1, int y1, HouseStyle style) in draft.FloorAreas)
            {
                var element = new XElement("FloorArea", new XAttribute("storey", storey), new XAttribute("x0", x0), new XAttribute("y0", y0),
                    new XAttribute("x1", x1), new XAttribute("y1", y1));
                AddStyle(element, "style", style);
                root.Add(element);
            }

            foreach ((int storey, int x, int y, string column, HouseStyle style) in draft.Stairs)
            {
                var element = new XElement("Stairs", new XAttribute("storey", storey), new XAttribute("x", x), new XAttribute("y", y),
                    new XAttribute("column", column));
                AddStyle(element, "style", style);
                root.Add(element);
            }

            return root;
        }

        /// <summary>Names a style by its name and, in a second attribute (name + "Id"), by its anchor piece.</summary>
        private static void AddStyle(XElement element, string name, HouseStyle style)
        {
            if (style == null)
            {
                return;
            }

            element.Add(new XAttribute(name, style.Name));
            if (style.Anchor != 0)
            {
                element.Add(new XAttribute(name + "Id", HouseStyleConfig.FormatId(style.Anchor)));
            }
        }

        private static void AddCells(XElement element, string name, IEnumerable<(int X, int Y)> cells)
        {
            string text = string.Join(" ", cells.Select(c => $"{c.X.ToString(CultureInfo.InvariantCulture)},{c.Y.ToString(CultureInfo.InvariantCulture)}"));
            if (text.Length > 0)
            {
                element.Add(new XAttribute(name, text));
            }
        }

        private static IEnumerable<(int X, int Y)> Cells(XElement element, string name)
        {
            foreach (string cell in ((string)element.Attribute(name) ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = cell.Split(',');
                yield return (int.Parse(parts[0], CultureInfo.InvariantCulture), int.Parse(parts[1], CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Reads a house back. <paramref name="materials"/> gives the styles for anything the
        /// element does not name or the client no longer has. Throws <see cref="InvalidDataException"/>
        /// when the element is not a house or is damaged.
        /// </summary>
        public static (List<RoofWing> Wings, HouseDraft Draft, string Name) FromXml(XElement root, HouseStyles styles, HouseDraft materials)
        {
            try
            {
                if (root == null || root.Name.LocalName != _root)
                {
                    throw new InvalidDataException("This is not a house draft.");
                }

                if (((int?)root.Attribute("version") ?? 0) > _version)
                {
                    throw new InvalidDataException("This house draft was saved by a newer multi editor.");
                }

                // By its anchor piece first: that still finds a style the style file has since renamed.
                HouseStyle Style(HousePartKind kind, XElement element, string attribute)
                {
                    if (HouseStyleConfig.TryParseId((string)element.Attribute(attribute + "Id"), out ushort anchor) &&
                        styles.Styles.FirstOrDefault(s => s.Kind == kind && s.Anchor == anchor) is { } found)
                    {
                        return found;
                    }

                    string name = (string)element.Attribute(attribute);
                    return name == null ? null : styles.Styles.FirstOrDefault(s => s.Kind == kind && string.Equals(s.Name, name, StringComparison.Ordinal));
                }

                HouseDraft draft = materials.Fresh();
                int baseZ = (int?)root.Attribute("baseZ") ?? 0;
                draft.BaseZ = baseZ;
                draft.Foundation = (bool?)root.Attribute("foundation") ?? false;
                draft.Gables = Enum.TryParse((string)root.Attribute("gables"), out RoofGables gables) ? gables : RoofGables.Front;
                draft.GableFill = Enum.TryParse((string)root.Attribute("gableFill"), out RoofGableFill gableFill) ? gableFill : RoofGableFill.Caps;
                draft.Parapet = (bool?)root.Attribute("battlements") ?? false;
                draft.AutoDoors = (bool?)root.Attribute("roomDoors") ?? false;
                draft.FrontDoor = Enum.TryParse((string)root.Attribute("frontDoor"), out FrontDoorSide side) ? side : FrontDoorSide.None;
                draft.WindowSpacing = Math.Clamp((int?)root.Attribute("windowSpacing") ?? 0, 0, 12);
                // A draft saved before rooms were set per wing names one count for the house.
                int houseRooms = (int?)root.Attribute("rooms") ?? 1;
                draft.RoofStyle = Style(HousePartKind.Roof, root, "roof") ?? draft.RoofStyle;
                draft.FlatRoofStyle = Style(HousePartKind.Floor, root, "flatRoof") ?? draft.FlatRoofStyle;
                draft.DoorStyle = Style(HousePartKind.Door, root, "door") ?? draft.DoorStyle;
                draft.StairStyle = Style(HousePartKind.Stair, root, "stairs") ?? draft.StairStyle;
                draft.FoundationStyle = Style(HousePartKind.Wall, root, "foundationStyle");

                var wings = new List<RoofWing>();
                foreach (XElement element in root.Elements("Wing"))
                {
                    string roof = (string)element.Attribute("roof") ?? "Gable";
                    string ridge = (string)element.Attribute("ridge") ?? "Auto";
                    var wing = new RoofWing
                    {
                        X0 = (int)element.Attribute("x0"), Y0 = (int)element.Attribute("y0"),
                        X1 = (int)element.Attribute("x1"), Y1 = (int)element.Attribute("y1"),
                        EaveZ = baseZ,
                        Storeys = Math.Clamp((int?)element.Attribute("storeys") ?? 1, 1, HouseDraft.MaxStoreys),
                        Rooms = Math.Clamp((int?)element.Attribute("rooms") ?? houseRooms, 1, HouseDraft.MaxRooms),
                        Layout = Enum.TryParse((string)element.Attribute("layout"), out RoomLayout layout) ? layout : RoomLayout.Row,
                        Walled = (bool?)element.Attribute("walled") ?? false,
                        NoRoof = roof == "None",
                        Shape = Enum.TryParse(roof, out RoofShape shape) ? shape : RoofShape.Gable,
                        RidgeAuto = ridge == "Auto"
                    };
                    wing.RidgeAlongY = wing.RidgeAuto
                        ? RoofPlanner.RidgeAlongY((wing.X0 + 1, wing.Y0 + 1, wing.X1 + 1, wing.Y1 + 1))
                        : ridge == "Y";
                    foreach (string part in ((string)element.Attribute("roomsAbove") ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    {
                        string[] fields = part.Split(':');
                        if (fields.Length == 3 && int.TryParse(fields[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int k) &&
                            k >= 1 && k < HouseDraft.MaxStoreys &&
                            int.TryParse(fields[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int count) &&
                            Enum.TryParse(fields[2], out RoomLayout own))
                        {
                            wing.RoomsAbove[k] = (Math.Clamp(count, 1, HouseDraft.MaxRooms), own);
                        }
                    }

                    if (wing.X1 - wing.X0 >= 2 && wing.Y1 - wing.Y0 >= 2)
                    {
                        wings.Add(wing);
                    }
                }

                foreach (XElement element in root.Elements("Storey"))
                {
                    int k = (int?)element.Attribute("index") ?? -1;
                    if (k < 0 || k >= HouseDraft.MaxStoreys)
                    {
                        continue;
                    }

                    draft.WallStyles[k] = Style(HousePartKind.Wall, element, "walls") ?? draft.WallStyles[k];
                    draft.FloorStyles[k] = Style(HousePartKind.Floor, element, "floor") ?? draft.FloorStyles[k];
                    draft.AddedSouth[k].UnionWith(Cells(element, "addedSouth"));
                    draft.AddedEast[k].UnionWith(Cells(element, "addedEast"));
                    draft.ErasedSouth[k].UnionWith(Cells(element, "erasedSouth"));
                    draft.ErasedEast[k].UnionWith(Cells(element, "erasedEast"));
                    foreach ((int x, int y) in Cells(element, "windows"))
                    {
                        draft.Windows.Add((k, x, y));
                    }

                    foreach ((int x, int y) in Cells(element, "setBack"))
                    {
                        draft.Suppressed.Add((k, x, y));
                    }
                }

                foreach (XElement element in root.Elements("Door"))
                {
                    if (Style(HousePartKind.Door, element, "style") is { } style && (string)element.Attribute("piece") is { } piece)
                    {
                        draft.Doors[((int)element.Attribute("storey"), (int)element.Attribute("x"), (int)element.Attribute("y"))] = (style, piece);
                    }
                }

                foreach (XElement element in root.Elements("FloorArea"))
                {
                    if (Style(HousePartKind.Floor, element, "style") is { } style)
                    {
                        draft.FloorAreas.Add(((int)element.Attribute("storey"), (int)element.Attribute("x0"), (int)element.Attribute("y0"),
                            (int)element.Attribute("x1"), (int)element.Attribute("y1"), style));
                    }
                }

                foreach (XElement element in root.Elements("Stairs"))
                {
                    if (Style(HousePartKind.Stair, element, "style") is { } style && (string)element.Attribute("column") is { } column)
                    {
                        draft.Stairs.Add(((int)element.Attribute("storey"), (int)element.Attribute("x"), (int)element.Attribute("y"), column, style));
                    }
                }

                return (wings, draft, (string)root.Attribute("name") ?? "House");
            }
            catch (Exception ex) when (ex is XmlException or FormatException or OverflowException or IndexOutOfRangeException or ArgumentNullException)
            {
                throw new InvalidDataException("The house draft is damaged: " + ex.Message, ex);
            }
        }

        /// <summary>The same house moved by a number of cells.</summary>
        public static XElement Shifted(XElement house, int dx, int dy, HouseStyles styles)
        {
            var (wings, draft, name) = FromXml(house, styles, new HouseDraft());
            foreach (RoofWing wing in wings)
            {
                (wing.X0, wing.X1, wing.Y0, wing.Y1) = (wing.X0 + dx, wing.X1 + dx, wing.Y0 + dy, wing.Y1 + dy);
            }

            draft.Shift(dx, dy);
            return ToXml(wings, draft, name);
        }
    }
}
