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

using System;
using System.Text.RegularExpressions;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.Classes
{
    public static class SearchHelper
    {
        public static Func<string, string, SearchResult> GetSearchMethod(bool useRegex = false)
        {
            return useRegex
                ? (Func<string, string, SearchResult>)FindEntryWithRegex
                : FindEntryUsingStringComparison;
        }

        private static SearchResult FindEntryUsingStringComparison(string pattern, string input)
        {
            var found = !string.IsNullOrEmpty(input) && input.ContainsCaseInsensitive(pattern);

            return new SearchResult(found);
        }

        private static SearchResult FindEntryWithRegex(string pattern, string input)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return new SearchResult(false);
            }
            try
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                return new SearchResult(regex.IsMatch(input));
            }
            catch
            {
                return new SearchResult(false, true);
            }
        }
    }
}
