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
    public class ImportEntryGump : ImportEntry
    {
        public override int MaxIndex => 0xFFFF;

        public override string Name => "Gump";

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

                    Ultima.Gumps.ReplaceGump(Index, bitmap);
                }
            }
            else
            {
                Ultima.Gumps.RemoveGump(Index);
            }

            if (!direct)
            {
                ControlEvents.FireGumpChangeEvent(this, Index);
                Options.ChangedUltimaClass["Gumps"] = true;
            }

            changedClasses["Gumps"] = true;
        }
    }
}