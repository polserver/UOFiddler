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
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.MultiEditor.Tools;
using UoFiddler.Plugin.MultiEditor.Editing;
using UoFiddler.Plugin.MultiEditor.Model;
using UoFiddler.Plugin.MultiEditor.Storage;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    public partial class MultiEditorControl
    {
        // ---------------------------------------------------------------- documents

        private void NewDocumentPrompt()
        {
            using var dialog = new NewMultiDialog();
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            if (dialog.Foundation != null)
            {
                // A custom house starts as a copy of the client's foundation; it is not tied to that id.
                AddDocument(new OpenedMulti(dialog.Foundation, "Custom house", null));
                Session.LevelIndex = 2;
                return;
            }

            NewDocument(dialog.MultiWidth, dialog.MultiHeight, "Untitled", dialog.FirstFloorZ);
        }

        private void NewDocument(int width, int height, string title, int firstFloorZ = 0)
        {
            EditorDocument document = CreateDocument(title, null);
            document.Session.NewBlank(width, height, firstFloorZ);
            ShowDocument(document);
        }

        private void OpenFromClient()
        {
            AddDocuments(DocumentIO.OpenFromClient(this, BlueprintsSaved));
        }

        private void ImportFile()
        {
            AddImported(DocumentIO.ImportFromFile(this, BlueprintsSaved));
        }

        private void ImportFolder()
        {
            AddImported(DocumentIO.ImportFromFolder(this, BlueprintsSaved));
        }

        /// <summary>Blueprints were written by an open or import dialog: the Blueprints page shows the group (null for all).</summary>
        private void BlueprintsSaved(string showGroup)
        {
            _browser.ReloadBlueprints(showGroup);
        }

        /// <summary>Opens what an import read, then lists what it could not.</summary>
        private void AddImported(ImportResult result)
        {
            if (result == null)
            {
                return;
            }

            int opened = AddDocuments(result.Opened);
            MultiFileImport.ShowSummary(this, opened, result.Failures);
        }

        /// <summary>Opens each multi in its own tab, after asking when there are a lot of them. Returns how many opened.</summary>
        private int AddDocuments(List<OpenedMulti> opened)
        {
            if (opened == null || opened.Count == 0 || !ConfirmOpenMany(opened.Count))
            {
                return 0;
            }

            foreach (OpenedMulti multi in opened)
            {
                AddDocument(multi);
            }

            return opened.Count;
        }

        /// <summary>Asks before a selection opens more tabs than anyone picks by accident.</summary>
        private bool ConfirmOpenMany(int count)
        {
            const int withoutAsking = 12;
            return count <= withoutAsking || MessageBox.Show(this, $"Open {count} multis, each in its own tab?", "Multi Editor",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK;
        }

        private void AddDocument(OpenedMulti opened)
        {
            EditorDocument document = CreateDocument(opened.Title, opened.ClientId);
            document.Session.Load(opened.Components);

            // A multi from the client gets back the layers it was saved with, when it is still that multi.
            (int X, int Y) origin = (opened.Components.Center.X, opened.Components.Center.Y);
            if (opened.ClientId is { } clientId &&
                LayerStore.Load(clientId, document.Session.CompList.Fingerprint(origin)) is { } saved)
            {
                document.Session.RestoreLayers(saved.Layers, saved.Active, origin);
                document.Session.SetPlacedHouses(saved.Houses, origin.X, origin.Y);
            }
            ShowDocument(document);
        }

        private EditorDocument CreateDocument(string title, int? clientId)
        {
            var session = new EditorSession(_masks, _houseStyles) { Title = title };
            EditorSettings settings = EditorSettings.Current;
            session.ShowWorkPlane = settings.StartWithWorkPlane;
            session.ShowGrid = settings.StartWithGrid;
            session.HideAboveLevel = settings.HideFloorsAbove;
            session.PreviewTerrain = (PreviewTerrain)Math.Clamp(settings.PreviewTerrain, 0, 4);
            if (_active != null)
            {
                session.FloorPattern = _active.Session.FloorPattern;
                session.Roof.Shape = _active.Session.Roof.Shape;
                session.Roof.Ridge = _active.Session.Roof.Ridge;
                session.Roof.Gables = _active.Session.Roof.Gables;
                session.Roof.GableFill = _active.Session.Roof.GableFill;

                // A new tab starts with the tool setup you were working with.
                session.SetDrawTile(_active.Session.DrawTileId);
                session.Placement = _active.Session.Placement;
                session.BrushSize = _active.Session.BrushSize;
                session.ZStep = _active.Session.ZStep;
                session.HouseStyle = _active.Session.HouseStyle;
            }

            return new EditorDocument { Session = session, ClientId = clientId };
        }

        private void ShowDocument(EditorDocument document)
        {
            _documents.Add(document);
            _docTabs.TabPages.Add(new TabPage(document.Session.Title));
            if (_docTabs.SelectedIndex == _documents.Count - 1)
            {
                Activate(_documents.Count - 1);
            }
            else
            {
                _docTabs.SelectedIndex = _documents.Count - 1;
            }

            _canvas.Focus();
        }

        private void SaveToClient()
        {
            if (!SettleOpenDraft())
            {
                return;
            }

            if (AskIncludeHiddenLayers("Leaves them out of the client's multi. They stay in the editor's layers and come back when you open it here.") is not { } includeHidden)
            {
                return;
            }

            MultiEditorComponentList list = Session.CompList;
            int? id = DocumentIO.SaveToClient(this, list, _active.ClientId, includeHidden);
            if (id == null)
            {
                return;
            }

            try
            {
                LayerStore.Save(id.Value, list.SaveLayers(includeHidden), list.Layers.ToList().IndexOf(list.ActiveLayer), list.Fingerprint(list.SaveOrigin, includeHidden),
                    Session.PlacedHousesShifted(-list.SaveOrigin.X, -list.SaveOrigin.Y));
            }
            catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException or ArgumentException or System.Xml.XmlException)
            {
                MessageBox.Show(this, $"The multi was saved, but its layers could not be written:\r\n{ex.Message}", "Multi Editor",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            _active.ClientId = id;
            Session.Title = $"Multi 0x{id:X4}";
            Session.MarkSaved();
        }

        private void ExportFile()
        {
            if (!SettleOpenDraft())
            {
                return;
            }

            if (AskIncludeHiddenLayers("Leaves them out of the file. Exported files do not keep layers.") is not { } includeHidden)
            {
                return;
            }

            if (DocumentIO.ExportToFile(this, Session.CompList, Session.Title, Session.FirstFloorZ, includeHidden))
            {
                Session.MarkSaved();
            }
        }

        /// <summary>
        /// Before a save: asks whether tiles on hidden layers go into it. True without asking when
        /// no hidden layer has tiles; null when the save is cancelled.
        /// </summary>
        /// <summary>
        /// Before a save: a roof or house still being shaped is not in the multi. Asks whether to
        /// place it first. False when the save was cancelled.
        /// </summary>
        private bool SettleOpenDraft()
        {
            WingDraftTool draft = Session.House.HasPending ? Session.House : Session.Roof.HasPending ? Session.Roof : null;
            if (draft == null)
            {
                return true;
            }

            string what = draft is HouseTool ? "house" : "roof";
            DialogResult answer = MessageBox.Show(this,
                $"The {what} being shaped is not placed yet, so it is not part of the multi.\r\n\r\nPlace it before saving?\r\n\r\n" +
                "Yes places it. No saves without it and keeps it open.",
                "Multi Editor", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (answer == DialogResult.Cancel)
            {
                return false;
            }

            if (answer == DialogResult.Yes)
            {
                draft.Commit();
                _commands.RefreshStates();
            }

            return true;
        }

        private bool? AskIncludeHiddenLayers(string whenLeftOut)
        {
            MultiEditorComponentList list = Session.CompList;
            int hidden = list.HiddenLayerTileCount;
            if (hidden == 0)
            {
                return true;
            }

            string names = string.Join(", ", list.Layers.Where(l => !l.Visible && list.LayerTileCount(l) > 0).Select(l => l.Name));
            DialogResult answer = MessageBox.Show(this,
                $"{hidden:N0} tile(s) are on hidden layers ({names}).\r\n\r\nInclude them in the save?\r\n\r\nYes: saved with the rest.\r\nNo: {whenLeftOut}",
                "Hidden layers", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (answer == DialogResult.No && hidden == list.TileCount)
            {
                MessageBox.Show(this, "Every tile is on a hidden layer, so nothing would be saved. Show a layer first, or include the hidden layers.",
                    "Hidden layers", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            return answer == DialogResult.Cancel ? null : answer == DialogResult.Yes;
        }

        /// <summary>Opens design files (the editor's own format, with layers and floors), each in its own tab.</summary>
        private void OpenDesignFile()
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Open design file",
                Filter = DesignFile.Filter,
                InitialDirectory = Options.OutputPath,
                Multiselect = true
            };
            if (dialog.ShowDialog(this) != DialogResult.OK || !ConfirmOpenMany(dialog.FileNames.Length))
            {
                return;
            }

            foreach (string path in dialog.FileNames)
            {
                // A file that is open already is shown, not opened a second time.
                EditorDocument open = _documents.Find(d => string.Equals(d.FilePath, path, StringComparison.OrdinalIgnoreCase));
                if (open != null)
                {
                    _docTabs.SelectedIndex = _documents.IndexOf(open);
                    continue;
                }

                try
                {
                    DesignData design = DesignFile.Load(path);
                    string title = string.IsNullOrWhiteSpace(design.Name) ? System.IO.Path.GetFileNameWithoutExtension(path) : design.Name;
                    EditorDocument document = CreateDocument(title, null);
                    document.FilePath = path;
                    document.Session.LoadDesign(design);
                    ShowDocument(document);
                }
                catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException or System.IO.InvalidDataException)
                {
                    MessageBox.Show(this, $"{System.IO.Path.GetFileName(path)} could not be opened:\r\n{ex.Message}", "Open design file",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        /// <summary>
        /// Saves the design with its layers and floors to its design file, asking for a name the
        /// first time or with <paramref name="saveAs"/>. Hidden layers are always saved.
        /// </summary>
        private void SaveDesignFile(bool saveAs)
        {
            if (!SettleOpenDraft())
            {
                return;
            }

            string path = _active.FilePath;
            if (saveAs || path == null)
            {
                using var dialog = new SaveFileDialog
                {
                    Title = "Save design file",
                    Filter = DesignFile.Filter,
                    DefaultExt = DesignFile.Extension,
                    AddExtension = true,
                    OverwritePrompt = true,
                    InitialDirectory = path != null ? System.IO.Path.GetDirectoryName(path) : Options.OutputPath,
                    FileName = path != null ? System.IO.Path.GetFileName(path) : SafeFileName(Session.Title)
                };
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                path = dialog.FileName;
            }

            try
            {
                DesignFile.Save(path, Session.ToDesign());
            }
            catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException or ArgumentException)
            {
                MessageBox.Show(this, $"The design could not be saved:\r\n{ex.Message}", "Save design file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _active.FilePath = path;
            Session.MarkSaved();
            Tell($"Saved {path}");
        }

        private static string SafeFileName(string title)
        {
            string name = string.Concat((title ?? string.Empty).Split(System.IO.Path.GetInvalidFileNameChars())).Trim();
            return name.Length > 0 ? name : "design";
        }

        private void ResizePrompt()
        {
            int[] size = Prompt.Numbers(this, "Resize multi",
                ("Width (X)", Session.CompList.Width, 1, 200), ("Height (Y)", Session.CompList.Height, 1, 200));
            if (size != null)
            {
                Session.Resize(size[0], size[1]);
            }
        }

        /// <summary>Closes every tab, asking once when any of them has unsaved changes.</summary>
        private void CloseAllDocuments()
        {
            if (UnsavedWarning() is { } warning && MessageBox.Show(this, warning + "\r\nClose all designs anyway?",
                    "Multi Editor", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
            {
                return;
            }

            // The others first, the active one last, so no other tab is opened on the way.
            foreach (EditorDocument document in _documents.Where(d => d != _active).Reverse().ToList())
            {
                CloseDocument(document, ask: false);
            }

            CloseDocument(_active, ask: false);
        }

        /// <summary>What would be lost by closing every design now, or null when nothing would.</summary>
        private string UnsavedWarning()
        {
            int modified = _documents.Count(d => d.Session.Modified);
            int drafts = _documents.Count(d => d.Session.OpenDraft != null);
            if (modified + drafts == 0)
            {
                return null;
            }

            var parts = new List<string>();
            if (modified > 0)
            {
                parts.Add((modified == 1 ? "1 design has" : $"{modified} designs have") + " changes that were not saved to a design file or the client, or exported.");
            }

            if (drafts > 0)
            {
                parts.Add((drafts == 1 ? "1 design has" : $"{drafts} designs have") + " a house or roof draft that was not placed.");
            }

            return string.Join("\r\n", parts);
        }

        /// <summary>The application is closing: ask once when designs have unsaved changes or drafts.</summary>
        private void HostForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.Cancel || !_built || UnsavedWarning() is not { } warning)
            {
                return;
            }

            if (MessageBox.Show(this, "Multi Editor: " + warning + "\r\nClose anyway?", "Multi Editor", MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning) != DialogResult.OK)
            {
                e.Cancel = true;
            }
        }

        private void CloseDocument(EditorDocument document, bool ask = true)
        {
            if (document == null)
            {
                return;
            }

            // A draft that is not placed is not in the multi: place it, drop it, or stay.
            if (ask && document.Session.OpenDraft is { } draft)
            {
                DialogResult answer = MessageBox.Show(this,
                    $"\"{document.Session.Title}\" has a {(draft is HouseTool ? "house" : "roof")} draft that is not placed.\r\n" +
                    "Yes places it in the design, No drops it.",
                    "Multi Editor", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (answer == DialogResult.Cancel)
                {
                    return;
                }

                if (answer == DialogResult.Yes)
                {
                    draft.Commit();
                }
                else
                {
                    draft.Discard();
                }
            }

            if (ask && document.Session.Modified && MessageBox.Show(this,
                    $"\"{document.Session.Title}\" has changes that were not saved to a design file or the client, or exported.\r\nClose it anyway?",
                    "Multi Editor", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
            {
                return;
            }

            int index = _documents.IndexOf(document);
            if (_active == document)
            {
                Detach();
            }

            _documents.RemoveAt(index);
            _removingTab = true;
            try
            {
                _docTabs.TabPages.RemoveAt(index);
            }
            finally
            {
                _removingTab = false;
            }

            Activate(_docTabs.SelectedIndex);
        }
    }
}
