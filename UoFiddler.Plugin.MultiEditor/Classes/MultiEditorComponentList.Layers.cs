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

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    // The layers of the design: the list, the active one, and the edits on them. Changes to the
    // list and to which layer a tile is on are undo steps; a layer's visibility is not.
    internal partial class MultiEditorComponentList
    {
        public const string BaseLayerName = "Base";

        /// <summary>Largest width or height a design is grown to for tiles only the layers file has.</summary>
        public const int MaxRestoredSize = 512;

        private List<TileLayer> _layers = new() { new TileLayer(BaseLayerName) };
        private TileLayer _activeLayer;

        /// <summary>The layers, bottom first. There is always at least one.</summary>
        public IReadOnlyList<TileLayer> Layers => _layers;

        /// <summary>The layer new tiles go on.</summary>
        public TileLayer ActiveLayer
        {
            get => _activeLayer != null && _layers.Contains(_activeLayer) ? _activeLayer : _layers[^1];
            set => _activeLayer = value;
        }

        /// <summary>Tiles per layer, counted in one pass. A layer without tiles is not listed.</summary>
        public Dictionary<TileLayer, int> LayerTileCounts()
        {
            var counts = new Dictionary<TileLayer, int>();
            foreach (MultiTile tile in RealTiles)
            {
                counts[tile.Layer] = counts.GetValueOrDefault(tile.Layer) + 1;
            }

            return counts;
        }

        public int LayerTileCount(TileLayer layer)
        {
            int count = 0;
            foreach (MultiTile tile in RealTiles)
            {
                if (tile.Layer == layer)
                {
                    ++count;
                }
            }

            return count;
        }

        /// <summary>Number of tiles on hidden layers.</summary>
        public int HiddenLayerTileCount
        {
            get
            {
                int count = 0;
                foreach (MultiTile tile in RealTiles)
                {
                    if (!tile.Layer.Visible)
                    {
                        ++count;
                    }
                }

                return count;
            }
        }

        /// <summary>A layer name not in use yet: the name itself, or the name with the lowest free number.</summary>
        public string FreeLayerName(string name)
        {
            if (_layers.All(l => l.Name != name))
            {
                return name;
            }

            for (int i = 2; ; i++)
            {
                string numbered = $"{name} {i}";
                if (_layers.All(l => l.Name != numbered))
                {
                    return numbered;
                }
            }
        }

        // ---------------------------------------------------------------- layer edits

        /// <summary>Adds an empty layer above the active one and makes it active.</summary>
        public TileLayer LayerAdd(string name)
        {
            BeginEdit("Add Layer");
            RecordLayers();
            var layer = new TileLayer(name);
            _layers.Insert(_layers.IndexOf(ActiveLayer) + 1, layer);
            ActiveLayer = layer;
            EndEdit();
            return layer;
        }

        /// <summary>Removes a layer with its tiles. The last layer stays.</summary>
        public bool LayerRemove(TileLayer layer)
        {
            int index = _layers.IndexOf(layer);
            if (index < 0 || _layers.Count == 1)
            {
                return false;
            }

            BeginEdit("Delete Layer");
            RecordLayers();
            foreach (MultiTile tile in RealTiles.Where(t => t.Layer == layer).ToList())
            {
                RecordRemove(tile);
            }

            _layers.RemoveAt(index);
            ActiveLayer = _layers[Math.Max(0, index - 1)];
            EndEdit();
            return true;
        }

        /// <summary>Adds a copy of a layer, with copies of its tiles, above it and makes it active.</summary>
        public TileLayer LayerDuplicate(TileLayer layer)
        {
            int index = _layers.IndexOf(layer);
            if (index < 0)
            {
                return null;
            }

            BeginEdit("Duplicate Layer");
            RecordLayers();
            var copy = new TileLayer(FreeLayerName(layer.Name + " copy")) { Visible = layer.Visible };
            _layers.Insert(index + 1, copy);
            foreach (MultiTile tile in RealTiles.Where(t => t.Layer == layer).ToList())
            {
                RecordNew(tile.Id, tile.X, tile.Y, tile.Z, tile.Invisible, tile.Extra, layer: copy);
            }

            ActiveLayer = copy;
            EndEdit();
            return copy;
        }

        /// <summary>
        /// Moves a layer's tiles to the layer below it and removes the layer. A tile the lower
        /// layer already has (same id, cell and Z) is dropped.
        /// </summary>
        public bool LayerMergeDown(TileLayer layer)
        {
            int index = _layers.IndexOf(layer);
            if (index < 1)
            {
                return false;
            }

            TileLayer below = _layers[index - 1];
            BeginEdit("Merge Layer");
            RecordLayers();
            MoveToLayer(RealTiles.Where(t => t.Layer == layer).ToList(), below);
            _layers.RemoveAt(index);
            ActiveLayer = below;
            EndEdit();
            return true;
        }

        /// <summary>Moves a layer up (+1) or down (-1) in the list.</summary>
        public bool LayerMove(TileLayer layer, int delta)
        {
            int index = _layers.IndexOf(layer);
            int target = index + delta;
            if (index < 0 || target < 0 || target >= _layers.Count || delta == 0)
            {
                return false;
            }

            BeginEdit("Move Layer");
            RecordLayers();
            _layers.RemoveAt(index);
            _layers.Insert(target, layer);
            EndEdit();
            return true;
        }

        public bool LayerRename(TileLayer layer, string name)
        {
            if (!_layers.Contains(layer) || string.IsNullOrWhiteSpace(name) || layer.Name == name)
            {
                return false;
            }

            BeginEdit("Rename Layer");
            RecordLayers();
            layer.Name = name;
            EndEdit();
            return true;
        }

        /// <summary>Puts tiles on another layer in one undo step.</summary>
        public void TilesSetLayer(IEnumerable<MultiTile> tiles, TileLayer layer)
        {
            if (!_layers.Contains(layer))
            {
                return;
            }

            BeginEdit("Move To Layer");
            MoveToLayer(new List<MultiTile>(tiles), layer);
            EndEdit();
        }

        private void MoveToLayer(List<MultiTile> tiles, TileLayer layer)
        {
            foreach (MultiTile tile in tiles)
            {
                if (!Contains(tile) || tile.Layer == layer)
                {
                    continue;
                }

                if (Cell(tile.X, tile.Y).Any(o => o.Layer == layer && o.Id == tile.Id && o.Z == tile.Z))
                {
                    RecordRemove(tile);
                }
                else
                {
                    RecordChange(tile, tile.Id, tile.X, tile.Y, tile.Z, tile.Invisible, layer);
                }
            }
        }

        // ---------------------------------------------------------------- undo support

        private List<(TileLayer Layer, string Name)> LayerSnapshot() => _layers.Select(l => (l, l.Name)).ToList();

        /// <summary>Call inside an edit before the layer list or a layer name changes.</summary>
        private void RecordLayers()
        {
            if (_current.LayersBefore == null)
            {
                _current.LayersBefore = LayerSnapshot();
                _current.ActiveLayerBefore = ActiveLayer;
            }
        }

        private void CaptureLayersAfter(EditCommand command)
        {
            if (command.LayersBefore == null)
            {
                return;
            }

            List<(TileLayer Layer, string Name)> after = LayerSnapshot();
            if (after.SequenceEqual(command.LayersBefore))
            {
                command.LayersBefore = null;
            }
            else
            {
                command.LayersAfter = after;
                command.ActiveLayerAfter = ActiveLayer;
            }
        }

        private void RestoreLayerList(List<(TileLayer Layer, string Name)> layers)
        {
            _layers = layers.Select(l => l.Layer).ToList();
            foreach ((TileLayer layer, string name) in layers)
            {
                layer.Name = name;
            }
        }

        // ---------------------------------------------------------------- layers file

        /// <summary>
        /// The cell the saved multi counts its tile offsets from (the same rule the client list
        /// uses when it is made from this design). A saved multi is cut down to its tiles, so
        /// cells shift when it is opened again, but a tile's offset from this cell stays. The
        /// layers file therefore keeps offsets, not cells.
        /// </summary>
        public (int X, int Y) SaveOrigin
        {
            get
            {
                int x = (int)Math.Round(Width / 2.0) - 1;
                int y = (int)Math.Round(Height / 2.0) - 1;
                return (x < 0 ? Width / 2 : x, y < 0 ? Height / 2 : y);
            }
        }

        /// <summary>
        /// A number that names the tiles <see cref="ConvertToSdk"/> writes, by their offsets from
        /// <paramref name="origin"/> and whatever their order. The layers file keeps it to know
        /// the multi it was saved with.
        /// </summary>
        public long Fingerprint((int X, int Y) origin, bool includeHidden = true)
        {
            long sum = 0;
            var seen = new HashSet<(ushort, int, int, int)>();
            foreach (MultiTile tile in RealTiles)
            {
                if ((!includeHidden && !tile.Layer.Visible) || !seen.Add((tile.Id, tile.X, tile.Y, tile.Z)))
                {
                    continue;
                }

                unchecked
                {
                    long h = (tile.Id * 73856093L) ^ ((tile.X - origin.X) * 19349663L) ^ ((tile.Y - origin.Y) * 83492791L) ^ (tile.Z * 2654435761L);
                    sum += (h * 1099511628211L) + (h >> 7) + 1;
                }
            }

            return sum;
        }

        /// <summary>
        /// The layers as the layers file keeps them, tiles as offsets from <see cref="SaveOrigin"/>.
        /// With <paramref name="includeHidden"/> false the hidden layers are marked as not in the
        /// saved multi.
        /// </summary>
        public List<SavedLayer> SaveLayers(bool includeHidden)
        {
            (int ox, int oy) = SaveOrigin;
            var result = new List<SavedLayer>();
            for (int i = 0; i < _layers.Count; i++)
            {
                TileLayer layer = _layers[i];
                bool inFile = includeHidden || layer.Visible;
                // The first layer takes whatever the others do not claim, so it lists only the
                // tiles another layer has too: the multi holds such a tile once.
                var tiles = RealTiles
                    .Where(t => t.Layer == layer && (i > 0 || !inFile || Cell(t.X, t.Y).Any(o => o != t && o.Id == t.Id && o.Z == t.Z)))
                    .Select(t => (t.Id, t.X - ox, t.Y - oy, t.Z, t.Invisible, t.Extra)).ToList();
                result.Add(new SavedLayer(layer.Name, layer.Visible, inFile, tiles));
            }

            return result;
        }

        /// <summary>The layers with all their tiles by cell, as a design file keeps them.</summary>
        public List<SavedLayer> DesignLayers()
        {
            return _layers.Select(layer => new SavedLayer(layer.Name, layer.Visible, false,
                RealTiles.Where(t => t.Layer == layer).Select(t => (t.Id, t.X, t.Y, t.Z, t.Invisible, t.Extra)).ToList())).ToList();
        }

        /// <summary>
        /// Puts a freshly loaded design back on its saved layers. Not an undo step.
        /// <paramref name="origin"/> is the cell of the loaded multi the saved offsets count from
        /// (its centre). A layer that was saved with the multi claims its tiles by id, cell and Z,
        /// and what the multi no longer has is dropped; a layer that was left out of the multi
        /// gets its tiles back from the file, and the design grows when they reach past its edge.
        /// Tiles nobody claims stay on the first layer.
        /// </summary>
        public void RestoreLayers(IReadOnlyList<SavedLayer> saved, int activeIndex, (int X, int Y) origin)
        {
            if (saved == null || saved.Count == 0)
            {
                return;
            }

            // Room for the tiles only the layers file has: everything moves right and down by the shift.
            var extra = saved.Where(l => !l.InFile).SelectMany(l => l.Tiles ?? new()).ToList();
            if (extra.Count > 0)
            {
                int shiftX = Math.Max(0, -(extra.Min(t => t.X) + origin.X));
                int shiftY = Math.Max(0, -(extra.Min(t => t.Y) + origin.Y));
                int width = Math.Max(Width + shiftX, extra.Max(t => t.X) + origin.X + shiftX + 1);
                int height = Math.Max(Height + shiftY, extra.Max(t => t.Y) + origin.Y + shiftY + 1);
                if (width > MaxRestoredSize || height > MaxRestoredSize)
                {
                    // No design is that large: the file is damaged. The design opens without layers.
                    return;
                }

                if (shiftX != 0 || shiftY != 0 || width != Width || height != Height)
                {
                    var all = new List<MultiTile>(RealTiles);
                    foreach (MultiTile tile in all)
                    {
                        tile.X += shiftX;
                        tile.Y += shiftY;
                    }

                    RebuildGrid(width, height, all);
                    origin = (origin.X + shiftX, origin.Y + shiftY);
                }
            }

            TileLayer first = _layers[0];
            first.Name = saved[0].Name;
            first.Visible = saved[0].Visible;
            _layers = new List<TileLayer> { first };
            for (int i = 1; i < saved.Count; i++)
            {
                var layer = new TileLayer(saved[i].Name) { Visible = saved[i].Visible };
                _layers.Add(layer);
                if (!saved[i].InFile)
                {
                    continue;
                }

                foreach ((ushort id, int dx, int dy, int z, _, _) in saved[i].Tiles ?? new())
                {
                    int x = dx + origin.X;
                    int y = dy + origin.Y;
                    if (InBounds(x, y) && Cell(x, y).FirstOrDefault(t => t.Layer == first && t.Id == id && t.Z == z) is { } tile)
                    {
                        tile.Layer = layer;
                    }
                }
            }

            // A tile that sat on two layers is in the multi once and went to one of them: the
            // other layers that list it get a copy.
            for (int i = 0; i < saved.Count; i++)
            {
                if (!saved[i].InFile)
                {
                    continue;
                }

                foreach ((ushort id, int dx, int dy, int z, bool invisible, int extraField) in saved[i].Tiles ?? new())
                {
                    int x = dx + origin.X;
                    int y = dy + origin.Y;
                    if (InBounds(x, y) && Cell(x, y).All(t => t.Layer != _layers[i] || t.Id != id || t.Z != z) &&
                        Cell(x, y).Any(t => t.Id == id && t.Z == z))
                    {
                        GridInsert(new MultiTile(id, x, y, z, invisible) { Extra = extraField, Solver = ++_solverCounter, Layer = _layers[i] });
                    }
                }
            }

            // After the claims, so a claim never takes a tile that only the layers file has.
            for (int i = 0; i < saved.Count; i++)
            {
                if (saved[i].InFile)
                {
                    continue;
                }

                foreach ((ushort id, int dx, int dy, int z, bool invisible, int extraField) in saved[i].Tiles ?? new())
                {
                    GridInsert(new MultiTile(id, dx + origin.X, dy + origin.Y, ClampZ(z), invisible)
                    {
                        Extra = extraField,
                        Solver = ++_solverCounter,
                        Layer = _layers[i]
                    });
                }
            }

            ActiveLayer = _layers[Math.Clamp(activeIndex, 0, _layers.Count - 1)];
            RecalculateMinMax();
        }
    }
}
