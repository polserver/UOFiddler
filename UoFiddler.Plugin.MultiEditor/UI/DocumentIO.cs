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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Plugin.MultiEditor.Classes;
using UoFiddler.Plugin.MultiEditor.Tools;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>What an open/import produced.</summary>
    internal sealed record OpenedMulti(MultiComponentList Components, string Title, int? ClientId);

    /// <summary>What an import read, and what it could not.</summary>
    internal sealed record ImportResult(List<OpenedMulti> Opened, List<ImportFailure> Failures);

    /// <summary>Open, import, export, save-to-client and map-extract dialogs of the multi editor.</summary>
    internal static class DocumentIO
    {
        private const string _caption = "Multi Editor";

        private static readonly (Multis.ImportType Type, string Extension, string Description)[] _exportTypes =
        {
            // Every format that can be imported, in the import dialog's order.
            (Multis.ImportType.TXT, "txt", "Multi text"),
            (Multis.ImportType.UOA, "uoa.txt", "UO Architect text"),
            (Multis.ImportType.UOAB, "uoa", "UOA binary file"),
            (Multis.ImportType.WSC, "wsc", "WSC"),
            (Multis.ImportType.MULTICACHE, "dat", "MultiCache file"),
            (Multis.ImportType.UOADESIGN, "idx", "UOA design files, idx + bin"),
            (Multis.ImportType.CSV, "csv", "CSV"),
            (Multis.ImportType.UOX3, "uox3", "UOX3"),
            (Multis.ImportType.XML, "xml", "XML"),
            (Multis.ImportType.UOH, "uoh", "UO House Editor design"),
            (Multis.ImportType.WORLDFORGE, "xml", "WorldForge export"),
            (Multis.ImportType.MULTIFORGE, "txt", "Multiforge text (POL)")
        };

        /// <summary>
        /// Lists the client's multis (multi.mul and the UOP collection when present) and returns the chosen one.
        /// Null when cancelled.
        /// </summary>
        public static List<OpenedMulti> OpenFromClient(IWin32Window owner)
        {
            Dictionary<int, string> names = LoadNames();
            var entries = new List<MultiPickEntry>();

            Cursor previous = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                for (int i = 0; i < Multis.MaximumMultiIndex; ++i)
                {
                    MultiComponentList comp = Multis.GetComponents(i);
                    if (comp != MultiComponentList.Empty)
                    {
                        entries.Add(new MultiPickEntry(i, NameFor(names, i), "multi.mul", comp));
                    }
                }

                if (Multis.HasUopFile)
                {
                    for (int i = 0; i < Multis.MaximumMultiIndex; ++i)
                    {
                        MultiComponentList comp = Multis.GetUopComponents(i);
                        if (comp != MultiComponentList.Empty)
                        {
                            entries.Add(new MultiPickEntry(i, NameFor(names, i), "UOP", comp));
                        }
                    }
                }
            }
            finally
            {
                Cursor.Current = previous;
            }

            if (entries.Count == 0)
            {
                MessageBox.Show(owner, "The client has no multis loaded.", _caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            return Pick(owner, "Open multi from client", entries, true, true).ConvertAll(p => new OpenedMulti(p.Components, p.Name, p.Id));
        }

        /// <summary>
        /// Asks for one or more files of a supported import type and reads them. A file that fails
        /// is skipped and listed in the result; the multis of files that hold several are chosen
        /// from a list. Null when cancelled.
        /// </summary>
        public static ImportResult ImportFromFile(IWin32Window owner)
        {
            var filters = new List<string>();
            foreach ((_, string description, string pattern) in MultiFileImport.Formats)
            {
                filters.Add($"{description} ({pattern})|{pattern}");
            }

            using var dialog = new OpenFileDialog
            {
                Title = "Import multis from files",
                Multiselect = true,
                CheckFileExists = true,
                Filter = string.Join("|", filters),
                FilterIndex = 1
            };

            if (dialog.ShowDialog(owner) != DialogResult.OK)
            {
                return null;
            }

            // Formats share extensions, so the type is the selected filter entry's, for every file.
            Multis.ImportType type = MultiFileImport.Formats[dialog.FilterIndex - 1].Type;
            string[] files = dialog.FileNames;
            var opened = new List<OpenedMulti>();
            var failures = new List<ImportFailure>();
            var choices = new List<MultiPickEntry>();

            Cursor previous = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                foreach (string file in files)
                {
                    List<ImportedMulti> multis;
                    try
                    {
                        multis = MultiFileImport.Load(file, type);
                    }
                    catch (Exception ex)
                    {
                        failures.Add(new ImportFailure(file, MultiFileImport.Reason(ex)));
                        continue;
                    }

                    foreach (ImportedMulti multi in multis)
                    {
                        MultiComponentList components;
                        try
                        {
                            components = multi.Components;
                        }
                        catch (Exception ex)
                        {
                            failures.Add(new ImportFailure(file, $"{multi.Name}: {MultiFileImport.Reason(ex)}"));
                            continue;
                        }

                        if (multis.Count == 1)
                        {
                            opened.Add(new OpenedMulti(components, multi.Name, null));
                        }
                        else
                        {
                            choices.Add(new MultiPickEntry(null, multi.Name, Path.GetFileName(file), components));
                        }
                    }
                }
            }
            finally
            {
                Cursor.Current = previous;
            }

            // Files holding several multis (MultiCache, UOA batches and design sets) share one list.
            if (choices.Count > 0)
            {
                string title = files.Length == 1 ? $"Choose multis from {Path.GetFileName(files[0])}" : "Choose multis to import";
                opened.AddRange(Pick(owner, title, choices, files.Length > 1, false).ConvertAll(p => new OpenedMulti(p.Components, p.Name, null)));
            }

            return new ImportResult(opened, failures);
        }

        /// <summary>Lists the multis in every supported file under a folder and returns the chosen ones. Null when cancelled.</summary>
        public static ImportResult ImportFromFolder(IWin32Window owner)
        {
            using var dialog = new FolderImportDialog();
            return dialog.ShowDialog(owner) == DialogResult.OK ? new ImportResult(dialog.Opened, dialog.Failures) : null;
        }

        /// <summary>A UO Architect design as an opened multi, named after the design (or its file). Null when it holds no items.</summary>
        public static OpenedMulti FromDesign(UoaDesign design)
        {
            MultiComponentList multi = UoaDesignFile.Load(design);
            return multi == MultiComponentList.Empty ? null : new OpenedMulti(multi, DesignTitle(design), null);
        }

        public static string DesignTitle(UoaDesign design)
        {
            return string.IsNullOrWhiteSpace(design.Name) ? Path.GetFileNameWithoutExtension(design.FileName) : design.Name.Trim();
        }

        /// <summary>Asks for a file name and format and writes the design. True when written.</summary>
        /// <param name="firstFloorZ">Z of the design's first floor, written as the base height of a UO House Editor design.</param>
        public static bool ExportToFile(IWin32Window owner, MultiEditorComponentList list, string suggestedName, int firstFloorZ = 7)
        {
            if (list == null)
            {
                return false;
            }

            var filters = new List<string>();
            foreach ((_, string extension, string description) in _exportTypes)
            {
                filters.Add($"{description} (*.{extension})|*.{extension}");
            }

            filters.Add("All files (*.*)|*.*");

            string designName = (suggestedName ?? string.Empty).Trim();
            using var dialog = new SaveFileDialog
            {
                Title = "Export multi",
                InitialDirectory = Options.OutputPath,
                FileName = designName.Length > 0 ? designName : "multi",
                Filter = string.Join("|", filters),
                FilterIndex = 1,
                OverwritePrompt = true
            };

            if (dialog.ShowDialog(owner) != DialogResult.OK)
            {
                return false;
            }

            // The "All files" entry has no format of its own; fall back to the first one (text).
            int index = dialog.FilterIndex <= _exportTypes.Length ? dialog.FilterIndex - 1 : 0;
            (Multis.ImportType type, string ext, _) = _exportTypes[index];

            string fileName = dialog.FileName;
            if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
            {
                fileName += "." + ext;
            }

            if (designName.Length == 0)
            {
                // The XML entry is named after the design; take it from the file name.
                designName = Path.GetFileNameWithoutExtension(fileName);
            }

            try
            {
                MultiComponentList sdkList = list.ConvertToSdk();
                switch (type)
                {
                    case Multis.ImportType.TXT:
                        sdkList.ExportToTextFile(fileName);
                        break;
                    case Multis.ImportType.WSC:
                        sdkList.ExportToWscFile(fileName);
                        break;
                    case Multis.ImportType.UOA:
                        sdkList.ExportToUOAFile(fileName);
                        break;
                    case Multis.ImportType.CSV:
                        sdkList.ExportToCsvFile(fileName);
                        break;
                    case Multis.ImportType.UOX3:
                        sdkList.ExportToUox3File(fileName);
                        break;
                    case Multis.ImportType.XML:
                        sdkList.ExportToXmlFile(fileName, designName);
                        break;
                    case Multis.ImportType.UOAB:
                        sdkList.ExportToUOABinaryFile(fileName, designName);
                        break;
                    case Multis.ImportType.MULTICACHE:
                        sdkList.ExportToCacheFile(fileName);
                        break;
                    case Multis.ImportType.UOADESIGN:
                        sdkList.ExportToDesignerFiles(fileName, designName);
                        break;
                    case Multis.ImportType.UOH:
                        sdkList.ExportToUohFile(fileName, firstFloorZ);
                        break;
                    case Multis.ImportType.WORLDFORGE:
                        sdkList.ExportToWorldForgeFile(fileName);
                        break;
                    case Multis.ImportType.MULTIFORGE:
                        sdkList.ExportToMultiforgeFile(fileName);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(owner, $"Export failed: {ex.Message}", _caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            FileSavedDialog.Show(owner, fileName, "Multi design saved successfully.");
            return true;
        }

        /// <summary>
        /// Asks for the multi id and stores the design into the client table. Returns the id, or null when cancelled.
        /// </summary>
        public static int? SaveToClient(IWin32Window owner, MultiEditorComponentList list, int? suggestedId)
        {
            if (list == null)
            {
                return null;
            }

            // Every slot is listed, free ones marked, so a free slot can be picked without guessing ids.
            using var dialog = new DocumentIOSaveDialog(LoadNames(), suggestedId);
            if (dialog.ShowDialog(owner) != DialogResult.OK)
            {
                return null;
            }

            list.AddToSdkComponentList(dialog.SelectedId); // marks Multis changed and fires MultiChangeEvent
            if (dialog.WriteFormat is { } format)
            {
                WriteMultiFiles(owner, format);
            }

            return dialog.SelectedId;
        }

        /// <summary>
        /// Writes the client's multi table to the output folder in the chosen container, the way the
        /// Multis tab's Save does: first anything the save would cost is confirmed, then the files are
        /// written and the result shown.
        /// </summary>
        private static void WriteMultiFiles(IWin32Window owner, Ultima.Uop.ContainerFormat format)
        {
            try
            {
                IReadOnlyList<string> concerns = Ultima.Uop.ClientFileSaver.Preflight(Ultima.Uop.FileType.MultiCollection, format);
                if (concerns.Count > 0 && MessageBox.Show(owner, string.Join("\r\n\r\n", concerns) + "\r\n\r\nSave anyway?", _caption,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                {
                    return;
                }

                Ultima.Uop.ClientFileSaveResult result = WriteMultiTable(format);
                string message = "Saved " + string.Join(", ", result.FilesWritten) + ".";
                foreach (string warning in result.Warnings)
                {
                    message += "\r\n\r\n" + warning;
                }

                FileSavedDialog.Show(owner, Options.OutputPath, message);
            }
            catch (Exception ex)
            {
                // The slot is already set in memory, so nothing is lost: the Multis tab can still save it.
                MessageBox.Show(owner, "The multi was put into its slot, but writing the file failed:\r\n" + ex.Message +
                                       "\r\n\r\nYou can save it from the Multis tab.", _caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>Writes the multi table to <see cref="Options.OutputPath"/> and clears its unsaved mark.</summary>
        internal static Ultima.Uop.ClientFileSaveResult WriteMultiTable(Ultima.Uop.ContainerFormat format)
        {
            Ultima.Uop.ClientFileSaveResult result = Ultima.Uop.ClientFileSaver.Save(
                Ultima.Uop.FileType.MultiCollection, Options.OutputPath, format, Multis.Save);
            Options.ChangedUltimaClass["Multis"] = false;
            return result;
        }

        /// <summary>
        /// Asks for the map area (facet, position, size, Z range) and returns the statics in it as tiles to paste.
        /// Null when cancelled or the area holds no statics.
        /// </summary>
        public static List<ClipboardTile> ExtractFromMap(IWin32Window owner, Map map, Point location)
        {
            const int defaultSize = 16;
            var initial = new MapArea(map ?? Map.Felucca, Math.Max(0, location.X - (defaultSize / 2)), Math.Max(0, location.Y - (defaultSize / 2)),
                defaultSize, defaultSize, sbyte.MinValue, sbyte.MaxValue, false);

            // The caller pastes the tiles, so the "open as new design" choice is not offered.
            MapArea area = MapExtract.Ask(owner, initial, canPlaceIntoDesign: false);
            if (area == null)
            {
                return null;
            }

            List<ClipboardTile> tiles = MapExtract.Extract(area);
            if (tiles.Count == 0)
            {
                MessageBox.Show(owner, "There are no statics in that area.", _caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            return tiles;
        }

        /// <summary>The rows picked in the list (several with Ctrl or Shift); empty when cancelled.</summary>
        private static List<MultiPickEntry> Pick(IWin32Window owner, string title, List<MultiPickEntry> entries, bool showSource, bool clientSources)
        {
            using var dialog = new DocumentIOPickDialog(title, entries, showSource, clientSources);
            return dialog.ShowDialog(owner) == DialogResult.OK ? new List<MultiPickEntry>(dialog.SelectedEntries) : new List<MultiPickEntry>();
        }

        private static string NameFor(Dictionary<int, string> names, int id)
        {
            return names.TryGetValue(id, out string name) && !string.IsNullOrWhiteSpace(name) ? name : $"Multi 0x{id:X4}";
        }

        /// <summary>Multi names from Multilist.xml in the app data folder, the same file the old editor's list reads.</summary>
        private static Dictionary<int, string> LoadNames()
        {
            var names = new Dictionary<int, string>();
            try
            {
                string fileName = Path.Combine(Options.AppDataPath ?? string.Empty, "Multilist.xml");
                if (!File.Exists(fileName))
                {
                    return names;
                }

                var dom = new XmlDocument();
                dom.Load(fileName);
                XmlNodeList nodes = dom.SelectNodes("/Multis/Multi");
                if (nodes == null)
                {
                    return names;
                }

                foreach (XmlNode node in nodes)
                {
                    string idText = node.Attributes?["id"]?.Value;
                    string name = node.Attributes?["name"]?.Value;
                    if (name != null && int.TryParse(idText, out int id))
                    {
                        names[id] = name; // last one wins, like the old list
                    }
                }
            }
            catch (Exception)
            {
                // Names are cosmetic; a broken file falls back to "Multi 0xNNNN".
            }

            return names;
        }
    }
}
