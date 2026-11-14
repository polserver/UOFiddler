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
using System.Xml;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>
    /// Facing table for tiles the client's house tables do not list (classic stairs, signs,
    /// furniture): which id is the same piece a quarter turn to the left. Read from
    /// MultiRotationTable.xml (<c>&lt;Map from="0x0751" to="0x0752" /&gt;</c> rows), first from the
    /// profile's plugins folder, then from beside the plugin. Ids not listed keep their graphic.
    /// </summary>
    internal sealed class RotationTable
    {
        private const string _fileName = "MultiRotationTable.xml";

        private readonly Dictionary<ushort, ushort> _left = new Dictionary<ushort, ushort>();
        private readonly Dictionary<ushort, ushort> _right = new Dictionary<ushort, ushort>();

        public int Count => _left.Count;

        public static RotationTable LoadDefault()
        {
            var candidates = new List<string>();
            if (!string.IsNullOrEmpty(Options.AppDataPath))
            {
                candidates.Add(Path.Combine(Options.AppDataPath, "plugins", _fileName));
            }

            string pluginDir = Path.GetDirectoryName(typeof(RotationTable).Assembly.Location);
            if (!string.IsNullOrEmpty(pluginDir))
            {
                candidates.Add(Path.Combine(pluginDir, _fileName));
            }

            foreach (string path in candidates)
            {
                if (!File.Exists(path))
                {
                    continue;
                }

                try
                {
                    return Load(path);
                }
                catch (Exception ex) when (ex is IOException or XmlException or UnauthorizedAccessException)
                {
                    // An unreadable table only means untabled pieces keep their graphic.
                }
            }

            return new RotationTable();
        }

        public static RotationTable Load(string path)
        {
            var table = new RotationTable();
            var dom = new XmlDocument();
            dom.Load(path);

            foreach (XmlNode node in dom.GetElementsByTagName("Map"))
            {
                if (TryParseId(node.Attributes?["from"]?.Value, out ushort from) && TryParseId(node.Attributes?["to"]?.Value, out ushort to))
                {
                    table._left[from] = to;
                    table._right[to] = from;
                }
            }

            return table;
        }

        /// <summary>The id of the same piece turned a quarter left, or the id itself.</summary>
        public ushort TurnLeft(ushort id) => _left.TryGetValue(id, out ushort to) ? to : id;

        /// <summary>The id of the same piece turned a quarter right, or the id itself.</summary>
        public ushort TurnRight(ushort id) => _right.TryGetValue(id, out ushort to) ? to : id;

        private static bool TryParseId(string text, out ushort value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.Trim();
            return text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? ushort.TryParse(text[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value)
                : ushort.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }
    }
}
