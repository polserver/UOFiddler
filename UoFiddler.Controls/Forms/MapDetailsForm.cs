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
using System.Drawing;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class MapDetailsForm : Form
    {
        private Func<string, string?>? _localizationGetter;
        private Map _currentMap;
        private Point _point;

        public MapDetailsForm(Map currentMap, Point point)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();
            TopMost = true;

            _currentMap = currentMap;
            _point = point;

            // 先生成英文内容
            GenerateContent();
        }

        public void SetLocalization(Func<string, string?> getLocalized)
        {
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;

            this.Text = _localizationGetter("Forms.MapDetailsForm.Title") ?? "MapDetails";
            
            // 重新生成内容以应用汉化
            GenerateContent();
        }

        private void GenerateContent()
        {
            richTextBox.Clear();

            string coordLabel = _localizationGetter?.Invoke("Forms.MapDetailsForm.Labels.Coordinate") ?? "X";
            string landTileLabel = _localizationGetter?.Invoke("Forms.MapDetailsForm.Labels.LandTile") ?? "LandTile";
            string altitudeLabel = _localizationGetter?.Invoke("Forms.MapDetailsForm.Labels.Altitude") ?? "Altitude";
            string staticsLabel = _localizationGetter?.Invoke("Forms.MapDetailsForm.Labels.Statics") ?? "Statics";
            string hueLabel = _localizationGetter?.Invoke("Forms.MapDetailsForm.Labels.Hue") ?? "Hue";

            Tile currentTile = _currentMap.Tiles.GetLandTile(_point.X, _point.Y);
            richTextBox.AppendText($"{coordLabel}: {_point.X} Y: {_point.Y}\n\n");
            richTextBox.AppendText($"{landTileLabel}:\n");
            richTextBox.AppendText($"{TileData.LandTable[currentTile.Id].Name}: 0x{currentTile.Id:X} {altitudeLabel}: {currentTile.Z}\n\n");
            HuedTile[] staticsAtPoint = _currentMap.Tiles.GetStaticTiles(_point.X, _point.Y);
            richTextBox.AppendText($"{staticsLabel}:\n");
            foreach (HuedTile @static in staticsAtPoint)
            {
                ushort id = @static.Id;
                richTextBox.AppendText($"{TileData.ItemTable[id].Name}: 0x{id:X} {hueLabel}: {@static.Hue} {altitudeLabel}: {@static.Z}\n");
            }
        }
    }
}
