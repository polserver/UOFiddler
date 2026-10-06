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
using System.IO;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class FontsControl : UserControl
    {
        public FontsControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            _refMarker = this;
            setOffsetsToolStripMenuItem.Visible = false;

            splitContainer2.SplitterDistance = splitContainer2.Height - 40;
        }

        private bool _loaded;
        private static FontsControl _refMarker;
        private List<int> _fonts = new List<int>();
        private Func<string, string?>? _localizationGetter;

        /// <summary>
        /// Set localization for FontsControl
        /// </summary>
        public void SetLocalization(Func<string, string?> getLocalized)
        {
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        /// <summary>
        /// Apply localization to all UI elements
        /// </summary>
        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;

            // 右键菜单项
            writeTextToolStripMenuItem.Text = _localizationGetter("Forms.FontsControl.writeTextToolStripMenuItem") ?? "Write Text";
            setOffsetsToolStripMenuItem.Text = _localizationGetter("Forms.FontsControl.setOffsetsToolStripMenuItem") ?? "Set Offsets";
            extractCharacterToolStripMenuItem.Text = _localizationGetter("Forms.FontsControl.extractCharacterToolStripMenuItem") ?? "Extract Character";
            importCharacterToolStripMenuItem.Text = _localizationGetter("Forms.FontsControl.importCharacterToolStripMenuItem") ?? "Import Character";
            saveToolStripMenuItem.Text = _localizationGetter("Forms.FontsControl.saveToolStripMenuItem") ?? "Save";

            // 复选框
            LoadUnicodeFontsCheckBox.Text = _localizationGetter("Forms.FontsControl.LoadUnicodeFontsCheckBox") ?? "Load Unicode Fonts";

            // 状态栏初始文本
            if (string.IsNullOrEmpty(toolStripStatusLabel1.Text) || toolStripStatusLabel1.Text == "<no selection>")
            {
                toolStripStatusLabel1.Text = _localizationGetter("Forms.FontsControl.toolStripStatusLabel1") ?? "<no selection>";
            }
        }

        /// <summary>
        /// Reload when loaded (file changed)
        /// </summary>
        private void Reload()
        {
            if (_loaded)
            {
                OnLoad(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Refreshes view if Offset of Unicode char is changed
        /// </summary>
        public static void RefreshOnCharChange()
        {
            if ((int)_refMarker.treeView.SelectedNode.Parent.Tag != 1)
            {
                return;
            }

            _refMarker.FontsTileView.Invalidate();

            if (_refMarker.FontsTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            int i = _refMarker.FontsTileView.SelectedIndices[0];
            _refMarker.toolStripStatusLabel1.Text =
                string.Format("'{0}' : {1} (0x{1:X}) XOffset: {2} YOffset: {3}",
                    (char)i, i,
                    UnicodeFonts.Fonts[(int)_refMarker.treeView.SelectedNode.Tag].Chars[i].XOffset,
                    UnicodeFonts.Fonts[(int)_refMarker.treeView.SelectedNode.Tag].Chars[i].YOffset);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            FontsTileView.BackColor = Options.DarkMode ? Color.LightGray : Color.White;

            using (new WaitCursorScope(this))
            {
                Options.LoadedUltimaClass["ASCIIFont"] = true;
                Options.LoadedUltimaClass["UnicodeFont"] = true;

                treeView.BeginUpdate();
                try
                {
                    treeView.Nodes.Clear();

                    TreeNode node = new TreeNode("ASCII")
                    {
                        Tag = 0
                    };
                    treeView.Nodes.Add(node);

                    for (int i = 0; i < AsciiText.Fonts.Length; ++i)
                    {
                        node = new TreeNode(i.ToString())
                        {
                            Tag = i
                        };
                        treeView.Nodes[0].Nodes.Add(node);
                    }

                    if (LoadUnicodeFontsCheckBox.Checked)
                    {
                        node = new TreeNode("Unicode")
                        {
                            Tag = 1
                        };
                        treeView.Nodes.Add(node);

                        for (int i = 0; i < UnicodeFonts.Fonts.Length; ++i)
                        {
                            if (UnicodeFonts.Fonts[i] == null)
                            {
                                continue;
                            }

                            node = new TreeNode(i.ToString())
                            {
                                Tag = i
                            };
                            treeView.Nodes[1].Nodes.Add(node);
                        }
                    }

                    treeView.ExpandAll();
                }
                finally
                {
                    treeView.EndUpdate();
                }

                treeView.SelectedNode = treeView.Nodes[0].Nodes[0];

                UpdateTileView();

                if (!_loaded)
                {
                    ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                }

                _loaded = true;
            }
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void OnSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView.SelectedNode.Parent == null)
            {
                treeView.SelectedNode = treeView.SelectedNode.Nodes[0];
            }

            int font = (int)treeView.SelectedNode.Tag;

            try
            {
                if ((int)treeView.SelectedNode.Parent.Tag == 1)
                {
                    setOffsetsToolStripMenuItem.Visible = true;

                    FontsTileView.VirtualListSize = 0x10000;
                }
                else
                {
                    setOffsetsToolStripMenuItem.Visible = false;

                    if (AsciiText.Fonts[font] == null)
                    {
                        return;
                    }

                    var length = AsciiText.Fonts[font].Characters.Length;
                    FontsTileView.VirtualListSize = length;

                    _fonts = new List<int>(length);
                    for (int i = 0; i < AsciiText.Fonts[font].Characters.Length; ++i)
                    {
                        _fonts.Add(i);
                    }
                }
            }
            finally
            {
                UpdateStatusStrip();
                FontsTileView.Invalidate();
            }
        }

        private void OnClickExport(object sender, EventArgs e)
        {
            if (FontsTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            string path = Options.OutputPath;
            string fileType = (int)treeView.SelectedNode.Parent.Tag == 1 ? "Unicode" : "ASCII";
            string fileName = (int)treeView.SelectedNode.Parent.Tag == 1
                ? Path.Combine(path, $"{fileType} {(int)treeView.SelectedNode.Tag} {Utils.FormatExportId(FontsTileView.SelectedIndices[0])}.tiff")
                : Path.Combine(path, $"{fileType} {(int)treeView.SelectedNode.Tag} {Utils.FormatExportId(_fonts[FontsTileView.SelectedIndices[0]] + AsciiFontOffset)}.tiff");

            if ((int)treeView.SelectedNode.Parent.Tag == 1)
            {
                Bitmap bmp = UnicodeFonts.Fonts[(int)treeView.SelectedNode.Tag].Chars[FontsTileView.SelectedIndices[0]].GetImage(true)
                             ?? new Bitmap(10, 10);

                bmp.Save(fileName, ImageFormat.Tiff);
            }
            else
            {
                var font = (int)treeView.SelectedNode.Tag;
                Bitmap bmp = AsciiText.Fonts[font].Characters[_fonts[FontsTileView.SelectedIndices[0]]]
                             ?? new Bitmap(10, 10);

                bmp.Save(fileName, ImageFormat.Tiff);
            }

            string message = _localizationGetter?.Invoke("Forms.FontsControl.Messages.characterSavedSuccess") 
                             ?? "Character saved successfully.";
            string title = _localizationGetter?.Invoke("Forms.FileSavedDialog.Title") ?? "Saved";
            FileSavedDialog.Show(FindForm(), fileName, message, title, key => _localizationGetter?.Invoke(key));
        }

        private static int AsciiFontOffset => 32;

        private void OnClickImport(object sender, EventArgs e)
        {
            if (FontsTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose an image file to import";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp;*.png)|*.tif;*.tiff;*.bmp;*.png";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Bitmap import = new Bitmap(dialog.FileName);
                if (import.Height > 255 || import.Width > 255)
                {
                    import.Dispose();

                    string message = _localizationGetter?.Invoke("Forms.FontsControl.Messages.imageSizeExceeds") 
                                     ?? "Image Height or Width exceeds 255";
                    string title = _localizationGetter?.Invoke("Forms.FontsControl.Messages.importTitle") 
                                   ?? "Import";
                    MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);

                    return;
                }

                int font = (int)treeView.SelectedNode.Tag;

                if ((int)treeView.SelectedNode.Parent.Tag == 1)
                {
                    UnicodeFonts.Fonts[font].Chars[FontsTileView.SelectedIndices[0]].SetBuffer(import);
                    Options.ChangedUltimaClass["UnicodeFont"] = true;
                }
                else
                {
                    AsciiText.Fonts[font].ReplaceCharacter(FontsTileView.SelectedIndices[0], import);
                    Options.ChangedUltimaClass["ASCIIFont"] = true;
                }

                FontsTileView.Invalidate();
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            if ((int)treeView.SelectedNode.Parent.Tag == 1)
            {
                string fileName = UnicodeFonts.Save(path, (int)treeView.SelectedNode.Tag);
                Options.ChangedUltimaClass["UnicodeFont"] = false;
                string message = _localizationGetter?.Invoke("Forms.FontsControl.Messages.unicodeFontsSavedSuccess") 
                                 ?? "Unicode fonts saved successfully.";
                string title = _localizationGetter?.Invoke("Forms.FileSavedDialog.Title") ?? "Saved";
                FileSavedDialog.Show(FindForm(), fileName, message, title, key => _localizationGetter?.Invoke(key));
            }
            else
            {
                string fileName = Path.Combine(path, "fonts.mul");
                AsciiText.Save(fileName);
                Options.ChangedUltimaClass["ASCIIFont"] = false;
                string message = _localizationGetter?.Invoke("Forms.FontsControl.Messages.fontsSavedSuccess") 
                                 ?? "Fonts saved successfully.";
                string title = _localizationGetter?.Invoke("Forms.FileSavedDialog.Title") ?? "Saved";
                FileSavedDialog.Show(FindForm(), fileName, message, title, key => _localizationGetter?.Invoke(key));
            }
        }

        private FontOffsetForm _form;

        private void OnClickSetOffsets(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
            {
                return;
            }

            if (FontsTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            int font = (int)treeView.SelectedNode.Tag;
            int cha = FontsTileView.SelectedIndices[0];
            if (_form?.IsDisposed == false)
            {
                return;
            }

            _form = new FontOffsetForm(font, cha)
            {
                TopMost = true
            };
            _form.Show();

            RefreshOnCharChange();
        }

        private void OnClickWriteText(object sender, EventArgs e)
        {
            int type = (int)treeView.SelectedNode.Parent.Tag;
            int font = (int)treeView.SelectedNode.Tag;

            new FontTextForm(type, font).Show();
        }

        private void FontsTileView_DrawItem(object sender, TileView.TileViewControl.DrawTileListItemEventArgs e)
        {
            if (treeView.Nodes.Count == 0)
            {
                return;
            }

            if (treeView.SelectedNode == null)
            {
                return;
            }

            int i;
            char c;

            if ((int)treeView.SelectedNode.Parent.Tag == 1)
            {
                // Unicode fonts
                i = e.Index;
                c = (char)i;

                // draw what should be in tile
                e.Graphics.DrawString(c.ToString(), DefaultFont, Brushes.DimGray, e.Bounds.X + (e.Bounds.Width / 2), e.Bounds.Y + (e.Bounds.Height / 2));

                // draw using font from uo if character exists
                var bmp = UnicodeFonts.Fonts[(int)treeView.SelectedNode.Tag].Chars[i].GetImage();
                if (bmp == null)
                {
                    return;
                }

                int width = bmp.Width;
                int height = bmp.Height;

                if (width > e.Bounds.Width)
                {
                    width = e.Bounds.Width - 2;
                }

                if (height > e.Bounds.Height)
                {
                    height = e.Bounds.Height - 2;
                }

                e.Graphics.DrawImage(bmp, new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, width, height));

                bmp.Dispose();
            }
            else
            {
                // ASCII Fonts
                i = e.Index;
                c = (char)(i + AsciiFontOffset);

                // draw what should be in tile
                e.Graphics.DrawString(c.ToString(), DefaultFont, Brushes.DimGray, e.Bounds.X + (e.Bounds.Width / 2), e.Bounds.Y + (e.Bounds.Height / 2));

                // draw using font from uo if character exists
                var font = (int)treeView.SelectedNode.Tag;
                e.Graphics.DrawImage(AsciiText.Fonts[font].Characters[_fonts[i]], new Point(e.Bounds.X + 2, e.Bounds.Y + 2));
            }
        }

        private void FontsTileView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
            {
                return;
            }

            if (treeView.Nodes.Count == 0)
            {
                return;
            }

            UpdateStatusStrip();
        }

        private void UpdateStatusStrip()
        {
            if (FontsTileView.SelectedIndices.Count == 0)
            {
                toolStripStatusLabel1.Text = string.Empty;
                return;
            }

            int i = FontsTileView.SelectedIndices[0];

            if ((int)treeView.SelectedNode.Parent.Tag == 1)
            {
                // Unicode fonts
                toolStripStatusLabel1.Text = string.Format("'{0}' : {1} (0x{1:X}) XOffset: {2} YOffset: {3}", (char)i, i,
                    UnicodeFonts.Fonts[(int)treeView.SelectedNode.Tag].Chars[i].XOffset,
                    UnicodeFonts.Fonts[(int)treeView.SelectedNode.Tag].Chars[i].YOffset);
            }
            else
            {
                // ASCII fonts - 需要边界检查
                if (i >= 0 && i < _fonts.Count)
                {
                    toolStripStatusLabel1.Text = string.Format("'{0}' : {1} (0x{1:X})", 
                        (char)(_fonts[i] + AsciiFontOffset), _fonts[i] + AsciiFontOffset);
                }
                else
                {
                    toolStripStatusLabel1.Text = string.Empty;
                }
            }
        }

        private void LoadUnicodeFontsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            string message = LoadUnicodeFontsCheckBox.Checked
                ? (_localizationGetter?.Invoke("Forms.FontsControl.Messages.loadAllFontsConfirm") ?? "Would you like to load all fonts including Unicode?")
                : (_localizationGetter?.Invoke("Forms.FontsControl.Messages.loadAsciiOnlyConfirm") ?? "Load only ASCII fonts?");

            string title = _localizationGetter?.Invoke("Forms.FontsControl.Messages.fontsReloadTitle") ?? "Fonts reload";

            DialogResult result =
                MessageBox.Show(message, title,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Reload();
        }

        private void FontsControl_Resize(object sender, EventArgs e)
        {
            splitContainer2.SplitterDistance = splitContainer2.Height - 40;
        }

        public void UpdateTileView()
        {
            FontsTileView.TileBorderColor = Options.RemoveTileBorder
                ? Color.Transparent
                : Color.Gray;

            var sameFocusColor = FontsTileView.TileFocusColor == Options.TileFocusColor;
            var sameSelectionColor = FontsTileView.TileHighlightColor == Options.TileSelectionColor;
            if (sameFocusColor && sameSelectionColor)
            {
                return;
            }

            FontsTileView.TileFocusColor = Options.TileFocusColor;
            FontsTileView.TileHighlightColor = Options.TileSelectionColor;

            FontsTileView.Invalidate();
        }
    }
}
