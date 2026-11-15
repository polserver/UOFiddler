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
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>
    /// The layers file: the client's multi cannot hold layers, so they are kept beside the
    /// settings, one file per client folder and multi id. Each set of layers names the multi it
    /// belongs to by a fingerprint of its tiles, so layers are never put on a multi that was
    /// changed elsewhere. The file keeps the last few sets: a multi saved to the client is in
    /// memory until the client files are written, and until then the files still hold an older one.
    /// </summary>
    internal static class LayerStore
    {
        private const string _folderName = "MultiEditorLayers";
        private const int _setsKept = 5;

        /// <summary>Where the layers of a client multi are kept.</summary>
        public static string FilePath(int clientId)
        {
            // One folder per client install, named by a short hash of its path.
            string root = (Files.RootDir ?? string.Empty).TrimEnd('\\', '/').ToUpperInvariant();
            string key = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(root)), 0, 6);
            return Path.Combine(Options.AppDataPath ?? string.Empty, _folderName, key, $"0x{clientId:X4}.xml");
        }

        /// <summary>
        /// Keeps the layers of a multi saved to the client. A design with only the one plain layer
        /// every design starts with has nothing to keep, and an older set for the same tiles goes.
        /// </summary>
        public static void Save(int clientId, List<SavedLayer> layers, int activeIndex, long fingerprint)
        {
            string path = FilePath(clientId);
            string multi = fingerprint.ToString(CultureInfo.InvariantCulture);
            List<XElement> sets = ReadSets(path).Where(s => (string)s.Attribute("multi") != multi).ToList();

            bool plain = layers.Count == 1 && layers[0].InFile && layers[0].Visible &&
                         layers[0].Name == MultiEditorComponentList.BaseLayerName;
            if (!plain)
            {
                var set = new XElement("Layers", new XAttribute("multi", multi), new XAttribute("active", activeIndex));
                foreach (SavedLayer layer in layers)
                {
                    var element = new XElement("Layer",
                        new XAttribute("name", XmlSafe(layer.Name)),
                        new XAttribute("visible", layer.Visible),
                        new XAttribute("inFile", layer.InFile));
                    if (layer.Tiles != null && layer.Tiles.Count > 0)
                    {
                        element.Value = string.Join(";", layer.Tiles.Select(t =>
                            string.Create(CultureInfo.InvariantCulture, $"{t.Id:X} {t.X} {t.Y} {t.Z} {(t.Invisible ? 0 : 1)} {t.Extra}")));
                    }

                    set.Add(element);
                }

                sets.Insert(0, set);
            }

            if (sets.Count == 0)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            new XDocument(new XElement("LayerSets", sets.Take(_setsKept))).Save(path);
        }

        /// <summary>
        /// Reads the layers kept for a client multi with these tiles. Null when there are none or
        /// the file cannot be read.
        /// </summary>
        public static (List<SavedLayer> Layers, int Active)? Load(int clientId, long fingerprint)
        {
            try
            {
                string multi = fingerprint.ToString(CultureInfo.InvariantCulture);
                XElement set = ReadSets(FilePath(clientId)).FirstOrDefault(s => (string)s.Attribute("multi") == multi);
                if (set == null)
                {
                    return null;
                }

                var layers = new List<SavedLayer>();
                foreach (XElement element in set.Elements("Layer"))
                {
                    var tiles = new List<(ushort, int, int, int, bool, int)>();
                    foreach (string record in element.Value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        string[] parts = record.Split(' ');
                        tiles.Add((ushort.Parse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                            int.Parse(parts[1], CultureInfo.InvariantCulture), int.Parse(parts[2], CultureInfo.InvariantCulture),
                            int.Parse(parts[3], CultureInfo.InvariantCulture), parts[4] == "0",
                            int.Parse(parts[5], CultureInfo.InvariantCulture)));
                    }

                    layers.Add(new SavedLayer((string)element.Attribute("name") ?? "Layer",
                        (bool?)element.Attribute("visible") ?? true, (bool?)element.Attribute("inFile") ?? true, tiles));
                }

                return layers.Count == 0 ? null : (layers, (int?)set.Attribute("active") ?? 0);
            }
            catch (Exception ex) when (ex is FormatException or IndexOutOfRangeException or OverflowException)
            {
                // A damaged set costs the layers, not the design.
                return null;
            }
        }

        /// <summary>The sets of layers in a file, newest first; none when it is missing or damaged.</summary>
        private static List<XElement> ReadSets(string path)
        {
            if (!File.Exists(path))
            {
                return new List<XElement>();
            }

            try
            {
                return XDocument.Load(path).Root?.Elements("Layers").ToList() ?? new List<XElement>();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or XmlException)
            {
                return new List<XElement>();
            }
        }

        /// <summary>A name without the characters XML cannot hold (control characters, half surrogate pairs).</summary>
        private static string XmlSafe(string name)
        {
            var text = new StringBuilder(name.Length);
            for (int i = 0; i < name.Length; i++)
            {
                if (char.IsHighSurrogate(name[i]) && i + 1 < name.Length && char.IsLowSurrogate(name[i + 1]))
                {
                    text.Append(name[i]).Append(name[++i]);
                }
                else if (XmlConvert.IsXmlChar(name[i]))
                {
                    text.Append(name[i]);
                }
            }

            return text.Length > 0 ? text.ToString() : "Layer";
        }
    }
}
