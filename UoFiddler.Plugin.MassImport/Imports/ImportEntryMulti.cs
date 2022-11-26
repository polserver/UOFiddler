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
using System.IO;
using System.Linq;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.MassImport.Imports
{
    internal class ImportEntryMulti : ImportEntry
    {
        public override int MaxIndex => Multis.MaximumMultiIndex;

        public override string Name => "Multi";

        public override void Import(bool direct, ref Dictionary<string, bool> changedClasses)
        {
            if (!Remove)
            {
                var fileExtension = Path.GetExtension(File);

                Multis.ImportType importType;

                switch (fileExtension)
                {
                    case ".txt":
                        importType = Multis.ImportType.TXT;
                        break;
                    case ".wsc":
                        importType = Multis.ImportType.WSC;
                        break;
                    case ".uoa":
                        importType = Multis.ImportType.UOA;
                        break;
                    case ".uoab":
                        importType = Multis.ImportType.UOAB;
                        break;
                    case ".csv":
                        importType = Multis.ImportType.CSV;
                        break;
                    default:
                        importType = Multis.ImportType.TXT;
                        break;
                }

                Multis.ImportFromFile(Index, File, importType);
            }
            else
            {
                Multis.Remove(Index);
            }

            if (!direct)
            {
                ControlEvents.FireMultiChangeEvent(this, Index);

                Options.ChangedUltimaClass["Multis"] = true;
            }

            changedClasses["Multis"] = true;
        }

        protected override void TestFile(ref string message)
        {
            string[] validExtensions = new [] {".txt", ".wsc", ".csv", ".uoa", ".uoab"};

            var fileExtension = Path.GetExtension(File);

            if (!validExtensions.Contains(fileExtension))
            {
                message += $" Invalid multi file format ({File})";

                Valid = false;
            }
            else
            {
                Valid = true;
            }
        }
    }
}