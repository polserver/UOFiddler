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
    /// be opened as a draft again. Styles are kept by kind and name, as the client's tables name
    /// them; one the client no longer has is left to the draft's defaults.
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
            if (draft.RoofStyle != null)
            {
                root.Add(new XAttribute("roof", draft.RoofStyle.Name));
            }

            foreach (RoofWing wing in wings)
            {
                root.Add(new XElement("Wing",
                    new XAttribute("x0", wing.X0), new XAttribute("y0", wing.Y0), new XAttribute("x1", wing.X1), new XAttribute("y1", wing.Y1),
                    new XAttribute("storeys", wing.Storeys),
                    new XAttribute("rooms", wing.Rooms),
                    new XAttribute("roof", wing.NoRoof ? "None" : wing.Shape.ToString()),
                    new XAttribute("ridge", wing.RidgeAuto ? "Auto" : wing.RidgeAlongY ? "Y" : "X")));
            }

            for (int k = 0; k < HouseDraft.MaxStoreys; ++k)
            {
                var storey = new XElement("Storey", new XAttribute("index", k));
                if (draft.WallStyles[k] != null)
                {
                    storey.Add(new XAttribute("walls", draft.WallStyles[k].Name));
                }

                if (draft.FloorStyles[k] != null)
                {
                    storey.Add(new XAttribute("floor", draft.FloorStyles[k].Name));
                }

                AddCells(storey, "addedSouth", draft.AddedSouth[k]);
                AddCells(storey, "addedEast", draft.AddedEast[k]);
                AddCells(storey, "erasedSouth", draft.ErasedSouth[k]);
                AddCells(storey, "erasedEast", draft.ErasedEast[k]);
                AddCells(storey, "windows", draft.Windows.Where(w => w.Storey == k).Select(w => (w.X, w.Y)));
                root.Add(storey);
            }

            foreach (var door in draft.Doors)
            {
                root.Add(new XElement("Door", new XAttribute("storey", door.Key.Storey), new XAttribute("x", door.Key.X), new XAttribute("y", door.Key.Y),
                    new XAttribute("style", door.Value.Style.Name), new XAttribute("piece", door.Value.Piece)));
            }

            foreach ((int storey, int x0, int y0, int x1, int y1, HouseStyle style) in draft.FloorAreas)
            {
                root.Add(new XElement("FloorArea", new XAttribute("storey", storey), new XAttribute("x0", x0), new XAttribute("y0", y0),
                    new XAttribute("x1", x1), new XAttribute("y1", y1), new XAttribute("style", style.Name)));
            }

            foreach ((int storey, int x, int y, string column, HouseStyle style) in draft.Stairs)
            {
                root.Add(new XElement("Stairs", new XAttribute("storey", storey), new XAttribute("x", x), new XAttribute("y", y),
                    new XAttribute("column", column), new XAttribute("style", style.Name)));
            }

            return root;
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

                HouseStyle Style(HousePartKind kind, string name) =>
                    name == null ? null : styles.Styles.FirstOrDefault(s => s.Kind == kind && string.Equals(s.Name, name, StringComparison.Ordinal));

                HouseDraft draft = materials.Fresh();
                int baseZ = (int?)root.Attribute("baseZ") ?? 0;
                draft.BaseZ = baseZ;
                draft.Foundation = (bool?)root.Attribute("foundation") ?? false;
                // A draft saved before rooms were set per wing names one count for the house.
                int houseRooms = (int?)root.Attribute("rooms") ?? 1;
                draft.RoofStyle = Style(HousePartKind.Roof, (string)root.Attribute("roof")) ?? draft.RoofStyle;

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
                        NoRoof = roof == "None",
                        Shape = Enum.TryParse(roof, out RoofShape shape) ? shape : RoofShape.Gable,
                        RidgeAuto = ridge == "Auto"
                    };
                    wing.RidgeAlongY = wing.RidgeAuto
                        ? RoofPlanner.RidgeAlongY((wing.X0 + 1, wing.Y0 + 1, wing.X1 + 1, wing.Y1 + 1))
                        : ridge == "Y";
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

                    draft.WallStyles[k] = Style(HousePartKind.Wall, (string)element.Attribute("walls")) ?? draft.WallStyles[k];
                    draft.FloorStyles[k] = Style(HousePartKind.Floor, (string)element.Attribute("floor")) ?? draft.FloorStyles[k];
                    draft.AddedSouth[k].UnionWith(Cells(element, "addedSouth"));
                    draft.AddedEast[k].UnionWith(Cells(element, "addedEast"));
                    draft.ErasedSouth[k].UnionWith(Cells(element, "erasedSouth"));
                    draft.ErasedEast[k].UnionWith(Cells(element, "erasedEast"));
                    foreach ((int x, int y) in Cells(element, "windows"))
                    {
                        draft.Windows.Add((k, x, y));
                    }
                }

                foreach (XElement element in root.Elements("Door"))
                {
                    if (Style(HousePartKind.Door, (string)element.Attribute("style")) is { } style && (string)element.Attribute("piece") is { } piece)
                    {
                        draft.Doors[((int)element.Attribute("storey"), (int)element.Attribute("x"), (int)element.Attribute("y"))] = (style, piece);
                    }
                }

                foreach (XElement element in root.Elements("FloorArea"))
                {
                    if (Style(HousePartKind.Floor, (string)element.Attribute("style")) is { } style)
                    {
                        draft.FloorAreas.Add(((int)element.Attribute("storey"), (int)element.Attribute("x0"), (int)element.Attribute("y0"),
                            (int)element.Attribute("x1"), (int)element.Attribute("y1"), style));
                    }
                }

                foreach (XElement element in root.Elements("Stairs"))
                {
                    if (Style(HousePartKind.Stair, (string)element.Attribute("style")) is { } style && (string)element.Attribute("column") is { } column)
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
