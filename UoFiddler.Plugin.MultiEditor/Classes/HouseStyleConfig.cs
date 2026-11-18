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
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>One <c>Style</c> element of the style file: what it says about one house style.</summary>
    internal sealed class HouseStyleEntry
    {
        public const string LayoutRun = "Run";
        public const string LayoutPaired = "Paired";
        public const string LayoutCycle = "Cycle";

        public HousePartKind Kind { get; set; }

        /// <summary>A piece of the style: the entry is about the first style of its kind that lists it.</summary>
        public ushort Anchor { get; set; }

        /// <summary>True for a style the tables do not have: it is made from the entry's pieces.</summary>
        public bool IsNew { get; set; }

        /// <summary>Pieces of further styles this one takes the place of.</summary>
        public List<ushort> Replaces { get; } = new List<ushort>();

        public string Name { get; set; }

        public string Group { get; set; }

        public string Family { get; set; }

        public bool? Hidden { get; set; }

        /// <summary>How a wall run is laid: <see cref="LayoutRun"/>, <see cref="LayoutPaired"/> or <see cref="LayoutCycle"/>. Null leaves it.</summary>
        public string Layout { get; set; }

        public bool? Borrows { get; set; }

        /// <summary>The style's pieces by column label, in place of the table's. Null keeps the table's.</summary>
        public List<(string Role, ushort Id)> Pieces { get; set; }

        public (HousePartKind Kind, ushort Anchor, bool IsNew) Key => (Kind, Anchor, IsNew);
    }

    /// <summary>
    /// The style file (MultiEditorHouseStyles.xml): names, groups, order and corrected pieces for
    /// the house styles read from the client's tables. One ships beside the plugin; a file of the
    /// same name in the profile's plugins folder holds the user's changes, entry by entry, on
    /// top of it.
    /// </summary>
    internal sealed class HouseStyleConfig
    {
        public const string FileName = "MultiEditorHouseStyles.xml";

        private const string _root = "HouseStyles";
        private const int _version = 1;

        public List<HouseStyleEntry> Styles { get; } = new List<HouseStyleEntry>();

        public List<HouseTheme> Themes { get; } = new List<HouseTheme>();

        public List<HouseFoundation> Foundations { get; } = new List<HouseFoundation>();

        /// <summary>The order of the styles of a kind, as anchors, where it is other than the order of the Style elements.</summary>
        public Dictionary<HousePartKind, List<ushort>> Order { get; } = new Dictionary<HousePartKind, List<ushort>>();

        public List<string> Errors { get; } = new List<string>();

        public static string ShippedPath
        {
            get
            {
                string folder = Path.GetDirectoryName(typeof(HouseStyleConfig).Assembly.Location);
                return string.IsNullOrEmpty(folder) ? null : Path.Combine(folder, FileName);
            }
        }

        public static string UserPath => string.IsNullOrEmpty(Options.AppDataPath) ? null : Path.Combine(Options.AppDataPath, "plugins", FileName);

        /// <summary>The shipped file with the user's file on top. A file that is missing or cannot be read counts as empty.</summary>
        public static HouseStyleConfig Load()
        {
            HouseStyleConfig config = LoadFile(ShippedPath);
            HouseStyleConfig user = LoadFile(UserPath);
            foreach (HouseTheme theme in user.Themes)
            {
                theme.IsUser = true;
            }

            config.Merge(user);
            return config;
        }

        public static HouseStyleConfig LoadFile(string path)
        {
            var config = new HouseStyleConfig();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return config;
            }

            try
            {
                XElement root = XDocument.Load(path).Root;
                if (root == null || root.Name.LocalName != _root)
                {
                    config.Errors.Add($"{Path.GetFileName(path)}: not a house style file");
                    return config;
                }

                if (ParseInt((string)root.Attribute("version")) > _version)
                {
                    config.Errors.Add($"{Path.GetFileName(path)}: written by a newer version of the editor");
                    return config;
                }

                foreach (XElement element in root.Elements("Style"))
                {
                    HouseStyleEntry entry = ReadStyle(element);
                    if (entry != null)
                    {
                        config.Styles.Add(entry);
                    }
                    else
                    {
                        config.Errors.Add($"{Path.GetFileName(path)}: a Style without a kind or an anchor (line {((IXmlLineInfo)element).LineNumber})");
                    }
                }

                foreach (XElement element in root.Elements("Order"))
                {
                    if (!Enum.TryParse((string)element.Attribute("kind"), out HousePartKind kind))
                    {
                        continue;
                    }

                    var anchors = new List<ushort>();
                    foreach (string text in element.Value.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (TryParseId(text, out ushort id))
                        {
                            anchors.Add(id);
                        }
                    }

                    config.Order[kind] = anchors;
                }

                foreach (XElement element in root.Elements("Theme"))
                {
                    string name = ((string)element.Attribute("name"))?.Trim();
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    ushort Id(string attribute) => TryParseId((string)element.Attribute(attribute), out ushort id) ? id : (ushort)0;
                    config.Themes.Add(new HouseTheme
                    {
                        Name = name,
                        Walls = Id("walls"),
                        Floor = Id("floor"),
                        Roof = Id("roof"),
                        FlatRoof = Id("flatRoof"),
                        Door = Id("door"),
                        Stairs = Id("stairs"),
                        Foundation = Id("foundation"),
                        FoundationTiers = Math.Clamp(ParseInt((string)element.Attribute("foundationTiers") ?? "1"), 1, 3),
                        FoundationHeight = Math.Clamp(ParseInt((string)element.Attribute("foundationHeight") ?? "7"), 1, 20)
                    });
                }

                foreach (XElement element in root.Elements("Foundation"))
                {
                    string name = ((string)element.Attribute("name"))?.Trim();
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    config.Foundations.Add(new HouseFoundation
                    {
                        Name = name,
                        Style = TryParseId((string)element.Attribute("style"), out ushort style) ? style : (ushort)0,
                        Tiers = Math.Clamp(ParseInt((string)element.Attribute("tiers") ?? "1"), 1, 3),
                        Height = Math.Clamp(ParseInt((string)element.Attribute("height") ?? "7"), 1, 20)
                    });
                }
            }
            catch (Exception ex) when (ex is IOException or XmlException or UnauthorizedAccessException)
            {
                config.Errors.Add($"{Path.GetFileName(path)}: {ex.Message}");
            }

            return config;
        }

        /// <summary>
        /// Lays another file's entries over these. An entry for a style both name takes the other
        /// file's values where it gives any; its other entries are added at the end. A theme of
        /// the other file takes the place of one of the same name.
        /// </summary>
        public void Merge(HouseStyleConfig other)
        {
            Dictionary<(HousePartKind, ushort, bool), HouseStyleEntry> mine = Styles
                .GroupBy(entry => entry.Key)
                .ToDictionary(group => group.Key, group => group.First());
            foreach (HouseStyleEntry theirs in other.Styles)
            {
                if (!mine.TryGetValue(theirs.Key, out HouseStyleEntry entry))
                {
                    Styles.Add(theirs);
                    mine[theirs.Key] = theirs;
                    continue;
                }

                entry.Name = theirs.Name ?? entry.Name;
                entry.Group = theirs.Group ?? entry.Group;
                entry.Family = theirs.Family ?? entry.Family;
                entry.Hidden = theirs.Hidden ?? entry.Hidden;
                entry.Layout = theirs.Layout ?? entry.Layout;
                entry.Borrows = theirs.Borrows ?? entry.Borrows;
                entry.Pieces = theirs.Pieces ?? entry.Pieces;
                if (theirs.Replaces.Count > 0)
                {
                    entry.Replaces.Clear();
                    entry.Replaces.AddRange(theirs.Replaces);
                }
            }

            foreach (KeyValuePair<HousePartKind, List<ushort>> order in other.Order)
            {
                Order[order.Key] = order.Value;
            }

            foreach (HouseFoundation foundation in other.Foundations)
            {
                Foundations.RemoveAll(mine => string.Equals(mine.Name, foundation.Name, StringComparison.OrdinalIgnoreCase));
                Foundations.Add(foundation);
            }

            foreach (HouseTheme theme in other.Themes)
            {
                int index = Themes.FindIndex(mine => string.Equals(mine.Name, theme.Name, StringComparison.OrdinalIgnoreCase));
                if (index >= 0)
                {
                    Themes[index] = theme;
                }
                else
                {
                    Themes.Add(theme);
                }
            }

            Errors.AddRange(other.Errors);
        }

        /// <summary>Adds a theme to the user's copy of the style file, in place of one of the same name.</summary>
        public static void SaveUserTheme(HouseTheme theme)
        {
            HouseStyleConfig user = LoadFile(UserPath);
            user.Themes.RemoveAll(other => string.Equals(other.Name, theme.Name, StringComparison.OrdinalIgnoreCase));
            user.Themes.Add(theme);
            user.Save(UserPath);
        }

        /// <summary>Takes a theme out of the user's copy of the style file.</summary>
        public static void DeleteUserTheme(string name)
        {
            HouseStyleConfig user = LoadFile(UserPath);
            if (user.Themes.RemoveAll(other => string.Equals(other.Name, name, StringComparison.OrdinalIgnoreCase)) > 0)
            {
                user.Save(UserPath);
            }
        }

        public void Save(string path)
        {
            var root = new XElement(_root, new XAttribute("version", _version));
            foreach (HouseStyleEntry entry in Styles)
            {
                root.Add(WriteStyle(entry));
            }

            foreach (KeyValuePair<HousePartKind, List<ushort>> order in Order)
            {
                root.Add(new XElement("Order", new XAttribute("kind", order.Key), string.Join(" ", order.Value.Select(FormatId))));
            }

            foreach (HouseTheme theme in Themes)
            {
                var element = new XElement("Theme", new XAttribute("name", theme.Name));
                void Id(string attribute, ushort id)
                {
                    if (id != 0)
                    {
                        element.Add(new XAttribute(attribute, FormatId(id)));
                    }
                }

                Id("walls", theme.Walls);
                Id("floor", theme.Floor);
                Id("roof", theme.Roof);
                Id("flatRoof", theme.FlatRoof);
                Id("door", theme.Door);
                Id("stairs", theme.Stairs);
                Id("foundation", theme.Foundation);
                if (theme.FoundationTiers != 1)
                {
                    element.Add(new XAttribute("foundationTiers", theme.FoundationTiers));
                }

                if (theme.FoundationHeight != HouseDraft.UsualFoundationHeight)
                {
                    element.Add(new XAttribute("foundationHeight", theme.FoundationHeight));
                }

                root.Add(element);
            }

            foreach (HouseFoundation foundation in Foundations)
            {
                var element = new XElement("Foundation", new XAttribute("name", foundation.Name));
                if (foundation.Style != 0)
                {
                    element.Add(new XAttribute("style", FormatId(foundation.Style)));
                }

                element.Add(new XAttribute("tiers", foundation.Tiers), new XAttribute("height", foundation.Height));
                root.Add(element);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            new XDocument(root).Save(path);
        }

        private static HouseStyleEntry ReadStyle(XElement element)
        {
            if (!Enum.TryParse((string)element.Attribute("kind"), out HousePartKind kind) ||
                !TryParseId((string)element.Attribute("anchor"), out ushort anchor) || anchor == 0)
            {
                return null;
            }

            var entry = new HouseStyleEntry
            {
                Kind = kind,
                Anchor = anchor,
                IsNew = (bool?)element.Attribute("new") ?? false,
                Name = (string)element.Attribute("name"),
                Group = (string)element.Attribute("group"),
                Family = (string)element.Attribute("family"),
                Hidden = (bool?)element.Attribute("hidden"),
                Layout = (string)element.Attribute("layout"),
                Borrows = (bool?)element.Attribute("borrows")
            };
            foreach (string text in ((string)element.Attribute("replaces") ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (TryParseId(text, out ushort id))
                {
                    entry.Replaces.Add(id);
                }
            }

            List<XElement> pieces = element.Elements("Piece").ToList();
            if (pieces.Count > 0)
            {
                entry.Pieces = new List<(string, ushort)>();
                foreach (XElement piece in pieces)
                {
                    string role = (string)piece.Attribute("role");
                    if (!string.IsNullOrEmpty(role) && TryParseId((string)piece.Attribute("id"), out ushort id))
                    {
                        entry.Pieces.Add((role, id));
                    }
                }
            }

            return entry;
        }

        private static XElement WriteStyle(HouseStyleEntry entry)
        {
            var element = new XElement("Style",
                new XAttribute("kind", entry.Kind),
                new XAttribute("anchor", FormatId(entry.Anchor)));
            if (entry.IsNew)
            {
                element.Add(new XAttribute("new", true));
            }

            if (entry.Replaces.Count > 0)
            {
                element.Add(new XAttribute("replaces", string.Join(" ", entry.Replaces.Select(FormatId))));
            }

            AddText(element, "name", entry.Name);
            AddText(element, "group", entry.Group);
            AddText(element, "family", entry.Family);
            AddText(element, "layout", entry.Layout);
            if (entry.Hidden.HasValue)
            {
                element.Add(new XAttribute("hidden", entry.Hidden.Value));
            }

            if (entry.Borrows.HasValue)
            {
                element.Add(new XAttribute("borrows", entry.Borrows.Value));
            }

            if (entry.Pieces != null)
            {
                foreach ((string role, ushort id) in entry.Pieces)
                {
                    element.Add(new XElement("Piece", new XAttribute("role", role), new XAttribute("id", FormatId(id))));
                }
            }

            return element;
        }

        private static void AddText(XElement element, string name, string value)
        {
            if (value != null)
            {
                element.Add(new XAttribute(name, value));
            }
        }

        public static string FormatId(ushort id) => "0x" + id.ToString("X4", CultureInfo.InvariantCulture);

        public static bool TryParseId(string text, out ushort id)
        {
            id = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.Trim();
            return text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? ushort.TryParse(text.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out id)
                : ushort.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out id);
        }

        private static int ParseInt(string text) => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : 0;
    }
}
