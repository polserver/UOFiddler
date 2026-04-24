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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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

        private TreeNode BuildMulNode(int i, MultiComponentList multi)
        {
            TreeNode node;
            if (_xmlDocument == null)
            {
                node = new TreeNode(BuildNodeLabel(i));
            }
            else
            {
                node = new TreeNode(BuildNodeLabel(i));
                XmlNodeList xMultiNodeList = _xmlElementMultis.SelectNodes("/Multis/Multi[@id='" + i + "']");
                string name = "";
                foreach (XmlNode xMultiNode in xMultiNodeList)
                {
                    name = xMultiNode.Attributes["name"].Value;
                }

                XmlNodeList tooltipList = _xmlElementMultis.SelectNodes("/Multis/ToolTip[@id='" + i + "']");
                foreach (XmlNode xMultiNode in tooltipList)
                {
                    node.ToolTipText = name + "\r\n" + xMultiNode.Attributes["text"].Value;
                }

                if (tooltipList.Count == 0)
                {
                    node.ToolTipText = name;
                }
            }

            node.Tag = multi;
            node.Name = i.ToString();
            return node;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            Options.LoadedUltimaClass["Multis"] = true;
            Options.LoadedUltimaClass["Hues"] = true;

            TreeViewMulti.BeginUpdate();
            try
            {
                TreeViewMulti.Nodes.Clear();
                var cache = new List<TreeNode>();
                for (int i = 0; i < Multis.MaximumMultiIndex; ++i)
                {
                    MultiComponentList multi = Multis.GetComponents(i);
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    cache.Add(BuildMulNode(i, multi));
                }

                TreeViewMulti.Nodes.AddRange(cache.ToArray());
            }
            finally
            {
                TreeViewMulti.EndUpdate();
            }

            if (TreeViewMulti.Nodes.Count > 0)
            {
                TreeViewMulti.SelectedNode = TreeViewMulti.Nodes[0];
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

            bool done = false;
            for (int i = 0; i < TreeViewMulti.Nodes.Count; ++i)
            {
                if (id == int.Parse(TreeViewMulti.Nodes[i].Name))
                {
                    TreeViewMulti.Nodes[i].Tag = multi;
                    TreeViewMulti.Nodes[i].ForeColor = Color.Black;
                    if (i == TreeViewMulti.SelectedNode.Index)
                    {
                        AfterSelect_Multi(this, null);
                    }

                    done = true;
                    break;
                }

                if (id >= int.Parse(TreeViewMulti.Nodes[i].Name))
                {
                    continue;
                }

                TreeNode node = new TreeNode(string.Format("{0,5} (0x{0:X})", id))
                {
                    Tag = multi,
                    Name = id.ToString()
                };
                TreeViewMulti.Nodes.Insert(i, node);
                done = true;
                break;
            }

            if (!done)
            {
                TreeNode node = new TreeNode(string.Format("{0,5} (0x{0:X})", id))
                {
                    Tag = multi,
                    Name = id.ToString()
                };
                TreeViewMulti.Nodes.Add(node);
            }
        }

        public void ChangeMulti(int id, MultiComponentList multi)
        {
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            int index = _refMarker.TreeViewMulti.SelectedNode.Index;
            if (int.Parse(_refMarker.TreeViewMulti.SelectedNode.Name) != id)
            {
                for (int i = 0; i < _refMarker.TreeViewMulti.Nodes.Count; ++i)
                {
                    if (int.Parse(_refMarker.TreeViewMulti.Nodes[i].Name) != id)
                    {
                        continue;
                    }

                    index = i;
                    break;
                }
            }
            _refMarker.TreeViewMulti.Nodes[index].Tag = multi;
            _refMarker.TreeViewMulti.Nodes[index].ForeColor = Color.Black;
            if (index != _refMarker.TreeViewMulti.SelectedNode.Index)
            {
                _refMarker.TreeViewMulti.SelectedNode = _refMarker.TreeViewMulti.Nodes[index];
            }

            AfterSelect_Multi(this, null);
            ControlEvents.FireMultiChangeEvent(this, index);
        }

        private void AfterSelect_Multi(object sender, TreeViewEventArgs e)
        {
            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
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
            if (TreeViewMulti.SelectedNode?.Tag is MultiComponentList multi && multi != MultiComponentList.Empty)
            {
                int h = HeightChangeMulti.Maximum - HeightChangeMulti.Value;
                _mulBitmap = multi.GetImage(h);
            }
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
            string fileName = Path.Combine(Options.OutputPath, $"Multi 0x{int.Parse(TreeViewMulti.SelectedNode.Name):X4}{floorSuffix}.{fileExtension}");
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
            TreeViewMulti.BeginUpdate();
            TreeViewMulti.Nodes.Clear();

            if (_showFreeSlots)
            {
                for (int i = 0; i < Multis.MaximumMultiIndex; ++i)
                {
                    MultiComponentList multi = Multis.GetComponents(i);
                    TreeNode node = BuildMulNode(i, multi);
                    if (multi == MultiComponentList.Empty)
                    {
                        node.ForeColor = Color.Red;
                    }

                    TreeViewMulti.Nodes.Add(node);
                }
            }
            else
            {
                for (int i = 0; i < Multis.MaximumMultiIndex; ++i)
                {
                    MultiComponentList multi = Multis.GetComponents(i);
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    TreeViewMulti.Nodes.Add(BuildMulNode(i, multi));
                }
            }
            TreeViewMulti.EndUpdate();
        }

        private void OnExportTextFile(object sender, EventArgs e)
        {
            if (TreeViewMulti.SelectedNode == null)
            {
                return;
            }

            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            int id = int.Parse(TreeViewMulti.SelectedNode.Name);

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Multi 0x{id:X}.txt");
            multi.ExportToTextFile(fileName);

            FileSavedDialog.Show(FindForm(), fileName, "Multi saved successfully.");
        }

        private void OnExportWscFile(object sender, EventArgs e)
        {
            if (TreeViewMulti.SelectedNode == null)
            {
                return;
            }

            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            int id = int.Parse(TreeViewMulti.SelectedNode.Name);

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Multi 0x{id:X}.wsc");
            multi.ExportToWscFile(fileName);

            FileSavedDialog.Show(FindForm(), fileName, "Multi saved successfully.");
        }

        private void OnExportUOAFile(object sender, EventArgs e)
        {
            if (TreeViewMulti.SelectedNode == null)
            {
                return;
            }

            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            int id = int.Parse(TreeViewMulti.SelectedNode.Name);

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Multi 0x{id:X}.uoa");
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
            if (TreeViewMulti.SelectedNode == null)
            {
                return;
            }

            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            int id = int.Parse(TreeViewMulti.SelectedNode.Name);
            DialogResult result = MessageBox.Show(string.Format("Are you sure to remove {0} (0x{0:X})", id), "Remove",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Multis.Remove(id);
            TreeViewMulti.SelectedNode.Remove();
            Options.ChangedUltimaClass["Multis"] = true;
            ControlEvents.FireMultiChangeEvent(this, id);
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            int id = int.Parse(TreeViewMulti.SelectedNode.Name);
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

                for (int i = 0; i < _refMarker.TreeViewMulti.Nodes.Count; i++)
                {
                    int index = int.Parse(_refMarker.TreeViewMulti.Nodes[i].Name);
                    if (index < 0)
                    {
                        continue;
                    }

                    const int maximumMultiHeight = 127;
                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi 0x{index:X4}.{fileExtension}");

                    using (Bitmap multiBitmap = ((MultiComponentList)_refMarker.TreeViewMulti.Nodes[i].Tag)?.GetImage(maximumMultiHeight))
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

                for (int i = 0; i < _refMarker.TreeViewMulti.Nodes.Count; ++i)
                {
                    int index = int.Parse(_refMarker.TreeViewMulti.Nodes[i].Name);
                    if (index < 0)
                    {
                        continue;
                    }

                    MultiComponentList multi = (MultiComponentList)_refMarker.TreeViewMulti.Nodes[i].Tag;
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi 0x{index:X4}.txt");
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

                for (int i = 0; i < _refMarker.TreeViewMulti.Nodes.Count; ++i)
                {
                    int index = int.Parse(_refMarker.TreeViewMulti.Nodes[i].Name);
                    if (index < 0)
                    {
                        continue;
                    }

                    MultiComponentList multi = (MultiComponentList)_refMarker.TreeViewMulti.Nodes[i].Tag;
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi 0x{index:X4}.uoa");
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

                for (int i = 0; i < _refMarker.TreeViewMulti.Nodes.Count; ++i)
                {
                    int index = int.Parse(_refMarker.TreeViewMulti.Nodes[i].Name);
                    if (index < 0)
                    {
                        continue;
                    }

                    MultiComponentList multi = (MultiComponentList)_refMarker.TreeViewMulti.Nodes[i].Tag;
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi 0x{index:X4}.wsc");
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

                for (int i = 0; i < _refMarker.TreeViewMulti.Nodes.Count; ++i)
                {
                    int index = int.Parse(_refMarker.TreeViewMulti.Nodes[i].Name);
                    if (index < 0)
                    {
                        continue;
                    }

                    MultiComponentList multi = (MultiComponentList)_refMarker.TreeViewMulti.Nodes[i].Tag;
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

                for (int i = 0; i < _refMarker.TreeViewMulti.Nodes.Count; ++i)
                {
                    int index = int.Parse(_refMarker.TreeViewMulti.Nodes[i].Name);
                    if (index < 0)
                    {
                        continue;
                    }

                    MultiComponentList multi = (MultiComponentList)_refMarker.TreeViewMulti.Nodes[i].Tag;
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi 0x{index:X4}.uox3");
                    multi.ExportToUox3File(fileName);
                }

                FileSavedDialog.Show(FindForm(), dialog.SelectedPath, "All Multis saved successfully.");
            }
        }

        private void OnExportCsvFile(object sender, EventArgs e)
        {
            if (TreeViewMulti.SelectedNode == null)
            {
                return;
            }

            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            int id = int.Parse(TreeViewMulti.SelectedNode.Name);

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"{id:D4}.csv");
            multi.ExportToCsvFile(fileName);
            FileSavedDialog.Show(FindForm(), fileName, "Multi saved successfully.");
        }

        private void OnExportUox3File(object sender, EventArgs e)
        {
            if (TreeViewMulti.SelectedNode == null)
            {
                return;
            }

            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            int id = int.Parse(TreeViewMulti.SelectedNode.Name);

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Multi 0x{id:X4}.uox3");
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
            treeViewUop.BeginUpdate();
            treeViewUop.Nodes.Clear();

            if (!Multis.HasUopFile)
            {
                treeViewUop.Nodes.Add(new TreeNode("multicollection.uop not found or path is not set.") { Name = "-1" });
                treeViewUop.EndUpdate();
                return;
            }

            var cache = new List<TreeNode>();
            for (int i = 0; i < Multis.MaximumMultiIndex; ++i)
            {
                MultiComponentList multi = Multis.GetUopComponents(i);
                if (multi == MultiComponentList.Empty)
                {
                    continue;
                }

                cache.Add(new TreeNode(BuildNodeLabel(i)) { Tag = multi, Name = i.ToString() });
            }

            treeViewUop.Nodes.AddRange(cache.ToArray());
            treeViewUop.EndUpdate();

            if (treeViewUop.Nodes.Count > 0)
            {
                treeViewUop.SelectedNode = treeViewUop.Nodes[0];
            }
        }

        private void AfterSelect_UopMulti(object sender, TreeViewEventArgs e)
        {
            if (treeViewUop.SelectedNode?.Tag is not MultiComponentList multi)
            {
                return;
            }

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
            if (treeViewUop.SelectedNode?.Tag is MultiComponentList multi && multi != MultiComponentList.Empty)
            {
                int h = HeightChangeUop.Maximum - HeightChangeUop.Value;
                _uopBitmap = multi.GetImage(h);
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
            int id = int.Parse(treeViewUop.SelectedNode.Name);
            string fileName = Path.Combine(Options.OutputPath, $"UopMulti 0x{id:X4}{floorSuffix}.{fileExtension}");
            SaveImage(_uopBitmap, fileName, imageFormat, backgroundColor);
            FileSavedDialog.Show(FindForm(), fileName, "Multi saved successfully.");
        }

        private void OnUopExportTextFile(object sender, EventArgs e)
        {
            if (treeViewUop.SelectedNode?.Tag is not MultiComponentList multi || multi == MultiComponentList.Empty)
            {
                return;
            }

            int id = int.Parse(treeViewUop.SelectedNode.Name);
            string fileName = Path.Combine(Options.OutputPath, $"UopMulti 0x{id:X}.txt");
            multi.ExportToTextFile(fileName);
            FileSavedDialog.Show(FindForm(), fileName, "Multi saved successfully.");
        }

        private void OnUopExportUOAFile(object sender, EventArgs e)
        {
            if (treeViewUop.SelectedNode?.Tag is not MultiComponentList multi || multi == MultiComponentList.Empty)
            {
                return;
            }

            int id = int.Parse(treeViewUop.SelectedNode.Name);
            string fileName = Path.Combine(Options.OutputPath, $"UopMulti 0x{id:X}.uoa");
            multi.ExportToUOAFile(fileName);
            FileSavedDialog.Show(FindForm(), fileName, "Multi saved successfully.");
        }

        private void OnUopExportWscFile(object sender, EventArgs e)
        {
            if (treeViewUop.SelectedNode?.Tag is not MultiComponentList multi || multi == MultiComponentList.Empty)
            {
                return;
            }

            int id = int.Parse(treeViewUop.SelectedNode.Name);
            string fileName = Path.Combine(Options.OutputPath, $"UopMulti 0x{id:X}.wsc");
            multi.ExportToWscFile(fileName);
            FileSavedDialog.Show(FindForm(), fileName, "Multi saved successfully.");
        }

        private void OnUopExportCsvFile(object sender, EventArgs e)
        {
            if (treeViewUop.SelectedNode?.Tag is not MultiComponentList multi || multi == MultiComponentList.Empty)
            {
                return;
            }

            int id = int.Parse(treeViewUop.SelectedNode.Name);
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
            for (int i = 0; i < treeViewUop.Nodes.Count; i++)
            {
                if (!int.TryParse(treeViewUop.Nodes[i].Name, out int index) || index < 0)
                {
                    continue;
                }

                if (treeViewUop.Nodes[i].Tag is not MultiComponentList multi || multi == MultiComponentList.Empty)
                {
                    continue;
                }

                string fileName = Path.Combine(dialog.SelectedPath, $"UopMulti 0x{index:X4}.{fileExtension}");
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

            for (int i = 0; i < treeViewUop.Nodes.Count; ++i)
            {
                if (!int.TryParse(treeViewUop.Nodes[i].Name, out int index) || index < 0)
                {
                    continue;
                }

                if (treeViewUop.Nodes[i].Tag is not MultiComponentList multi || multi == MultiComponentList.Empty)
                {
                    continue;
                }

                multi.ExportToTextFile(Path.Combine(dialog.SelectedPath, $"UopMulti 0x{index:X4}.txt"));
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

            for (int i = 0; i < treeViewUop.Nodes.Count; ++i)
            {
                if (!int.TryParse(treeViewUop.Nodes[i].Name, out int index) || index < 0)
                {
                    continue;
                }

                if (treeViewUop.Nodes[i].Tag is not MultiComponentList multi || multi == MultiComponentList.Empty)
                {
                    continue;
                }

                multi.ExportToUOAFile(Path.Combine(dialog.SelectedPath, $"UopMulti 0x{index:X4}.uoa"));
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

            for (int i = 0; i < treeViewUop.Nodes.Count; ++i)
            {
                if (!int.TryParse(treeViewUop.Nodes[i].Name, out int index) || index < 0)
                {
                    continue;
                }

                if (treeViewUop.Nodes[i].Tag is not MultiComponentList multi || multi == MultiComponentList.Empty)
                {
                    continue;
                }

                multi.ExportToWscFile(Path.Combine(dialog.SelectedPath, $"UopMulti 0x{index:X4}.wsc"));
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

            for (int i = 0; i < treeViewUop.Nodes.Count; ++i)
            {
                if (!int.TryParse(treeViewUop.Nodes[i].Name, out int index) || index < 0)
                {
                    continue;
                }

                if (treeViewUop.Nodes[i].Tag is not MultiComponentList multi || multi == MultiComponentList.Empty)
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

                for (int i = 0; i < _refMarker.TreeViewMulti.Nodes.Count; ++i)
                {
                    int index = int.Parse(_refMarker.TreeViewMulti.Nodes[i].Name);
                    if (index < 0)
                    {
                        continue;
                    }

                    MultiComponentList multi = (MultiComponentList)_refMarker.TreeViewMulti.Nodes[i].Tag;
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    groupWriter.WriteStartElement("Entry");
                    groupWriter.WriteAttributeString("ID", index.ToString());
                    groupWriter.WriteAttributeString("Name", _refMarker.TreeViewMulti.Nodes[i].Text.Trim());

                    writer.WriteStartElement("Entry");
                    writer.WriteAttributeString("ID", index.ToString());
                    writer.WriteAttributeString("Name", _refMarker.TreeViewMulti.Nodes[i].Text.Trim());

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
