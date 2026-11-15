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
using System.Xml;
using System.Xml.Linq;
using UoFiddler.Plugin.MultiEditor.Editing;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>
    /// What a design file holds: the design as the editor has it, which the client's multi and
    /// the export formats cannot keep (its size as drawn, the layers, the floors).
    /// </summary>
    /// <param name="Name">Title of the design.</param>
    /// <param name="Width">Width in cells, as drawn: nothing is cut down to the tiles.</param>
    /// <param name="Height">Height in cells.</param>
    /// <param name="FirstFloorZ">Z of the first floor.</param>
    /// <param name="Levels">The floors when they were set by hand, else null.</param>
    /// <param name="Layers">The layers, bottom first, each with all its tiles by cell.</param>
    /// <param name="ActiveLayer">Index of the active layer.</param>
    internal sealed record DesignData(string Name, int Width, int Height, int FirstFloorZ,
        List<LevelDefinition> Levels, List<SavedLayer> Layers, int ActiveLayer);

    /// <summary>
    /// The editor's own design file (*.uomd): XML with the size, the floors and one element per
    /// layer listing its tiles as <c>id x y z flag extra</c> (id in hex, flag 0 for a dynamic tile).
    /// The root element is the header: <c>version</c> is the format version, which says how to
    /// read the rest. It counts changes of this format and has nothing to do with the plugin's version.
    /// </summary>
    internal static class DesignFile
    {
        public const string Extension = "uomd";

        public const string Filter = "Multi editor design (*.uomd)|*.uomd|All files (*.*)|*.*";

        /// <summary>
        /// The format version written to every file. Raise it when the content changes in a way
        /// an older editor would misread, and keep reading the versions below it: Load refuses a
        /// file with a higher version than this, or with none.
        /// </summary>
        public const int Version = 1;

        /// <summary>Largest width or height read: nothing the editor makes comes near it.</summary>
        private const int _maxSize = 512;

        public static void Save(string path, DesignData design)
        {
            var root = new XElement("MultiDesign",
                new XAttribute("version", Version),
                new XAttribute("name", LayerStore.XmlSafe(design.Name ?? string.Empty, string.Empty)),
                new XAttribute("width", design.Width),
                new XAttribute("height", design.Height),
                new XAttribute("firstFloorZ", design.FirstFloorZ));

            if (design.Levels != null)
            {
                root.Add(new XElement("Levels", design.Levels.Select(l =>
                    new XElement("Level", new XAttribute("name", LayerStore.XmlSafe(l.Name, "Floor")), new XAttribute("startZ", l.StartZ)))));
            }

            var layers = new XElement("Layers", new XAttribute("active", design.ActiveLayer));
            foreach (SavedLayer layer in design.Layers)
            {
                layers.Add(new XElement("Layer",
                    new XAttribute("name", LayerStore.XmlSafe(layer.Name, "Layer")),
                    new XAttribute("visible", layer.Visible),
                    LayerStore.TilesText(layer.Tiles)));
            }

            root.Add(layers);
            new XDocument(root).Save(path);
        }

        /// <summary>Reads a design file. Throws <see cref="InvalidDataException"/> when it is not one or is damaged.</summary>
        public static DesignData Load(string path)
        {
            try
            {
                XElement root = XDocument.Load(path).Root;
                if (root == null || root.Name.LocalName != "MultiDesign")
                {
                    throw new InvalidDataException("This is not a multi editor design file.");
                }

                int version = (int?)root.Attribute("version") ?? 0;
                if (version < 1)
                {
                    throw new InvalidDataException("The design file does not say which format version it is.");
                }

                if (version > Version)
                {
                    throw new InvalidDataException(
                        $"This design is format version {version}, saved by a newer multi editor. This editor reads up to version {Version}.");
                }

                int width = (int)root.Attribute("width");
                int height = (int)root.Attribute("height");
                if (width < 1 || height < 1 || width > _maxSize || height > _maxSize)
                {
                    throw new InvalidDataException($"The design's size ({width} x {height}) is not valid.");
                }

                List<LevelDefinition> levels = root.Element("Levels")?.Elements("Level")
                    .Select(l => new LevelDefinition((string)l.Attribute("name") ?? "Floor", (int)l.Attribute("startZ"))).ToList();

                var layers = new List<SavedLayer>();
                XElement layersElement = root.Element("Layers");
                foreach (XElement element in layersElement?.Elements("Layer") ?? Enumerable.Empty<XElement>())
                {
                    // Not "in the file" in the layers file's sense: every tile is listed here, by cell.
                    layers.Add(new SavedLayer((string)element.Attribute("name") ?? "Layer",
                        (bool?)element.Attribute("visible") ?? true, InFile: false, LayerStore.ParseTiles(element.Value)));
                }

                if (layers.Count == 0)
                {
                    layers.Add(new SavedLayer(MultiEditorComponentList.BaseLayerName, true, false, new()));
                }

                return new DesignData((string)root.Attribute("name") ?? string.Empty, width, height,
                    (int?)root.Attribute("firstFloorZ") ?? 0, levels is { Count: > 0 } ? levels : null,
                    layers, (int?)layersElement?.Attribute("active") ?? 0);
            }
            catch (Exception ex) when (ex is XmlException or FormatException or OverflowException or IndexOutOfRangeException or ArgumentNullException)
            {
                throw new InvalidDataException("The design file is damaged: " + ex.Message, ex);
            }
        }
    }
}
