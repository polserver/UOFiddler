using System;

namespace FiddlerControls.Helpers
{
    static class StringExtensions
    {
        public static bool ContainsCaseInsensitive(this string haystack, string needle)
        {
            return haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
