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
    public class ImportEntryItem : ImportEntry
    {
        public override int MaxIndex => Ultima.Art.GetMaxItemId();

        public override string Name => "Item";

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

                    Ultima.Art.ReplaceStatic(Index, bitmap);
                }
            }
            else
            {
                Ultima.Art.RemoveStatic(Index);
            }

            if (!direct)
            {
                ControlEvents.FireItemChangeEvent(this, Index);
                Options.ChangedUltimaClass["Art"] = true;
            }

            changedClasses["Art"] = true;
        }
    }
}