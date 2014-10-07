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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Ultima;

namespace FiddlerControls
{
    public partial class Map : UserControl
    {
        public Map()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            if (!Files.CacheData)
                PreloadMap.Visible = false;
            ProgressBar.Visible = false;
            refmarker = this;
            panel1.Visible = false;
        }

        public static Map refmarker;
        private Bitmap map;
        private Ultima.Map currmap;
        private int currmapint = 0;
        private bool SyncWithClient = false;
        private int ClientX = 0;
        private int ClientY = 0;
        private int ClientZ = 0;
        private int ClientMap = 0;
        private Point currPoint;
        public static double Zoom = 1;
        private bool moving = false;
        private Point movingpoint;


        public static int HScrollBar { get { return refmarker.hScrollBar.Value; } }
        public static int VScrollBar { get { return refmarker.vScrollBar.Value; } }
        public static Ultima.Map CurrMap { get { return refmarker.currmap; } }

        private static bool Loaded = false;

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (!Loaded)
                return;
            Zoom = 1;
            moving = false;
            OnLoad(this, EventArgs.Empty);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            LoadMapOverlays();
            Options.LoadedUltimaClass["Map"] = true;
            Options.LoadedUltimaClass["RadarColor"] = true;

            currmap = Ultima.Map.Felucca;
            feluccaToolStripMenuItem.Checked = true;
            trammelToolStripMenuItem.Checked = false;
            ilshenarToolStripMenuItem.Checked = false;
            malasToolStripMenuItem.Checked = false;
            tokunoToolStripMenuItem.Checked = false;
            PreloadMap.Visible = true;
            ChangeMapNames();
            ZoomLabel.Text = String.Format("Zoom: {0}", Zoom);
            SetScrollBarValues();
            Refresh();
            pictureBox.Invalidate();
            Cursor.Current = Cursors.Default;

