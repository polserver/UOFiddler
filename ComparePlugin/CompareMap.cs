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
using System.IO;
using System.Windows.Forms;

namespace ComparePlugin
{
    public partial class CompareMap : UserControl
    {
        public CompareMap()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        bool Loaded = false;
        bool moving = false;
        Point movingpoint = new Point();
        private Point currPoint;
        private Ultima.Map currmap;
        private Ultima.Map origmap;
        private int currmapint;
        private Bitmap map;
        public static double Zoom = 1;
        private bool[][][][] diffs;

        private void OnLoad(object sender, EventArgs e)
        {
            currmap = Ultima.Map.Custom;
            origmap = Ultima.Map.Felucca;
            feluccaToolStripMenuItem.Checked = true;
            trammelToolStripMenuItem.Checked = false;
            ilshenarToolStripMenuItem.Checked = false;
            malasToolStripMenuItem.Checked = false;
            tokunoToolStripMenuItem.Checked = false;
            terMurToolStripMenuItem.Checked = false;
            showDifferencesToolStripMenuItem.Checked = true;
            showMap1ToolStripMenuItem.Checked = true;
            showMap2ToolStripMenuItem.Checked = false;
            SetScrollBarValues();
            ChangeMapNames();
            ZoomLabel.Text = String.Format("Zoom: {0}", Zoom);

            FiddlerControls.Options.LoadedUltimaClass["Map"] = true;
            FiddlerControls.Options.LoadedUltimaClass["RadarColor"] = true;

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
            CalculateDiffs();
            pictureBox.Invalidate();
        }

        private void OnMapNameChangeEvent()
        {
            ChangeMapNames();
        }

        private void OnMapSizeChangeEvent()
        {
            SetScrollBarValues();
            if (currmap != null)
                ChangeMap();
            pictureBox.Invalidate();
        }

        private void OnFilePathChangeEvent()
        {
            SetScrollBarValues();
            if (currmap != null)
                ChangeMap();
            pictureBox.Invalidate();
        }

        private void ChangeMapNames()
        {
            if (!Loaded)
                return;
            feluccaToolStripMenuItem.Text = FiddlerControls.Options.MapNames[0];
            trammelToolStripMenuItem.Text = FiddlerControls.Options.MapNames[1];
            ilshenarToolStripMenuItem.Text = FiddlerControls.Options.MapNames[2];
            malasToolStripMenuItem.Text = FiddlerControls.Options.MapNames[3];
            tokunoToolStripMenuItem.Text = FiddlerControls.Options.MapNames[4];
            terMurToolStripMenuItem.Text = FiddlerControls.Options.MapNames[5];
        }
        public static int Round(int x)
        {
            return (int)((x >> 3) << 3);
        }

        private void onMouseDown(object sender, MouseEventArgs e)
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

