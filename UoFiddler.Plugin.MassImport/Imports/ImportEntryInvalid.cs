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

namespace UoFiddler.Plugin.MassImport.Imports
{
    public class ImportEntryInvalid : ImportEntry
    {
        public override string Name => "Invalid";

        protected override void TestFile(ref string message)
        {
            Valid = false;
        }

        public override void Import(bool direct, ref Dictionary<string, bool> changedClasses)
        {
        }
    }
}