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
    public class ImportEntryHue : ImportEntry
    {
        public override int MaxIndex => 3000;

        public override string Name => "Hue";

        protected override void TestFile(ref string message)
        {
            if (!File.Contains(".txt"))
            {
                message += " Invalid file format";
                Valid = false;
            }
            else
            {
                Valid = true;
            }
        }

        public override void Import(bool direct, ref Dictionary<string, bool> changedClasses)
        {
            if (!Remove)
            {
                Ultima.Hues.List[Index].Import(File);
            }
            else
            {
                Ultima.Hues.List[Index].Name = "";

                for (int i = 0; i < Ultima.Hues.List[Index].Colors.Length; ++i)
                {
                    Ultima.Hues.List[Index].Colors[i] = 0;
                }

                Ultima.Hues.List[Index].TableStart = 0;
                Ultima.Hues.List[Index].TableEnd = 0;
            }

            if (!direct)
            {
                ControlEvents.FireHueChangeEvent();
                Options.ChangedUltimaClass["Hues"] = true;
            }

            changedClasses["Hues"] = true;
        }
    }
}