        private void onMouseMove(object sender, MouseEventArgs e)
        {
            int xDelta = Math.Min(origmap.Width, (int)(e.X / Zoom) + Round(hScrollBar.Value));
            int yDelta = Math.Min(origmap.Height, (int)(e.Y / Zoom) + Round(vScrollBar.Value));
            CoordsLabel.Text = String.Format("Coords: {0},{1}", xDelta, yDelta);
            string diff = "";
            if (moving)
            {
                toolTip1.RemoveAll();
                int deltax = (int)(-1 * (e.X - movingpoint.X) / Zoom);
                int deltay = (int)(-1 * (e.Y - movingpoint.Y) / Zoom);
                movingpoint.X = e.X;
                movingpoint.Y = e.Y;
                hScrollBar.Value = Math.Max(0, Math.Min(hScrollBar.Maximum, hScrollBar.Value + deltax));
                vScrollBar.Value = Math.Max(0, Math.Min(vScrollBar.Maximum, vScrollBar.Value + deltay));
                pictureBox.Invalidate();
            }
            else if ((Zoom >= 2) && (currmap != null))
            {
                if (BlockDiff(xDelta >> 3, yDelta >> 3))
                {

                    Ultima.Tile customTile = currmap.Tiles.GetLandTile(xDelta, yDelta);
                    Ultima.Tile origTile = origmap.Tiles.GetLandTile(xDelta, yDelta);
                    if ((customTile.ID != origTile.ID) || (customTile.Z != origTile.Z))
                        diff = String.Format("Tile:\n\r0x{0:X} {1} -> 0x{2:X} {3}\n\r", origTile.ID, origTile.Z, customTile.ID, customTile.Z);
                    Ultima.HuedTile[] customStatics = currmap.Tiles.GetStaticTiles(xDelta, yDelta);
                    Ultima.HuedTile[] origStatics = origmap.Tiles.GetStaticTiles(xDelta, yDelta);
                    if (customStatics.Length != origStatics.Length)
                    {
                        diff += "Statics:\n\rorig:\n\r";
                        foreach (Ultima.HuedTile tile in origStatics)
                        {
                            diff += String.Format("0x{0:X} {1} {2}\n\r", tile.ID, tile.Z, tile.Hue);
                        }
                        diff += "new:\n\r";
                        foreach (Ultima.HuedTile tile in customStatics)
                        {
                            diff += String.Format("0x{0:X} {1} {2}\n\r", tile.ID, tile.Z, tile.Hue);
                        }
                    }
                    else
                    {
                        bool changed = false;
                        for (int i = 0; i < customStatics.Length; i++)
                        {
                            if ((customStatics[i].ID != origStatics[i].ID)
                                || (customStatics[i].Z != origStatics[i].Z)
                                || (customStatics[i].Hue != origStatics[i].Hue))
                            {
                                if (!changed)
                                {
                                    diff += "Statics diff:\n\r";
                                    changed = true;
                                }
                                diff += String.Format("0x{0:X} {1} {2} -> 0x{3:X} {4} {5}\n\r",
                                    origStatics[i].ID, origStatics[i].Z, origStatics[i].Hue,
                                    customStatics[i].ID, customStatics[i].Z, customStatics[i].Hue);
                            }
                        }
                    }
                }
                toolTip1.SetToolTip(pictureBox, diff);
                pictureBox.Invalidate();
            }

            if ((Zoom >= 2) && (markDiffToolStripMenuItem.Checked) && (String.IsNullOrEmpty(diff)))
            {
                Ultima.Map drawmap;
                if (showMap1ToolStripMenuItem.Checked)
                    drawmap = origmap;
                else
                    drawmap = currmap;
                if (drawmap.Tiles.Patch.LandBlocksCount > 0)
                {
                    if (drawmap.Tiles.Patch.IsLandBlockPatched(xDelta >> 3, yDelta >> 3))
                    {
                        Ultima.Tile patchTile = drawmap.Tiles.Patch.GetLandTile(xDelta, yDelta);
                        Ultima.Tile origTile = drawmap.Tiles.GetLandTile(xDelta, yDelta, false);
                        diff = String.Format("Tile:\n\r0x{0:X} {1} -> 0x{2:X} {3}\n\r", origTile.ID, origTile.Z, patchTile.ID, patchTile.Z);
                    }
                }
                if (drawmap.Tiles.Patch.StaticBlocksCount > 0)
                {
                    if (drawmap.Tiles.Patch.IsStaticBlockPatched(xDelta >> 3, yDelta >> 3))
                    {
                        Ultima.HuedTile[] patchStatics = drawmap.Tiles.Patch.GetStaticTiles(xDelta, yDelta);
                        Ultima.HuedTile[] origStatics = drawmap.Tiles.GetStaticTiles(xDelta, yDelta, false);
                        diff += "Statics:\n\rorig:\n\r";
                        foreach (Ultima.HuedTile tile in origStatics)
                        {
                            diff += String.Format("0x{0:X} {1} {2}\n\r", tile.ID, tile.Z, tile.Hue);
                        }
                        diff += "patch:\n\r";
                        foreach (Ultima.HuedTile tile in patchStatics)
                        {
                            diff += String.Format("0x{0:X} {1} {2}\n\r", tile.ID, tile.Z, tile.Hue);
                        }
                    }
                }
                toolTip1.SetToolTip(pictureBox, diff);
                pictureBox.Invalidate();
            }
        }

