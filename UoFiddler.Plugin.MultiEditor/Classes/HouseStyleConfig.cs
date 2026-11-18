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
            config.Merge(LoadFile(UserPath));
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
            }
            catch (Exception ex) when (ex is IOException or XmlException or UnauthorizedAccessException)
            {
                config.Errors.Add($"{Path.GetFileName(path)}: {ex.Message}");
            }

            return config;
        }

        /// <summary>
        /// Lays another file's entries over these. An entry for a style both name takes the other
        /// file's values where it gives any; its other entries are added at the end.
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

            Errors.AddRange(other.Errors);
        }

        public void Save(string path)
        {
            var root = new XElement(_root, new XAttribute("version", _version));
            foreach (HouseStyleEntry entry in Styles)
            {
                root.Add(WriteStyle(entry));
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
