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
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Editing;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// Lets the user split a design into levels: each row a name and the Z the level starts at.
    /// A level runs up to where the next one starts; the lowest also takes what is below it and
    /// the highest what is above. Presets fill in the usual splits.
    /// </summary>
    internal sealed partial class FloorsDialog : Form
    {
        private readonly EditorSession _session;

        public FloorsDialog(EditorSession session)
        {
            _session = session;

            InitializeComponent();

            buttonDetected.Text = $"Detected first floor (Z {session.FirstFloorZ})";
            Fill(session.LevelDefinitions);
        }

        /// <summary>The levels as edited, valid after the dialog returned OK.</summary>
        public List<LevelDefinition> Result { get; } = new List<LevelDefinition>();

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            AddAbove();
        }

        private void ButtonRemove_Click(object sender, EventArgs e)
        {
            RemoveSelected();
        }

        private void ButtonGround_Click(object sender, EventArgs e)
        {
            Fill(EditorSession.StandardLevels(0));
        }

        private void ButtonFoundation_Click(object sender, EventArgs e)
        {
            Fill(EditorSession.StandardLevels(7));
        }

        private void ButtonDetected_Click(object sender, EventArgs e)
        {
            Fill(EditorSession.StandardLevels(_session.FirstFloorZ));
        }

        private void ButtonFitted_Click(object sender, EventArgs e)
        {
            Fill(_session.FittedLevels());
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            Accept();
        }

        private void Fill(IEnumerable<LevelDefinition> levels)
        {
            _grid.Rows.Clear();
            foreach (LevelDefinition level in levels)
            {
                _grid.Rows.Add(level.Name, level.StartZ.ToString());
            }
        }

        /// <summary>Adds a level one storey (20) above the highest, named as the next floor.</summary>
        private void AddAbove()
        {
            int top = int.MinValue;
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (int.TryParse(Convert.ToString(row.Cells[1].Value), out int z))
                {
                    top = Math.Max(top, z);
                }
            }

            int start = top == int.MinValue ? 0 : Math.Min(sbyte.MaxValue, top + 20);
            _grid.Rows.Add($"Level {_grid.Rows.Count + 1}", start.ToString());
            _grid.CurrentCell = _grid.Rows[^1].Cells[0];
        }

        private void RemoveSelected()
        {
            if (_grid.CurrentRow != null && _grid.Rows.Count > 1)
            {
                _grid.Rows.Remove(_grid.CurrentRow);
            }
        }

        private void Accept()
        {
            _grid.EndEdit();
            Result.Clear();
            var starts = new HashSet<int>();
            foreach (DataGridViewRow row in _grid.Rows)
            {
                string name = Convert.ToString(row.Cells[0].Value)?.Trim();
                if (!int.TryParse(Convert.ToString(row.Cells[1].Value), out int z) || z < sbyte.MinValue || z > sbyte.MaxValue)
                {
                    Complain(row, $"Enter a whole number from {sbyte.MinValue} to {sbyte.MaxValue} for where the level starts.");
                    return;
                }

                if (!starts.Add(z))
                {
                    Complain(row, $"Two levels start at Z {z}. Each level needs its own start.");
                    return;
                }

                Result.Add(new LevelDefinition(string.IsNullOrEmpty(name) ? $"Level {Result.Count + 1}" : name, z));
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void Complain(DataGridViewRow row, string message)
        {
            _grid.CurrentCell = row.Cells[1];
            MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