        private void onMouseUp(object sender, MouseEventArgs e)
        {
            moving = false;
            this.Cursor = Cursors.Default;
        }
        static Pen redpen = Pens.Red;
        private void onPaint(object sender, PaintEventArgs e)
        {

            if (!Loaded)
                return;
            if (showMap1ToolStripMenuItem.Checked)
                map = origmap.GetImage(hScrollBar.Value >> 3, vScrollBar.Value >> 3,
                    (int)((e.ClipRectangle.Width / Zoom) + 8) >> 3, (int)((e.ClipRectangle.Height / Zoom) + 8) >> 3,
                    true);
            else
                map = currmap.GetImage(hScrollBar.Value >> 3, vScrollBar.Value >> 3,
                    (int)((e.ClipRectangle.Width / Zoom) + 8) >> 3, (int)((e.ClipRectangle.Height / Zoom) + 8) >> 3,
                    true);

            if (currmap != null)
            {
                if (showDifferencesToolStripMenuItem.Checked)
                {
                    using (Graphics mapg = Graphics.FromImage(map))
                    {
                        int maxx = ((int)((e.ClipRectangle.Width / Zoom) + 8) >> 3) + (hScrollBar.Value >> 3);
                        int maxy = ((int)((e.ClipRectangle.Height / Zoom) + 8) >> 3) + (vScrollBar.Value >> 3);
                        if (maxx > (origmap.Width >> 3))
                            maxx = (origmap.Width >> 3);
                        if (maxy > (origmap.Height >> 3))
                            maxy = (origmap.Height >> 3);
                        int gx = 0, gy = 0;
                        for (int x = (hScrollBar.Value >> 3); x < maxx; x++, gx += 8)
                        {
                            gy = 0;
                            for (int y = (vScrollBar.Value >> 3); y < maxy; y++, gy += 8)
                            {
                                for (int xb = 0; xb < 8; xb++)
                                {
                                    for (int yb = 0; yb < 8; yb++)
                                    {
                                        if (diffs[x][y][xb][yb])
                                        {
                                            mapg.DrawRectangle(redpen, (gx + xb), (gy + yb), 1, 1);
                                            mapg.DrawRectangle(redpen, (gx + xb), 0, 1, 2);
                                            mapg.DrawRectangle(redpen, 0, (gy + yb), 2, 1);
                                        }
                                    }
                                }
                            }
                        }
                        mapg.Save();
                    }
                }
            }

            if (markDiffToolStripMenuItem.Checked)
            {
                Ultima.Map drawmap;
                if (showMap1ToolStripMenuItem.Checked)
                    drawmap = origmap;
                else
                    drawmap = currmap;
                int count = drawmap.Tiles.Patch.LandBlocksCount + drawmap.Tiles.Patch.StaticBlocksCount;
                if (count > 0)
                {
                    using (Graphics mapg = Graphics.FromImage(map))
                    {
                        int maxx = ((int)((e.ClipRectangle.Width / Zoom) + 8) >> 3) + (hScrollBar.Value >> 3);
                        int maxy = ((int)((e.ClipRectangle.Height / Zoom) + 8) >> 3) + (vScrollBar.Value >> 3);
                        if (maxx > drawmap.Width >> 3)
                            maxx = drawmap.Width >> 3;
                        if (maxy > drawmap.Height >> 3)
                            maxy = drawmap.Height >> 3;

                        int gx = 0, gy = 0;
                        for (int x = (hScrollBar.Value >> 3); x < maxx; x++, gx += 8)
                        {
                            gy = 0;
                            for (int y = (vScrollBar.Value >> 3); y < maxy; y++, gy += 8)
                            {
                                if (drawmap.Tiles.Patch.IsLandBlockPatched(x, y))
                                {
                                    mapg.FillRectangle(Brushes.Azure, gx, gy, 8, 8);
                                    mapg.FillRectangle(Brushes.Azure, gx, 0, 8, 2);
                                    mapg.FillRectangle(Brushes.Azure, 0, gy, 2, 8);
                                }
                                if (drawmap.Tiles.Patch.IsStaticBlockPatched(x, y))
                                {
                                    mapg.FillRectangle(Brushes.Azure, gx, gy, 8, 8);
                                    mapg.FillRectangle(Brushes.Azure, gx, 0, 8, 2);
                                    mapg.FillRectangle(Brushes.Azure, 0, gy, 2, 8);
                                }
                            }
                        }
                    }
                }
            }
            ZoomMap(ref map);
            e.Graphics.DrawImageUnscaledAndClipped(map, e.ClipRectangle);
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

        private void OnResize(object sender, EventArgs e)
        {
            if (Loaded)
            {
                ChangeScrollBar();
                pictureBox.Invalidate();
            }
        }

        private void ChangeScrollBar()
        {
            hScrollBar.Maximum = (int)(origmap.Width);
            hScrollBar.Maximum -= Round((int)(pictureBox.ClientSize.Width / Zoom) - 8);
            if (Zoom >= 1)
                hScrollBar.Maximum += (int)(40 * Zoom);
            else if (Zoom < 1)
                hScrollBar.Maximum += (int)(40 / Zoom);
            hScrollBar.Maximum = Math.Max(0, Round(hScrollBar.Maximum));
            vScrollBar.Maximum = (int)(origmap.Height);
            vScrollBar.Maximum -= Round((int)(pictureBox.ClientSize.Height / Zoom) - 8);
            if (Zoom >= 1)
                vScrollBar.Maximum += (int)(40 * Zoom);
            else if (Zoom < 1)
                vScrollBar.Maximum += (int)(40 / Zoom);
            vScrollBar.Maximum = Math.Max(0, Round(vScrollBar.Maximum));
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

        private void onZoomPlus(object sender, EventArgs e)
        {
            Zoom *= 2;
            DoZoom();
        }

        private void OnZoomMinus(object sender, EventArgs e)
        {
            Zoom /= 2;
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

        private void OnOpeningContext(object sender, CancelEventArgs e)
        {
            currPoint = pictureBox.PointToClient(Control.MousePosition);
            currPoint.X = (int)(currPoint.X / Zoom);
            currPoint.Y = (int)(currPoint.Y / Zoom);
            currPoint.X += hScrollBar.Value;
            currPoint.Y += vScrollBar.Value;
        }

        private void OnClickBrowseLoc(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory containing the map files";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                    toolStripTextBox1.Text = dialog.SelectedPath;
            }
        }

        private void OnClickLoad(object sender, EventArgs e)
        {
            ChangeMap();
        }

        private void ChangeMap()
        {
            SetScrollBarValues();
            string path = toolStripTextBox1.Text;
            if (Directory.Exists(path))
                currmap = Ultima.Map.Custom = new Ultima.Map(path, origmap.FileIndex, currmapint, origmap.Width, origmap.Height);

            CalculateDiffs();
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

        private void OnClickChangeFelucca(object sender, EventArgs e)
        {
            if (!feluccaToolStripMenuItem.Checked)
            {
                ResetCheckedMap();
                feluccaToolStripMenuItem.Checked = true;
                origmap = Ultima.Map.Felucca;
                currmapint = 0;
                ChangeMap();
            }
        }

        private void OnClickChangeTrammel(object sender, EventArgs e)
        {
            if (!trammelToolStripMenuItem.Checked)
            {
                ResetCheckedMap();
                trammelToolStripMenuItem.Checked = true;
                origmap = Ultima.Map.Trammel;
                currmapint = 1;
                ChangeMap();
            }
        }

        private void OnClickChangeIlshenar(object sender, EventArgs e)
        {
            if (!ilshenarToolStripMenuItem.Checked)
            {
                ResetCheckedMap();
                ilshenarToolStripMenuItem.Checked = true;
                origmap = Ultima.Map.Ilshenar;
                currmapint = 2;
                ChangeMap();
            }
        }

        private void OnClickChangeMalas(object sender, EventArgs e)
        {
            if (!malasToolStripMenuItem.Checked)
            {
                ResetCheckedMap();
                malasToolStripMenuItem.Checked = true;
                origmap = Ultima.Map.Malas;
                currmapint = 3;
                ChangeMap();
            }
        }

        private void OnClickChangeTokuno(object sender, EventArgs e)
        {
            if (!tokunoToolStripMenuItem.Checked)
            {
                ResetCheckedMap();
                tokunoToolStripMenuItem.Checked = true;
                origmap = Ultima.Map.Tokuno;
                currmapint = 4;
                ChangeMap();
            }
        }

        private void OnClickChangeTerMur(object sender, EventArgs e)
        {
            if (!terMurToolStripMenuItem.Checked)
            {
                ResetCheckedMap();
                terMurToolStripMenuItem.Checked = true;
                origmap = Ultima.Map.TerMur;
                currmapint = 5;
                ChangeMap();
            }
        }

        private void OnClickShowDiff(object sender, EventArgs e)
        {
            pictureBox.Invalidate();
        }

        private void OnClickShowMap2(object sender, EventArgs e)
        {
            if (!showMap2ToolStripMenuItem.Checked)
            {
                if (currmap != null)
                {
                    showMap1ToolStripMenuItem.Checked = false;
                    showMap2ToolStripMenuItem.Checked = true;
                    pictureBox.Invalidate();
                }
            }
        }

        private void OnClickShowMap1(object sender, EventArgs e)
        {
            if (!showMap1ToolStripMenuItem.Checked)
            {
                showMap2ToolStripMenuItem.Checked = false;
                showMap1ToolStripMenuItem.Checked = true;
                pictureBox.Invalidate();
            }
        }

        private void OnClickMarkDiff(object sender, EventArgs e)
        {
            pictureBox.Invalidate();
        }

        private bool BlockDiff(int x, int y)
        {
            if (diffs == null)
                return false;
            if (x < 0 || y < 0 || x >= diffs.GetLength(0) || y >= diffs[x].GetLength(0))
                return false;
            for (int xb = 0; xb < 8; xb++)
            {
                for (int yb = 0; yb < 8; yb++)
                {
                    if (diffs[x][y][xb][yb])
                        return true;
                }
            }
            return false;

        }

        private void CalculateDiffs()
        {
            int width = currmap.Width >> 3;
            int height = currmap.Height >> 3;
            diffs = new bool[width][][][];
            if (currmap == null || origmap == null)
                return;
            Cursor.Current = Cursors.WaitCursor;
            for (int x = 0; x < width; ++x)
            {
                diffs[x] = new bool[height][][];
                for (int y = 0; y < height; ++y)
                {
                    diffs[x][y] = new bool[8][];
                    Ultima.Tile[] customTiles = currmap.Tiles.GetLandBlock(x, y);
                    Ultima.Tile[] origTiles = origmap.Tiles.GetLandBlock(x, y);
                    Ultima.HuedTile[][][] customStatics = currmap.Tiles.GetStaticBlock(x, y);
                    Ultima.HuedTile[][][] origStatics = origmap.Tiles.GetStaticBlock(x, y);
                    for (int xb = 0; xb < 8; xb++)
                    {
                        diffs[x][y][xb] = new bool[8];
                        for (int yb = 0; yb < 8; yb++)
                        {
                            if ((customTiles[((yb & 0x7) << 3) + (xb & 0x7)].ID != origTiles[((yb & 0x7) << 3) + (xb & 0x7)].ID)
                             || (customTiles[((yb & 0x7) << 3) + (xb & 0x7)].Z != origTiles[((yb & 0x7) << 3) + (xb & 0x7)].Z))
                            {
                                diffs[x][y][xb][yb] = true;
                            }
                            else
                            {
                                if (customStatics[xb][yb].Length != origStatics[xb][yb].Length)
                                {
                                    diffs[x][y][xb][yb] = true;
                                }
                                else
                                {
                                    for (int i = 0; i < customStatics[xb][yb].Length; i++)
                                    {
                                        if ((customStatics[xb][yb][i].ID != origStatics[xb][yb][i].ID)
                                            || (customStatics[xb][yb][i].Z != origStatics[xb][yb][i].Z)
                                            || (customStatics[xb][yb][i].Hue != origStatics[xb][yb][i].Hue))
                                        {
                                            diffs[x][y][xb][yb] = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Cursor.Current = Cursors.Default;
        }

        private void HandleScroll(object sender, ScrollEventArgs e)
        {
            pictureBox.Invalidate();
        }
    }
}
