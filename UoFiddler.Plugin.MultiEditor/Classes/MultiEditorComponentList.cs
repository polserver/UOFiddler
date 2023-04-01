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
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.MultiEditor.UserControls;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    internal class MultiEditorComponentList
    {
        private const int _undoListMaxSize = 10;

        private bool _modified;
        private static MultiEditorControl _parent;
        private static Rectangle _drawDestRectangle;

        public const int GapXMod = 44;
        public const int GapYMod = 22;

        public int WalkableCount;
        public int DoubleSurfaceCount;

        /// <summary>
        /// Create a blank ComponentList
        /// </summary>
        public MultiEditorComponentList(int width, int height, MultiEditorControl parent)
        {
            _parent = parent;
            Width = width;
            Height = height;
            Tiles = new List<MultiTile>();
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    Tiles.Add(new FloorTile(x, y, _parent.DrawFloorZ));
                }
            }

            UndoList = new UndoStruct[_undoListMaxSize];
            _modified = true;
            RecalculateMinMax();
        }

        /// <summary>
        /// Create a ComponentList from UltimaSDK
        /// </summary>
        public MultiEditorComponentList(MultiComponentList list, MultiEditorControl parent)
        {
            _parent = parent;
            Width = list.Width;
            Height = list.Height;
            Tiles = new List<MultiTile>();
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    for (int i = 0; i < list.Tiles[x][y].Length; ++i)
                    {
                        Tiles.Add(new MultiTile(
                            list.Tiles[x][y][i].Id,
                            x,
                            y,
                            list.Tiles[x][y][i].Z,
                            list.Tiles[x][y][i].Flag));
                    }

                    Tiles.Add(new FloorTile(x, y, _parent.DrawFloorZ));
                }
            }

            CalcSolver();
            Tiles.Sort();
            UndoList = new UndoStruct[_undoListMaxSize];
            _modified = true;
            RecalculateMinMax();
        }

        public int Height { get; private set; }

        public List<MultiTile> Tiles { get; private set; }

        public UndoStruct[] UndoList { get; }

        public int Width { get; private set; }

        public int XMax { get; private set; }

        private int XMaxOrg { get; set; }

        public int XMin { get; private set; }

        private int XMinOrg { get; set; }

        private int YMax { get; set; }

        public int YMaxOrg { get; private set; }

        public int YMin { get; private set; }

        public int YMinOrg { get; private set; }

        public int ZMax { get; private set; }

        public int ZMin { get; private set; }

        /// <summary>
        /// Export to given multi id
        /// </summary>
        public void AddToSdkComponentList(int id)
        {
            Multis.Add(id, ConvertToSdk());
            Options.ChangedUltimaClass["Multis"] = true;
            ControlEvents.FireMultiChangeEvent(this, id);
        }

        public MultiComponentList ConvertToSdk()
        {
            int count = 0;
            var tiles = new MTileList[Width][];
            for (int x = 0; x < Width; ++x)
            {
                tiles[x] = new MTileList[Height];
                for (int y = 0; y < Height; ++y)
                {
                    tiles[x][y] = new MTileList();
                }
            }

            foreach (MultiTile tile in Tiles)
            {
                if (tile.IsVirtualFloor)
                {
                    continue;
                }

                tiles[tile.X][tile.Y].Add(tile.Id, (sbyte)tile.Z, tile.Invisible ? (sbyte)0 : (sbyte)1, 0);
                ++count;
            }

            return new MultiComponentList(tiles, count, Width, Height);
        }

        /// <summary>
        /// Gets Bitmap of Multi and sets HoverTile
        /// </summary>
        public void GetImage(Graphics gfx, int xOff, int yOff, int maxHeight, Point mouseLoc, bool drawFloor)
        {
            if (Width == 0 || Height == 0)
            {
                return;
            }

            if (_modified)
            {
                RecalculateMinMax();
            }

            XMin = XMinOrg;
            XMax = XMaxOrg;
            YMin = YMinOrg;
            YMax = YMaxOrg;

            if (drawFloor)
            {
                int floorZMod = (-_parent.DrawFloorZ * 4) - 44;
                if (YMin > floorZMod)
                {
                    YMin = floorZMod;
                }

                floorZMod = ((Width + Height) * 22) - (_parent.DrawFloorZ * 4);
                if (YMaxOrg < floorZMod)
                {
                    YMax = floorZMod;
                }
            }

            _parent.HoverTile = GetSelected(mouseLoc, maxHeight, drawFloor);

            Rectangle clipBounds = Rectangle.Round(gfx.ClipBounds);

            foreach (MultiTile tile in Tiles)
            {
                if (!tile.IsVirtualFloor && tile.Z > maxHeight)
                {
                    continue;
                }

                Bitmap bmp = tile.GetBitmap();
                if (bmp == null)
                {
                    continue;
                }

                _drawDestRectangle.X = tile.XMod;
                _drawDestRectangle.Y = tile.YMod;
                _drawDestRectangle.X -= XMin + xOff;
                _drawDestRectangle.Y -= YMin + yOff;
                _drawDestRectangle.Width = bmp.Width;
                _drawDestRectangle.Height = bmp.Height;

                if (!clipBounds.IntersectsWith(_drawDestRectangle))
                {
                    continue;
                }

                if (tile.IsVirtualFloor)
                {
                    if (drawFloor)
                        gfx.DrawImageUnscaled(bmp, _drawDestRectangle);
                }
                else
                {
                    if (_parent.HoverTile != null && _parent.HoverTile == tile)
                    {
                        gfx.DrawImage(bmp, _drawDestRectangle, 0, 0, _drawDestRectangle.Width,
                            _drawDestRectangle.Height, GraphicsUnit.Pixel, MultiTile.HoverColor);
                    }
                    else if (_parent.SelectedTile != null && _parent.SelectedTile == tile)
                    {
                        gfx.DrawImage(bmp, _drawDestRectangle, 0, 0, _drawDestRectangle.Width,
                            _drawDestRectangle.Height, GraphicsUnit.Pixel, MultiTile.SelectedColor);
                    }
                    else if (tile.Transparent)
                    {
                        gfx.DrawImage(bmp, _drawDestRectangle, 0, 0, _drawDestRectangle.Width,
                            _drawDestRectangle.Height, GraphicsUnit.Pixel, MultiTile.TransColor);
                    }
                    else if (_parent.ShowWalkables && tile.Walkable)
                    {
                        gfx.DrawImage(bmp, _drawDestRectangle, 0, 0, _drawDestRectangle.Width,
                            _drawDestRectangle.Height, GraphicsUnit.Pixel, MultiTile.StandableColor);
                    }
                    else if (_parent.ShowDoubleSurface && tile.DoubleSurface)
                    {
                        gfx.DrawImage(bmp, _drawDestRectangle, 0, 0, _drawDestRectangle.Width,
                            _drawDestRectangle.Height, GraphicsUnit.Pixel, MultiTile.StandableColor);
                    }
                    else
                    {
                        gfx.DrawImageUnscaled(bmp, _drawDestRectangle);
                    }
                }
            }
        }

        /// <summary>
        /// Gets <see cref="MultiTile"/> from given Pixel Location
        /// </summary>
        private MultiTile GetSelected(Point mouseLoc, int maxHeight, bool drawFloor)
        {
            if (Width == 0 || Height == 0)
            {
                return null;
            }

            if (mouseLoc == Point.Empty)
            {
                return null;
            }

            MultiTile selected = null;
            for (int i = Tiles.Count - 1; i >= 0; --i) // inverse for speedup
            {
                MultiTile tile = Tiles[i];
                if (tile.IsVirtualFloor)
                {
                    continue;
                }

                if (tile.Z > maxHeight)
                {
                    continue;
                }

                if (drawFloor && _parent.DrawFloorZ > tile.Z)
                {
                    continue;
                }

                Bitmap bmp = tile.GetBitmap();
                if (bmp == null)
                {
                    continue;
                }

                int px = tile.XMod;
                int py = tile.YMod;
                px -= XMin;
                py -= YMin;

                if (mouseLoc.X <= px || mouseLoc.X >= px + bmp.Width || mouseLoc.Y <= py || mouseLoc.Y >= py + bmp.Height)
                {
                    continue;
                }

                // Check for transparent part
                Color p = bmp.GetPixel(mouseLoc.X - px, mouseLoc.Y - py);
                if (p.R == 0 && p.G == 0 && p.B == 0)
                {
                    continue;
                }

                selected = tile;
                break;
            }

            return selected;
        }

        /// <summary>
        /// Resizes Multi size
        /// </summary>
        public void Resize(int width, int height)
        {
            AddToUndoList("Resize");
            if (Width > width || Height > height)
            {
                for (int i = Tiles.Count - 1; i >= 0; --i)
                {
                    MultiTile tile = Tiles[i];
                    if (tile.X >= width)
                    {
                        Tiles.RemoveAt(i);
                    }
                    else if (tile.Y >= height)
                    {
                        Tiles.RemoveAt(i);
                    }
                }
            }

            if (Width < width || Height < height)
            {
                for (int x = 0; x < width; ++x)
                {
                    for (int y = 0; y < height; ++y)
                    {
                        if (x < Width && y < Height)
                        {
                            continue;
                        }

                        Tiles.Add(new FloorTile(x, y, _parent.DrawFloorZ));
                    }
                }
            }

            Width = width;
            Height = height;
            _modified = true;
            RecalculateMinMax();
        }

        public void SetFloorZ(int z)
        {
            foreach (MultiTile tile in Tiles)
            {
                if (tile.IsVirtualFloor)
                {
                    tile.Z = z;
                }
            }

            Tiles.Sort();
        }

        /// <summary>
        /// Adds an <see cref="MultiTile"/> to specific location
        /// </summary>
        public void TileAdd(int x, int y, int z, ushort id)
        {
            if (x > Width || y > Height)
            {
                return;
            }

            AddToUndoList("Add Tile");
            MultiTile tile = new MultiTile(id, x, y, z, 1);
            Tiles.Add(tile);
            CalcSolver(x, y);
            _modified = true;
            Tiles.Sort();
            RecalculateMinMax(tile);
        }

        /// <summary>
        /// Moves given <see cref="MultiTile"/>
        /// </summary>
        public void TileMove(MultiTile tile, int newX, int newY)
        {
            if (Width == 0 || Height == 0)
            {
                return;
            }

            AddToUndoList("Move Tile");

            if (tile == null)
            {
                return;
            }

            tile.X = newX;
            tile.Y = newY;
            CalcSolver(newX, newY);
            Tiles.Sort();
            _modified = true;
            RecalculateMinMax(tile);
        }

        /// <summary>
        /// Removes specific <see cref="MultiTile"/>
        /// </summary>
        public void TileRemove(MultiTile tile)
        {
            if (Width == 0 || Height == 0)
            {
                return;
            }

            AddToUndoList("Remove Tile");

            if (tile == null)
            {
                return;
            }

            Tiles.Remove(tile);
            _modified = true;
            RecalculateMinMax(tile);
        }

        /// <summary>
        /// Alters Z level for given <see cref="MultiTile"/>
        /// </summary>
        public void TileZMod(MultiTile tile, int modZ)
        {
            AddToUndoList("Modify Z");
            tile.Z += modZ;
            if (tile.Z > 127)
            {
                tile.Z = 127;
            }

            if (tile.Z < -128)
            {
                tile.Z = -128;
            }

            CalcSolver(tile.X, tile.Y);
            Tiles.Sort();
            _modified = true;
            RecalculateMinMax(tile);
        }

        /// <summary>
        /// Sets Z value of given <see cref="MultiTile"/>
        /// </summary>
        public void TileZSet(MultiTile tile, int setZ)
        {
            AddToUndoList("Set Z");
            tile.Z = setZ;
            if (tile.Z > 127)
            {
                tile.Z = 127;
            }

            if (tile.Z < -128)
            {
                tile.Z = -128;
            }

            CalcSolver(tile.X, tile.Y);
            Tiles.Sort();
            _modified = true;
            RecalculateMinMax(tile);
        }

        public void Undo(int index)
        {
            if (UndoList[index].Tiles == null)
            {
                return;
            }

            Width = UndoList[index].Width;
            Height = UndoList[index].Height;
            Tiles = new List<MultiTile>();
            foreach (MultiTile tile in UndoList[index].Tiles)
            {
                Tiles.Add(tile.IsVirtualFloor
                    ? new FloorTile(tile.X, tile.Y, tile.Z)
                    : new MultiTile(tile.Id, tile.X, tile.Y, tile.Z, tile.Invisible));
            }

            _modified = true;
            RecalculateMinMax();
        }

        public void UndoClear()
        {
            for (int i = 0; i < _undoListMaxSize; ++i)
            {
                UndoList[i].Action = null;
                UndoList[i].Tiles = null;
            }
        }

        public void CalcWalkable()
        {
            const int lastX = -1;
            const int lastY = -1;

            List<MultiTile> xyArray = new List<MultiTile>();
            WalkableCount = 0;
            foreach (MultiTile tile in Tiles)
            {
                if (tile.IsVirtualFloor)
                {
                    continue;
                }

                ItemData itemData = TileData.ItemTable[tile.Id];
                if ((itemData.Flags & TileFlag.Surface) == 0)
                {
                    continue;
                }

                if (tile.X != lastX && tile.Y != lastY)
                {
                    xyArray = GetMultiTileLitAtCoordinate(tile.X, tile.Y);
                }

                int start = tile.Z + itemData.CalcHeight;
                if (tile.IsWalkable(start, xyArray))
                {
                    ++WalkableCount;
                }
            }
        }

        public void CalcDoubleSurface()
        {
            const int lastX = -1;
            const int lastY = -1;
            List<MultiTile> xyArray = new List<MultiTile>();
            DoubleSurfaceCount = 0;
            foreach (MultiTile tile in Tiles)
            {
                if (tile.IsVirtualFloor)
                {
                    continue;
                }

                ItemData itemData = TileData.ItemTable[tile.Id];
                if ((itemData.Flags & TileFlag.Surface) == 0)
                {
                    continue;
                }

                if (tile.X != lastX && tile.Y != lastY)
                {
                    xyArray = GetMultiTileLitAtCoordinate(tile.X, tile.Y);
                }

                if (tile.IsDoubleSurface(xyArray))
                {
                    ++DoubleSurfaceCount;
                }
            }
        }

        private void CalcSolver()
        {
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    CalcSolver(x, y);
                }
            }
        }

        private void CalcSolver(int x, int y)
        {
            List<MultiTile> multiTiles = GetMultiTileLitAtCoordinate(x, y);
            for (int i = 0; i < multiTiles.Count; ++i)
            {
                multiTiles[i].Solver = i;
            }
        }

        public List<MultiTile> GetMultiTileLitAtCoordinate(int x, int y)
        {
            List<MultiTile> multiTiles = new List<MultiTile>();
            foreach (MultiTile tile in Tiles)
            {
                if (tile.IsVirtualFloor)
                {
                    continue;
                }

                if (tile.X != x)
                {
                    continue;
                }

                if (tile.Y != y)
                {
                    continue;
                }

                multiTiles.Add(tile);
            }

            return multiTiles;
        }

        private void AddToUndoList(string action)
        {
            for (int i = _undoListMaxSize - 2; i >= 0; --i)
            {
                UndoList[i + 1] = UndoList[i];
            }

            UndoList[0].Action = action;
            UndoList[0].Tiles = new List<MultiTile>();
            UndoList[0].Width = Width;
            UndoList[0].Height = Height;

            foreach (MultiTile tile in Tiles)
            {
                UndoList[0].Tiles.Add(tile.IsVirtualFloor
                    ? new FloorTile(tile.X, tile.Y, tile.Y)
                    : new MultiTile(tile.Id, tile.X, tile.Y, tile.Z, tile.Invisible));
            }
        }

        /// <summary>
        /// Recalculates bitmap size
        /// </summary>
        private void RecalculateMinMax()
        {
            // CalcEdgeTiles
            YMin = -44; // 0,0
            YMax = (Width + Height) * 22; // width,height
            XMin = (-Height * 22) - 22; // 0,height
            XMax = (Width * 22) + 22; // width,0
            ZMin = 127;
            ZMax = -128;

            foreach (MultiTile tile in Tiles)
            {
                if (tile.IsVirtualFloor)
                {
                    continue;
                }

                if (tile.GetBitmap() == null)
                {
                    continue;
                }

                int px = tile.XMod - GapXMod;
                int py = tile.YMod - GapYMod;

                if (px < XMin)
                {
                    XMin = px;
                }

                if (py < YMin)
                {
                    YMin = py;
                }

                px += tile.GetBitmap().Width;
                py += tile.GetBitmap().Height;

                if (px > XMax)
                    XMax = px;
                if (py > YMax)
                    YMax = py;
                if (tile.Z > ZMax)
                    ZMax = tile.Z;
                if (tile.Z < ZMin)
                    ZMin = tile.Z;
            }

            _modified = false;
            XMinOrg = XMin;
            XMaxOrg = XMax;
            YMinOrg = YMin;
            YMaxOrg = YMax;

            if (_parent.ShowWalkables)
            {
                CalcWalkable();
            }

            if (_parent.ShowDoubleSurface)
            {
                CalcDoubleSurface();
            }
        }

        private void RecalculateMinMax(MultiTile tile)
        {
            if (tile.IsVirtualFloor)
            {
                return;
            }

            if (tile.GetBitmap() == null)
            {
                return;
            }

            int px = tile.XMod - GapXMod;
            int py = tile.YMod - GapYMod;

            if (px < XMin)
            {
                XMin = px;
            }

            if (py < YMin)
            {
                YMin = py;
            }

            px += tile.GetBitmap().Width;
            py += tile.GetBitmap().Height;

            if (px > XMax)
            {
                XMax = px;
            }

            if (py > YMax)
            {
                YMax = py;
            }

            if (tile.Z > ZMax)
            {
                ZMax = tile.Z;
            }

            if (tile.Z < ZMin)
            {
                ZMin = tile.Z;
            }

            _modified = false;

            XMinOrg = XMin;
            XMaxOrg = XMax;
            YMinOrg = YMin;
            YMaxOrg = YMax;

            if (_parent.ShowWalkables)
            {
                CalcWalkable();
            }

            if (_parent.ShowDoubleSurface)
            {
                CalcDoubleSurface();
            }
        }

        public struct UndoStruct
        {
            public string Action;
            public int Height;
            public List<MultiTile> Tiles;
            public int Width;
        }
    }

    public class FloorTile : MultiTile
    {
        private static Bitmap _floorBmp;

        public FloorTile(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override bool IsVirtualFloor => true;

        public override Bitmap GetBitmap()
        {
            if (_floorBmp != null)
            {
                return _floorBmp;
            }

            _floorBmp = new Bitmap(44, 44);

            using (Graphics g = Graphics.FromImage(_floorBmp))
            {
                using (Brush floorBrush = new SolidBrush(Color.FromArgb(96, 32, 192, 32)))
                {
                    Point[] drawFloorPoint = new Point[4];
                    drawFloorPoint[0].X = 22;
                    drawFloorPoint[0].Y = 0;
                    drawFloorPoint[1].X = 44;
                    drawFloorPoint[1].Y = 22;
                    drawFloorPoint[2].X = 22;
                    drawFloorPoint[2].Y = 44;
                    drawFloorPoint[3].X = 0;
                    drawFloorPoint[3].Y = 22;

                    g.FillPolygon(floorBrush, drawFloorPoint);
                    g.DrawPolygon(Pens.White, drawFloorPoint);
                }
            }

            return _floorBmp;
        }
    }

    public class MultiTile : IComparable
    {
        private static readonly ColorMatrix _drawMatrix = new ColorMatrix(new float[][]
        {
            new float[] {0, 0, 0, 0, 0},
            new float[] {0, 0, 0, 0, 0},
            new float[] {0, 0, 0, 0, 0},
            new float[] {0, 0, 0, .5f, 0},
            new float[] {0, 0, 0, 0, 1}
        });

        private static readonly ColorMatrix _hoverMatrix = new ColorMatrix(new float[][]
        {
            new float[] {1, 0, 0, 0, 0},
            new float[] {0, 1, 0, 0, 0},
            new float[] {0, 0, 1, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {0, 0, .8f, 0, 1}
        });

        private static readonly ColorMatrix _selectedMatrix = new ColorMatrix(new float[][]
        {
            new float[] {0, 1, 0, 0, 0},
            new float[] {0, 0, 1, 0, 0},
            new float[] {1, 0, 0, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {.8f, 0, 0, 0, 1}
        });

        private static readonly ColorMatrix _transMatrix = new ColorMatrix(new float[][]
        {
            new float[] {1, 0, 0, 0, 0},
            new float[] {0, 1, 0, 0, 0},
            new float[] {0, 0, 1, 0, 0},
            new float[] {0, 0, 0, .3f, 0},
            new float[] {0, 0, 0, 0, 1}
        });

        private static readonly ColorMatrix _standableMatrix = new ColorMatrix(new float[][]
        {
            new float[] {0, 0, .5f, 0, 0},
            new float[] {0, .8f, 0, .8f, 0},
            new float[] {.8f, 0, 0, 0, .8f},
            new float[] {0, .8f, 0, .8f, 0},
            new float[] {0, 0, .5f, 0, 0}
        });

        private int _x;
        private int _y;
        private int _z;

        public MultiTile(ushort id, int x, int y, int z, int flag)
        {
            Id = id;
            X = x;
            Y = y;
            Z = z;
            Invisible = flag == 0;
            Solver = 0;
        }

        public MultiTile(ushort id, int x, int y, int z, bool flag)
        {
            Id = id;
            X = x;
            Y = y;
            Z = z;
            Invisible = flag;
            Solver = 0;
        }

        public MultiTile()
        {
            Id = 0xFFFF;
        }

        static MultiTile()
        {
            if (HoverColor == null)
            {
                HoverColor = new ImageAttributes();
                HoverColor.SetColorMatrix(_hoverMatrix);
            }

            if (SelectedColor == null)
            {
                SelectedColor = new ImageAttributes();
                SelectedColor.SetColorMatrix(_selectedMatrix);
            }

            if (DrawColor == null)
            {
                DrawColor = new ImageAttributes();
                DrawColor.SetColorMatrix(_drawMatrix);
            }

            if (TransColor == null)
            {
                TransColor = new ImageAttributes();
                TransColor.SetColorMatrix(_transMatrix);
            }

            if (StandableColor == null)
            {
                StandableColor = new ImageAttributes();
                StandableColor.SetColorMatrix(_standableMatrix);
            }
        }

        public static ImageAttributes DrawColor { get; }

        public int Height => TileData.ItemTable[Id].Height;

        public static ImageAttributes HoverColor { get; }

        public ushort Id { get; private set; }

        public virtual bool IsVirtualFloor => false;

        public static ImageAttributes SelectedColor { get; }

        public static ImageAttributes TransColor { get; }

        public static ImageAttributes StandableColor { get; }

        public int X
        {
            get => _x;
            set
            {
                _x = value;
                RecalculateMod();
            }
        }

        public int XMod { get; private set; }

        public int Y
        {
            get => _y;
            set
            {
                _y = value;
                RecalculateMod();
            }
        }

        public int YMod { get; private set; }

        public int Z
        {
            get => _z;
            set
            {
                _z = value;
                RecalculateMod();
            }
        }

        public bool Invisible { get; set; }

        public bool Walkable { get; private set; }

        public bool DoubleSurface { get; private set; }

        public bool Transparent { get; set; }

        public int Solver { get; set; }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (obj is not MultiTile multiTile)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (X > multiTile.X)
            {
                return 1;
            }

            if (X < multiTile.X)
            {
                return -1;
            }

            if (Y > multiTile.Y)
            {
                return 1;
            }

            if (Y < multiTile.Y)
            {
                return -1;
            }

            if (!multiTile.IsVirtualFloor && !IsVirtualFloor)
            {
                ItemData ourData = TileData.ItemTable[Id];
                ItemData theirData = TileData.ItemTable[multiTile.Id];

                int ourThreshold = 0;
                if (ourData.Height > 0)
                {
                    ++ourThreshold;
                }

                if (!ourData.Background)
                {
                    ++ourThreshold;
                }

                int ourZ = Z;

                int theirThreshold = 0;
                if (theirData.Height > 0)
                {
                    ++theirThreshold;
                }

                if (!theirData.Background)
                {
                    ++theirThreshold;
                }

                int theirZ = multiTile.Z;

                ourZ += ourThreshold;
                theirZ += theirThreshold;

                int res = ourZ - theirZ;
                if (res == 0)
                {
                    res = ourThreshold - theirThreshold;
                }

                if (res == 0)
                {
                    res = Solver - multiTile.Solver;
                }

                return res;
            }
            else
            {
                if (Z > multiTile.Z)
                {
                    return 1;
                }

                if (multiTile.Z > Z)
                {
                    return -1;
                }

                if (multiTile.IsVirtualFloor)
                {
                    return 1;
                }

                if (IsVirtualFloor)
                {
                    return -1;
                }
            }

            return 0;
        }

        public virtual Bitmap GetBitmap()
        {
            return Id != 0xFFFF
                ? Art.GetStatic(Id)
                : null;
        }

        public void Set(ushort id, int z)
        {
            Id = id;
            Z = z;

            RecalculateMod();
        }

        private void RecalculateMod()
        {
            if (GetBitmap() != null)
            {
                XMod = (_x - _y) * 22;
                XMod -= GetBitmap().Width / 2;
                XMod += MultiEditorComponentList.GapXMod;
                YMod = (_x + _y) * 22;
                YMod -= _z * 4;
                YMod -= GetBitmap().Height;
                YMod += MultiEditorComponentList.GapYMod;
            }
            else if (IsVirtualFloor)
            {
                XMod = (_x - _y) * 22;
                XMod -= 44 / 2;
                XMod += MultiEditorComponentList.GapXMod;
                YMod = (_x + _y) * 22;
                YMod -= _z * 4;
                YMod -= 44;
                YMod += MultiEditorComponentList.GapYMod;
            }
        }

        private const TileFlag _impassableOrSurface = TileFlag.Impassable | TileFlag.Surface;

        public bool IsWalkable(int z, List<MultiTile> tiles)
        {
            int top = z + 16; // Player Height

            foreach (MultiTile tile in tiles)
            {
                ItemData itemData = TileData.ItemTable[tile.Id];
                if ((itemData.Flags & _impassableOrSurface) == 0)
                {
                    continue;
                }

                if ((itemData.Flags & TileFlag.Door) != 0)
                {
                    continue;
                }

                int checkTop = tile.Z + itemData.CalcHeight;
                if (checkTop <= z || tile.Z >= top)
                {
                    continue;
                }

                Walkable = false;
                return false;
            }

            Walkable = true;
            return true;
        }

        public bool IsDoubleSurface(List<MultiTile> tiles)
        {
            foreach (MultiTile tile in tiles)
            {
                if (tile.Z != Z)
                {
                    continue;
                }

                ItemData itemData = TileData.ItemTable[tile.Id];
                if ((itemData.Flags & TileFlag.Surface) == 0)
                {
                    continue;
                }

                if (tile == this)
                {
                    continue;
                }

                DoubleSurface = true;
                return true;
            }

            DoubleSurface = false;
            return false;
        }
    }
}