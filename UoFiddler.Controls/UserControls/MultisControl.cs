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
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class MultisControl : UserControl
    {
        private readonly string _multiXmlFileName = Path.Combine(Options.AppDataPath, "Multilist.xml");
        private readonly XmlDocument _xmlDocument;
        private readonly XmlElement _xmlElementMultis;

        public MultisControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            _refMarker = this;

            if (!File.Exists(_multiXmlFileName))
            {
                return;
            }

            _xmlDocument = new XmlDocument();
            _xmlDocument.Load(_multiXmlFileName);
            _xmlElementMultis = _xmlDocument["Multis"];
        }

        private bool _loaded;
        private bool _showFreeSlots;
        private readonly MultisControl _refMarker;

        // Virtual ListView backing: row index → multi id. _mulIds includes both
        // present and (when _showFreeSlots is on) empty slots; emptiness is
        // resolved at draw time via Multis.GetComponents.
        private int[] _mulIds = Array.Empty<int>();
        private int[] _uopIds = Array.Empty<int>();

        private int GetSelectedMulId()
        {
            return listViewMulti.SelectedIndices.Count > 0 && listViewMulti.SelectedIndices[0] < _mulIds.Length
                ? _mulIds[listViewMulti.SelectedIndices[0]]
                : -1;
        }

        private int GetSelectedUopId()
        {
            return listViewUop.SelectedIndices.Count > 0 && listViewUop.SelectedIndices[0] < _uopIds.Length
                ? _uopIds[listViewUop.SelectedIndices[0]]
                : -1;
        }

        private void OnRetrieveMultiVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if ((uint)e.ItemIndex >= (uint)_mulIds.Length)
            {
                e.Item = new ListViewItem(string.Empty);
                return;
            }

            int id = _mulIds[e.ItemIndex];
            // Special-case the "missing UOP file" placeholder row (id == -1).
            if (id < 0)
            {
                e.Item = new ListViewItem("multicollection.uop not found or path is not set.") { Tag = -1 };
                return;
            }

            var lvi = new ListViewItem(BuildNodeLabel(id))
            {
                Tag = id,
                ToolTipText = BuildToolTip(id)
            };
            if (_showFreeSlots && Multis.GetComponents(id) == MultiComponentList.Empty)
            {
                lvi.ForeColor = Color.Red;
            }
            e.Item = lvi;
        }

        private void OnRetrieveUopVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if ((uint)e.ItemIndex >= (uint)_uopIds.Length)
            {
                e.Item = new ListViewItem(string.Empty);
                return;
            }

            int id = _uopIds[e.ItemIndex];
            if (id < 0)
            {
                e.Item = new ListViewItem("multicollection.uop not found or path is not set.") { Tag = -1 };
                return;
            }

            e.Item = new ListViewItem(BuildNodeLabel(id))
            {
                Tag = id,
                ToolTipText = BuildToolTip(id)
            };
        }

        private string BuildToolTip(int id)
        {
            if (_xmlElementMultis == null)
            {
                return null;
            }
            string name = "";
            foreach (XmlNode xMultiNode in _xmlElementMultis.SelectNodes("/Multis/Multi[@id='" + id + "']"))
            {
                name = xMultiNode.Attributes["name"].Value;
            }
            string tooltipText = null;
            foreach (XmlNode xMultiNode in _xmlElementMultis.SelectNodes("/Multis/ToolTip[@id='" + id + "']"))
            {
                tooltipText = xMultiNode.Attributes["text"].Value;
            }
            if (tooltipText != null)
            {
                return name + "\r\n" + tooltipText;
            }
            return name;
        }

        private void SelectMulRow(int rowPos)
        {
            listViewMulti.SelectedIndices.Clear();
            if ((uint)rowPos < (uint)_mulIds.Length)
            {
                listViewMulti.SelectedIndices.Add(rowPos);
                listViewMulti.EnsureVisible(rowPos);
                listViewMulti.FocusedItem = listViewMulti.Items[rowPos];
            }
        }

        private void SelectUopRow(int rowPos)
        {
            listViewUop.SelectedIndices.Clear();
            if ((uint)rowPos < (uint)_uopIds.Length)
            {
                listViewUop.SelectedIndices.Add(rowPos);
                listViewUop.EnsureVisible(rowPos);
                listViewUop.FocusedItem = listViewUop.Items[rowPos];
            }
        }
        private bool _useTransparencyForPng = true;
        private bool _previewFitMode = true;
        private Bitmap _mulBitmap;
        private Bitmap _uopBitmap;
        private bool _isPanning;
        private Point _panStartScreen;
        private double _zoomLevel = 1.0;
        private const double _zoomFactor = 1.25;
        private const double _zoomMin = 0.25;
        private const double _zoomMax = 2.0;
        private string _mulStatusBase = string.Empty;
        private string _uopStatusBase = string.Empty;
        private MouseWheelFilter _mouseWheelFilter;

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (_loaded)
            {
                OnLoad(this, EventArgs.Empty);
            }
        }

        private string BuildNodeLabel(int i)
        {
            if (_xmlDocument == null)
            {
                return string.Format("{0,5} (0x{0:X})", i);
            }

            XmlNodeList xMultiNodeList = _xmlElementMultis.SelectNodes("/Multis/Multi[@id='" + i + "']");
            string name = "";
            foreach (XmlNode xMultiNode in xMultiNodeList)
            {
                name = xMultiNode.Attributes["name"].Value;
            }

            return $"{i,5} (0x{i:X}) {name}";
        }

        private void ApplyDarkModeIfNeeded()
        {
            if (Options.DarkMode)
            {
                Color tabBg = Color.FromArgb(32, 32, 32);
                TabPage[] tabPages = { tabPage5, tabPage6, tabPageMul, tabPageUop, tabPageUopPreview, tabPageUopComponents };
                foreach (var tp in tabPages)
                {
                    tp.UseVisualStyleBackColor = false;
                    tp.BackColor = tabBg;
                }
            }

            ApplyPreviewBackgroundColor();
            ControlEvents.PreviewBackgroundColorChangeEvent += ApplyPreviewBackgroundColor;
        }

        private void ApplyPreviewBackgroundColor()
        {
            MultiPictureBox.BackColor = Options.PreviewBackgroundColor;
            UopPictureBox.BackColor = Options.PreviewBackgroundColor;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            ApplyDarkModeIfNeeded();

            Cursor.Current = Cursors.WaitCursor;

            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            Options.LoadedUltimaClass["Multis"] = true;
            Options.LoadedUltimaClass["Hues"] = true;

            RebuildMulIds(includeEmpty: false);

            if (_mulIds.Length > 0)
            {
                SelectMulRow(0);
            }

            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                ControlEvents.MultiChangeEvent += OnMultiChangeEvent;
                ControlEvents.PreviewBackgroundColorChangeEvent += OnPreviewBackgroundColorChanged;
            }

            _loaded = true;

            LoadUopTree();

            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Multis.ReloadUop();
            Reload();
        }

        private void OnPreviewBackgroundColorChanged()
        {
            MultiPictureBox.BackColor = Options.PreviewBackgroundColor;
            UopPictureBox.BackColor = Options.PreviewBackgroundColor;
        }

        private void OnMultiChangeEvent(object sender, int id)
        {
            if (!_loaded)
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            MultiComponentList multi = Multis.GetComponents(id);
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            int existing = Array.IndexOf(_mulIds, id);
            if (existing >= 0)
            {
                // Already in the list — just repaint the row (text might depend
                // on XML lookups that could now resolve differently).
                listViewMulti.RedrawItems(existing, existing, false);
                if (listViewMulti.SelectedIndices.Count > 0 && listViewMulti.SelectedIndices[0] == existing)
                {
                    AfterSelect_Multi(this, EventArgs.Empty);
                }
                return;
            }

            // Find insertion point to keep the list sorted by id.
            int insertAt = _mulIds.Length;
            for (int i = 0; i < _mulIds.Length; ++i)
            {
                if (id < _mulIds[i])
                {
                    insertAt = i;
                    break;
                }
            }

            var next = new int[_mulIds.Length + 1];
            Array.Copy(_mulIds, 0, next, 0, insertAt);
            next[insertAt] = id;
            Array.Copy(_mulIds, insertAt, next, insertAt + 1, _mulIds.Length - insertAt);
            _mulIds = next;
            listViewMulti.VirtualListSize = _mulIds.Length;
            listViewMulti.Invalidate();
        }

        public void ChangeMulti(int id, MultiComponentList multi)
        {
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            int pos = Array.IndexOf(_refMarker._mulIds, id);
            if (pos < 0)
            {
                // Not yet in the list — insert sorted.
                _refMarker.OnMultiChangeEvent(null, id);
                pos = Array.IndexOf(_refMarker._mulIds, id);
                if (pos < 0)
                {
                    return;
                }
            }

            _refMarker.SelectMulRow(pos);
            _refMarker.AfterSelect_Multi(this, EventArgs.Empty);
            ControlEvents.FireMultiChangeEvent(this, pos);
        }

        private void AfterSelect_Multi(object sender, EventArgs e)
        {
            int id = GetSelectedMulId();
            MultiComponentList multi = id >= 0 ? Multis.GetComponents(id) : MultiComponentList.Empty;
            if (multi == MultiComponentList.Empty)
            {
                HeightChangeMulti.Maximum = 0;
                toolTip.SetToolTip(HeightChangeMulti, "MaxHeight: 0");
                SetMulStatus("Size: 0,0 MaxHeight: 0 MultiRegion: 0,0,0,0");
            }
            else
            {
                HeightChangeMulti.Maximum = multi.MaxHeight;
                toolTip.SetToolTip(HeightChangeMulti,
                    $"MaxHeight: {HeightChangeMulti.Maximum - HeightChangeMulti.Value}");
                SetMulStatus($"Size: {multi.Width},{multi.Height} MaxHeight: {multi.MaxHeight} MultiRegion: {multi.Min.X},{multi.Min.Y},{multi.Max.X},{multi.Max.Y} Surface: {multi.Surface}");
            }
            ChangeComponentList(multi);
            RefreshMulBitmap();
            UpdateMulPictureBox();
        }

        private void RefreshMulBitmap()
        {
            _mulBitmap?.Dispose();
            _mulBitmap = null;
            int id = GetSelectedMulId();
            if (id >= 0)
            {
                MultiComponentList multi = Multis.GetComponents(id);
                if (multi != MultiComponentList.Empty)
                {
                    int h = HeightChangeMulti.Maximum - HeightChangeMulti.Value;
                    _mulBitmap = multi.GetImage(h);
                }
            }
        }

        private void RebuildMulIds(bool includeEmpty)
        {
            var ids = new List<int>(Multis.MaximumMultiIndex);
            for (int i = 0; i < Multis.MaximumMultiIndex; ++i)
            {
                if (includeEmpty || Multis.GetComponents(i) != MultiComponentList.Empty)
                {
                    ids.Add(i);
                }
            }
            _mulIds = ids.ToArray();
            listViewMulti.VirtualListSize = _mulIds.Length;
            listViewMulti.Invalidate();
        }

        private void UpdateMulPictureBox()
        {
            if (_previewFitMode || _mulBitmap == null)
            {
                MultiPictureBox.Dock = DockStyle.Fill;
                MultiPictureBox.Cursor = Cursors.Default;
            }
            else
            {
                MultiPictureBox.Dock = DockStyle.None;
                CenterPictureBox(MultiPictureBox, panelMultiScroll, GetZoomedSize(_mulBitmap.Size));
                MultiPictureBox.Cursor = Cursors.Hand;
            }
            MultiPictureBox.Invalidate();
        }

        private void OnPaint_MultiPic(object sender, PaintEventArgs e)
        {
            if (_mulBitmap == null)
            {
                e.Graphics.Clear(MultiPictureBox.BackColor);
                return;
            }

            if (_previewFitMode)
            {
                DrawFit(e.Graphics, _mulBitmap, MultiPictureBox.Size);
            }
            else
            {
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                e.Graphics.DrawImage(_mulBitmap, 0, 0, MultiPictureBox.Width, MultiPictureBox.Height);
            }
        }

        private static void DrawFit(Graphics g, Bitmap bmp, Size box)
        {
            Point location = Point.Empty;
            Rectangle destRect;
            if (bmp.Height < box.Height && bmp.Width < box.Width)
            {
                location.X = (box.Width - bmp.Width) / 2;
                location.Y = (box.Height - bmp.Height) / 2;
                destRect = new Rectangle(location, bmp.Size);
            }
            else if (bmp.Height < box.Height)
            {
                location.Y = (box.Height - bmp.Height) / 2;
                destRect = bmp.Width > box.Width
                    ? new Rectangle(location, new Size(box.Width, bmp.Height))
                    : new Rectangle(location, bmp.Size);
            }
            else if (bmp.Width < box.Width)
            {
                location.X = (box.Width - bmp.Width) / 2;
                destRect = bmp.Height > box.Height
                    ? new Rectangle(location, new Size(bmp.Width, box.Height))
                    : new Rectangle(location, bmp.Size);
            }
            else
            {
                destRect = new Rectangle(Point.Empty, box);
            }

            g.DrawImage(bmp, destRect, 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel);
        }

        private void OnValue_HeightChangeMulti(object sender, EventArgs e)
        {
            toolTip.SetToolTip(HeightChangeMulti, $"MaxHeight: {HeightChangeMulti.Maximum - HeightChangeMulti.Value}");
            RefreshMulBitmap();
            UpdateMulPictureBox();
        }

        private void ChangeComponentList(MultiComponentList multi)
        {
            MultiComponentBox.Clear();
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            bool isUohsa = Art.IsUOAHS();
            for (int x = 0; x < multi.Width; ++x)
            {
                for (int y = 0; y < multi.Height; ++y)
                {
                    foreach (var mTile in multi.Tiles[x][y])
                    {
                        MultiComponentBox.AppendText(
                            isUohsa
                                ? $"0x{mTile.Id:X4} {x,3} {y,3} {mTile.Z,2} {mTile.Flag,2} {mTile.Unk1,2}\n"
                                : $"0x{mTile.Id:X4} {x,3} {y,3} {mTile.Z,2} {mTile.Flag,2}\n");
                    }
                }
            }
        }

        private void Extract_Image_ClickBmp(object sender, EventArgs e)
        {
            ExtractMultiImage(ImageFormat.Bmp, Options.PreviewBackgroundColor);
        }

        private void Extract_Image_ClickTiff(object sender, EventArgs e)
        {
            ExtractMultiImage(ImageFormat.Tiff, Options.PreviewBackgroundColor);
        }

        private void Extract_Image_ClickJpg(object sender, EventArgs e)
        {
            ExtractMultiImage(ImageFormat.Jpeg, Options.PreviewBackgroundColor);
        }

        private void Extract_Image_ClickPng(object sender, EventArgs e)
        {
            ExtractMultiImage(ImageFormat.Png, _useTransparencyForPng ? Color.Transparent : Options.PreviewBackgroundColor);
        }

        private void ExtractMultiImage(ImageFormat imageFormat, Color backgroundColor)
        {
            if (_mulBitmap == null)
            {
                return;
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string floorSuffix = HeightChangeMulti.Value > 0 ? $"_Z{HeightChangeMulti.Value:000}" : string.Empty;
            string fileName = Path.Combine(Options.OutputPath, $"Multi {Utils.FormatExportId(GetSelectedMulId())}{floorSuffix}.{fileExtension}");
            SaveImage(_mulBitmap, fileName, imageFormat, backgroundColor);
            FileSavedDialog.Show(FindForm(), fileName, "Multi saved successfully.");
        }

        private static void SaveImage(Image sourceImage, string fileName, ImageFormat imageFormat, Color backgroundColor)
        {
            using (Bitmap newBitmap = new Bitmap(sourceImage.Width, sourceImage.Height))
            using (Graphics newGraph = Graphics.FromImage(newBitmap))
            {
                newGraph.Clear(backgroundColor);
                newGraph.DrawImage(sourceImage, new Point(0, 0));
                newGraph.Save();

                newBitmap.Save(fileName, imageFormat);
            }
        }

        private void OnClickFreeSlots(object sender, EventArgs e)
        {
            _showFreeSlots = !_showFreeSlots;
            RebuildMulIds(includeEmpty: _showFreeSlots);
        }

        private void OnExportTextFile(object sender, EventArgs e)
        {
            int id = GetSelectedMulId();
            if (id < 0)
            {
                return;
            }

            MultiComponentList multi = Multis.GetComponents(id);
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Multi {Utils.FormatExportId(id)}.txt");
            multi.ExportToTextFile(fileName);

            FileSavedDialog.Show(FindForm(), fileName, "Multi saved successfully.");
        }

        private void OnExportWscFile(object sender, EventArgs e)
        {
            int id = GetSelectedMulId();
            if (id < 0)
            {
                return;
            }

            MultiComponentList multi = Multis.GetComponents(id);
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Multi {Utils.FormatExportId(id)}.wsc");
            multi.ExportToWscFile(fileName);

            FileSavedDialog.Show(FindForm(), fileName, "Multi saved successfully.");
        }

        private void OnExportUOAFile(object sender, EventArgs e)
        {
            int id = GetSelectedMulId();
            if (id < 0)
            {
                return;
            }

            MultiComponentList multi = Multis.GetComponents(id);
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Multi {Utils.FormatExportId(id)}.uoa");
            multi.ExportToUOAFile(fileName);

            FileSavedDialog.Show(FindForm(), fileName, "Multi saved successfully.");
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            Multis.Save(Options.OutputPath);
            Options.ChangedUltimaClass["Multis"] = false;

            FileSavedDialog.Show(FindForm(), Options.OutputPath, "Files saved successfully.");
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            int id = GetSelectedMulId();
            if (id < 0)
            {
                return;
            }

            MultiComponentList multi = Multis.GetComponents(id);
            if (multi == MultiComponentList.Empty)
            {
                return;
            }
            DialogResult result = MessageBox.Show(string.Format("Are you sure to remove {0} (0x{0:X})", id), "Remove",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Multis.Remove(id);
            int pos = Array.IndexOf(_mulIds, id);
            if (pos >= 0)
            {
                var next = new int[_mulIds.Length - 1];
                Array.Copy(_mulIds, 0, next, 0, pos);
                Array.Copy(_mulIds, pos + 1, next, pos, _mulIds.Length - pos - 1);
                _mulIds = next;
                listViewMulti.VirtualListSize = _mulIds.Length;
                listViewMulti.Invalidate();
            }
            Options.ChangedUltimaClass["Multis"] = true;
            ControlEvents.FireMultiChangeEvent(this, id);
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            int id = GetSelectedMulId();
            if (id < 0)
            {
                return;
            }
            MultiComponentList multi = Multis.GetComponents(id);
            if (multi != MultiComponentList.Empty)
            {
                DialogResult result = MessageBox.Show(string.Format("Are you sure to replace {0} (0x{0:X})", id),
                    "Import", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            using (var dialog = new MultiImportForm(id, ChangeMulti))
            {
                dialog.TopMost = true;
                dialog.ShowDialog();
            }
        }

        private void OnClick_SaveAllBmp(object sender, EventArgs e)
        {
            ExportAllMultis(ImageFormat.Bmp, Options.PreviewBackgroundColor);
        }

        private void OnClick_SaveAllTiff(object sender, EventArgs e)
        {
            ExportAllMultis(ImageFormat.Tiff, Options.PreviewBackgroundColor);
        }

        private void OnClick_SaveAllJpg(object sender, EventArgs e)
        {
            ExportAllMultis(ImageFormat.Jpeg, Options.PreviewBackgroundColor);
        }

        private void OnClick_SaveAllPng(object sender, EventArgs e)
        {
            ExportAllMultis(ImageFormat.Png, _useTransparencyForPng ? Color.Transparent : Options.PreviewBackgroundColor);
        }

        private void ExportAllMultis(ImageFormat imageFormat, Color backgroundColor)
        {
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                for (int i = 0; i < _refMarker._mulIds.Length; i++)
                {
                    int index = _refMarker._mulIds[i];
                    if (index < 0)
                    {
                        continue;
                    }

                    const int maximumMultiHeight = 127;
                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi {Utils.FormatExportId(index)}.{fileExtension}");

                    using (Bitmap multiBitmap = Multis.GetComponents(index)?.GetImage(maximumMultiHeight))
                    {
                        if (multiBitmap != null)
                        {
                            SaveImage(multiBitmap, fileName, imageFormat, backgroundColor);
                        }
                    }
                }

                FileSavedDialog.Show(FindForm(), dialog.SelectedPath, "All Multis saved successfully.");
            }
        }

        private void OnClick_SaveAllText(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                for (int i = 0; i < _refMarker._mulIds.Length; ++i)
                {
                    int index = _refMarker._mulIds[i];
                    if (index < 0)
                    {
                        continue;
                    }

                    MultiComponentList multi = Multis.GetComponents(index);
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi {Utils.FormatExportId(index)}.txt");
                    multi.ExportToTextFile(fileName);
                }

                FileSavedDialog.Show(FindForm(), dialog.SelectedPath, "All Multis saved successfully.");
            }
        }

        private void OnClick_SaveAllUOA(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                for (int i = 0; i < _refMarker._mulIds.Length; ++i)
                {
                    int index = _refMarker._mulIds[i];
                    if (index < 0)
                    {
                        continue;
                    }

                    MultiComponentList multi = Multis.GetComponents(index);
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi {Utils.FormatExportId(index)}.uoa");
                    multi.ExportToUOAFile(fileName);
                }

                FileSavedDialog.Show(FindForm(), dialog.SelectedPath, "All Multis saved successfully.");
            }
        }

        private void OnClick_SaveAllWSC(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                for (int i = 0; i < _refMarker._mulIds.Length; ++i)
                {
                    int index = _refMarker._mulIds[i];
                    if (index < 0)
                    {
                        continue;
                    }

                    MultiComponentList multi = Multis.GetComponents(index);
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi {Utils.FormatExportId(index)}.wsc");
                    multi.ExportToWscFile(fileName);
                }

                FileSavedDialog.Show(FindForm(), dialog.SelectedPath, "All Multis saved successfully.");
            }
        }

        private void OnClick_SaveAllCSV(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                for (int i = 0; i < _refMarker._mulIds.Length; ++i)
                {
                    int index = _refMarker._mulIds[i];
                    if (index < 0)
                    {
                        continue;
                    }

                    MultiComponentList multi = Multis.GetComponents(index);
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"{index:D4}.csv");
                    multi.ExportToCsvFile(fileName);
                }

                FileSavedDialog.Show(FindForm(), dialog.SelectedPath, "All Multis saved successfully.");
            }
        }

        private void OnClick_SaveAllUox3(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                for (int i = 0; i < _refMarker._mulIds.Length; ++i)
                {
                    int index = _refMarker._mulIds[i];
                    if (index < 0)
                    {
                        continue;
                    }

                    MultiComponentList multi = Multis.GetComponents(index);
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi {Utils.FormatExportId(index)}.uox3");
                    multi.ExportToUox3File(fileName);
                }

                FileSavedDialog.Show(FindForm(), dialog.SelectedPath, "All Multis saved successfully.");
            }
        }

        private void OnExportCsvFile(object sender, EventArgs e)
        {
            int id = GetSelectedMulId();
            if (id < 0)
            {
                return;
            }

            MultiComponentList multi = Multis.GetComponents(id);
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"{id:D4}.csv");
            multi.ExportToCsvFile(fileName);
            FileSavedDialog.Show(FindForm(), fileName, "Multi saved successfully.");
        }

        private void OnExportUox3File(object sender, EventArgs e)
        {
            int id = GetSelectedMulId();
            if (id < 0)
            {
                return;
            }

            MultiComponentList multi = Multis.GetComponents(id);
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Multi {Utils.FormatExportId(id)}.uox3");
            multi.ExportToUox3File(fileName);
            FileSavedDialog.Show(FindForm(), fileName, "Multi saved successfully.");
        }

        private void ChangeBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            Options.PreviewBackgroundColor = colorDialog.Color;
            ControlEvents.FirePreviewBackgroundColorChangeEvent();
        }

        private void UseTransparencyForPNGToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _useTransparencyForPng = UseTransparencyForPNGToolStripMenuItem.Checked;
        }

        private void LoadUopTree()
        {
            if (!Multis.HasUopFile)
            {
                _uopIds = new[] { -1 }; // placeholder row rendered as the "not found" message
                listViewUop.VirtualListSize = 1;
                listViewUop.Invalidate();
                return;
            }

            var ids = new List<int>(Multis.MaximumMultiIndex);
            for (int i = 0; i < Multis.MaximumMultiIndex; ++i)
            {
                if (Multis.GetUopComponents(i) != MultiComponentList.Empty)
                {
                    ids.Add(i);
                }
            }
            _uopIds = ids.ToArray();
            listViewUop.VirtualListSize = _uopIds.Length;
            listViewUop.Invalidate();

            if (_uopIds.Length > 0)
            {
                SelectUopRow(0);
            }
        }

        private void AfterSelect_UopMulti(object sender, EventArgs e)
        {
            int id = GetSelectedUopId();
            MultiComponentList multi = id >= 0 ? Multis.GetUopComponents(id) : MultiComponentList.Empty;
            if (multi == MultiComponentList.Empty)
            {
                HeightChangeUop.Maximum = 0;
                toolTip.SetToolTip(HeightChangeUop, "MaxHeight: 0");
                SetUopStatus("Size: 0,0 MaxHeight: 0 MultiRegion: 0,0,0,0");
            }
            else
            {
                HeightChangeUop.Maximum = multi.MaxHeight;
                toolTip.SetToolTip(HeightChangeUop, $"MaxHeight: {HeightChangeUop.Maximum - HeightChangeUop.Value}");
                SetUopStatus($"Size: {multi.Width},{multi.Height} MaxHeight: {multi.MaxHeight} MultiRegion: {multi.Min.X},{multi.Min.Y},{multi.Max.X},{multi.Max.Y} Surface: {multi.Surface}");
            }

            ChangeUopComponentList(multi);
            RefreshUopBitmap();
            UpdateUopPictureBox();
        }

        private void RefreshUopBitmap()
        {
            _uopBitmap?.Dispose();
            _uopBitmap = null;
            int id = GetSelectedUopId();
            if (id >= 0)
            {
                MultiComponentList multi = Multis.GetUopComponents(id);
                if (multi != MultiComponentList.Empty)
                {
                    int h = HeightChangeUop.Maximum - HeightChangeUop.Value;
                    _uopBitmap = multi.GetImage(h);
                }
            }
        }

        private void UpdateUopPictureBox()
        {
            if (_previewFitMode || _uopBitmap == null)
            {
                UopPictureBox.Dock = DockStyle.Fill;
                UopPictureBox.Cursor = Cursors.Default;
            }
            else
            {
                UopPictureBox.Dock = DockStyle.None;
                CenterPictureBox(UopPictureBox, panelUopScroll, GetZoomedSize(_uopBitmap.Size));
                UopPictureBox.Cursor = Cursors.Hand;
            }
            UopPictureBox.Invalidate();
        }

        private void ChangeUopComponentList(MultiComponentList multi)
        {
            UopComponentBox.Clear();
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            bool isUohsa = Art.IsUOAHS();
            for (int x = 0; x < multi.Width; ++x)
            {
                for (int y = 0; y < multi.Height; ++y)
                {
                    foreach (var mTile in multi.Tiles[x][y])
                    {
                        UopComponentBox.AppendText(
                            isUohsa
                                ? $"0x{mTile.Id:X4} {x,3} {y,3} {mTile.Z,2} {mTile.Flag,2} {mTile.Unk1,2}\n"
                                : $"0x{mTile.Id:X4} {x,3} {y,3} {mTile.Z,2} {mTile.Flag,2}\n");
                    }
                }
            }
        }

        private void OnValue_HeightChangeUop(object sender, EventArgs e)
        {
            toolTip.SetToolTip(HeightChangeUop, $"MaxHeight: {HeightChangeUop.Maximum - HeightChangeUop.Value}");
            RefreshUopBitmap();
            UpdateUopPictureBox();
        }

        private void OnPaint_UopMultiPic(object sender, PaintEventArgs e)
        {
            if (_uopBitmap == null)
            {
                e.Graphics.Clear(UopPictureBox.BackColor);
                return;
            }

            if (_previewFitMode)
            {
                DrawFit(e.Graphics, _uopBitmap, UopPictureBox.Size);
            }
            else
            {
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                e.Graphics.DrawImage(_uopBitmap, 0, 0, UopPictureBox.Width, UopPictureBox.Height);
            }
        }

        private void OnToggleFitMode(object sender, EventArgs e)
        {
            _previewFitMode = ((System.Windows.Forms.ToolStripButton)sender).Checked;
            fitModeToolStripMenuItem.CheckedChanged -= OnToggleFitMode;
            uopFitModeToolStripMenuItem.CheckedChanged -= OnToggleFitMode;
            fitModeToolStripMenuItem.Checked = _previewFitMode;
            uopFitModeToolStripMenuItem.Checked = _previewFitMode;
            fitModeToolStripMenuItem.CheckedChanged += OnToggleFitMode;
            uopFitModeToolStripMenuItem.CheckedChanged += OnToggleFitMode;
            UpdateMulPictureBox();
            UpdateUopPictureBox();
            UpdateZoomStatus();
        }

        private static void CenterPictureBox(PictureBox pic, Panel panel, Size bitmapSize)
        {
            // Set size first so the panel can clamp AutoScrollPosition to the new valid range.
            pic.Size = bitmapSize;

            int vx = Math.Max(0, (panel.ClientSize.Width - bitmapSize.Width) / 2);
            int vy = Math.Max(0, (panel.ClientSize.Height - bitmapSize.Height) / 2);

            // AutoScrollPosition getter returns a negative offset (e.g. (0,-100) when scrolled 100 down).
            // Control.Location in a scrolled Panel is relative to the current view, not the virtual origin,
            // so we add the scroll offset to land at the correct virtual position.
            Point scroll = panel.AutoScrollPosition;
            pic.Location = new Point(vx + scroll.X, vy + scroll.Y);
        }

        private void OnPanelMultiScroll_Resize(object sender, EventArgs e)
        {
            if (!_previewFitMode && _mulBitmap != null)
            {
                CenterPictureBox(MultiPictureBox, panelMultiScroll, GetZoomedSize(_mulBitmap.Size));
            }
        }

        private void OnPanelUopScroll_Resize(object sender, EventArgs e)
        {
            if (!_previewFitMode && _uopBitmap != null)
            {
                CenterPictureBox(UopPictureBox, panelUopScroll, GetZoomedSize(_uopBitmap.Size));
            }
        }

        private void OnMulPan_MouseDown(object sender, MouseEventArgs e)
        {
            if (_previewFitMode || e.Button != MouseButtons.Left)
            {
                return;
            }

            _isPanning = true;
            _panStartScreen = Cursor.Position;
            MultiPictureBox.Cursor = Cursors.SizeAll;
        }

        private void OnMulPan_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isPanning)
            {
                return;
            }

            PanPanel(panelMultiScroll);
        }

        private void OnUopPan_MouseDown(object sender, MouseEventArgs e)
        {
            if (_previewFitMode || e.Button != MouseButtons.Left)
            {
                return;
            }

            _isPanning = true;
            _panStartScreen = Cursor.Position;
            UopPictureBox.Cursor = Cursors.SizeAll;
        }

        private void OnUopPan_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isPanning)
            {
                return;
            }

            PanPanel(panelUopScroll);
        }

        private void OnPan_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_isPanning)
            {
                return;
            }

            _isPanning = false;
            ((Control)sender).Cursor = _previewFitMode ? Cursors.Default : Cursors.Hand;
        }

        private void PanPanel(Panel panel)
        {
            Point pos = Cursor.Position;
            int dx = pos.X - _panStartScreen.X;
            int dy = pos.Y - _panStartScreen.Y;
            Point scroll = panel.AutoScrollPosition;
            panel.AutoScrollPosition = new Point(
                Math.Max(0, -scroll.X - dx),
                Math.Max(0, -scroll.Y - dy)
            );
            _panStartScreen = pos;
        }

        private Size GetZoomedSize(Size bitmapSize) =>
            new Size(Math.Max(1, (int)(bitmapSize.Width * _zoomLevel)),
                     Math.Max(1, (int)(bitmapSize.Height * _zoomLevel)));

        private void ZoomIn() => SetZoom(_zoomLevel * _zoomFactor);
        private void ZoomOut() => SetZoom(_zoomLevel / _zoomFactor);
        private void ZoomReset() => SetZoom(1.0);

        private void SetZoom(double zoom)
        {
            _zoomLevel = Math.Clamp(zoom, _zoomMin, _zoomMax);
            if (_previewFitMode)
            {
                return;
            }

            UpdateMulPictureBox();
            UpdateUopPictureBox();
            UpdateZoomStatus();
        }

        private void UpdateZoomStatus()
        {
            SetMulStatus(_mulStatusBase);
            SetUopStatus(_uopStatusBase);
        }

        private void SetMulStatus(string baseText)
        {
            _mulStatusBase = baseText;
            StatusMultiText.Text = _previewFitMode
                ? baseText
                : $"{baseText}  Zoom: {_zoomLevel * 100:F0}%";
        }

        private void SetUopStatus(string baseText)
        {
            _uopStatusBase = baseText;
            StatusUopText.Text = _previewFitMode
                ? baseText
                : $"{baseText}  Zoom: {_zoomLevel * 100:F0}%";
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _mouseWheelFilter = new MouseWheelFilter(this);
            Application.AddMessageFilter(_mouseWheelFilter);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (_mouseWheelFilter != null)
            {
                Application.RemoveMessageFilter(_mouseWheelFilter);
            }

            base.OnHandleDestroyed(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_previewFitMode)
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            switch (keyData)
            {
                case Keys.Oemplus | Keys.Shift:
                case Keys.Add:
                    ZoomIn();
                    return true;
                case Keys.OemMinus:
                case Keys.Subtract:
                    ZoomOut();
                    return true;
                case Keys.D0 | Keys.Control:
                case Keys.NumPad0 | Keys.Control:
                    ZoomReset();
                    return true;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        private void OnClickHelp(object sender, EventArgs e)
        {
            using var form = new MultisHelpForm();
            form.ShowDialog(this);
        }

        private void UopExtract_Image_ClickBmp(object sender, EventArgs e) =>
            ExtractUopMultiImage(ImageFormat.Bmp, Options.PreviewBackgroundColor);

        private void UopExtract_Image_ClickTiff(object sender, EventArgs e) =>
            ExtractUopMultiImage(ImageFormat.Tiff, Options.PreviewBackgroundColor);

        private void UopExtract_Image_ClickJpg(object sender, EventArgs e) =>
            ExtractUopMultiImage(ImageFormat.Jpeg, Options.PreviewBackgroundColor);

        private void UopExtract_Image_ClickPng(object sender, EventArgs e) =>
            ExtractUopMultiImage(ImageFormat.Png, _useTransparencyForPng ? Color.Transparent : Options.PreviewBackgroundColor);

        private void ExtractUopMultiImage(ImageFormat imageFormat, Color backgroundColor)
        {
            if (_uopBitmap == null)
            {
                return;
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string floorSuffix = HeightChangeUop.Value > 0 ? $"_Z{HeightChangeUop.Value:000}" : string.Empty;
            int id = GetSelectedUopId();
            string fileName = Path.Combine(Options.OutputPath, $"UopMulti {Utils.FormatExportId(id)}{floorSuffix}.{fileExtension}");
            SaveImage(_uopBitmap, fileName, imageFormat, backgroundColor);
            FileSavedDialog.Show(FindForm(), fileName, "Multi saved successfully.");
        }

        private void OnUopExportTextFile(object sender, EventArgs e)
        {
            int id = GetSelectedUopId();
            if (id < 0)
            {
                return;
            }
            MultiComponentList multi = Multis.GetUopComponents(id);
            if (multi == MultiComponentList.Empty)
            {
                return;
            }
            string fileName = Path.Combine(Options.OutputPath, $"UopMulti {Utils.FormatExportId(id)}.txt");
            multi.ExportToTextFile(fileName);
            FileSavedDialog.Show(FindForm(), fileName, "Multi saved successfully.");
        }

        private void OnUopExportUOAFile(object sender, EventArgs e)
        {
            int id = GetSelectedUopId();
            if (id < 0)
            {
                return;
            }
            MultiComponentList multi = Multis.GetUopComponents(id);
            if (multi == MultiComponentList.Empty)
            {
                return;
            }
            string fileName = Path.Combine(Options.OutputPath, $"UopMulti {Utils.FormatExportId(id)}.uoa");
            multi.ExportToUOAFile(fileName);
            FileSavedDialog.Show(FindForm(), fileName, "Multi saved successfully.");
        }

        private void OnUopExportWscFile(object sender, EventArgs e)
        {
            int id = GetSelectedUopId();
            if (id < 0)
            {
                return;
            }
            MultiComponentList multi = Multis.GetUopComponents(id);
            if (multi == MultiComponentList.Empty)
            {
                return;
            }
            string fileName = Path.Combine(Options.OutputPath, $"UopMulti {Utils.FormatExportId(id)}.wsc");
            multi.ExportToWscFile(fileName);
            FileSavedDialog.Show(FindForm(), fileName, "Multi saved successfully.");
        }

        private void OnUopExportCsvFile(object sender, EventArgs e)
        {
            int id = GetSelectedUopId();
            if (id < 0)
            {
                return;
            }
            MultiComponentList multi = Multis.GetUopComponents(id);
            if (multi == MultiComponentList.Empty)
            {
                return;
            }
            string fileName = Path.Combine(Options.OutputPath, $"{id:D4}_uop.csv");
            multi.ExportToCsvFile(fileName);
            FileSavedDialog.Show(FindForm(), fileName, "Multi saved successfully.");
        }

        private void OnUopClick_SaveAllBmp(object sender, EventArgs e) =>
            ExportAllUopMultis(ImageFormat.Bmp, Options.PreviewBackgroundColor);

        private void OnUopClick_SaveAllTiff(object sender, EventArgs e) =>
            ExportAllUopMultis(ImageFormat.Tiff, Options.PreviewBackgroundColor);

        private void OnUopClick_SaveAllJpg(object sender, EventArgs e) =>
            ExportAllUopMultis(ImageFormat.Jpeg, Options.PreviewBackgroundColor);

        private void OnUopClick_SaveAllPng(object sender, EventArgs e) =>
            ExportAllUopMultis(ImageFormat.Png, _useTransparencyForPng ? Color.Transparent : Options.PreviewBackgroundColor);

        private void ExportAllUopMultis(ImageFormat imageFormat, Color backgroundColor)
        {
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            using var dialog = new FolderBrowserDialog { Description = "Select directory", ShowNewFolderButton = true };
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            const int maxHeight = 127;
            for (int i = 0; i < _uopIds.Length; i++)
            {
                int index = _uopIds[i];
                if (index < 0)
                {
                    continue;
                }

                MultiComponentList multi = Multis.GetUopComponents(index);
                if (multi == MultiComponentList.Empty)
                {
                    continue;
                }

                string fileName = Path.Combine(dialog.SelectedPath, $"UopMulti {Utils.FormatExportId(index)}.{fileExtension}");
                using Bitmap bitmap = multi.GetImage(maxHeight);
                if (bitmap != null)
                {
                    SaveImage(bitmap, fileName, imageFormat, backgroundColor);
                }
            }

            FileSavedDialog.Show(FindForm(), dialog.SelectedPath, "All UOP Multis saved successfully.");
        }

        private void OnUopClick_SaveAllText(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog { Description = "Select directory", ShowNewFolderButton = true };
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            for (int i = 0; i < _uopIds.Length; ++i)
            {
                int index = _uopIds[i];
                if (index < 0)
                {
                    continue;
                }

                MultiComponentList multi = Multis.GetUopComponents(index);
                if (multi == MultiComponentList.Empty)
                {
                    continue;
                }

                multi.ExportToTextFile(Path.Combine(dialog.SelectedPath, $"UopMulti {Utils.FormatExportId(index)}.txt"));
            }

            FileSavedDialog.Show(FindForm(), dialog.SelectedPath, "All UOP Multis saved successfully.");
        }

        private void OnUopClick_SaveAllUOA(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog { Description = "Select directory", ShowNewFolderButton = true };
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            for (int i = 0; i < _uopIds.Length; ++i)
            {
                int index = _uopIds[i];
                if (index < 0)
                {
                    continue;
                }

                MultiComponentList multi = Multis.GetUopComponents(index);
                if (multi == MultiComponentList.Empty)
                {
                    continue;
                }

                multi.ExportToUOAFile(Path.Combine(dialog.SelectedPath, $"UopMulti {Utils.FormatExportId(index)}.uoa"));
            }

            FileSavedDialog.Show(FindForm(), dialog.SelectedPath, "All UOP Multis saved successfully.");
        }

        private void OnUopClick_SaveAllWSC(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog { Description = "Select directory", ShowNewFolderButton = true };
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            for (int i = 0; i < _uopIds.Length; ++i)
            {
                int index = _uopIds[i];
                if (index < 0)
                {
                    continue;
                }

                MultiComponentList multi = Multis.GetUopComponents(index);
                if (multi == MultiComponentList.Empty)
                {
                    continue;
                }

                multi.ExportToWscFile(Path.Combine(dialog.SelectedPath, $"UopMulti {Utils.FormatExportId(index)}.wsc"));
            }

            FileSavedDialog.Show(FindForm(), dialog.SelectedPath, "All UOP Multis saved successfully.");
        }

        private void OnUopClick_SaveAllCSV(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog { Description = "Select directory", ShowNewFolderButton = true };
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            for (int i = 0; i < _uopIds.Length; ++i)
            {
                int index = _uopIds[i];
                if (index < 0)
                {
                    continue;
                }

                MultiComponentList multi = Multis.GetUopComponents(index);
                if (multi == MultiComponentList.Empty)
                {
                    continue;
                }

                multi.ExportToCsvFile(Path.Combine(dialog.SelectedPath, $"{index:D4}_uop.csv"));
            }

            FileSavedDialog.Show(FindForm(), dialog.SelectedPath, "All UOP Multis saved successfully.");
        }

        private void OnClick_SaveAllToXML(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, "TilesEntry.xml");
            string groupFileName = Path.Combine(path, "TilesGroup-Multis.xml");

            using (XmlWriter writer = XmlWriter.Create(fileName, new XmlWriterSettings { Indent = true }))
            using (XmlWriter groupWriter = XmlWriter.Create(groupFileName, new XmlWriterSettings { Indent = true }))
            {
                writer.WriteStartDocument();
                groupWriter.WriteStartDocument();

                writer.WriteStartElement("TilesEntry");
                groupWriter.WriteStartElement("TilesGroup");

                groupWriter.WriteStartElement("Group");
                groupWriter.WriteAttributeString("Name", "Exported Multis");

                for (int i = 0; i < _refMarker._mulIds.Length; ++i)
                {
                    int index = _refMarker._mulIds[i];
                    if (index < 0)
                    {
                        continue;
                    }

                    MultiComponentList multi = Multis.GetComponents(index);
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    groupWriter.WriteStartElement("Entry");
                    groupWriter.WriteAttributeString("ID", index.ToString());
                    groupWriter.WriteAttributeString("Name", _refMarker.BuildNodeLabel(index).Trim());

                    writer.WriteStartElement("Entry");
                    writer.WriteAttributeString("ID", index.ToString());
                    writer.WriteAttributeString("Name", _refMarker.BuildNodeLabel(index).Trim());

                    for (int x = 0; x < multi.Width; x++)
                    {
                        for (int y = 0; y < multi.Height; y++)
                        {
                            foreach (var tile in multi.Tiles[x][y])
                            {
                                writer.WriteStartElement("Item");
                                writer.WriteAttributeString("X", x.ToString());
                                writer.WriteAttributeString("Y", y.ToString());
                                writer.WriteAttributeString("Z", tile.Z.ToString());
                                writer.WriteAttributeString("ID", $"0x{tile.Id:X4}");
                                writer.WriteEndElement(); // Item
                            }
                        }
                    }

                    writer.WriteEndElement(); // Entry
                    groupWriter.WriteEndElement(); // Entry (group)
                }

                writer.WriteEndElement(); // TilesEntry
                groupWriter.WriteEndElement(); // Group
                groupWriter.WriteEndElement(); // TilesGroup

                writer.WriteEndDocument();
                groupWriter.WriteEndDocument();
            }

            FileSavedDialog.Show(FindForm(), fileName, "All Multis saved successfully.");
        }

        private sealed class MouseWheelFilter : IMessageFilter
        {
            private const int _wmMouseWheel = 0x020A;

            private readonly MultisControl _owner;

            public MouseWheelFilter(MultisControl owner) => _owner = owner;

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg != _wmMouseWheel)
                {
                    return false;
                }

                if ((Control.ModifierKeys & Keys.Control) == 0)
                {
                    return false;
                }

                Point cursor = Cursor.Position;
                if (IsOver(_owner.panelMultiScroll, cursor) || IsOver(_owner.panelUopScroll, cursor))
                {
                    int delta = (short)((int)m.WParam >> 16);
                    if (delta > 0)
                    {
                        _owner.ZoomIn();
                    }
                    else
                    {
                        _owner.ZoomOut();
                    }

                    return true;
                }
                return false;
            }

            private static bool IsOver(Control c, Point screenPt)
            {
                if (!c.IsHandleCreated)
                {
                    return false;
                }

                return new Rectangle(c.PointToScreen(Point.Empty), c.Size).Contains(screenPt);
            }
        }
    }
}
