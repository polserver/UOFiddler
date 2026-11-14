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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// Asks which blueprint group several multis are saved to, optionally one group each by
    /// something the caller knows (a design's category, the file it came from). Shown through
    /// <see cref="Run"/>.
    /// </summary>
    internal partial class BlueprintSaveDialog : Form
    {
        private const string _noGroup = "(no group)";
        private const string _caption = "Save as blueprints";

        public BlueprintSaveDialog()
        {
            InitializeComponent();
        }

        /// <param name="splitText">The text of the "one group each" check box; null hides it.</param>
        public BlueprintSaveDialog(string summary, IEnumerable<string> groups, string group, string splitText)
            : this()
        {
            _summary.Text = summary;
            _group.Items.Add(_noGroup);
            foreach (string name in groups)
            {
                _group.Items.Add(name);
            }

            _group.Text = string.IsNullOrEmpty(group) ? _noGroup : group;
            if (splitText != null)
            {
                _split.Text = splitText;
                _split.Visible = true;
            }
        }

        /// <summary>The group typed or chosen, made safe for a folder name; empty for no group.</summary>
        public string Group
        {
            get
            {
                string text = _group.Text.Trim();
                return text.Length == 0 || text == _noGroup ? string.Empty : Blueprints.SafeName(text);
            }
        }

        /// <summary>Whether each multi goes to its own group, as the caller decides.</summary>
        public bool Split => _split.Visible && _split.Checked;

        /// <summary>
        /// Asks for the group and writes the blueprints, then lists any that could not be written.
        /// Null when cancelled.
        /// </summary>
        /// <param name="showGroup">The group to show afterwards: the one chosen, or null (all) when split.</param>
        /// <param name="splitText">Offers one group per source with this check box text, using <paramref name="splitGroup"/>.</param>
        /// <param name="splitGroup">The group of a source when split; empty puts it in the chosen group.</param>
        public static BlueprintSaveResult Run(IWin32Window owner, IReadOnlyList<BlueprintSource> sources, out string showGroup,
            string splitText = null, Func<BlueprintSource, string> splitGroup = null)
        {
            showGroup = null;
            if (sources.Count == 0)
            {
                return null;
            }

            int large = sources.Count(s => s.TileCount > Blueprints.LargeBlueprintTiles);
            string summary = sources.Count == 1 ? $"Save \"{sources[0].Name}\" as a blueprint." : $"Save {sources.Count:N0} multis as blueprints.";
            if (large > 0)
            {
                summary += large == sources.Count && large == 1
                    ? $" It has {sources[0].TileCount:N0} tiles, which is slow to place and preview."
                    : $" {large:N0} of them have more than {Blueprints.LargeBlueprintTiles:N0} tiles, which is slow to place and preview.";
            }

            string group;
            bool split;
            using (var dialog = new BlueprintSaveDialog(summary, Blueprints.Groups(), Blueprints.LastGroup, splitGroup != null ? splitText : null))
            {
                if (dialog.ShowDialog(owner) != DialogResult.OK)
                {
                    return null;
                }

                group = dialog.Group;
                split = dialog.Split;
            }

            Blueprints.LastGroup = group;
            Func<BlueprintSource, string> groupOf = split
                ? s => splitGroup(s) is { Length: > 0 } own ? own : group
                : _ => group;

            BlueprintSaveResult result;
            Cursor previous = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                result = Blueprints.SaveMany(sources, groupOf);
            }
            finally
            {
                Cursor.Current = previous;
            }

            if (result.Failures.Count > 0)
            {
                var text = new StringBuilder($"{result.Saved:N0} saved, {result.Failures.Count:N0} could not be saved:\r\n\r\n");
                foreach (string failure in result.Failures.Take(20))
                {
                    text.Append(failure).Append("\r\n");
                }

                if (result.Failures.Count > 20)
                {
                    text.Append($"...and {result.Failures.Count - 20:N0} more");
                }

                MessageBox.Show(owner, text.ToString(), _caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            showGroup = split ? null : group;
            return result;
        }

        /// <summary>The status line after a save, for the dialog the multis were picked in.</summary>
        public static string Describe(BlueprintSaveResult result, string showGroup)
        {
            string where = showGroup == null ? "their groups" : showGroup.Length == 0 ? "no group" : $"group \"{showGroup}\"";
            return $"Saved {result.Saved:N0} blueprint(s) to {where}.";
        }
    }
}
