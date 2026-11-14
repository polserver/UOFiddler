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
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace UoFiddler.Controls.Classes
{
    /// <summary>
    /// One frame listed in a frames XML file.
    /// </summary>
    public sealed class FramesXmlFrame
    {
        public int Direction { get; set; }
        public int Index { get; set; }
        public Point Center { get; set; }

        /// <summary>Image file name, in the XML file's folder or its <c>_frames</c> folder.</summary>
        public string File { get; set; }
    }

    /// <summary>
    /// Animation frames saved as one PNG per frame plus an XML file listing each frame's direction, index and
    /// center. The images go in a <c>{name}_frames</c> folder next to <c>{name}.xml</c>.
    /// </summary>
    public static class FramesXml
    {
        public static string GetFramesFolder(string xmlFile)
        {
            return Path.Combine(Path.GetDirectoryName(xmlFile) ?? string.Empty, Path.GetFileNameWithoutExtension(xmlFile) + "_frames");
        }

        /// <summary>
        /// Saves each frame as <c>{direction}_{index}.png</c> in the frames folder and lists them in <paramref name="xmlFile"/>.
        /// </summary>
        public static void Write(string xmlFile, IReadOnlyList<(int Direction, int Index, Point Center, Bitmap Image)> frames)
        {
            string framesFolder = GetFramesFolder(xmlFile);
            Directory.CreateDirectory(framesFolder);

            var root = new XElement("Animation");
            foreach (var (direction, index, center, image) in frames)
            {
                string fileName = $"{direction}_{index:D2}.png";
                image.Save(Path.Combine(framesFolder, fileName), ImageFormat.Png);
                root.Add(new XElement("Frame",
                    new XAttribute("dir", direction),
                    new XAttribute("index", index),
                    new XAttribute("centerX", center.X),
                    new XAttribute("centerY", center.Y),
                    new XAttribute("file", fileName)));
            }

            new XDocument(root).Save(xmlFile);
        }

        /// <exception cref="InvalidDataException">The file is not a frames XML file.</exception>
        public static List<FramesXmlFrame> Read(string xmlFile)
        {
            string name = Path.GetFileName(xmlFile);
            XDocument document;
            try
            {
                document = XDocument.Load(xmlFile);
            }
            catch (XmlException ex)
            {
                throw new InvalidDataException($"{name} is not a valid frames XML file: {ex.Message}", ex);
            }

            if (document.Root?.Name.LocalName != "Animation")
            {
                throw new InvalidDataException($"{name} is not a frames XML file: the root element is not <Animation>.");
            }

            var frames = new List<FramesXmlFrame>();
            foreach (XElement element in document.Root.Elements("Frame"))
            {
                frames.Add(new FramesXmlFrame
                {
                    Direction = ReadInt(element, "dir", name),
                    Index = ReadInt(element, "index", name),
                    Center = new Point(ReadInt(element, "centerX", name), ReadInt(element, "centerY", name)),
                    File = (string)element.Attribute("file") ?? throw new InvalidDataException($"{name}: a <Frame> has no file attribute.")
                });
            }

            return frames;
        }

        /// <summary>
        /// Path of a frame's image, or null if it is in neither the XML file's folder nor the frames folder.
        /// Only the file name is used, so an XML file can't point outside those folders.
        /// </summary>
        public static string FindImage(string xmlFile, FramesXmlFrame frame)
        {
            string fileName = Path.GetFileName(frame.File);
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            string beside = Path.Combine(Path.GetDirectoryName(xmlFile) ?? string.Empty, fileName);
            if (System.IO.File.Exists(beside))
            {
                return beside;
            }

            string inFolder = Path.Combine(GetFramesFolder(xmlFile), fileName);
            return System.IO.File.Exists(inFolder) ? inFolder : null;
        }

        /// <summary>
        /// Loads an image as 32bpp ARGB without keeping the file locked.
        /// </summary>
        public static Bitmap LoadImage(string file)
        {
            using (var loaded = new Bitmap(file))
            {
                return loaded.Clone(new Rectangle(0, 0, loaded.Width, loaded.Height), PixelFormat.Format32bppArgb);
            }
        }

        private static int ReadInt(XElement element, string attribute, string name)
        {
            string value = (string)element.Attribute(attribute);
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            {
                throw new InvalidDataException($"{name}: a <Frame> has no valid {attribute} attribute.");
            }

            return result;
        }
    }
}
