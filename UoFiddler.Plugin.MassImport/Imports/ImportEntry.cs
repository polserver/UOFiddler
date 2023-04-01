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
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.MassImport.Imports
{
    public abstract class ImportEntry
    {
        public virtual int MaxIndex => 0x3FFF;

        public abstract void Import(bool direct, ref Dictionary<string, bool> changedClasses);

        public abstract string Name { get; }

        public string File { get; set; }

        public int Index { get; set; }

        public bool Remove { get; set; }

        public bool Valid { get; set; }

        protected virtual void TestFile(ref string message)
        {
            if (File.Contains(".bmp") || File.Contains(".tiff"))
            {
                return;
            }

            message += " Invalid image format";
            Valid = false;
        }

        public string Test()
        {
            string message = $"{Name}: ({Index})";
            Valid = true;

            if (Index < 0)
            {
                message += " Invalid Index ";
                Valid = false;
            }

            if (Index > MaxIndex)
            {
                message += " Invalid Index ";
                Valid = false;
            }

            if (!Remove)
            {
                if (!System.IO.File.Exists(File))
                {
                    message += $" File not found ({File}) ";
                    Valid = false;
                }
                else
                {
                    TestFile(ref message);
                }
            }

            if (!Valid)
            {
                return message;
            }

            message += !Remove ? $" Add/Replace ({File})" : " Remove";

            return message;
        }

        protected bool GetTileDataInfo(string fileName, ref string message, ref string[] tiledata)
        {
            try
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    while (sr.ReadLine() is { } line)
                    {
                        line = line.Trim();

                        if (line.Length == 0 || line.StartsWith("#"))
                        {
                            continue;
                        }

                        if (line.StartsWith("ID;"))
                        {
                            continue;
                        }

                        string[] split = line.Split(';');

                        if (split.Length < 36)
                        {
                            continue;
                        }

                        if (!Utils.ConvertStringToInt(split[0], out int id))
                        {
                            continue;
                        }

                        if (Index != id)
                        {
                            continue;
                        }

                        tiledata = split;

                        return true;
                    }
                }
            }
            catch
            {
                // ignored
            }

            message += " No Tiledata information found";

            return false;
        }
    }
}