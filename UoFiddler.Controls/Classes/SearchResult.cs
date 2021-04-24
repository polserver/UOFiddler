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

namespace UoFiddler.Controls.Classes
{
    public sealed class SearchResult
    {
        public bool EntryFound { get; }
        public bool HasErrors { get; }

        public SearchResult(bool entryFound, bool hasErrors = false)
        {
            EntryFound = entryFound;
            HasErrors = hasErrors;
        }
    }
}