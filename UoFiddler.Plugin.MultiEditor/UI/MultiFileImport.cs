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
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Ultima;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// One multi read from an import file. UO Architect designs read their items only when
    /// <see cref="Components"/> is first used, so a large batch lists quickly.
    /// </summary>
    internal sealed class ImportedMulti
    {
        private readonly Func<MultiComponentList> _load;
        private MultiComponentList _components;

        public ImportedMulti(string name, Multis.ImportType format, string file, int tileCount, MultiComponentList components)
        {
            Name = name;
            Format = format;
            File = file;
            TileCount = tileCount;
            _components = components;
        }

        public ImportedMulti(string name, Multis.ImportType format, string file, int tileCount, Func<MultiComponentList> load)
        {
            Name = name;
            Format = format;
            File = file;
            TileCount = tileCount;
            _load = load;
        }

        /// <summary>The tab title: the file name, or file and entry for one multi out of several.</summary>
        public string Name { get; }

        public Multis.ImportType Format { get; }

        /// <summary>Full path of the file it came from.</summary>
        public string File { get; }

        public int TileCount { get; }

        /// <summary>The multi's tiles. Throws when the file cannot be read.</summary>
        public MultiComponentList Components => _components ??= _load();
    }

    /// <summary>A file, or one multi in it, that could not be imported.</summary>
    internal sealed record ImportFailure(string File, string Reason);

    /// <summary>Recognises and reads the multi file formats the editor imports.</summary>
    internal static class MultiFileImport
    {
        // Several formats share an extension (UOA text and plain text are both .txt, XML and
        // WorldForge both .xml); Detect tells them apart by their content.
        public static readonly (Multis.ImportType Type, string Description, string Pattern)[] Formats =
        {
            (Multis.ImportType.TXT, "Txt file", "*.txt"),
            (Multis.ImportType.UOA, "UOA text file", "*.txt"),
            (Multis.ImportType.UOAB, "UOA binary file", "*.uoa"),
            (Multis.ImportType.WSC, "WSC file", "*.wsc"),
            (Multis.ImportType.MULTICACHE, "MultiCache file", "Multicache.dat"),
            (Multis.ImportType.UOADESIGN, "UOA design file", "Designs.*"),
            (Multis.ImportType.CSV, "CSV file", "*.csv"),
            (Multis.ImportType.UOX3, "UOX3 file", "*.uox3"),
            (Multis.ImportType.XML, "XML file (UOFiddler export)", "*.xml"),
            (Multis.ImportType.UOH, "UO House Editor design", "*.uoh"),
            (Multis.ImportType.WORLDFORGE, "WorldForge export", "*.xml")
        };

        /// <summary>Short format names for lists.</summary>
        public static string FormatName(Multis.ImportType type) => type switch
        {
            Multis.ImportType.TXT => "Multi text",
            Multis.ImportType.UOA => "UOA text",
            Multis.ImportType.UOAB => "UOA binary",
            Multis.ImportType.WSC => "WSC",
            Multis.ImportType.MULTICACHE => "MultiCache",
            Multis.ImportType.UOADESIGN => "UOA design set",
            Multis.ImportType.CSV => "CSV",
            Multis.ImportType.UOX3 => "UOX3",
            Multis.ImportType.XML => "UOFiddler XML",
            Multis.ImportType.UOH => "UO House Editor",
            Multis.ImportType.WORLDFORGE => "WorldForge",
            _ => type.ToString()
        };

        /// <summary>
        /// The files under a folder that may hold a multi, by extension. Folders that cannot be
        /// read are skipped.
        /// </summary>
        public static IEnumerable<string> CandidateFiles(string folder, bool recursive)
        {
            var options = new EnumerationOptions { RecurseSubdirectories = recursive, IgnoreInaccessible = true };
            foreach (string file in Directory.EnumerateFiles(folder, "*", options))
            {
                string extension = Path.GetExtension(file).ToLowerInvariant();
                if (extension is ".txt" or ".xml" or ".uoa" or ".wsc" or ".csv" or ".uox3" or ".uoh" or ".idx" ||
                    IsMultiCache(file))
                {
                    yield return file;
                }
            }
        }

        /// <summary>
        /// The format of a file, from its extension and, where formats share one, its content.
        /// Null when the file is not one the editor reads (a readme.txt, an unrelated .xml or .idx).
        /// </summary>
        public static Multis.ImportType? Detect(string path)
        {
            if (IsMultiCache(path))
            {
                return Multis.ImportType.MULTICACHE;
            }

            try
            {
                return Path.GetExtension(path).ToLowerInvariant() switch
                {
                    ".txt" => DetectText(path),
                    ".xml" => DetectXml(path),
                    ".uoa" => Multis.ImportType.UOAB,
                    ".wsc" => Multis.ImportType.WSC,
                    ".csv" => Multis.ImportType.CSV,
                    ".uox3" => Multis.ImportType.UOX3,
                    ".uoh" => Multis.ImportType.UOH,
                    ".idx" => IsDesignSet(path) ? Multis.ImportType.UOADESIGN : null,
                    _ => null
                };
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Unreadable here means unreadable on import too; the caller lists it by extension.
                return null;
            }
        }

        /// <summary>
        /// The multis a file holds. Throws with a readable message when the file cannot be read or
        /// holds nothing.
        /// </summary>
        public static List<ImportedMulti> Load(string path, Multis.ImportType type)
        {
            string title = Path.GetFileNameWithoutExtension(path);
            var result = new List<ImportedMulti>();
            switch (type)
            {
                case Multis.ImportType.MULTICACHE:
                    {
                        List<MultiComponentList> list = Multis.LoadFromCache(path);
                        for (int i = 0; i < list.Count; ++i)
                        {
                            if (!IsEmpty(list[i]))
                            {
                                result.Add(new ImportedMulti($"{title} - Entry {i}", type, path, list[i].SortedTiles.Length, list[i]));
                            }
                        }

                        break;
                    }
                case Multis.ImportType.UOAB:
                case Multis.ImportType.UOADESIGN:
                    {
                        // A design set may be picked by its .bin; the headers are in the .idx.
                        string file = type == Multis.ImportType.UOADESIGN ? Path.ChangeExtension(path, ".idx") : path;
                        if (type == Multis.ImportType.UOADESIGN && !File.Exists(Path.ChangeExtension(path, ".bin")))
                        {
                            throw new InvalidDataException("The design set has no .bin next to its .idx.");
                        }

                        List<UoaDesign> designs = UoaDesignFile.ReadHeaders(file).Where(d => d.ItemCount > 0).ToList();
                        foreach (UoaDesign design in designs)
                        {
                            string name = designs.Count == 1 ? DocumentIO.DesignTitle(design) : $"{title} - {DocumentIO.DesignTitle(design)}";
                            result.Add(new ImportedMulti(name, type, path, design.ItemCount, () => LoadDesign(design)));
                        }

                        break;
                    }
                default:
                    {
                        var multi = new MultiComponentList(path, type);
                        if (!IsEmpty(multi))
                        {
                            result.Add(new ImportedMulti(title, type, path, multi.SortedTiles.Length, multi));
                        }

                        break;
                    }
            }

            if (result.Count == 0)
            {
                throw new InvalidDataException($"No multi found in it as {FormatName(type)}.");
            }

            return result;
        }

        /// <summary>A short reason for a failed import, for summaries.</summary>
        public static string Reason(Exception ex) => ex switch
        {
            InvalidDataException => ex.Message,
            FormatException or IndexOutOfRangeException or OverflowException or XmlException or EndOfStreamException =>
                "Not in the expected format (" + ex.Message + ")",
            _ => ex.Message
        };

        /// <summary>Lists what could not be imported. Shows nothing when everything went in.</summary>
        public static void ShowSummary(IWin32Window owner, int opened, IReadOnlyList<ImportFailure> failures)
        {
            if (failures == null || failures.Count == 0)
            {
                return;
            }

            const int listed = 15;
            var text = new StringBuilder();
            text.Append(opened == 1 ? "Opened 1 multi. " : $"Opened {opened} multis. ");
            text.Append(failures.Count == 1 ? "1 could not be imported:" : $"{failures.Count} could not be imported:");
            text.Append("\r\n");
            foreach (ImportFailure failure in failures.Take(listed))
            {
                text.Append("\r\n").Append(Path.GetFileName(failure.File)).Append(": ").Append(failure.Reason);
            }

            if (failures.Count > listed)
            {
                text.Append("\r\n\r\n").Append(CultureInfo.CurrentCulture, $"and {failures.Count - listed} more.");
            }

            MessageBox.Show(owner, text.ToString(), "Multi Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static MultiComponentList LoadDesign(UoaDesign design)
        {
            MultiComponentList multi = UoaDesignFile.Load(design);
            return IsEmpty(multi) ? throw new InvalidDataException("The design holds no items.") : multi;
        }

        private static bool IsEmpty(MultiComponentList multi) =>
            multi == null || multi == MultiComponentList.Empty || multi.SortedTiles == null || multi.SortedTiles.Length == 0;

        private static bool IsMultiCache(string path) =>
            string.Equals(Path.GetFileName(path), "Multicache.dat", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// UO Architect text starts with "6 version"; plain multi text has an item on every line:
        /// id (hex, 0x optional), x, y, z and flags.
        /// </summary>
        private static Multis.ImportType? DetectText(string path)
        {
            using var reader = new StreamReader(path);
            string line;
            do
            {
                line = reader.ReadLine();
            }
            while (line != null && line.Trim().Length == 0);

            if (line == null)
            {
                return null;
            }

            string[] parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 && int.TryParse(parts[0], out _) && parts[1].Equals("version", StringComparison.OrdinalIgnoreCase))
            {
                return Multis.ImportType.UOA;
            }

            if (parts.Length == 5)
            {
                string id = parts[0].StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? parts[0][2..] : parts[0];
                if (ushort.TryParse(id, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _) &&
                    parts.Skip(1).All(p => int.TryParse(p, NumberStyles.Integer, CultureInfo.InvariantCulture, out _)))
                {
                    return Multis.ImportType.TXT;
                }
            }

            return null;
        }

        /// <summary>A UOFiddler export has Item elements with an ID; a WorldForge export has tile elements with an id.</summary>
        private static Multis.ImportType? DetectXml(string path)
        {
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null };
            try
            {
                using XmlReader reader = XmlReader.Create(path, settings);
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }

                    if (reader.LocalName == "Item" && reader.GetAttribute("ID") != null)
                    {
                        return Multis.ImportType.XML;
                    }

                    if (reader.LocalName.Equals("tile", StringComparison.OrdinalIgnoreCase) && reader.GetAttribute("id") != null)
                    {
                        return Multis.ImportType.WORLDFORGE;
                    }
                }
            }
            catch (XmlException)
            {
                // Not well-formed: not something either reader would take.
            }

            return null;
        }

        /// <summary>A UO Architect design set: an .idx starting with a count and version 0, with a .bin beside it.</summary>
        private static bool IsDesignSet(string path)
        {
            if (!File.Exists(Path.ChangeExtension(path, ".bin")))
            {
                return false;
            }

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (stream.Length < 8)
            {
                return false;
            }

            using var reader = new BinaryReader(stream);
            return reader.ReadInt32() >= 0 && reader.ReadInt32() == 0;
        }
    }
}
