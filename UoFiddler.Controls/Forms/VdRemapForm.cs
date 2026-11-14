/***************************************************************************
 *
 * $Author: Turley
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
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    /// <summary>
    /// Picks, for each action of a target body type's layout, which action of the source body goes there,
    /// for a .vd export in that layout.
    /// </summary>
    public partial class VdRemapForm : Form
    {
        private static readonly MobType[] _layouts = { MobType.Monster, MobType.Sea, MobType.Animal, MobType.Human };

        private readonly MobType _sourceType;
        private readonly string[] _sourceNames;
        private readonly bool[] _sourceHasFrames;
        private readonly Func<MobType, string[]> _actionNames;
        private readonly List<ComboBox> _rows = new();

        /// <param name="body">Body being exported, for the title.</param>
        /// <param name="sourceType">The body's own action layout.</param>
        /// <param name="sourceHasFrames">For each action of the body, whether it has frames.</param>
        /// <param name="actionNames">Action names of a layout.</param>
        public VdRemapForm(int body, MobType sourceType, bool[] sourceHasFrames, Func<MobType, string[]> actionNames)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            _sourceType = sourceType;
            _sourceHasFrames = sourceHasFrames;
            _actionNames = actionNames;
            string[] names = actionNames(sourceType);
            _sourceNames = Enumerable.Range(0, sourceHasFrames.Length)
                .Select(i => i < names.Length ? names[i] : $"Action {i}")
                .ToArray();

            Text = $"Body {body} to .vd, remapped";
            SourceLabel.Text = $"Source: body {body}, {DescribeLayout(sourceType)}";

            foreach (MobType layout in _layouts)
            {
                LayoutComboBox.Items.Add(DescribeLayout(layout));
            }

            // Start on another layout than the body's own; that is what a remap is for
            MobType start = sourceType == MobType.Animal ? MobType.Monster : MobType.Animal;
            LayoutComboBox.SelectedIndex = Array.IndexOf(_layouts, start);
        }

        public MobType TargetLayout => _layouts[LayoutComboBox.SelectedIndex];

        /// <summary>For each action of the target layout, the source action written there, or -1 for none.</summary>
        public int[] SourceActions => _rows.Select(row => row.SelectedIndex - 1).ToArray();

        public int ScalePercent => (int)ScaleUpDown.Value;

        private static string DescribeLayout(MobType layout)
        {
            return layout switch
            {
                MobType.Monster => "monster (22 actions)",
                MobType.Sea => "sea monster (9 actions, saved as a 22-action monster .vd)",
                MobType.Animal => "animal (13 actions)",
                MobType.Equipment => "equipment (35 actions)",
                _ => "human (35 actions)"
            };
        }

        private void OnLayoutChanged(object sender, EventArgs e)
        {
            BuildRows();
        }

        private void OnClickDefaults(object sender, EventArgs e)
        {
            ApplyDefaults();
        }

        private void BuildRows()
        {
            MobType layout = TargetLayout;
            IReadOnlyList<ActionRole> roles = MobActionMap.GetRoles(layout);
            string[] targetNames = _actionNames(layout);

            var sourceItems = new List<string> { "(empty)" };
            for (int i = 0; i < _sourceNames.Length; ++i)
            {
                sourceItems.Add(_sourceHasFrames[i] ? $"{i:D2} {_sourceNames[i]}" : $"{i:D2} {_sourceNames[i]} (no frames)");
            }

            MappingTable.SuspendLayout();
            MappingTable.Controls.Clear();
            MappingTable.RowStyles.Clear();
            _rows.Clear();
            MappingTable.RowCount = roles.Count;

            for (int target = 0; target < roles.Count; ++target)
            {
                string name = target < targetNames.Length ? targetNames[target] : $"Action {target}";
                var label = new Label
                {
                    Text = $"{target:D2} {name}",
                    AutoSize = true,
                    Anchor = AnchorStyles.Left
                };
                var combo = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Dock = DockStyle.Fill
                };
                combo.Items.AddRange(sourceItems.ToArray<object>());

                MappingTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                MappingTable.Controls.Add(label, 0, target);
                MappingTable.Controls.Add(combo, 1, target);
                _rows.Add(combo);
            }

            MappingTable.ResumeLayout();
            ApplyDefaults();
        }

        /// <summary>
        /// Fills every row with the source action that shows the closest thing, or empty when that action has no frames.
        /// </summary>
        private void ApplyDefaults()
        {
            MobType layout = TargetLayout;
            for (int target = 0; target < _rows.Count; ++target)
            {
                int source = MobActionMap.MapAction(layout, target, _sourceType);
                if (source >= _sourceHasFrames.Length || (source >= 0 && !_sourceHasFrames[source]))
                {
                    source = -1;
                }

                _rows[target].SelectedIndex = source + 1;
            }
        }
    }
}
