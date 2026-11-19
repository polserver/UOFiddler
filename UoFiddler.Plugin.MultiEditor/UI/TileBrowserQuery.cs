/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// What the search box asks for: either ids (decimal, 0x or $ hex, ranges like 0x100-0x1FF,
    /// several separated by spaces or commas) or name words that must all occur.
    /// </summary>
    internal sealed class TileBrowserQuery
    {
        private static readonly char[] _separators = { ' ', ',', ';', '\t' };

        private readonly List<(int Low, int High)> _ranges;
        private readonly string[] _words;

        private TileBrowserQuery(List<(int, int)> ranges, string[] words)
        {
            _ranges = ranges;
            _words = words;
        }

        public bool IsIdQuery => _ranges != null;

        /// <summary>The query, or null when the text is blank.</summary>
        public static TileBrowserQuery Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            string[] tokens = text.Split(_separators, StringSplitOptions.RemoveEmptyEntries);
            var ranges = new List<(int, int)>();
            foreach (string token in tokens)
            {
                if (!TryParseRange(token, out int low, out int high))
                {
                    ranges = null;
                    break;
                }

                ranges.Add((low, high));
            }

            return ranges != null ? new TileBrowserQuery(ranges, null) : new TileBrowserQuery(null, tokens);
        }

        public bool MatchesId(int id)
        {
            if (_ranges == null)
            {
                return false;
            }

            foreach ((int low, int high) in _ranges)
            {
                if (id >= low && id <= high)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>True when every word occurs in the text (ignoring case). Id queries match no text.</summary>
        public bool MatchesText(string text)
        {
            if (_words == null || string.IsNullOrEmpty(text))
            {
                return false;
            }

            foreach (string word in _words)
            {
                if (text.IndexOf(word, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool MatchesTile(int id) => IsIdQuery ? MatchesId(id) : MatchesText(TileBrowserThumbnails.NameOf(id));

        /// <summary>An id query matches when one of the ids does, a text query when the text does.</summary>
        public bool Matches(IEnumerable<int> ids, string text) => IsIdQuery ? ids.Any(MatchesId) : MatchesText(text);

        /// <inheritdoc cref="Matches(IEnumerable{int}, string)"/>
        public bool Matches(IEnumerable<ushort> ids, string text) => IsIdQuery ? ids.Any(id => MatchesId(id)) : MatchesText(text);

        private static bool TryParseRange(string token, out int low, out int high)
        {
            int dash = token.IndexOf('-', 1);
            if (dash > 0 && dash < token.Length - 1)
            {
                if (TryParseId(token[..dash], out low) && TryParseId(token[(dash + 1)..], out high))
                {
                    if (low > high)
                    {
                        (low, high) = (high, low);
                    }

                    return true;
                }

                high = 0;
                return false;
            }

            bool ok = TryParseId(token, out low);
            high = low;
            return ok;
        }

        private static bool TryParseId(string text, out int id)
        {
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(text.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out id);
            }

            if (text.StartsWith('$'))
            {
                return int.TryParse(text.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out id);
            }

            return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out id);
        }
    }
}
