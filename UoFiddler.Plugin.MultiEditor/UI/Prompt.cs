/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Windows.Forms;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// Small input dialogs.
    /// </summary>
    internal static class Prompt
    {
        /// <summary>Asks for whole numbers. Returns null when cancelled.</summary>
        public static int[] Numbers(IWin32Window owner, string title, params (string Label, int Value, int Min, int Max)[] fields)
        {
            using var form = new NumbersPromptForm(title, fields);
            if (form.ShowDialog(owner) != DialogResult.OK)
            {
                return null;
            }

            return form.Values;
        }

        /// <summary>Asks for one line of text. Returns null when cancelled or left empty.</summary>
        public static string Text(IWin32Window owner, string title, string label, string value = "")
        {
            using var form = new TextPromptForm(title, label, value);
            return form.ShowDialog(owner) == DialogResult.OK && form.Value.Trim().Length > 0 ? form.Value.Trim() : null;
        }

        /// <summary>Reads a tile id typed as decimal or 0x hex.</summary>
        public static bool TryParseId(string text, out ushort id)
        {
            text = (text ?? string.Empty).Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return ushort.TryParse(text.AsSpan(2), System.Globalization.NumberStyles.HexNumber, null, out id);
            }

            return ushort.TryParse(text, out id);
        }
    }
}
