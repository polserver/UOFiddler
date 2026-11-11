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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    /// <summary>
    /// Edits skillgrp.mul as a tree: groups at the root (Misc first and fixed), every skill under exactly one group.
    /// Skills come from the file and skills.idx; they can be moved between groups but not removed.
    /// </summary>
    public partial class SkillGroupControl : UserControl
    {
        public SkillGroupControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        private bool _loaded;
        private int _invalidGroupCount;

        private static Color MiscColor => Options.DarkMode ? Color.CornflowerBlue : Color.Blue;

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (_loaded)
            {
                OnLoad(this, EventArgs.Empty);
            }
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            using (new WaitCursorScope(this))
            {
                Options.LoadedUltimaClass["SkillGrp"] = true;

                var groupNodes = new List<TreeNode>();
                foreach (SkillGroup group in SkillGroups.List)
                {
                    groupNodes.Add(new TreeNode(group.Name));
                }

                groupNodes[0].ForeColor = MiscColor;
                groupNodes[0].ToolTipText = "Skills without a group. Misc is always the first group and can't be renamed or removed.";

                // Every skill gets a node: those in the file and those only in skills.idx
                int entryCount = Math.Max(SkillGroups.SkillList.Count, Skills.SkillEntries.Count);
                _invalidGroupCount = 0;
                for (int id = 0; id < entryCount; ++id)
                {
                    int group = id < SkillGroups.SkillList.Count ? SkillGroups.SkillList[id] : 0;
                    if (group < 0 || group >= groupNodes.Count)
                    {
                        // Points at a group that doesn't exist; show it under Misc, where saving puts it
                        group = 0;
                        _invalidGroupCount++;
                    }

                    groupNodes[group].Nodes.Add(CreateSkillNode(id));
                }

                treeView1.BeginUpdate();
                treeView1.Nodes.Clear();
                treeView1.Nodes.AddRange(groupNodes.ToArray());
                treeView1.EndUpdate();

                UpdateStatus();

                if (!_loaded)
                {
                    ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                }

                _loaded = true;
            }
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private static TreeNode CreateSkillNode(int id)
        {
            string name = Skills.GetSkill(id)?.Name;
            var node = new TreeNode(name == null ? $"{id}: (not in skills.idx)" : $"{id}: {name}")
            {
                Tag = id
            };

            if (id >= SkillGroups.ClientSkillLimit)
            {
                node.ForeColor = Color.Gray;
                node.ToolTipText = $"The client only reads the first {SkillGroups.ClientSkillLimit} skill entries; this skill won't appear in its skill groups.";
            }

            return node;
        }

        private static bool IsSkill(TreeNode node) => node?.Tag is int;

        private static bool IsMisc(TreeNode node) => node != null && node.Parent == null && node.Index == 0;

        /// <summary>
        /// Adds a skill to a group, keeping the group's skills in id order.
        /// </summary>
        private static void InsertSkill(TreeNode group, TreeNode skill)
        {
            int id = (int)skill.Tag;
            int index = 0;
            while (index < group.Nodes.Count && (int)group.Nodes[index].Tag < id)
            {
                index++;
            }

            group.Nodes.Insert(index, skill);
        }

        private void MarkChanged()
        {
            Options.ChangedUltimaClass["SkillGrp"] = true;
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            int groups = treeView1.Nodes.Count;
            int entries = treeView1.Nodes.Cast<TreeNode>().Sum(group => group.Nodes.Count);

            var lines = new List<string>
            {
                $"{groups} groups including Misc (client limit {SkillGroups.ClientGroupLimit}). " +
                $"{entries} skills, {Skills.SkillEntries.Count} in skills.idx; the client reads the first {SkillGroups.ClientSkillLimit}.",
            };

            bool warn = false;
            if (groups > SkillGroups.ClientGroupLimit)
            {
                lines.Add($"More than {SkillGroups.ClientGroupLimit} groups: the client can't load this file safely.");
                warn = true;
            }

            if (_invalidGroupCount > 0)
            {
                lines.Add($"{_invalidGroupCount} skills pointed at a missing group and are shown under Misc.");
                warn = true;
            }

            lines.Add(@"After replacing skillgrp.mul, delete the copies the client keeps in its Desktop\<account>\<shard>\<character> and Desktop\common folders.");

            StatusLabel.Text = string.Join(Environment.NewLine, lines);
            StatusLabel.ForeColor = warn ? (Options.DarkMode ? Color.OrangeRed : Color.Red) : SystemColors.ControlText;
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            int entryCount = Math.Max(SkillGroups.SkillList.Count,
                treeView1.Nodes.Cast<TreeNode>().SelectMany(group => group.Nodes.Cast<TreeNode>()).Select(skill => (int)skill.Tag + 1).DefaultIfEmpty(0).Max());

            var assignment = new int[entryCount];
            SkillGroups.List.Clear();
            foreach (TreeNode group in treeView1.Nodes)
            {
                SkillGroups.List.Add(new SkillGroup(group.Text));
                foreach (TreeNode skill in group.Nodes)
                {
                    assignment[(int)skill.Tag] = group.Index;
                }
            }

            SkillGroups.SkillList.Clear();
            SkillGroups.SkillList.AddRange(assignment);
            SkillGroups.Save(Options.OutputPath);

            _invalidGroupCount = 0;
            Options.ChangedUltimaClass["SkillGrp"] = false;
            UpdateStatus();

            FileSavedDialog.Show(FindForm(), Options.OutputPath, "SkillGrp saved successfully.");
        }

        private void OnNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // Right-click acts on the node under the mouse
            if (e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = e.Node;
            }
        }

        private void OnItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Item is TreeNode node && !IsMisc(node))
            {
                DoDragDrop(node, DragDropEffects.Move);
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(TreeNode)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            var source = (TreeNode)e.Data.GetData(typeof(TreeNode));
            TreeNode target = treeView1.GetNodeAt(treeView1.PointToClient(new Point(e.X, e.Y)));
            if (source == null || target == null || source == target)
            {
                return;
            }

            if (IsSkill(source))
            {
                // A skill dropped on a group or on one of its skills joins that group
                TreeNode group = IsSkill(target) ? target.Parent : target;
                if (group == source.Parent)
                {
                    return;
                }

                source.Remove();
                InsertSkill(group, source);
            }
            else
            {
                // A group dropped on another group takes its place; Misc stays first
                TreeNode group = IsSkill(target) ? target.Parent : target;
                if (group == source)
                {
                    return;
                }

                int index = Math.Max(1, group.Index);
                source.Remove();
                treeView1.Nodes.Insert(index, source);
            }

            source.EnsureVisible();
            treeView1.SelectedNode = source;
            MarkChanged();
        }

        private void OnOpeningContext(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TreeNode node = treeView1.SelectedNode;
            bool editableGroup = node != null && !IsSkill(node) && !IsMisc(node);
            removeToolStripMenuItem.Enabled = editableGroup;
            renameToolStripMenuItem.Enabled = editableGroup;
        }

        private void OnClickAdd(object sender, EventArgs e)
        {
            if (treeView1.Nodes.Count >= SkillGroups.ClientGroupLimit)
            {
                MessageBox.Show($"The client supports at most {SkillGroups.ClientGroupLimit} groups, Misc included.",
                    "Skill Groups", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var group = new TreeNode("New Group");
            treeView1.Nodes.Add(group);
            treeView1.SelectedNode = group;
            MarkChanged();

            treeView1.LabelEdit = true;
            group.BeginEdit();
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            TreeNode group = treeView1.SelectedNode;
            if (group == null || IsSkill(group) || IsMisc(group))
            {
                return;
            }

            // Its skills go back to Misc; skills are never dropped from the file
            TreeNode misc = treeView1.Nodes[0];
            while (group.Nodes.Count > 0)
            {
                TreeNode skill = group.Nodes[0];
                skill.Remove();
                InsertSkill(misc, skill);
            }

            group.Remove();
            MarkChanged();
        }

        private void OnClickRename(object sender, EventArgs e)
        {
            TreeNode group = treeView1.SelectedNode;
            if (group == null || IsSkill(group) || IsMisc(group))
            {
                return;
            }

            treeView1.LabelEdit = true;
            group.BeginEdit();
        }

        private void AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            treeView1.LabelEdit = false;

            if (e.Label == null)
            {
                return;
            }

            string error = null;
            string name = e.Label.Trim();
            if (name.Length == 0)
            {
                error = "Name cannot be empty.";
            }
            else if (name.Length > SkillGroups.MaxNameLength)
            {
                error = $"Name can be at most {SkillGroups.MaxNameLength} characters.";
            }
            else if (name.Any(c => c > 127))
            {
                // Even the 16-bit variant of the file is narrowed to single bytes by the client
                error = "Use ASCII characters only: the client stores group names as single bytes.";
            }

            if (error != null)
            {
                e.CancelEdit = true;
                MessageBox.Show(error, "Skill Groups", MessageBoxButtons.OK, MessageBoxIcon.Information);
                treeView1.LabelEdit = true;
                e.Node.BeginEdit();
                return;
            }

            if (name != e.Label)
            {
                e.CancelEdit = true;
                e.Node.Text = name;
            }

            MarkChanged();
        }
    }
}
