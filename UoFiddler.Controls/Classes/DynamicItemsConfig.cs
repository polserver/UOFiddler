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
using Ultima;

namespace UoFiddler.Controls.Classes
{
    public static class DynamicItemsConfig
    {
        private static bool _loaded;

        public static void EnsureLoaded()
        {
            if (_loaded)
            {
                return;
            }
            _loaded = true;

            string path = Path.Combine(Options.AppDataPath, "DynamicItems.xml");
            if (!File.Exists(path))
            {
                Options.Logger?.Warning("DynamicItems.xml not found at {Path}", path);
                return;
            }

            var ids = new HashSet<ushort>();
            var doc = new XmlDocument();
            doc.Load(path);

            foreach (XmlNode node in doc.DocumentElement!.ChildNodes)
            {
                if (node is not XmlElement elem)
                {
                    continue;
                }

                switch (elem.LocalName)
                {
                    case "Item":
                        if (TryParseId(elem.GetAttribute("id"), out ushort id))
                        {
                            ids.Add(id);
                        }
                        break;
                    case "Range":
                        if (TryParseId(elem.GetAttribute("from"), out ushort from) &&
                            TryParseId(elem.GetAttribute("to"), out ushort to))
                        {
                            for (ushort i = from; i <= to; i++)
                            {
                                ids.Add(i);
                            }
                        }
                        break;
                }
            }

            MultiComponentList.DynamicItemIds = ids;
        }

        private static bool TryParseId(string value, out ushort result)
        {
            result = 0;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return ushort.TryParse(value[2..], NumberStyles.HexNumber, null, out result);
            }
            return ushort.TryParse(value, out result);
        }
    }
}
