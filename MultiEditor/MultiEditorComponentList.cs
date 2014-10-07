/***************************************************************************
 *
 * $Author: MuadDib & Turley
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
using Ultima;

namespace MultiEditor
{
    class MultiEditorComponentList
    {
        #region Fields (6)

        public const int GapXMod = 44;
        public const int GapYMod = 22;
        private bool Modified;
        private static MultiEditor Parent;
        public const int UndoListMaxSize = 10;
        public int WalkableCount = 0;
        public int DoubleSurfaceCount = 0;
        private static Rectangle drawdestRectangle = new Rectangle();

        #endregion Fields

        #region Constructors (2)

        /// <summary>
        /// Create a blank ComponentList
        /// </summary>
        public MultiEditorComponentList(int width, int height, MultiEditor parent)
        {
            Parent = parent;
            Width = width;
            Height = height;
            Tiles = new List<MultiTile>();
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    Tiles.Add(new FloorTile(x, y, Parent.DrawFloorZ));
                }
            }
            UndoList = new UndoStruct[UndoListMaxSize];
            Modified = true;
            RecalcMinMax();
        }

        /// <summary>
        /// Create a ComponentList from UltimaSDK
        /// </summary>
        public MultiEditorComponentList(MultiComponentList list, MultiEditor parent)
        {
            Parent = parent;
            Width = list.Width;
            Height = list.Height;
            Tiles = new List<MultiTile>();
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    for (int i = 0; i < list.Tiles[x][y].Length; ++i)
                    {
                        Tiles.Add(new MultiTile(list.Tiles[x][y][i].ID, x, y, list.Tiles[x][y][i].Z, list.Tiles[x][y][i].Flag));
                    }
                    Tiles.Add(new FloorTile(x, y, Parent.DrawFloorZ));
                }
            }
            CalcSolver();
            Tiles.Sort();
            UndoList = new UndoStruct[UndoListMaxSize];
            Modified = true;
            RecalcMinMax();
        }

        #endregion Constructors

        #region Properties (14)

        public int Height { get; private set; }

        public List<MultiTile> Tiles { get; private set; }

        public UndoStruct[] UndoList { get; private set; }

        public int Width { get; private set; }

        public int xMax { get; private set; }

        public int xMaxOrg { get; private set; }

        public int xMin { get; private set; }

        public int xMinOrg { get; private set; }

        public int yMax { get; private set; }

        public int yMaxOrg { get; private set; }

        public int yMin { get; private set; }

        public int yMinOrg { get; private set; }

        public int zMax { get; private set; }

        public int zMin { get; private set; }

        #endregion Properties

        #region Methods (16)

        // Public Methods (13) 

        /// <summary>
        /// Export to given multi id
        /// </summary>
        public void AddToSDKComponentList(int id)
        {
            Ultima.Multis.Add(id, ConvertToSDK());
            FiddlerControls.Options.ChangedUltimaClass["Multis"] = true;
            FiddlerControls.Events.FireMultiChangeEvent(this, id);
        }

        public MultiComponentList ConvertToSDK()
        {
            int count = 0;
            MTileList[][] tiles = new MTileList[Width][];
            for (int x = 0; x < Width; ++x)
            {
                tiles[x] = new MTileList[Height];
                for (int y = 0; y < Height; ++y)
                {
                    tiles[x][y] = new MTileList();
                }
            }
            for (int i = 0; i < Tiles.Count; ++i)
            {
                MultiTile tile = Tiles[i];
                if (tile.isVirtualFloor)
                    continue;
                tiles[tile.X][tile.Y].Add((ushort)(tile.ID), (sbyte)tile.Z, tile.Invisible ? (sbyte)0 : (sbyte)1, 0);
                ++count;
            }
            return new MultiComponentList(tiles, count, Width, Height);
        }

        /// <summary>
        /// Gets Bitmap of Multi and sets HoverTile
        /// </summary>
        public void GetImage(Graphics gfx, int xoff, int yoff, int maxheight, Point mouseLoc, bool drawFloor, bool forcerefresh)
        {
            if (Width == 0 || Height == 0)
                return;

            if (Modified)
                RecalcMinMax();

            xMin = xMinOrg;
            xMax = xMaxOrg;
            yMin = yMinOrg;
            yMax = yMaxOrg;

            if (drawFloor)
            {
                int floorzmod = -Parent.DrawFloorZ << 2 - 44;
                if (yMin > floorzmod)
                    yMin = floorzmod;
                floorzmod = (Width + Height) * 22 - Parent.DrawFloorZ << 2;
                if (yMaxOrg < floorzmod)
                    yMax = floorzmod;
            }
            Parent.HoverTile = GetSelected(mouseLoc, maxheight, drawFloor);

            Rectangle clipBounds = Rectangle.Round(gfx.ClipBounds);
            for (int i = 0; i < Tiles.Count; ++i)
            {
                MultiTile tile = Tiles[i];
                if (!tile.isVirtualFloor)
                    if (tile.Z > maxheight)
                        continue;
                Bitmap bmp = tile.GetBitmap();
                if (bmp == null)
                    continue;
                drawdestRectangle.X = tile.Xmod;
                drawdestRectangle.Y = tile.Ymod;
                drawdestRectangle.X -= xMin + xoff;
                drawdestRectangle.Y -= yMin + yoff;
                drawdestRectangle.Width = bmp.Width;
                drawdestRectangle.Height = bmp.Height;

                if (!clipBounds.IntersectsWith(drawdestRectangle))
                    continue;
                if (tile.isVirtualFloor)
                {
                    if (drawFloor)
                        gfx.DrawImageUnscaled(bmp, drawdestRectangle);
                }
                else
                {
                    if ((Parent.HoverTile != null) && (Parent.HoverTile == tile))
                        gfx.DrawImage(bmp, drawdestRectangle, 0, 0, drawdestRectangle.Width, drawdestRectangle.Height, GraphicsUnit.Pixel, MultiTile.HoverColor);
                    else if ((Parent.SelectedTile != null) && (Parent.SelectedTile == tile))
                        gfx.DrawImage(bmp, drawdestRectangle, 0, 0, drawdestRectangle.Width, drawdestRectangle.Height, GraphicsUnit.Pixel, MultiTile.SelectedColor);
                    else if (tile.Transparent)
                        gfx.DrawImage(bmp, drawdestRectangle, 0, 0, drawdestRectangle.Width, drawdestRectangle.Height, GraphicsUnit.Pixel, MultiTile.TransColor);
                    else if ((Parent.ShowWalkables) && (tile.Walkable))
                        gfx.DrawImage(bmp, drawdestRectangle, 0, 0, drawdestRectangle.Width, drawdestRectangle.Height, GraphicsUnit.Pixel, MultiTile.StandableColor);
                    else if ((Parent.ShowDoubleSurface) && (tile.DoubleSurface))
                        gfx.DrawImage(bmp, drawdestRectangle, 0, 0, drawdestRectangle.Width, drawdestRectangle.Height, GraphicsUnit.Pixel, MultiTile.StandableColor);
                    else
                        gfx.DrawImageUnscaled(bmp, drawdestRectangle);
                }
            }
        }

        /// <summary>
        /// Gets <see cref="MultiTile"/> from given Pixel Location
        /// </summary>
        public MultiTile GetSelected(Point mouseLoc, int maxheight, bool drawFloor)
        {
            if (Width == 0 || Height == 0)
                return null;
            MultiTile selected = null;
            if (mouseLoc != Point.Empty)
            {
                for (int i = Tiles.Count - 1; i >= 0; --i) // inverse for speedup
                {
                    MultiTile tile = Tiles[i];
                    if (tile.isVirtualFloor)
                        continue;
                    if (tile.Z > maxheight)
                        continue;
                    if ((drawFloor) && (Parent.DrawFloorZ > tile.Z))
                        continue;
                    Bitmap bmp = tile.GetBitmap();
                    if (bmp == null)
                        continue;
                    int px = tile.Xmod;
                    int py = tile.Ymod;
                    px -= xMin;
                    py -= yMin;
                    if (((mouseLoc.X > px) && (mouseLoc.X < (px + bmp.Width))) &&
                        ((mouseLoc.Y > py) && (mouseLoc.Y < (py + bmp.Height))))
                    {
                        //Check for transparent part
                        Color p = bmp.GetPixel(mouseLoc.X - px, mouseLoc.Y - py);
                        if (!((p.R == 0) && (p.G == 0) && (p.B == 0)))
                        {
                            selected = tile;
                            break;
                        }
                    }
                }
            }
            return selected;
        }

        /// <summary>
        /// Resizes Multi size
        /// </summary>
        public void Resize(int width, int height)
        {
            AddToUndoList("Resize");
            if ((Width > width) || (Height > height))
            {
                for (int i = Tiles.Count - 1; i >= 0; --i)
                {
                    MultiTile tile = Tiles[i];
                    if (tile.X >= width)
                        Tiles.RemoveAt(i);
                    else if (tile.Y >= height)
                        Tiles.RemoveAt(i);
                }
            }

            if ((Width < width) || (Height < height))
            {
                for (int x = 0; x < width; ++x)
                {
                    for (int y = 0; y < height; ++y)
                    {
                        if ((x < Width) && (y < Height))
                            continue;
                        Tiles.Add(new FloorTile(x, y, Parent.DrawFloorZ));
                    }
                }
            }
            Width = width;
            Height = height;
            Modified = true;
            RecalcMinMax();
        }

        public void SetFloorZ(int Z)
        {
            foreach (MultiTile tile in Tiles)
            {
                if (tile.isVirtualFloor)
                    tile.Z = Z;
            }
            Tiles.Sort();
        }

        /// <summary>
        /// Adds an <see cref="MultiTile"/> to specific location
        /// </summary>
        public void TileAdd(int x, int y, int z, ushort id)
        {
            if ((x > Width) || (y > Height))
                return;
            AddToUndoList("Add Tile");
            MultiTile tile = new MultiTile(id, x, y, z, 1);
            Tiles.Add(tile);
            CalcSolver(x, y);
            Modified = true;
            Tiles.Sort();
            RecalcMinMax(tile);
        }

        /// <summary>
        /// Moves given <see cref="MultiTile"/>
        /// </summary>
        public void TileMove(MultiTile tile, int newx, int newy)
        {
            if (Width == 0 || Height == 0)
                return;
            AddToUndoList("Move Tile");
            if (tile != null)
            {
                tile.X = newx;
                tile.Y = newy;
                CalcSolver(newx, newy);
                Tiles.Sort();
                Modified = true;
                RecalcMinMax(tile);
            }
        }

        /// <summary>
        /// Removes specific <see cref="MultiTile"/>
        /// </summary>
        public void TileRemove(MultiTile tile)
        {
            if (Width == 0 || Height == 0)
                return;
            AddToUndoList("Remove Tile");
            if (tile != null)
            {
                Tiles.Remove(tile);
                Modified = true;
                RecalcMinMax(tile);
            }
        }

        /// <summary>
        /// Alters Z level for given <see cref="MultiTile"/>
        /// </summary>
        public void TileZMod(MultiTile tile, int modz)
        {
            AddToUndoList("Modify Z");
            tile.Z += modz;
            if (tile.Z > 127)
                tile.Z = 127;
            if (tile.Z < -128)
                tile.Z = -128;
            CalcSolver(tile.X, tile.Y);
            Tiles.Sort();
            Modified = true;
            RecalcMinMax(tile);
        }

        /// <summary>
        /// Sets Z value of given <see cref="MultiTile"/>
        /// </summary>
        public void TileZSet(MultiTile tile, int setz)
        {
            AddToUndoList("Set Z");
            tile.Z = setz;
            if (tile.Z > 127)
                tile.Z = 127;
            if (tile.Z < -128)
                tile.Z = -128;
            CalcSolver(tile.X, tile.Y);
            Tiles.Sort();
            Modified = true;
            RecalcMinMax(tile);
        }

        public void Undo(int index)
        {
            if (UndoList[index].Tiles != null)
            {
                Width = UndoList[index].Width;
                Height = UndoList[index].Height;
                Tiles = new List<MultiTile>();
                foreach (MultiTile tile in UndoList[index].Tiles)
                {
                    if (tile.isVirtualFloor)
                        Tiles.Add(new FloorTile(tile.X, tile.Y, tile.Z));
                    else
                        Tiles.Add(new MultiTile(tile.ID, tile.X, tile.Y, tile.Z, tile.Invisible));
                }
                Modified = true;
                RecalcMinMax();
            }
        }

        public void UndoClear()
        {
            for (int i = 0; i < UndoListMaxSize; ++i)
            {
                UndoList[i].Action = null;
                UndoList[i].Tiles = null;
            }
        }

        public void CalcWalkable()
        {
            int lastx = -1;
            int lasty = -1;
            List<MultiTile> xyarr = new List<MultiTile>();
            WalkableCount = 0;
            foreach (MultiTile tile in Tiles)
            {
                if (tile.isVirtualFloor)
                    continue;
                ItemData itemdata = TileData.ItemTable[tile.ID];
                if ((itemdata.Flags & TileFlag.Surface) != 0)
                {
                    if (tile.X != lastx && tile.Y != lasty)
                        xyarr = GetXYArray(tile.X, tile.Y);

                    int Start = tile.Z + itemdata.CalcHeight;
                    if (tile.IsWalkable(Start, xyarr))
                        ++WalkableCount;
                }
            }
        }

        public void CalcDoubleSurface()
        {
            int lastx = -1;
            int lasty = -1;
            List<MultiTile> xyarr = new List<MultiTile>();
            DoubleSurfaceCount = 0;
            foreach (MultiTile tile in Tiles)
            {
                if (tile.isVirtualFloor)
                    continue;
                ItemData itemdata = TileData.ItemTable[tile.ID];
                if ((itemdata.Flags & TileFlag.Surface) != 0)
                {
                    if (tile.X != lastx && tile.Y != lasty)
                        xyarr = GetXYArray(tile.X, tile.Y);

                    if (tile.IsDoubleSurface(xyarr))
                        ++DoubleSurfaceCount;
                }
            }
        }

        public void CalcSolver()
        {
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                    CalcSolver(x, y);
            }
        }

        public void CalcSolver(int x, int y)
        {
            List<MultiTile> arr = GetXYArray(x, y);
            for (int i = 0; i < arr.Count; ++i)
                arr[i].Solver = i;
        }

        public List<MultiTile> GetXYArray(int x, int y)
        {
            List<MultiTile> arr = new List<MultiTile>();
            foreach (MultiTile tile in Tiles)
            {
                if (tile.isVirtualFloor)
                    continue;
                if (tile.X != x)
                    continue;
                if (tile.Y != y)
                    continue;
                arr.Add(tile);
            }
            return arr;
        }
        // Private Methods (3) 

        private void AddToUndoList(string Action)
        {
            for (int i = UndoListMaxSize - 2; i >= 0; --i)
            {
                UndoList[i + 1] = UndoList[i];
            }
            UndoList[0].Action = Action;
            UndoList[0].Tiles = new List<MultiTile>();
            UndoList[0].Width = Width;
            UndoList[0].Height = Height;
            foreach (MultiTile tile in Tiles)
            {
                if (tile.isVirtualFloor)
                    UndoList[0].Tiles.Add(new FloorTile(tile.X, tile.Y, tile.Y));
                else
                    UndoList[0].Tiles.Add(new MultiTile(tile.ID, tile.X, tile.Y, tile.Z, tile.Invisible));
            }
        }

        /// <summary>
        /// Recalcs Bitmap size
        /// </summary>
        private void RecalcMinMax()
        {
            //CalcEdgeTiles
            yMin = -44; // 0,0
            yMax = (Width + Height) * 22; // width,height
            xMin = -Height * 22 - 22; // 0,height
            xMax = Width * 22 + 22; // width,0
            zMin = 127;
            zMax = -128;
            foreach (MultiTile tile in Tiles)
            {
                if (tile.isVirtualFloor)
                    continue;
                if (tile.GetBitmap() == null)
                    continue;
                int px = tile.Xmod - GapXMod;
                int py = tile.Ymod - GapYMod;

                if (px < xMin)
                    xMin = px;
                if (py < yMin)
                    yMin = py;
                px += tile.GetBitmap().Width;
                py += tile.GetBitmap().Height;

                if (px > xMax)
                    xMax = px;
                if (py > yMax)
                    yMax = py;
                if (tile.Z > zMax)
                    zMax = tile.Z;
                if (tile.Z < zMin)
                    zMin = tile.Z;
            }

            Modified = false;
            xMinOrg = xMin;
            xMaxOrg = xMax;
            yMinOrg = yMin;
            yMaxOrg = yMax;

            if (Parent.ShowWalkables)
                CalcWalkable();
            if (Parent.ShowDoubleSurface)
                CalcDoubleSurface();
        }

        private void RecalcMinMax(MultiTile tile)
        {
            if (tile.isVirtualFloor)
                return;
            if (tile.GetBitmap() == null)
                return;
            int px = tile.Xmod - GapXMod;
            int py = tile.Ymod - GapYMod;

            if (px < xMin)
                xMin = px;
            if (py < yMin)
                yMin = py;
            px += tile.GetBitmap().Width;
            py += tile.GetBitmap().Height;

            if (px > xMax)
                xMax = px;
            if (py > yMax)
                yMax = py;
            if (tile.Z > zMax)
                zMax = tile.Z;
            if (tile.Z < zMin)
                zMin = tile.Z;
            Modified = false;
            xMinOrg = xMin;
            xMaxOrg = xMax;
            yMinOrg = yMin;
            yMaxOrg = yMax;

            if (Parent.ShowWalkables)
                CalcWalkable();
            if (Parent.ShowDoubleSurface)
                CalcDoubleSurface();
        }

        #endregion Methods

        public struct UndoStruct
        {
            #region Data Members (4)

            public string Action;
            public int Height;
            public List<MultiTile> Tiles;
            public int Width;

            #endregion Data Members
        }
    }

    public class FloorTile : MultiTile
    {
        #region Fields (1)

        private static Bitmap floorbmp;

        #endregion Fields

        #region Constructors (1)

        public FloorTile(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        #endregion Constructors

        #region Properties (1)

        public override bool isVirtualFloor { get { return true; } }

        #endregion Properties

        #region Methods (1)

        // Public Methods (1) 

        public override Bitmap GetBitmap()
        {
            if (floorbmp == null)
            {
                floorbmp = new Bitmap(44, 44);
                using (Graphics g = Graphics.FromImage(floorbmp))
                {
                    Brush FloorBrush = new SolidBrush(Color.FromArgb(96, 32, 192, 32));
                    Point[] drawFloorPoint = new Point[4];
                    drawFloorPoint[0].X = 22;
                    drawFloorPoint[0].Y = 0;
                    drawFloorPoint[1].X = 44;
                    drawFloorPoint[1].Y = 22;
                    drawFloorPoint[2].X = 22;
                    drawFloorPoint[2].Y = 44;
                    drawFloorPoint[3].X = 0;
                    drawFloorPoint[3].Y = 22;
                    g.FillPolygon(FloorBrush, drawFloorPoint);
                    g.DrawPolygon(Pens.White, drawFloorPoint);
                    FloorBrush.Dispose();
                }
            }
            return floorbmp;
        }

        #endregion Methods
    }
    public class MultiTile : IComparable
    {
        #region Fields (11)

        private static ImageAttributes m_DrawColor = null;
        private static ColorMatrix m_DrawMatrix = new ColorMatrix(new float[5][]
			{
                new float[5]{ 0, 0, 0, 0,   0 },
				new float[5]{ 0, 0, 0, 0,   0 },
				new float[5]{ 0, 0, 0, 0,   0 },
				new float[5]{ 0, 0, 0, .5f, 0 },
				new float[5]{ 0, 0, 0, 0,   1 }
			});
        private static ImageAttributes m_HoverColor = null;
        private static ColorMatrix m_HoverMatrix = new ColorMatrix(new float[5][]
			{
                new float[5] {1, 0, 0, 0, 0},
                new float[5] {0, 1, 0, 0, 0},
                new float[5] {0, 0, 1, 0, 0},
                new float[5] {0, 0, 0, 1, 0},
                new float[5] {0, 0, .8f, 0, 1}
			});
        private static ImageAttributes m_SelectedColor = null;
        private static ColorMatrix m_SelectedMatrix = new ColorMatrix(new float[5][]
			{
                new float[5] {1, 0, 0, 0, 0},
                new float[5] {0, 1, 0, 0, 0},
                new float[5] {0, 0, 1, 0, 0},
                new float[5] {0, 0, 0, 1, 0},
                new float[5] {.8f, 0, 0, 0, 1}
			});
        private static ImageAttributes m_TransColor = null;
        private static ColorMatrix m_TransMatrix = new ColorMatrix(new float[5][]
			{
                new float[5] {1, 0, 0, 0, 0},
                new float[5] {0, 1, 0, 0, 0},
                new float[5] {0, 0, 1, 0, 0},
                new float[5] {0, 0, 0, .3f, 0},
                new float[5] {0, 0, 0, 0, 1}
			});
        private static ImageAttributes m_StandableColor = null;
        private static ColorMatrix m_StandableMatrix = new ColorMatrix(new float[5][]
			{
                new float[5] {0, 0, .5f, 0, 0},
                new float[5] {0, .8f, 0, .8f, 0},
                new float[5] {.8f, 0, 0, 0, .8f},
                new float[5] {0, .8f, 0, .8f, 0},
                new float[5] {0, 0, .5f, 0, 0}
			});
        private int x;
        private int xmod;
        private int y;
        private int ymod;
        private int z;

        #endregion Fields

        #region Constructors (4)

        public MultiTile(ushort id, int x, int y, int z, int flag)
        {
            ID = id;
            X = x;
            Y = y;
            Z = z;
            Invisible = flag == 0 ? true : false;
            Solver = 0;
        }

        public MultiTile(ushort id, int x, int y, int z, bool flag)
        {
            ID = id;
            X = x;
            Y = y;
            Z = z;
            Invisible = flag;
            Solver = 0;
        }

        public MultiTile(ushort id, int z)
        {
            ID = id;
            Z = z;
        }

        public MultiTile()
        {
            ID = 0xFFFF;
        }

        static MultiTile()
        {
            if (m_HoverColor == null)
            {
                m_HoverColor = new ImageAttributes();
                m_HoverColor.SetColorMatrix(m_HoverMatrix);
            }
            if (m_SelectedColor == null)
            {
                m_SelectedColor = new ImageAttributes();
                m_SelectedColor.SetColorMatrix(m_SelectedMatrix);
            }
            if (m_DrawColor == null)
            {
                m_DrawColor = new ImageAttributes();
                m_DrawColor.SetColorMatrix(m_DrawMatrix);
            }
            if (m_TransColor == null)
            {
                m_TransColor = new ImageAttributes();
                m_TransColor.SetColorMatrix(m_TransMatrix);
            }
            if (m_StandableColor == null)
            {
                m_StandableColor = new ImageAttributes();
                m_StandableColor.SetColorMatrix(m_StandableMatrix);
            }
        }

        #endregion Constructors

        #region Properties (11)

        public static ImageAttributes DrawColor { get { return m_DrawColor; } }

        public int Height { get { return TileData.ItemTable[ID].Height; } }

        public static ImageAttributes HoverColor { get { return m_HoverColor; } }

        public ushort ID { get; private set; }

        public virtual bool isVirtualFloor { get { return false; } }

        public static ImageAttributes SelectedColor { get { return m_SelectedColor; } }

        public static ImageAttributes TransColor { get { return m_TransColor; } }

        public static ImageAttributes StandableColor { get { return m_StandableColor; } }

        public int X { get { return x; } set { x = value; RecalcMod(); } }

        public int Xmod { get { return xmod; } }

        public int Y { get { return y; } set { y = value; RecalcMod(); } }

        public int Ymod { get { return ymod; } }

        public int Z { get { return z; } set { z = value; RecalcMod(); } }

        public bool Invisible { get; set; }

        public bool Walkable { get; private set; }

        public bool DoubleSurface { get; set; }

        public bool Transparent { get; set; }

        public int Solver { get; set; }

        #endregion Properties

        #region Methods (4)

        // Public Methods (3) 

        public int CompareTo(object x)
        {
            if (x == null)
                return 1;

            if (!(x is MultiTile))
                throw new ArgumentNullException();

            MultiTile a = (MultiTile)x;
            if (X > a.X)
                return 1;
            else if (X < a.X)
                return -1;
            if (Y > a.Y)
                return 1;
            else if (Y < a.Y)
                return -1;

            if (!a.isVirtualFloor && !isVirtualFloor)
            {
                ItemData ourData = TileData.ItemTable[ID];
                ItemData theirData = TileData.ItemTable[a.ID];

                int ourTreshold = 0;
                if (ourData.Height > 0)
                    ++ourTreshold;
                if (!ourData.Background)
                    ++ourTreshold;
                int ourZ = Z;
                int theirTreshold = 0;
                if (theirData.Height > 0)
                    ++theirTreshold;
                if (!theirData.Background)
                    ++theirTreshold;
                int theirZ = a.Z;

                ourZ += ourTreshold;
                theirZ += theirTreshold;
                int res = ourZ - theirZ;
                if (res == 0)
                    res = ourTreshold - theirTreshold;
                if (res == 0)
                    res = Solver - a.Solver;
                return res;
            }
            else
            {
                if (Z > a.Z)
                    return 1;
                else if (a.Z > Z)
                    return -1;
                if (a.isVirtualFloor)
                    return 1;
                else if (isVirtualFloor)
                    return -1;
            }

            return 0;
        }

        public virtual Bitmap GetBitmap()
        {
            if (ID != 0xFFFF)
                return Art.GetStatic(ID);
            return null;
        }

        public void Set(ushort id, int z)
        {
            ID = id;
            Z = z;
            RecalcMod();
        }
        // Private Methods (1) 

        private void RecalcMod()
        {
            if (GetBitmap() != null)
            {
                xmod = (x - y) * 22;
                xmod -= GetBitmap().Width / 2;
                xmod += MultiEditorComponentList.GapXMod;
                ymod = (x + y) * 22;
                ymod -= z << 2;
                ymod -= GetBitmap().Height;
                ymod += MultiEditorComponentList.GapYMod;
            }
            else if (isVirtualFloor)
            {
                xmod = (x - y) * 22;
                xmod -= 44 / 2;
                xmod += MultiEditorComponentList.GapXMod;
                ymod = (x + y) * 22;
                ymod -= z << 2;
                ymod -= 44;
                ymod += MultiEditorComponentList.GapYMod;
            }
        }

        private const TileFlag ImpassableOrSurface = TileFlag.Impassable | TileFlag.Surface;
        public bool IsWalkable(int Z, List<MultiTile> Tiles)
        {
            int Top = Z + 16; // Playerheight
            foreach (MultiTile tile in Tiles)
            {
                ItemData itemdata = TileData.ItemTable[tile.ID];
                if ((itemdata.Flags & ImpassableOrSurface) != 0)
                {
                    if ((itemdata.Flags & TileFlag.Door) != 0)
                        continue;
                    int checkTop = tile.Z + itemdata.CalcHeight;
                    if ((checkTop > Z) && (tile.Z < Top))
                    {
                        Walkable = false;
                        return false;
                    }
                }
            }
            Walkable = true;
            return true;
        }

        public bool IsDoubleSurface(List<MultiTile> Tiles)
        {
            foreach (MultiTile tile in Tiles)
            {
                if (tile.Z == this.Z)
                {
                    ItemData itemdata = TileData.ItemTable[tile.ID];
                    if ((itemdata.Flags & TileFlag.Surface) != 0)
                    {
                        if (tile == this)
                            continue;
                        DoubleSurface = true;
                        return true;
                    }
                }
            }
            DoubleSurface = false;
            return false;
        }

        #endregion Methods
    }
}
