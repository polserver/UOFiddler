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
using UoFiddler.Plugin.MultiEditor.Editing;
using UoFiddler.Plugin.MultiEditor.House;
using UoFiddler.Plugin.MultiEditor.Model;
using UoFiddler.Plugin.MultiEditor.Storage;
using UoFiddler.Plugin.MultiEditor.Styles;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    public partial class MultiEditorControl
    {
        private bool _openingHousePage;

        private static bool IsHouseTool(ToolKind kind)
        {
            return kind is ToolKind.Wall or ToolKind.Floor or ToolKind.Door or ToolKind.Stair or ToolKind.Roof or ToolKind.Foundation;
        }

        private void ChooseHouseStyle(HouseStyle style)
        {
            if (Session == null || style == null)
            {
                return;
            }

            // The House page names its current style when it is brought up for a pick of the
            // house panel: that is not the pick, which is the next style clicked there.
            if (_housePick != null && _openingHousePage)
            {
                return;
            }

            // A pick asked for by the house panel is a material of the draft, not a tool's style.
            // It stays on, so one style after another can be tried, until Esc, another tool or
            // another button of the panel ends it.
            if (_housePick is { } pick && pick.Kind == style.Kind)
            {
                if (pick.Storey == HousePanel.FlatRoof)
                {
                    Session.House.SetFlatRoofStyle(style);
                }
                else if (pick.Storey == HousePanel.FoundationPick)
                {
                    Session.House.SetFoundationStyle(style);
                }
                else
                {
                    Session.House.SetMaterial(style, style.Kind == HousePartKind.Roof ? null : pick.Storey);
                }

                SetTool(ToolKind.House, fromHousePage: true);
                return;
            }

            CancelHousePick();

            // With the House tool in use, a style clicked on the House page is a material of the
            // house too: a wall or floor style of the storey the chosen floor means (of every
            // storey with all floors shown), a roof, door or stair style of the whole house. To
            // hang single doors or build stairs in another style, the Doors and Stairs tools are chosen first.
            if (Session.Tool == ToolKind.House &&
                style.Kind is HousePartKind.Wall or HousePartKind.Floor or HousePartKind.Roof or HousePartKind.Door or HousePartKind.Stair)
            {
                int? storey = style.Kind == HousePartKind.Roof || Session.WorkZ == null ? null : Session.DraftStorey;
                Session.House.SetMaterial(style, storey);
                _canvas.Focus();
                return;
            }

            Session.HouseStyle = style;
            ToolKind kind = style.Kind switch
            {
                HousePartKind.Floor => ToolKind.Floor,
                HousePartKind.Door => ToolKind.Door,
                HousePartKind.Stair => ToolKind.Stair,
                HousePartKind.Roof => ToolKind.Roof,
                _ => ToolKind.Wall
            };
            SetTool(kind, fromHousePage: true);
        }

        /// <summary>Brings the House page up on a kind, without taking that as a style picked there.</summary>
        private void OpenHousePage(HousePartKind kind, bool foundation)
        {
            _openingHousePage = true;
            try
            {
                _browser.ShowHousePage(kind, foundation);
            }
            finally
            {
                _openingHousePage = false;
            }
        }

        /// <summary>The house panel asks for a style: the House page comes up on that kind and the next pick there goes to the draft.</summary>
        private void PickHouseMaterial(HousePartKind kind, int storey)
        {
            if (Session == null)
            {
                return;
            }

            if (!_browserShown)
            {
                ToggleSidebar(browser: true);
            }

            _housePick = (kind, storey);
            _housePanel.MarkPick(kind, storey);
            OpenHousePage(kind, foundation: false);

            _pickHint = kind == HousePartKind.Roof ? "Pick the roof style of the house on the House page."
                : kind == HousePartKind.Door ? "Pick the door style of the house on the House page."
                : kind == HousePartKind.Stair ? "Pick the stair style of the house on the House page."
                : storey == HousePanel.FoundationPick ? "Pick the wall style of the foundation on the House page (a quarter wall suits it)."
                : storey == HousePanel.FlatRoof ? "Pick the floor style of the flat roofs on the House page."
                : $"Pick the {(kind == HousePartKind.Wall ? "wall" : "floor")} style of storey {storey + 1} on the House page.";
            _pickHint += " Click one style after another to try them; Esc ends.";
            UpdateStatus();
        }

        /// <summary>Drops a style pick the house panel asked for and has not got yet.</summary>
        private void CancelHousePick()
        {
            if (_housePick != null)
            {
                _housePick = null;
                _housePanel.MarkPick(null, 0);
                UpdateStatus();
            }
        }

        private string _pickHint;

        /// <summary>
        /// Opens the style editor. When it saved, the styles are new objects: the House page shows
        /// them, and every open design's draft and chosen style take their successors.
        /// </summary>
        private void EditHouseStyles(HouseStyle start)
        {
            if (_houseStyles == null)
            {
                return;
            }

            bool saved;
            using (var dialog = new HouseStyleEditorDialog(_houseStyles, start, () => _browser.CurrentTile))
            {
                dialog.ShowDialog(this);
                saved = dialog.Saved;
            }

            if (!saved)
            {
                return;
            }

            HouseStylesReloaded();
            Tell("House styles saved.");
        }

        /// <summary>
        /// The client files changed: the house tables are read again from the new client, so no
        /// style keeps tiles the new client does not have.
        /// </summary>
        private void ReloadHouseStylesForClient()
        {
            if (_houseStyles == null)
            {
                return;
            }

            try
            {
                _houseStyles.Reload();
            }
            catch (Exception ex)
            {
                Tell("The client's house tables could not be read again: " + ex.Message);
                return;
            }

            HouseStylesReloaded();
        }

        /// <summary>The styles are new objects: the House page shows them, and every open design's draft and chosen style take their successors.</summary>
        private void HouseStylesReloaded()
        {
            _browser.SetHouseStyles(_houseStyles);
            _styleErrors = _houseStyles.Errors.Count == 0 ? null : string.Join("\r\n", _houseStyles.Errors.Take(5));
            TellStyleErrors();
            foreach (EditorDocument document in _documents)
            {
                document.Session.HouseStyle = _houseStyles.Successor(document.Session.HouseStyle);
                document.Session.House.StylesReloaded();
            }

            _housePanel.RefreshDraft();
            _canvas.Invalidate();
        }

        // What went wrong reading the house tables or a style file; told once, when the House page is first used.
        private string _styleErrors;

        private void TellStyleErrors()
        {
            if (_styleErrors == null)
            {
                return;
            }

            string errors = _styleErrors;
            _styleErrors = null;
            MessageBox.Show(this, "Some house style data could not be read and was left out:\r\n\r\n" + errors +
                                  "\r\n\r\nYour own style file is " + (HouseStyleConfig.UserPath ?? "not set") + ".",
                _messageTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>Saves the styles of the house draft as a theme in the user's copy of the style file.</summary>
        private void SaveHouseTheme()
        {
            if (Session == null || _houseStyles == null)
            {
                return;
            }

            using var prompt = new TextPromptForm("Save theme", "Theme name", Session.House.CurrentTheme()?.Name ?? "My theme");
            if (prompt.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(prompt.Value))
            {
                return;
            }

            try
            {
                HouseStyleConfig.SaveUserTheme(Session.House.ThemeOfDraft(prompt.Value.Trim()));
                _houseStyles.ReloadThemes();
                _housePanel.RefreshDraft();
                Tell($"Theme \"{prompt.Value.Trim()}\" saved.");
            }
            catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException)
            {
                MessageBox.Show(this, "The theme could not be saved: " + ex.Message, "Save theme", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeleteHouseTheme(HouseTheme theme)
        {
            if (_houseStyles == null ||
                MessageBox.Show(this, $"Delete the theme \"{theme.Name}\"?", "Delete theme", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            try
            {
                HouseStyleConfig.DeleteUserTheme(theme.Name);
                _houseStyles.ReloadThemes();
                _housePanel.RefreshDraft();
            }
            catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException)
            {
                MessageBox.Show(this, "The theme could not be deleted: " + ex.Message, "Delete theme", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            }
        }

        private static string HouseDraftFolder
        {
            get
            {
                string folder = System.IO.Path.Combine(Options.AppDataPath, "MultiEditorHouses");
                System.IO.Directory.CreateDirectory(folder);
                return folder;
            }
        }

        /// <summary>Writes the house being designed to a house draft file, to open again in any design.</summary>
        private void SaveHouseDraft()
        {
            if (Session == null || !Session.House.HasPending)
            {
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Title = "Save house draft", Filter = HouseDraftFile.Filter, DefaultExt = HouseDraftFile.Extension,
                InitialDirectory = HouseDraftFolder, FileName = "House"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                new System.Xml.Linq.XDocument(Session.House.ToXml(System.IO.Path.GetFileNameWithoutExtension(dialog.FileName))).Save(dialog.FileName);
            }
            catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException)
            {
                MessageBox.Show(this, "The house draft could not be saved:\r\n" + ex.Message, _messageTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>Opens a house draft file as the draft of the House tool; the draft being designed can be had back with undo.</summary>
        private void OpenHouseDraft()
        {
            if (Session == null)
            {
                return;
            }

            using var dialog = new OpenFileDialog { Title = "Open house draft", Filter = HouseDraftFile.Filter, InitialDirectory = HouseDraftFolder };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                Session.House.OpenXml(System.Xml.Linq.XDocument.Load(dialog.FileName).Root);
                SetTool(ToolKind.House);
            }
            catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException or System.Xml.XmlException or System.IO.InvalidDataException)
            {
                MessageBox.Show(this, "The house draft could not be opened:\r\n" + ex.Message, _messageTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>Saves the house being designed, as built, to the blueprints: tiles to stamp, not a draft.</summary>
        private void SaveHouseBlueprint()
        {
            if (Session == null || !Session.House.HasPending)
            {
                return;
            }

            var built = Session.House.TilesOf(Session.House.ToXml("House"));
            if (built.Count == 0)
            {
                return;
            }

            int ox = built.Min(t => t.X), oy = built.Min(t => t.Y), oz = built.Min(t => t.Z);
            List<ClipboardTile> tiles = built.Select(t => new ClipboardTile(t.Id, t.X - ox, t.Y - oy, t.Z - oz, false, 0)).ToList();
            var source = new BlueprintSource("House", tiles.Count, () => tiles);
            if (BlueprintSaveDialog.Run(this, new[] { source }, out string showGroup) != null)
            {
                BlueprintsSaved(showGroup);
            }
        }

        /// <summary>Opens a placed house as a draft again.</summary>
        private void EditPlacedHouse(System.Xml.Linq.XElement house)
        {
            if (Session == null || house == null)
            {
                return;
            }

            string problem = Session.ReopenHouse(house);
            if (problem != null)
            {
                MessageBox.Show(this, problem, _messageTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                SetTool(ToolKind.House);
            }

            _commands.RefreshStates();
        }

        private readonly HousePanel _housePanel;
        private readonly TabControl _sideTabs;
        private readonly TabPage _designPage;
        private readonly TabPage _housePage;
        private bool _houseWasShown;
        private (HousePartKind Kind, int Storey)? _housePick;
    }
}
