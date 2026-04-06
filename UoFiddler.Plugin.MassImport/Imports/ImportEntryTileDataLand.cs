// /***************************************************************************
//  *
//  * $Author: Turley
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System.Collections.Generic;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.MassImport.Imports
{
    public class ImportEntryTileDataLand : ImportEntry
    {
        private string[] _tiledata;

        public override string Name => "TileDataLand";

        // Minimum column counts: 35 for pre-HSA clients (1 ID + 2 fields + 32 flags),
        // 66 for HSA clients (1 ID + 2 fields + 63 flags).
        private const int _landDataColumnsHsa = 66;

        protected override void TestFile(ref string message)
        {
            if (!File.Contains(".csv"))
            {
                message += " Invalid file format";
                Valid = false;
            }
            else
            {
                Valid = GetTileDataInfo(File, ref message, ref _tiledata);
                if (Valid && Ultima.Art.IsUOAHS() && _tiledata.Length < _landDataColumnsHsa)
                {
                    message += " (old-format CSV: missing extended HSA flags will default to 0)";
                }
            }
        }

        public override void Import(bool direct, ref Dictionary<string, bool> changedClasses)
        {
            Ultima.TileData.LandTable[Index].ReadData(_tiledata);
            if (!direct)
            {
                Options.ChangedUltimaClass["TileData"] = true;
                ControlEvents.FireTileDataChangeEvent(this, Index);
            }

            changedClasses["TileData"] = true;
        }
    }
}