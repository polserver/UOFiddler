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