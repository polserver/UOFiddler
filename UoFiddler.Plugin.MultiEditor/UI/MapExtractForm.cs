/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>Asks for the facet, the rectangle and the Z range to extract statics from.</summary>
    internal sealed partial class MapExtractForm : Form
    {
        public MapExtractForm(MapArea initial, bool canPlaceIntoDesign)
        {
            InitializeComponent();

            foreach (var (name, _) in MapExtract.Facets)
            {
                _facet.Items.Add(name);
            }

            int facetIndex = Array.FindIndex(MapExtract.Facets, f => f.Map() == initial.Map);
            _facet.SelectedIndex = Math.Max(0, facetIndex);

            SetValue(_x, initial.X);
            SetValue(_y, initial.Y);
            SetValue(_width, initial.Width);
            SetValue(_height, initial.Height);
            SetValue(_minZ, initial.MinZ);
            SetValue(_maxZ, initial.MaxZ);
            _newDesign.Checked = initial.NewDesign || !canPlaceIntoDesign;
            _newDesign.Enabled = canPlaceIntoDesign;
        }

        /// <summary>The area as entered, valid after the form returned OK.</summary>
        public MapArea Area => new MapArea(MapExtract.Facets[_facet.SelectedIndex].Map(), (int)_x.Value, (int)_y.Value, (int)_width.Value, (int)_height.Value,
            (int)Math.Min(_minZ.Value, _maxZ.Value), (int)Math.Max(_minZ.Value, _maxZ.Value), _newDesign.Checked);

        private static void SetValue(NumericUpDown box, int value)
        {
            box.Value = Math.Clamp(value, (int)box.Minimum, (int)box.Maximum);
        }
    }
}
