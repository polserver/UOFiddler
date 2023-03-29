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
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class SkillGroupControl : UserControl
    {
        public SkillGroupControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        private bool _loaded;
        private TreeNode _sourceNode;

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

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["SkillGrp"] = true;

            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            List<TreeNode> cache = new List<TreeNode>();

            foreach (SkillGroup group in SkillGroups.List)
            {
                TreeNode groupNode = new TreeNode
                {
                    Text = group.Name
                };

                if (string.Equals("Misc", group.Name))
                {
                    groupNode.ForeColor = Color.Blue;
                }

                for (int i = 0; i < SkillGroups.SkillList.Count; ++i)
                {
                    if (SkillGroups.SkillList[i] != cache.Count)
                    {
                        continue;
                    }

                    var skillInfo = Skills.GetSkill(i);

                    if (skillInfo == null)
                    {
                        continue;
                    }

                    TreeNode skillNode = new TreeNode
                    {
                        Text = skillInfo.Name,
                        Tag = i
                    };

                    groupNode.Nodes.Add(skillNode);
                }

                cache.Add(groupNode);
            }

            treeView1.Nodes.AddRange(cache.ToArray());
            treeView1.EndUpdate();

            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            }

            _loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            SkillGroups.List.Clear();
            int skillCount = int.MinValue;
            foreach (TreeNode root in treeView1.Nodes)
            {
                foreach (TreeNode child in root.Nodes)
                {
                    int id = (int)child.Tag;
                    if (id > skillCount)
                    {
                        skillCount = id;
                    }
                }
            }
            ++skillCount;
            SkillGroups.SkillList.Clear();
            for (int i = 0; i < skillCount; ++i)
            {
                SkillGroups.SkillList.Add(0);
            }
            foreach (TreeNode root in treeView1.Nodes)
            {
                SkillGroups.List.Add(new SkillGroup(root.Text));
                foreach (TreeNode skill in root.Nodes)
                {
                    SkillGroups.SkillList[(int)skill.Tag] = root.Index;
                }
            }
            SkillGroups.Save(Options.OutputPath);
            MessageBox.Show($"SkillGrp saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["SkillGrp"] = false;
        }

        private void OnItemDrag(object sender, ItemDragEventArgs e)
        {
            _sourceNode = (TreeNode)e.Item;
            if (_sourceNode == null)
            {
                return;
            }

            if (string.Equals("Misc", _sourceNode.Text))
            {
                return;
            }

            DoDragDrop(e.Item.ToString(), DragDropEffects.Move | DragDropEffects.Copy);
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.Text) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            Point pos = treeView1.PointToClient(new Point(e.X, e.Y));
            TreeNode targetNode = treeView1.GetNodeAt(pos);

            if (targetNode == null)
            {
                return;
            }

            var nodeCopy = new TreeNode(_sourceNode.Text, _sourceNode.ImageIndex, _sourceNode.SelectedImageIndex)
            {
                Tag = _sourceNode.Tag
            };

            bool targetIsSkill = targetNode.Tag != null;
            bool sourceIsSkill = _sourceNode.Tag != null;
            if (targetIsSkill && !sourceIsSkill)
            {
                return;
            }

            if (targetIsSkill && sourceIsSkill)
            {
                if (_sourceNode.Index > targetNode.Index)
                {
                    targetNode.Parent.Nodes.Insert(targetNode.Index, nodeCopy);
                }
                else
                {
                    targetNode.Parent.Nodes.Insert(targetNode.Index + 1, nodeCopy);
                }
            }
            else if (!targetIsSkill && sourceIsSkill)
            {
                targetNode.Nodes.Add(nodeCopy);
            }
            else if (!targetIsSkill && !sourceIsSkill)
            {
                if (string.Equals("Misc", targetNode.Text))
                {
                    treeView1.Invalidate();
                    return;
                }
                if (_sourceNode.Index > targetNode.Index)
                {
                    treeView1.Nodes.Insert(targetNode.Index, nodeCopy);
                }
                else
                {
                    treeView1.Nodes.Insert(targetNode.Index + 1, nodeCopy);
                }

                while (_sourceNode.GetNodeCount(false) > 0)
                {
                    TreeNode node = _sourceNode.FirstNode;
                    node.Remove();
                    nodeCopy.Nodes.Add(node);
                }
            }

            _sourceNode.Remove();
            nodeCopy.EnsureVisible();
            treeView1.SelectedNode = nodeCopy;
            treeView1.Invalidate();
            Options.ChangedUltimaClass["SkillGrp"] = true;
        }

        private void OnClickRename(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null && treeView1.SelectedNode.Parent == null &&
                !string.Equals("Misc", treeView1.SelectedNode.Text))
            {
                treeView1.LabelEdit = true;
                treeView1.SelectedNode.BeginEdit();
            }
        }

        private void AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label == null)
            {
                return;
            }

            if (string.Equals("Misc", e.Label))
            {
                e.CancelEdit = true;
                MessageBox.Show("Invalid name. Name is reserved.", "SkillGroup Edit");
                e.Node.BeginEdit();
            }
            else if (e.Label.Length > 0)
            {
                e.Node.EndEdit(false);
            }
            else
            {
                e.CancelEdit = true;
                MessageBox.Show("Invalid name. Name cannot be empty.", "SkillGroup Edit");
                e.Node.BeginEdit();
            }
            treeView1.LabelEdit = false;
        }

        private void OnClickAdd(object sender, EventArgs e)
        {
            TreeNode newNode = new TreeNode("new");
            treeView1.Nodes.Add(newNode);
            treeView1.LabelEdit = true;
            newNode.BeginEdit();
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                return;
            }

            while (treeView1.SelectedNode.GetNodeCount(false) > 0)
            {
                TreeNode node = treeView1.SelectedNode.FirstNode;
                node.Remove();
            }
            treeView1.SelectedNode.Remove();
        }

        private void OnOpeningContext(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                if (treeView1.SelectedNode.Tag == null)
                {
                    SkillIDTextBox.Text = "";
                }
                else
                {
                    int id = (int)treeView1.SelectedNode.Tag;
                    SkillIDTextBox.Text = id.ToString();
                }
                SkillIDTextBox.Enabled = true;
            }
            else
            {
                SkillIDTextBox.Text = "";
                SkillIDTextBox.Enabled = false;
            }
        }

        private void OnKeyDownSkillID(object sender, KeyEventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                return;
            }

            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            string line = SkillIDTextBox.Text.Trim();
            if (line.Length > 0 && int.TryParse(line, out int id))
            {
                treeView1.SelectedNode.Tag = id;
            }
        }
    }
}
