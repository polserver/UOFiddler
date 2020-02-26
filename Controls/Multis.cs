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

namespace FiddlerControls
{
    public partial class Multis : UserControl
    {
        private readonly string _multiXmlFileName = Path.Combine(Options.AppDataPath, "Multilist.xml");
        private readonly XmlDocument _xDom;
        private readonly XmlElement _xMultis;

        public Multis()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            _refMarker = this;

            if (!File.Exists(_multiXmlFileName))
            {
                return;
            }

            _xDom = new XmlDocument();
            _xDom.Load(_multiXmlFileName);
            _xMultis = _xDom["Multis"];
        }

        private bool _loaded;
        private bool _showFreeSlots;
        private readonly Multis _refMarker;

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

        private void OnLoad(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            Options.LoadedUltimaClass["Multis"] = true;
            Options.LoadedUltimaClass["Hues"] = true;

            TreeViewMulti.BeginUpdate();
            TreeViewMulti.Nodes.Clear();
            var cache = new List<TreeNode>();
            for (int i = 0; i < 0x3000; ++i)
            {
                MultiComponentList multi = Ultima.Multis.GetComponents(i);
                if (multi == MultiComponentList.Empty)
                {
                    continue;
                }

                TreeNode node = null;
                if (_xDom == null)
                {
                    node = new TreeNode(string.Format("{0,5} (0x{0:X})", i));
                }
                else
                {
                    XmlNodeList xMultiNodeList = _xMultis.SelectNodes("/Multis/Multi[@id='" + i + "']");
                    string j = "";
                    foreach (XmlNode xMultiNode in xMultiNodeList)
                    {
                        j = xMultiNode.Attributes["name"].Value;
                    }
                    node = new TreeNode(string.Format("{0,5} (0x{0:X}) {1}", i, j));
                    xMultiNodeList = _xMultis.SelectNodes("/Multis/ToolTip[@id='" + i + "']");
                    foreach (XmlNode xMultiNode in xMultiNodeList)
                    {
                        node.ToolTipText = j + "\r\n" + xMultiNode.Attributes["text"].Value;
                    }
                    if (xMultiNodeList.Count == 0)
                    {
                        node.ToolTipText = j;
                    }
                }
                node.Tag = multi;
                node.Name = i.ToString();
                cache.Add(node);
            }
            TreeViewMulti.Nodes.AddRange(cache.ToArray());
            TreeViewMulti.EndUpdate();
            if (TreeViewMulti.Nodes.Count > 0)
            {
                TreeViewMulti.SelectedNode = TreeViewMulti.Nodes[0];
            }

            if (!_loaded)
            {
                FiddlerControls.Events.FilePathChangeEvent += OnFilePathChangeEvent;
                FiddlerControls.Events.MultiChangeEvent += OnMultiChangeEvent;
            }
            _loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
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

            MultiComponentList multi = Ultima.Multis.GetComponents(id);
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
            FiddlerControls.Events.FireMultiChangeEvent(this, index);
        }

        private void AfterSelect_Multi(object sender, TreeViewEventArgs e)
        {
            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            if (multi == MultiComponentList.Empty)
            {
                HeightChangeMulti.Maximum = 0;
                toolTip.SetToolTip(HeightChangeMulti, "MaxHeight: 0");
                StatusMultiText.Text = "Size: 0,0 MaxHeight: 0 MultiRegion: 0,0,0,0";
            }
            else
            {
                HeightChangeMulti.Maximum = multi.MaxHeight;
                toolTip.SetToolTip(HeightChangeMulti,
                    $"MaxHeight: {HeightChangeMulti.Maximum - HeightChangeMulti.Value}");
                StatusMultiText.Text =
                    $"Size: {multi.Width},{multi.Height} MaxHeight: {multi.MaxHeight} MultiRegion: {multi.Min.X},{multi.Min.Y},{multi.Max.X},{multi.Max.Y} Surface: {multi.Surface}";
            }
            ChangeComponentList(multi);
            MultiPictureBox.Invalidate();
        }

