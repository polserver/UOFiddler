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

using System.Globalization;
using Ultima;

namespace UoFiddler.Controls.Classes.TilesInfo
{
    public sealed class TileSlot
    {
        private readonly string[] _rawRow;
        private readonly int _columnIndex;

        public string[] RawRow => _rawRow;
        public string Label { get; }

        public int TileId
        {
            get => int.TryParse(_rawRow[_columnIndex], NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : 0;
            set => _rawRow[_columnIndex] = value.ToString(CultureInfo.InvariantCulture);
        }

        public TileStyle Style { get; internal set; }

        public bool IsEmpty => TileId == 0;

        public bool IsValid => !IsEmpty && Art.IsValidStatic(TileId);

        public TileSlot(string label, string[] rawRow, int columnIndex)
        {
            Label = label;
            _rawRow = rawRow;
            _columnIndex = columnIndex;
        }
    }
}
