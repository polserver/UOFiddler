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
using System.Drawing;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.MassImport.Imports
{
    public class ImportEntryLandTile : ImportEntry
    {
        public override string Name => "LandTile";

        public override void Import(bool direct, ref Dictionary<string, bool> changedClasses)
        {
            if (!Remove)
            {
                using (var bmpTemp = new Bitmap(File))
                {
                    Bitmap bitmap = new Bitmap(bmpTemp);

                    if (File.Contains(".bmp"))
                    {
                        bitmap = Utils.ConvertBmp(bitmap);
                    }

                    Ultima.Art.ReplaceLand(Index, bitmap);
                }
            }
            else
            {
                Ultima.Art.RemoveLand(Index);
            }

            if (!direct)
            {
                ControlEvents.FireLandTileChangeEvent(this, Index);
                Options.ChangedUltimaClass["Art"] = true;
            }

            changedClasses["Art"] = true;
        }
    }
}