        private void OnPaint_MultiPic(object sender, PaintEventArgs e)
        {
            if (TreeViewMulti.SelectedNode == null)
            {
                return;
            }

            if ((MultiComponentList)TreeViewMulti.SelectedNode.Tag == MultiComponentList.Empty)
            {
                e.Graphics.Clear(Color.White);
                return;
            }
            int h = HeightChangeMulti.Maximum - HeightChangeMulti.Value;
            Bitmap mMainPictureMulti = ((MultiComponentList)TreeViewMulti.SelectedNode.Tag).GetImage(h);
            if (mMainPictureMulti == null)
            {
                e.Graphics.Clear(Color.White);
                return;
            }
            Point location = Point.Empty;
            Size size = MultiPictureBox.Size;
            Rectangle destRect;
            if (mMainPictureMulti.Height < size.Height && mMainPictureMulti.Width < size.Width)
            {
                location.X = (MultiPictureBox.Width - mMainPictureMulti.Width) / 2;
                location.Y = (MultiPictureBox.Height - mMainPictureMulti.Height) / 2;
                destRect = new Rectangle(location, mMainPictureMulti.Size);
            }
            else if (mMainPictureMulti.Height < size.Height)
            {
                location.X = 0;
                location.Y = (MultiPictureBox.Height - mMainPictureMulti.Height) / 2;
                destRect = mMainPictureMulti.Width > size.Width
                    ? new Rectangle(location, new Size(size.Width, mMainPictureMulti.Height))
                    : new Rectangle(location, mMainPictureMulti.Size);
            }
            else if (mMainPictureMulti.Width < size.Width)
            {
                location.X = (MultiPictureBox.Width - mMainPictureMulti.Width) / 2;
                location.Y = 0;
                destRect = mMainPictureMulti.Height > size.Height
                    ? new Rectangle(location, new Size(mMainPictureMulti.Width, size.Height))
                    : new Rectangle(location, mMainPictureMulti.Size);
            }
            else
            {
                destRect = new Rectangle(new Point(0, 0), size);
            }

            e.Graphics.DrawImage(mMainPictureMulti, destRect, 0, 0, mMainPictureMulti.Width, mMainPictureMulti.Height, GraphicsUnit.Pixel);
        }

        private void OnValue_HeightChangeMulti(object sender, EventArgs e)
        {
            toolTip.SetToolTip(HeightChangeMulti, $"MaxHeight: {HeightChangeMulti.Maximum - HeightChangeMulti.Value}");
            MultiPictureBox.Invalidate();
        }

        private void ChangeComponentList(MultiComponentList multi)
        {
            MultiComponentBox.Clear();
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            bool isUohsa = Art.IsUoahs();
            for (int x = 0; x < multi.Width; ++x)
            {
                for (int y = 0; y < multi.Height; ++y)
                {
                    MTile[] tiles = multi.Tiles[x][y];
                    for (int i = 0; i < tiles.Length; ++i)
                    {
                        if (isUohsa)
                        {
                            MultiComponentBox.AppendText(
                                $"0x{tiles[i].Id:X4} {x,3} {y,3} {tiles[i].Z,2} {tiles[i].Flag,2} {tiles[i].Unk1,2}\n");
                        }
                        else
                        {
                            MultiComponentBox.AppendText(
                                $"0x{tiles[i].Id:X4} {x,3} {y,3} {tiles[i].Z,2} {tiles[i].Flag,2}\n");
                        }
                    }
                }
            }
        }

