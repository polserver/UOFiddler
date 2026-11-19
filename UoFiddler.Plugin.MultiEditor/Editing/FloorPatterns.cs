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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.MultiEditor.Editing
{
    /// <summary>
    /// The floor patterns: a few shipped ones for tile sets whose border pieces are known, and the
    /// ones the user saved (MultiEditorFloorPatterns.xml next to the other multi editor files).
    /// </summary>
    internal static class FloorPatterns
    {
        private static List<FloorPattern> _user;

        public static string FilePath => Path.Combine(Options.AppDataPath ?? string.Empty, "plugins", "MultiEditorFloorPatterns.xml");

        /// <summary>User patterns first (newest last), then the shipped ones this client has the art for.</summary>
        public static List<FloorPattern> All()
        {
            _user ??= LoadUser();
            var all = new List<FloorPattern>(_user);
            foreach (FloorPattern pattern in BuiltIn())
            {
                if (pattern.Pieces().All(id => Art.IsValidStatic(id)) && all.All(p => p.Name != pattern.Name))
                {
                    all.Add(pattern);
                }
            }

            return all;
        }

        public static void Save(FloorPattern pattern)
        {
            _user ??= LoadUser();
            _user.RemoveAll(p => string.Equals(p.Name, pattern.Name, StringComparison.OrdinalIgnoreCase));
            _user.Add(pattern);
            SaveUser();
        }

        public static void Delete(FloorPattern pattern)
        {
            _user ??= LoadUser();
            if (_user.Remove(pattern))
            {
                SaveUser();
            }
        }

        /// <summary>Forgets what was read, after the profile or client changed.</summary>
        public static void Reset()
        {
            _user = null;
        }

        /// <summary>
        /// Sets read off the client's art: the piece with its band on a side goes on that side of
        /// the floor. Checked by eye against the 7.0.114.4 art.
        /// </summary>
        private static IEnumerable<FloorPattern> BuiltIn()
        {
            yield return new FloorPattern
            {
                Name = "Gargish Green, bordered",
                BuiltIn = true,
                NorthWest = 16821, NorthEast = 16823, SouthWest = 16826, SouthEast = 16828,
                North = { 16822 }, West = { 16824 }, East = { 16825 }, South = { 16827 },
                Fill = { 16815, 16816, 16817, 16818 },
                FillWidth = 2
            };
            yield return new FloorPattern
            {
                Name = "Gargish Motif, bordered",
                BuiltIn = true,
                NorthWest = 16860, NorthEast = 16862, SouthWest = 16866, SouthEast = 16868,
                North = { 16861 }, West = { 16863 }, East = { 16865 }, South = { 16867 },
                Fill = { 16864 },
                FillWidth = 1
            };
        }

        private static List<FloorPattern> LoadUser()
        {
            var result = new List<FloorPattern>();
            try
            {
                if (!File.Exists(FilePath))
                {
                    return result;
                }

                var dom = new XmlDocument();
                dom.Load(FilePath);
                foreach (XmlElement node in dom.SelectNodes("/FloorPatterns/Pattern"))
                {
                    var pattern = new FloorPattern
                    {
                        Name = node.GetAttribute("name"),
                        NorthWest = One(node, "nw"),
                        NorthEast = One(node, "ne"),
                        SouthWest = One(node, "sw"),
                        SouthEast = One(node, "se"),
                        North = Many(node, "north"),
                        South = Many(node, "south"),
                        West = Many(node, "west"),
                        East = Many(node, "east"),
                        Fill = Many(node, "fill"),
                        FillWidth = Math.Max(1, (int)One(node, "fillWidth"))
                    };
                    if (!string.IsNullOrWhiteSpace(pattern.Name) && pattern.Pieces().Any())
                    {
                        result.Add(pattern);
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or XmlException or UnauthorizedAccessException)
            {
                // A damaged file must not keep the editor from opening; the patterns in it are skipped.
            }

            return result;
        }

        private static void SaveUser()
        {
            try
            {
                var dom = new XmlDocument();
                XmlElement root = dom.CreateElement("FloorPatterns");
                dom.AppendChild(root);
                foreach (FloorPattern pattern in _user)
                {
                    XmlElement node = dom.CreateElement("Pattern");
                    node.SetAttribute("name", pattern.Name);
                    node.SetAttribute("nw", Text(pattern.NorthWest));
                    node.SetAttribute("ne", Text(pattern.NorthEast));
                    node.SetAttribute("sw", Text(pattern.SouthWest));
                    node.SetAttribute("se", Text(pattern.SouthEast));
                    node.SetAttribute("north", Text(pattern.North));
                    node.SetAttribute("south", Text(pattern.South));
                    node.SetAttribute("west", Text(pattern.West));
                    node.SetAttribute("east", Text(pattern.East));
                    node.SetAttribute("fillWidth", pattern.FillWidth.ToString(CultureInfo.InvariantCulture));
                    node.SetAttribute("fill", Text(pattern.Fill));
                    root.AppendChild(node);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                dom.Save(FilePath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                MessageBox.Show("The floor patterns could not be saved: " + ex.Message, "Multi Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string Text(ushort id) => "0x" + id.ToString("X4", CultureInfo.InvariantCulture);

        private static string Text(IEnumerable<ushort> ids) => string.Join(",", ids.Select(Text));

        private static ushort One(XmlElement node, string attribute) => Many(node, attribute).FirstOrDefault();

        private static List<ushort> Many(XmlElement node, string attribute)
        {
            var result = new List<ushort>();
            foreach (string part in node.GetAttribute(attribute).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                bool hex = part.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
                if (ushort.TryParse(hex ? part.AsSpan(2) : part.AsSpan(), hex ? NumberStyles.HexNumber : NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ushort id))
                {
                    result.Add(id);
                }
            }

            return result;
        }
    }
}