            if (!Loaded)
            {
                FiddlerControls.Events.MapDiffChangeEvent += new FiddlerControls.Events.MapDiffChangeHandler(OnMapDiffChangeEvent);
                FiddlerControls.Events.MapNameChangeEvent += new FiddlerControls.Events.MapNameChangeHandler(OnMapNameChangeEvent);
                FiddlerControls.Events.MapSizeChangeEvent += new FiddlerControls.Events.MapSizeChangeHandler(OnMapSizeChangeEvent);
                FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
            }
            Loaded = true;
        }

        private void OnMapDiffChangeEvent()
        {
            pictureBox.Invalidate();
        }
        private void OnMapNameChangeEvent()
        {
            ChangeMapNames();
        }
        private void OnMapSizeChangeEvent()
        {
            Reload();
        }
        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        public void RefreshMap()
        {
            pictureBox.Invalidate();
        }

        /// <summary>
        /// Changes the Names of maps
        /// </summary>
        private void ChangeMapNames()
        {
            if (!Loaded)
                return;
            feluccaToolStripMenuItem.Text = Options.MapNames[0];
            trammelToolStripMenuItem.Text = Options.MapNames[1];
            ilshenarToolStripMenuItem.Text = Options.MapNames[2];
            malasToolStripMenuItem.Text = Options.MapNames[3];
            tokunoToolStripMenuItem.Text = Options.MapNames[4];
            terMurToolStripMenuItem.Text = Options.MapNames[5];
            if (OverlayObjectTree.Nodes.Count > 0)
            {
                OverlayObjectTree.Nodes[0].Text = Options.MapNames[0];
                OverlayObjectTree.Nodes[1].Text = Options.MapNames[1];
                OverlayObjectTree.Nodes[2].Text = Options.MapNames[2];
                OverlayObjectTree.Nodes[3].Text = Options.MapNames[3];
                OverlayObjectTree.Nodes[4].Text = Options.MapNames[4];
                OverlayObjectTree.Nodes[5].Text = Options.MapNames[5];
                OverlayObjectTree.Invalidate();
            }
        }

        private void HandleScroll(object sender, ScrollEventArgs e)
        {
            pictureBox.Invalidate();
        }

        public static int Round(int x)
        {
            return (int)((x >> 3) << 3);
        }

        private void ZoomMap(ref Bitmap bmp0)
        {
            Bitmap bmp1 = new Bitmap((int)(map.Width * Zoom), (int)(map.Height * Zoom));
            Graphics graph = Graphics.FromImage(bmp1);
            graph.InterpolationMode = InterpolationMode.NearestNeighbor;
            graph.PixelOffsetMode = PixelOffsetMode.Half;
            graph.DrawImage(bmp0, new Rectangle(0, 0, bmp1.Width, bmp1.Height));
            graph.Dispose();
            bmp0 = bmp1;
        }

        private void SetScrollBarValues()
        {
            vScrollBar.Minimum = 0;
            hScrollBar.Minimum = 0;
            ChangeScrollBar();
            hScrollBar.LargeChange = 40;
            hScrollBar.SmallChange = 8;
            vScrollBar.LargeChange = 40;
            vScrollBar.SmallChange = 8;
            vScrollBar.Value = 0;
            hScrollBar.Value = 0;
        }

        private void ChangeScrollBar()
        {
            hScrollBar.Maximum = (int)(currmap.Width);
            hScrollBar.Maximum -= Round((int)(pictureBox.ClientSize.Width / Zoom) - 8);
            if (Zoom >= 1)
                hScrollBar.Maximum += (int)(40 * Zoom);
            else if (Zoom < 1)
                hScrollBar.Maximum += (int)(40 / Zoom);
            hScrollBar.Maximum = Math.Max(0, Round(hScrollBar.Maximum));
            vScrollBar.Maximum = (int)(currmap.Height);
            vScrollBar.Maximum -= Round((int)(pictureBox.ClientSize.Height / Zoom) - 8);
            if (Zoom >= 1)
                vScrollBar.Maximum += (int)(40 * Zoom);
            else if (Zoom < 1)
                vScrollBar.Maximum += (int)(40 / Zoom);
            vScrollBar.Maximum = Math.Max(0, Round(vScrollBar.Maximum));
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (Loaded)
            {
                ChangeScrollBar();
                pictureBox.Invalidate();
            }
        }

        private void ChangeMap()
        {
            PreloadMap.Visible = !currmap.IsCached(showStaticsToolStripMenuItem1.Checked);
            SetScrollBarValues();
            pictureBox.Invalidate();
        }

        private void ResetCheckedMap()
        {
            feluccaToolStripMenuItem.Checked = false;
            trammelToolStripMenuItem.Checked = false;
            malasToolStripMenuItem.Checked = false;
            ilshenarToolStripMenuItem.Checked = false;
            tokunoToolStripMenuItem.Checked = false;
            terMurToolStripMenuItem.Checked = false;
        }

        private void ChangeMapFelucca(object sender, EventArgs e)
        {
            if (!feluccaToolStripMenuItem.Checked)
            {
                ResetCheckedMap();
                feluccaToolStripMenuItem.Checked = true;
                currmap = Ultima.Map.Felucca;
                currmapint = 0;
                ChangeMap();
            }
        }

        private void ChangeMapTrammel(object sender, EventArgs e)
        {
            if (!trammelToolStripMenuItem.Checked)
            {
                ResetCheckedMap();
                trammelToolStripMenuItem.Checked = true;
                currmap = Ultima.Map.Trammel;
                currmapint = 1;
                ChangeMap();
            }
        }

        private void ChangeMapIlshenar(object sender, EventArgs e)
        {
            if (!ilshenarToolStripMenuItem.Checked)
            {
                ResetCheckedMap();
                ilshenarToolStripMenuItem.Checked = true;
                currmap = Ultima.Map.Ilshenar;
                currmapint = 2;
                ChangeMap();
            }
        }

        private void ChangeMapMalas(object sender, EventArgs e)
        {
            if (!malasToolStripMenuItem.Checked)
            {
                ResetCheckedMap();
                malasToolStripMenuItem.Checked = true;
                currmap = Ultima.Map.Malas;
                currmapint = 3;
                ChangeMap();
            }
        }

        private void ChangeMapTokuno(object sender, EventArgs e)
        {
            if (!tokunoToolStripMenuItem.Checked)
            {
                ResetCheckedMap();
                tokunoToolStripMenuItem.Checked = true;
                currmap = Ultima.Map.Tokuno;
                currmapint = 4;
                ChangeMap();
            }
        }

        private void ChangeMapTerMur(object sender, EventArgs e)
        {
            if (!terMurToolStripMenuItem.Checked)
            {
                ResetCheckedMap();
                terMurToolStripMenuItem.Checked = true;
                currmap = Ultima.Map.TerMur;
                currmapint = 5;
                ChangeMap();
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                moving = true;
                movingpoint.X = e.X;
                movingpoint.Y = e.Y;
                this.Cursor = Cursors.Hand;
            }
            else
            {
                moving = false;
                this.Cursor = Cursors.Default;
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            moving = false;
            this.Cursor = Cursors.Default;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            int xDelta = Math.Min(currmap.Width, (int)(e.X / Zoom) + Round(hScrollBar.Value));
            int yDelta = Math.Min(currmap.Height, (int)(e.Y / Zoom) + Round(vScrollBar.Value));
            CoordsLabel.Text = String.Format("Coords: {0},{1}", xDelta, yDelta);
            if (moving)
            {
                int deltax = (int)(-1 * (e.X - movingpoint.X) / Zoom);
                int deltay = (int)(-1 * (e.Y - movingpoint.Y) / Zoom);
                movingpoint.X = e.X;
                movingpoint.Y = e.Y;
                hScrollBar.Value = Math.Max(0, Math.Min(hScrollBar.Maximum, hScrollBar.Value + deltax));
                vScrollBar.Value = Math.Max(0, Math.Min(vScrollBar.Maximum, vScrollBar.Value + deltay));
                pictureBox.Invalidate();
            }
        }

        private void onClick_ShowClientLoc(object sender, EventArgs e)
        {
            SyncWithClient = !SyncWithClient;
        }

        private void onClick_GotoClientLoc(object sender, EventArgs e)
        {
            int x = 0;
            int y = 0;
            int z = 0;
            int mapClient = 0;
            if (!Ultima.Client.Running)
                return;
            Ultima.Client.Calibrate();
            if (!Ultima.Client.FindLocation(ref x, ref y, ref z, ref mapClient))
                return;
            if (currmapint != mapClient)
            {
                ResetCheckedMap();
                switch (mapClient)
                {
                    case 0:
                        feluccaToolStripMenuItem.Checked = true;
                        currmap = Ultima.Map.Felucca;
                        break;
                    case 1:
                        trammelToolStripMenuItem.Checked = true;
                        currmap = Ultima.Map.Trammel;
                        break;
                    case 2:
                        ilshenarToolStripMenuItem.Checked = true;
                        currmap = Ultima.Map.Ilshenar;
                        break;
                    case 3:
                        malasToolStripMenuItem.Checked = true;
                        currmap = Ultima.Map.Malas;
                        break;
                    case 4:
                        tokunoToolStripMenuItem.Checked = true;
                        currmap = Ultima.Map.Tokuno;
                        break;
                    case 5:
                        terMurToolStripMenuItem.Checked = true;
                        currmap = Ultima.Map.TerMur;
                        break;
                }
                currmapint = mapClient;
            }
            ClientX = x;
            ClientY = y;
            ClientZ = z;
            ClientMap = mapClient;
            SetScrollBarValues();
            hScrollBar.Value = (int)Math.Max(0, x - pictureBox.Right / Zoom / 2);
            vScrollBar.Value = (int)Math.Max(0, y - pictureBox.Bottom / Zoom / 2);
            pictureBox.Invalidate();
            ClientLocLabel.Text = String.Format("ClientLoc: {0},{1},{2},{3}", x, y, z, Options.MapNames[mapClient]);
        }

        private void SyncClientTimer(object sender, EventArgs e)
        {
            if (SyncWithClient)
            {
                int x = 0;
                int y = 0;
                int z = 0;
                int mapClient = 0;
                string mapname = "";
                if (Ultima.Client.Running)
                {
                    Ultima.Client.Calibrate();
                    if (Ultima.Client.FindLocation(ref x, ref y, ref z, ref mapClient))
                    {
                        if ((ClientX == x) && (ClientY == y) && (ClientZ == z) && (ClientMap == mapClient))
                            return;
                        ClientX = x;
                        ClientY = y;
                        ClientZ = z;
                        ClientMap = mapClient;
                        mapname = Options.MapNames[mapClient];
                    }
                }

                ClientLocLabel.Text = String.Format("ClientLoc: {0},{1},{2},{3}", x, y, z, mapname);
                pictureBox.Invalidate();
            }
        }

        private void GetMapInfo(object sender, EventArgs e)
        {
            new MapDetails(currmap, currPoint).Show();
        }

        private void OnOpenContext(object sender, CancelEventArgs e)  // Speichern für GetMapInfo
        {
            currPoint = pictureBox.PointToClient(Control.MousePosition);
            currPoint.X = (int)(currPoint.X / Zoom);
            currPoint.Y = (int)(currPoint.Y / Zoom);
            currPoint.X += Round(hScrollBar.Value);
            currPoint.Y += Round(vScrollBar.Value);
        }

        private void onContextClosed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            pictureBox.Invalidate();
        }

        private void OnDropDownClosed(object sender, EventArgs e)
        {
            pictureBox.Invalidate();
        }

        private void OnZoomMinus(object sender, EventArgs e)
        {
            Zoom /= 2;
            DoZoom();
        }

        private void OnZoomPlus(object sender, EventArgs e)
        {
            Zoom *= 2;
            DoZoom();
        }

        private void DoZoom()
        {
            ChangeScrollBar();
            ZoomLabel.Text = String.Format("Zoom: {0}", Zoom);
            int x, y;
            x = Math.Max(0, currPoint.X - (int)(pictureBox.ClientSize.Width / Zoom) / 2);
            y = Math.Max(0, currPoint.Y - (int)(pictureBox.ClientSize.Height / Zoom) / 2);
            x = Math.Min(x, hScrollBar.Maximum);
            y = Math.Min(y, vScrollBar.Maximum);
            hScrollBar.Value = Round(x);
            vScrollBar.Value = Round(y);
            pictureBox.Invalidate();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            map = currmap.GetImage(hScrollBar.Value >> 3, vScrollBar.Value >> 3,
                (int)((e.ClipRectangle.Width / Zoom) + 8) >> 3, (int)((e.ClipRectangle.Height / Zoom) + 8) >> 3,
                showStaticsToolStripMenuItem1.Checked);
            ZoomMap(ref map);
            e.Graphics.DrawImageUnscaledAndClipped(map, e.ClipRectangle);

            if (showCenterCrossToolStripMenuItem1.Checked)
            {
                Brush brush = new SolidBrush(Color.FromArgb(180, Color.White));
                Pen pen = new Pen(brush);
                int x = Round((int)(pictureBox.Width / 2));
                int y = Round((int)(pictureBox.Height / 2));
                e.Graphics.DrawLine(pen, x - 4, y, x + 4, y);
                e.Graphics.DrawLine(pen, x, y - 4, x, y + 4);
                pen.Dispose();
                brush.Dispose();
            }

            if (showClientCrossToolStripMenuItem.Checked)
            {
                if (Client.Running)
                {
                    if ((ClientX > hScrollBar.Value) &&
                        (ClientX < hScrollBar.Value + e.ClipRectangle.Width / Zoom) &&
                        (ClientY > vScrollBar.Value) &&
                        (ClientY < vScrollBar.Value + e.ClipRectangle.Height / Zoom) &&
                        (ClientMap == currmapint))
                    {
                        Brush brush = new SolidBrush(Color.FromArgb(180, Color.Yellow));
                        Pen pen = new Pen(brush);
                        int x = (int)((ClientX - Round(hScrollBar.Value)) * Zoom);
                        int y = (int)((ClientY - Round(vScrollBar.Value)) * Zoom);
                        e.Graphics.DrawLine(pen, x - 4, y, x + 4, y);
                        e.Graphics.DrawLine(pen, x, y - 4, x, y + 4);
                        e.Graphics.DrawEllipse(pen, x - 2, y - 2, 2 * 2, 2 * 2);
                        pen.Dispose();
                        brush.Dispose();
                    }
                }
            }

            if (OverlayObjectTree.Nodes.Count > 0)
            {
                if (showMarkersToolStripMenuItem.Checked)
                {
                    foreach (TreeNode obj in OverlayObjectTree.Nodes[currmapint].Nodes)
                    {
                        OverlayObject o = (OverlayObject)obj.Tag;
                        if (o.isVisible(e.ClipRectangle, currmapint))
                            o.Draw(e.Graphics);
                    }
                }
            }
        }

        private void onKeyDownGoto(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string line = TextBoxGoto.Text.Trim();
                if (line.Length > 0)
                {
                    string[] args = line.Split(' ');
                    if (args.Length != 2)
                        args = line.Split(',');
                    if (args.Length == 2)
                    {
                        int x, y;
                        if (int.TryParse(args[0], out x) && (int.TryParse(args[1], out y)))
                        {
                            if ((x >= 0) && (y >= 0))
                            {
                                if ((x <= currmap.Width) && (x <= currmap.Height))
                                {
                                    contextMenuStrip1.Close();
                                    hScrollBar.Value = (int)Math.Max(0, x - pictureBox.Right / Zoom / 2);
                                    vScrollBar.Value = (int)Math.Max(0, y - pictureBox.Bottom / Zoom / 2);
                                }
                            }
                        }
                    }
                }
                pictureBox.Invalidate();
            }
        }

        private void onClickSendClient(object sender, EventArgs e)
        {
            if (Client.Running)
            {
                int x = Round((int)(pictureBox.Width / Zoom / 2));
                int y = Round((int)(pictureBox.Height / Zoom / 2));
                x += Round(hScrollBar.Value);
                y += Round(vScrollBar.Value);
                SendCharTo(x, y);
            }
        }

        private void onClickSendClientToPos(object sender, EventArgs e)
        {
            if (Client.Running)
                SendCharTo(currPoint.X, currPoint.Y);
        }

        private void SendCharTo(int x, int y)
        {
            string format = "{0} " + Options.MapArgs;
            int z = currmap.Tiles.GetLandTile(x, y).Z;
            Client.SendText(String.Format(format, Options.MapCmd, x, y, z, currmapint, Options.MapNames[currmapint]));
        }

        private void ExtractMapBmp(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string path = FiddlerControls.Options.OutputPath;
            string name = String.Format("{0}.bmp", Options.MapNames[currmapint]);
            string FileName = Path.Combine(path, name);
            Bitmap extract = currmap.GetImage(0, 0, (currmap.Width >> 3), (currmap.Height >> 3), showStaticsToolStripMenuItem1.Checked);
            if (showMarkersToolStripMenuItem.Checked)
            {
                Graphics g = Graphics.FromImage(extract);
                foreach (TreeNode obj in OverlayObjectTree.Nodes[currmapint].Nodes)
                {
                    OverlayObject o = (OverlayObject)obj.Tag;
                    if (o.Visible)
                        o.Draw(g);
                }
                g.Save();
            }
            extract.Save(FileName, ImageFormat.Bmp);
            Cursor.Current = Cursors.Default;
            MessageBox.Show(String.Format("Map saved to {0}", FileName), "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void ExtractMapTiff(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string path = FiddlerControls.Options.OutputPath;
            string name = String.Format("{0}.tiff", Options.MapNames[currmapint]);
            string FileName = Path.Combine(path, name);
            Bitmap extract = currmap.GetImage(0, 0, (currmap.Width >> 3), (currmap.Height >> 3), showStaticsToolStripMenuItem1.Checked);
            if (showMarkersToolStripMenuItem.Checked)
            {
                Graphics g = Graphics.FromImage(extract);
                foreach (TreeNode obj in OverlayObjectTree.Nodes[currmapint].Nodes)
                {
                    OverlayObject o = (OverlayObject)obj.Tag;
                    if (o.Visible)
                        o.Draw(g);
                }
                g.Save();
            }
            extract.Save(FileName, ImageFormat.Tiff);
            Cursor.Current = Cursors.Default;
            MessageBox.Show(String.Format("Map saved to {0}", FileName), "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void ExtractMapJpg(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string path = FiddlerControls.Options.OutputPath;
            string name = String.Format("{0}.jpg", Options.MapNames[currmapint]);
            string FileName = Path.Combine(path, name);
            Bitmap extract = currmap.GetImage(0, 0, (currmap.Width >> 3), (currmap.Height >> 3), showStaticsToolStripMenuItem1.Checked);
            if (showMarkersToolStripMenuItem.Checked)
            {
                Graphics g = Graphics.FromImage(extract);
                foreach (TreeNode obj in OverlayObjectTree.Nodes[currmapint].Nodes)
                {
                    OverlayObject o = (OverlayObject)obj.Tag;
                    if (o.Visible)
                        o.Draw(g);
                }
                g.Save();
            }
            extract.Save(FileName, ImageFormat.Jpeg);
            Cursor.Current = Cursors.Default;
            MessageBox.Show(String.Format("Map saved to {0}", FileName), "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private MapMarker mapmarker;
        private void OnClickInsertMarker(object sender, EventArgs e)
        {
            if ((mapmarker == null) || (mapmarker.IsDisposed))
            {
                mapmarker = new MapMarker(currPoint.X, currPoint.Y, currmapint);
                mapmarker.TopMost = true;
                mapmarker.Show();
            }
        }

        public static void AddOverlay(int x, int y, int map, Color c, string text)
        {
            OverlayCursor o = new OverlayCursor(new Point(x, y), map, text, c);
            TreeNode node = new TreeNode(text);
            node.Tag = o;
            refmarker.OverlayObjectTree.Nodes[map].Nodes.Add(node);
            refmarker.pictureBox.Invalidate();
        }

        private void LoadMapOverlays()
        {
            string path = FiddlerControls.Options.AppDataPath;
            string FileName = Path.Combine(Path.GetDirectoryName(path), "MapOverlays.xml");
            OverlayObjectTree.BeginUpdate();
            OverlayObjectTree.Nodes.Clear();
            TreeNode node;
            node = new TreeNode(Options.MapNames[0]);
            node.Tag = 0;
            OverlayObjectTree.Nodes.Add(node);
            node = new TreeNode(Options.MapNames[1]);
            node.Tag = 1;
            OverlayObjectTree.Nodes.Add(node);
            node = new TreeNode(Options.MapNames[2]);
            node.Tag = 2;
            OverlayObjectTree.Nodes.Add(node);
            node = new TreeNode(Options.MapNames[3]);
            node.Tag = 3;
            OverlayObjectTree.Nodes.Add(node);
            node = new TreeNode(Options.MapNames[4]);
            node.Tag = 4;
            OverlayObjectTree.Nodes.Add(node);
            node = new TreeNode(Options.MapNames[5]);
            node.Tag = 5;
            OverlayObjectTree.Nodes.Add(node);
            if (File.Exists(FileName))
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(FileName);
                XmlElement xOptions = dom["Overlays"];
                foreach (XmlElement xMarker in xOptions.SelectNodes("Marker"))
                {
                    int x = int.Parse(xMarker.GetAttribute("x"));
                    int y = int.Parse(xMarker.GetAttribute("y"));
                    int m = int.Parse(xMarker.GetAttribute("map"));
                    int c = int.Parse(xMarker.GetAttribute("color"));
                    string text = xMarker.GetAttribute("text");
                    OverlayCursor o = new OverlayCursor(new Point(x, y), m, text, Color.FromArgb(c));
                    node = new TreeNode(text);
                    node.Tag = o;
                    OverlayObjectTree.Nodes[m].Nodes.Add(node);
                }
            }
            OverlayObjectTree.EndUpdate();
        }

        public static void SaveMapOverlays()
        {
            if (!Loaded)
                return;
            string filepath = FiddlerControls.Options.AppDataPath;

            string FileName = Path.Combine(filepath, "MapOverlays.xml");

            XmlDocument dom = new XmlDocument();
            XmlDeclaration decl = dom.CreateXmlDeclaration("1.0", "utf-8", null);
            dom.AppendChild(decl);
            XmlElement sr = dom.CreateElement("Overlays");
            bool entries = false;
            for (int i = 0; i < 5; ++i)
            {
                foreach (TreeNode obj in refmarker.OverlayObjectTree.Nodes[i].Nodes)
                {
                    OverlayObject o = (OverlayObject)obj.Tag;
                    XmlElement elem = dom.CreateElement("Marker");
                    o.Save(elem);
                    sr.AppendChild(elem);
                    entries = true;
                }
            }
            dom.AppendChild(sr);
            if (entries)
                dom.Save(FileName);
        }
        #region PreLoader
        private void OnClickPreloadMap(object sender, EventArgs e)
        {
            if (PreloadWorker.IsBusy)
                return;
            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = (currmap.Width >> 3) * (currmap.Height >> 3);
            ProgressBar.Step = 1;
            ProgressBar.Value = 0;
            ProgressBar.Visible = true;
            PreloadWorker.RunWorkerAsync(new Object[] { currmap, showStaticsToolStripMenuItem1.Checked });
        }

        private void PreLoadDoWork(object sender, DoWorkEventArgs e)
        {

            Ultima.Map workmap = (Ultima.Map)((Object[])e.Argument)[0];
            bool statics = (bool)((Object[])e.Argument)[1];
            int width = currmap.Width >> 3;
            int height = currmap.Height >> 3;
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    currmap.PreloadRenderedBlock(x, y, statics);
                    PreloadWorker.ReportProgress(1);
                }
            }
        }

        private void PreLoadProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.PerformStep();
        }

        private void PreLoadCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressBar.Visible = false;
            PreloadMap.Visible = false;
        }
        #endregion

        private void OnClickEditMarkers(object sender, EventArgs e)
        {
            panel1.Visible = !panel1.Visible;
            pictureBox.Invalidate();
        }

        private void OnDoubleClickMarker(object sender, TreeNodeMouseClickEventArgs e)
        {
            OnClickGotoMarker(this, null);
        }

        private void OnClickGotoMarker(object sender, EventArgs e)
        {
            if (OverlayObjectTree.SelectedNode == null)
                return;
            if (OverlayObjectTree.SelectedNode.Parent == null)
                return;
            OverlayObject o = (OverlayObject)OverlayObjectTree.SelectedNode.Tag;
            if (currmapint != o.DefMap)
            {
                ResetCheckedMap();
                switch (o.DefMap)
                {
                    case 0:
                        feluccaToolStripMenuItem.Checked = true;
                        currmap = Ultima.Map.Felucca;
                        break;
                    case 1:
                        trammelToolStripMenuItem.Checked = true;
                        currmap = Ultima.Map.Trammel;
                        break;
                    case 2:
                        ilshenarToolStripMenuItem.Checked = true;
                        currmap = Ultima.Map.Ilshenar;
                        break;
                    case 3:
                        malasToolStripMenuItem.Checked = true;
                        currmap = Ultima.Map.Malas;
                        break;
                    case 4:
                        tokunoToolStripMenuItem.Checked = true;
                        currmap = Ultima.Map.Tokuno;
                        break;
                    case 5:
                        terMurToolStripMenuItem.Checked = true;
                        currmap = Ultima.Map.TerMur;
                        break;
                }
                currmapint = o.DefMap;
            }
            SetScrollBarValues();
            hScrollBar.Value = (int)Math.Max(0, o.Loc.X - pictureBox.Right / Zoom / 2);
            vScrollBar.Value = (int)Math.Max(0, o.Loc.Y - pictureBox.Bottom / Zoom / 2);
            pictureBox.Invalidate();
        }

        private void OnClickRemoveMarker(object sender, EventArgs e)
        {
            if (OverlayObjectTree.SelectedNode == null)
                return;
            if (OverlayObjectTree.SelectedNode.Parent == null)
                return;
            OverlayObjectTree.SelectedNode.Remove();
            pictureBox.Invalidate();
        }

        private void OnClickSwitchVisible(object sender, EventArgs e)
        {
            if (OverlayObjectTree.SelectedNode == null)
                return;
            if (OverlayObjectTree.SelectedNode.Parent == null)
                return;
            OverlayObject o = (OverlayObject)OverlayObjectTree.SelectedNode.Tag;
            o.Visible = !o.Visible;
            if (!o.Visible)
                OverlayObjectTree.SelectedNode.ForeColor = Color.Red;
            else
                OverlayObjectTree.SelectedNode.ForeColor = Color.Black;
            OverlayObjectTree.Invalidate();
            pictureBox.Invalidate();
        }

        private void OnChangeView(object sender, EventArgs e)
        {
            PreloadMap.Visible = !currmap.IsCached(showStaticsToolStripMenuItem1.Checked);
            pictureBox.Invalidate();
        }

        private void OnClickDefragStatics(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Ultima.Map.DefragStatics(FiddlerControls.Options.OutputPath,
                currmap, currmap.Width, currmap.Height, false);
            Cursor.Current = Cursors.Default;
            MessageBox.Show(String.Format("Statics saved to {0}", FiddlerControls.Options.OutputPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickDefragRemoveStatics(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Ultima.Map.DefragStatics(FiddlerControls.Options.OutputPath,
                currmap, currmap.Width, currmap.Height, true);
            Cursor.Current = Cursors.Default;
            MessageBox.Show(String.Format("Statics saved to {0}", FiddlerControls.Options.OutputPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnResizeMap(object sender, EventArgs e)
        {
            if (Loaded)
            {
                ChangeScrollBar();
                pictureBox.Invalidate();
            }
        }

        private void OnClickRewriteMap(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Ultima.Map.RewriteMap(FiddlerControls.Options.OutputPath,
                currmapint, currmap.Width, currmap.Height);
            Cursor.Current = Cursors.Default;
            MessageBox.Show(String.Format("Map saved to {0}", FiddlerControls.Options.OutputPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickReportInvisStatics(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            currmap.ReportInvisStatics(FiddlerControls.Options.OutputPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show(String.Format("Report saved to {0}", FiddlerControls.Options.OutputPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickReportInvalidMapIDs(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            currmap.ReportInvalidMapIDs(FiddlerControls.Options.OutputPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show(String.Format("Report saved to {0}", FiddlerControls.Options.OutputPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private MapReplace showform = null;
        private void OnClickCopy(object sender, EventArgs e)
        {
            if ((showform == null) || (showform.IsDisposed))
            {
                showform = new MapReplace(currmap);
                showform.TopMost = true;
                showform.Show();
            }
        }

        private MapDiffInsert showformMapDiff = null;
        private void OnClickInsertDiffData(object sender, EventArgs e)
        {
            if ((showformMapDiff == null) || (showformMapDiff.IsDisposed))
            {
                showformMapDiff = new MapDiffInsert(currmap);
                showformMapDiff.TopMost = true;
                showformMapDiff.Show();
            }
        }

        private void OnClickStaticImport(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select WSC Staticfile to import";
            dialog.Multiselect = false;
            dialog.CheckFileExists = true;
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                dialog.Dispose();
                return;
            }
            string path = dialog.FileName;
            dialog.Dispose();
            StaticImport(path);
        }
        private void StaticImport(string Filename)
        {
            StreamReader ip = new StreamReader(Filename);

            string line;
            StaticTile newtile = new StaticTile();
            newtile.m_ID = 0xFFFF;
            newtile.m_Hue = 0;
            int x, y, blockx, blocky;
            blockx = blocky = 0;
            while ((line = ip.ReadLine()) != null)
            {
                if ((line = line.Trim()).Length == 0 || line.StartsWith("#") || line.StartsWith("//"))
                    continue;

                try
                {
                    if (line.StartsWith("SECTION WORLDITEM"))
                    {
                        if (newtile.m_ID != 0xFFFF)
                        {
                            currmap.Tiles.AddPendingStatic(blockx, blocky, newtile);
                            blockx = blocky = 0;
                        }
                        newtile = new StaticTile();
                        newtile.m_ID = 0xFFFF;
                        newtile.m_Hue = 0;
                    }
                    else if (line.StartsWith("ID"))
                    {
                        line = line.Remove(0, 2);
                        line = line.TrimStart(' ');
                        line = line.TrimEnd(' ');
                        newtile.m_ID = Art.GetLegalItemID(Convert.ToUInt16(line));
                    }
                    else if (line.StartsWith("X"))
                    {
                        line = line.Remove(0, 1);
                        line = line.TrimStart(' ');
                        line = line.TrimEnd(' ');
                        x = Convert.ToInt32(line);
                        blockx = x >> 3;
                        x &= 0x7;
                        newtile.m_X = (byte)x;
                    }
                    else if (line.StartsWith("Y"))
                    {
                        line = line.Remove(0, 1);
                        line = line.TrimStart(' ');
                        line = line.TrimEnd(' ');
                        y = Convert.ToInt32(line);
                        blocky = y >> 3;
                        y &= 0x7;
                        newtile.m_Y = (byte)y;
                    }
                    else if (line.StartsWith("Z"))
                    {
                        line = line.Remove(0, 1);
                        line = line.TrimStart(' ');
                        line = line.TrimEnd(' ');
                        newtile.m_Z = Convert.ToSByte(line);
                    }
                    else if (line.StartsWith("COLOR"))
                    {
                        line = line.Remove(0, 5);
                        line = line.TrimStart(' ');
                        line = line.TrimEnd(' ');
                        newtile.m_Hue = Convert.ToInt16(line);
                    }
                }
                catch { }
            }
            if (newtile.m_ID != 0xFFFF)
                currmap.Tiles.AddPendingStatic(blockx, blocky, newtile);

            ip.Close();

            MessageBox.Show("Done", "Freeze Static", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            currmap.ResetCache();
            pictureBox.Invalidate();
        }

        MapMeltStatics showmeltstatics = null;
        private void OnClickMeltStatics(object sender, EventArgs e)
        {
            if ((showmeltstatics == null) || (showmeltstatics.IsDisposed))
            {
                showmeltstatics = new MapMeltStatics(this, currmap);
                showmeltstatics.TopMost = true;
                showmeltstatics.Show();
            }
        }

        MapClearStatics showclearstatics = null;
        private void OnClickClearStatics(object sender, EventArgs e)
        {
            if ((showclearstatics == null) || (showclearstatics.IsDisposed))
            {
                showclearstatics = new MapClearStatics(this, currmap);
                showclearstatics.TopMost = true;
                showclearstatics.Show();
            }
        }

        MapReplaceTiles showmapreplacetiles = null;
        private void OnClickReplaceTiles(object sender, EventArgs e)
        {
            if ((showmapreplacetiles == null) || (showmapreplacetiles.IsDisposed))
            {
                showmapreplacetiles = new MapReplaceTiles(currmap);
                showmapreplacetiles.TopMost = true;
                showmapreplacetiles.Show();
            }
        }

    }

    public class OverlayObject
    {
        public virtual bool isVisible(Rectangle bounds, int m) { return false; }
        public virtual void Draw(Graphics g) { }
        public virtual void Save(XmlElement elem) { }
        public override string ToString() { return ""; }

        public bool Visible { get; set; }
        public Point Loc { get; set; }
        public int DefMap { get; set; }
    }

    public class OverlayCursor : OverlayObject
    {
        private string text;
        private Color col;
        private Brush brush;
        private Pen pen;
        private static Brush background;
        public OverlayCursor(Point location, int m, string t, Color c)
        {
            Loc = location;
            DefMap = m;
            text = t;
            col = c;
            Visible = true;
            brush = new SolidBrush(col);
            pen = new Pen(brush);
            background = new SolidBrush(Color.FromArgb(100, Color.White));
        }
        public override bool isVisible(Rectangle bounds, int m)
        {
            if (!Visible)
                return false;
            if (DefMap != m)
                return false;
            if ((Loc.X > Map.HScrollBar) &&
                (Loc.X < Map.HScrollBar + bounds.Width / Map.Zoom) &&
                (Loc.Y > Map.VScrollBar) &&
                (Loc.Y < Map.VScrollBar + bounds.Height / Map.Zoom))
                return true;
            return false;
        }

        public override void Draw(Graphics g)
        {
            int x = (int)((Loc.X - Map.Round(Map.HScrollBar)) * Map.Zoom);
            int y = (int)((Loc.Y - Map.Round(Map.VScrollBar)) * Map.Zoom);
            g.DrawLine(pen, x - 4, y, x + 4, y);
            g.DrawLine(pen, x, y - 4, x, y + 4);
            g.DrawEllipse(pen, x - 2, y - 2, 2 * 2, 2 * 2);
            SizeF tSize = g.MeasureString(text, Fonts.DefaultFont);
            int x_;
            if ((Loc.X + tSize.Width) > Map.CurrMap.Width)
                x_ = x - (int)tSize.Width - 6;
            else
                x_ = x + 6;
            g.FillRectangle(background, x_, y - tSize.Height, tSize.Width, tSize.Height);
            g.DrawString(text, Fonts.DefaultFont, Brushes.Black, x_, y - tSize.Height);
        }

        public override void Save(XmlElement elem)
        {
            elem.SetAttribute("x", Loc.X.ToString());
            elem.SetAttribute("y", Loc.Y.ToString());
            elem.SetAttribute("map", DefMap.ToString());
            elem.SetAttribute("color", col.ToArgb().ToString());
            elem.SetAttribute("text", text);
        }

        public override string ToString()
        {
            return text;
        }
    }
}