        private void Extract_Image_ClickBmp(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Multi 0x{int.Parse(TreeViewMulti.SelectedNode.Name):X}.bmp");
            int h = HeightChangeMulti.Maximum - HeightChangeMulti.Value;
            Bitmap bit = ((MultiComponentList)TreeViewMulti.SelectedNode.Tag).GetImage(h);
            bit.Save(fileName, ImageFormat.Bmp);
            MessageBox.Show($"Multi saved to {fileName}", "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void Extract_Image_ClickTiff(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Multi 0x{int.Parse(TreeViewMulti.SelectedNode.Name):X}.tiff");
            int h = HeightChangeMulti.Maximum - HeightChangeMulti.Value;
            Bitmap bit = ((MultiComponentList)TreeViewMulti.SelectedNode.Tag).GetImage(h);
            bit.Save(fileName, ImageFormat.Tiff);
            MessageBox.Show($"Multi saved to {fileName}", "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void Extract_Image_ClickJpg(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Multi 0x{int.Parse(TreeViewMulti.SelectedNode.Name):X}.jpg");
            int h = HeightChangeMulti.Maximum - HeightChangeMulti.Value;
            Bitmap bit = ((MultiComponentList)TreeViewMulti.SelectedNode.Tag).GetImage(h);
            bit.Save(fileName, ImageFormat.Jpeg);
            MessageBox.Show($"Multi saved to {fileName}", "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickFreeSlots(object sender, EventArgs e)
        {
            _showFreeSlots = !_showFreeSlots;
            TreeViewMulti.BeginUpdate();
            TreeViewMulti.Nodes.Clear();

            if (_showFreeSlots)
            {
                for (int i = 0; i < 0x3000; ++i)
                {
                    MultiComponentList multi = Ultima.Multis.GetComponents(i);
                    TreeNode node;
                    if (_xDom == null)
                    {
                        node = new TreeNode($"{i,5} (0x{i:X})");
                    }
                    else
                    {
                        XmlNodeList xMultiNodeList = _xMultis.SelectNodes("/Multis/Multi[@id='" + i + "']");
                        string j = "";
                        foreach (XmlNode xMultiNode in xMultiNodeList)
                        {
                            j = xMultiNode.Attributes["name"].Value;
                        }
                        node = new TreeNode(string.Format("{0,5} (0x{0:X}) {1}", i, j));
                    }
                    node.Name = i.ToString();
                    node.Tag = multi;
                    if (multi == MultiComponentList.Empty)
                    {
                        node.ForeColor = Color.Red;
                    }

                    TreeViewMulti.Nodes.Add(node);
                }
            }
            else
            {
                for (int i = 0; i < 0x3000; ++i)
                {
                    MultiComponentList multi = Ultima.Multis.GetComponents(i);
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    TreeNode node;
                    if (_xDom == null)
                    {
                        node = new TreeNode($"{i,5} (0x{i:X})");
                    }
                    else
                    {
                        XmlNodeList xMultiNodeList = _xMultis.SelectNodes("/Multis/Multi[@id='" + i + "']");
                        string j = "";
                        foreach (XmlNode xMultiNode in xMultiNodeList)
                        {
                            j = xMultiNode.Attributes["name"].Value;
                        }
                        node = new TreeNode(string.Format("{0,5} (0x{0:X}) {1}", i, j));
                    }
                    node.Tag = multi;
                    node.Name = i.ToString();
                    TreeViewMulti.Nodes.Add(node);
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
            MessageBox.Show($"Multi saved to {fileName}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
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
            MessageBox.Show($"Multi saved to {fileName}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
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
            multi.ExportToUoaFile(fileName);
            MessageBox.Show($"Multi saved to {fileName}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            Ultima.Multis.Save(Options.OutputPath);
            MessageBox.Show(
                $"Saved to {Options.OutputPath}",
                    "Save",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Multis"] = false;
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
            DialogResult result =
                        MessageBox.Show(string.Format("Are you sure to remove {0} (0x{0:X})", id),
                        "Remove",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Ultima.Multis.Remove(id);
            TreeViewMulti.SelectedNode.Remove();
            Options.ChangedUltimaClass["Multis"] = true;
            FiddlerControls.Events.FireMultiChangeEvent(this, id);
        }

        private MultiImport _multiImport;

        private void OnClickImport(object sender, EventArgs e)
        {
            if (_multiImport?.IsDisposed == false)
            {
                return;
            }

            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            int id = int.Parse(TreeViewMulti.SelectedNode.Name);
            if (multi != MultiComponentList.Empty)
            {
                DialogResult result =
                    MessageBox.Show(string.Format("Are you sure to replace {0} (0x{0:X})", id),
                        "Import",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            _multiImport = new MultiImport(this, id)
            {
                TopMost = true
            };
            _multiImport.Show();
        }

        private void OnClick_SaveAllBmp(object sender, EventArgs e)
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

                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi 0x{index:X}.bmp");
                    const int h = 120;
                    Bitmap bit = ((MultiComponentList)_refMarker.TreeViewMulti.Nodes[i].Tag).GetImage(h);
                    bit?.Save(fileName, ImageFormat.Bmp);
                    bit?.Dispose();
                }
                MessageBox.Show($"All Multis saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClick_SaveAllTiff(object sender, EventArgs e)
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

                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi 0x{index:X}.tiff");
                    const int h = 120;
                    Bitmap bit = ((MultiComponentList)_refMarker.TreeViewMulti.Nodes[i].Tag).GetImage(h);
                    bit?.Save(fileName, ImageFormat.Tiff);
                    bit?.Dispose();
                }
                MessageBox.Show($"All Multis saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClick_SaveAllJpg(object sender, EventArgs e)
        {
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

                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi 0x{index:X}.jpg");
                    const int h = 120;
                    Bitmap bit = ((MultiComponentList)_refMarker.TreeViewMulti.Nodes[i].Tag).GetImage(h);
                    bit?.Save(fileName, ImageFormat.Jpeg);
                    bit?.Dispose();
                }
                MessageBox.Show($"All Multis saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
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

                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi 0x{index:X}.txt");
                    multi.ExportToTextFile(fileName);
                }
                MessageBox.Show($"All Multis saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
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

                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi 0x{index:X}.uoa");
                    multi.ExportToUoaFile(fileName);
                }
                MessageBox.Show($"All Multis saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
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

                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi 0x{index:X}.wsc");
                    multi.ExportToWscFile(fileName);
                }
                MessageBox.Show($"All Multis saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }
    }
}
