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

namespace FiddlerControls
{
    public partial class SkillGrp : UserControl
    {
        public SkillGrp()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        private bool Loaded = false;
        private TreeNode sourceNode;

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (Loaded)
                OnLoad(this, EventArgs.Empty);
        }
        private void OnLoad(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["SkillGrp"] = true;

            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            List<TreeNode> cache = new List<TreeNode>();

            foreach (SkillGroup group in SkillGroups.List)
            {
                TreeNode groupnode = new TreeNode();
                groupnode.Text = group.Name;
                if (String.Equals("Misc", group.Name))
                    groupnode.ForeColor = Color.Blue;
                for (int i = 0; i < SkillGroups.SkillList.Count; ++i)
                {
                    if ((SkillGroups.SkillList[i]) == cache.Count)
                    {
                        TreeNode skillnode = new TreeNode();
                        skillnode.Text = Ultima.Skills.GetSkill(i).Name;
                        skillnode.Tag = i;
                        groupnode.Nodes.Add(skillnode);
                    }
                }
                cache.Add(groupnode);
            }

            treeView1.Nodes.AddRange(cache.ToArray());
            treeView1.EndUpdate();

            if (!Loaded)
                FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
            Loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            SkillGroups.List.Clear();
            int skillcount=int.MinValue;
            foreach (TreeNode root in treeView1.Nodes)
            {
                foreach (TreeNode child in root.Nodes)
                {
                    int id = (int)child.Tag;
                    if (id > skillcount)
                        skillcount = id;
                }
            }
            ++skillcount;
            SkillGroups.SkillList.Clear();
            for (int i = 0; i < skillcount; ++i)
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
            SkillGroups.Save(FiddlerControls.Options.OutputPath);
            MessageBox.Show(
                String.Format("SkillGrp saved to {0}", FiddlerControls.Options.OutputPath),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["SkillGrp"] = false;
        }


        private void OnItemDrag(object sender, ItemDragEventArgs e)
        {
            sourceNode = (TreeNode)e.Item;
            if (sourceNode == null)
                return;
            if (String.Equals("Misc", sourceNode.Text))
                return;
            DoDragDrop(e.Item.ToString(), DragDropEffects.Move | DragDropEffects.Copy);
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            Point pos = treeView1.PointToClient(new Point(e.X, e.Y));
            TreeNode targetNode = treeView1.GetNodeAt(pos);
            TreeNode nodeCopy;

            if (targetNode != null)
            {
                nodeCopy = new TreeNode(sourceNode.Text, sourceNode.ImageIndex, sourceNode.SelectedImageIndex);
                nodeCopy.Tag = sourceNode.Tag;

                bool targetIsSkill = targetNode.Tag != null;
                bool sourceIsSkill = sourceNode.Tag != null;
                if (targetIsSkill && !sourceIsSkill)
                    return;
                if (targetIsSkill && sourceIsSkill)
                {
                    if (sourceNode.Index > targetNode.Index)
                        targetNode.Parent.Nodes.Insert(targetNode.Index, nodeCopy);
                    else
                        targetNode.Parent.Nodes.Insert(targetNode.Index + 1, nodeCopy);
                }
                else if (!targetIsSkill && sourceIsSkill)
                {
                    targetNode.Nodes.Add(nodeCopy);
                }
                else if (!targetIsSkill && !sourceIsSkill)
                {
                    if (String.Equals("Misc", targetNode.Text))
                    {
                        treeView1.Invalidate();
                        return;
                    }
                    if (sourceNode.Index > targetNode.Index)
                        treeView1.Nodes.Insert(targetNode.Index, nodeCopy);
                    else
                        treeView1.Nodes.Insert(targetNode.Index + 1, nodeCopy);

                    while (sourceNode.GetNodeCount(false) > 0)
                    {
                        TreeNode node = sourceNode.FirstNode;
                        node.Remove();
                        nodeCopy.Nodes.Add(node);
                    }
                }
                else
                    return;
                sourceNode.Remove();
                nodeCopy.EnsureVisible();
                treeView1.SelectedNode = nodeCopy;
                treeView1.Invalidate();
                Options.ChangedUltimaClass["SkillGrp"] = true;
            }
        }

        private void OnClickRename(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                if (treeView1.SelectedNode.Parent == null)
                {
                    if (!String.Equals("Misc", treeView1.SelectedNode.Text))
                    {
                        treeView1.LabelEdit = true;
                        treeView1.SelectedNode.BeginEdit();
                    }
                }
            }
        }

        private void AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label != null)
            {
                if (String.Equals("Misc", e.Label))
                {
                    e.CancelEdit = true;
                    MessageBox.Show("Invalid name. Name is reserved.", "SkillGroup Edit");
                    e.Node.BeginEdit();
                }
                else if (e.Label.Length > 0)
                    e.Node.EndEdit(false);
                else
                {
                    e.CancelEdit = true;
                    MessageBox.Show("Invalid name. Name cannot be empty.", "SkillGroup Edit");
                    e.Node.BeginEdit();
                }
                treeView1.LabelEdit = false;
            }
        }

        private void OnClickAdd(object sender, EventArgs e)
        {
            TreeNode newnode = new TreeNode("new");
            treeView1.Nodes.Add(newnode);
            treeView1.LabelEdit = true;
            newnode.BeginEdit();
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                while (treeView1.SelectedNode.GetNodeCount(false) > 0)
                {
                    TreeNode node = treeView1.SelectedNode.FirstNode;
                    node.Remove();
                }
                treeView1.SelectedNode.Remove();
            }
        }

        private void onOpeningContext(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                if (treeView1.SelectedNode.Tag == null)
                    SkillIDTextBox.Text = "";
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

        private void onKeyDownSkillID(object sender, KeyEventArgs e)
        {
            if (treeView1.SelectedNode == null)
                return;
            if (e.KeyCode == Keys.Enter)
            {
                string line = SkillIDTextBox.Text.Trim();
                if (line.Length > 0)
                {
                    int id;
                    if (int.TryParse(line, out id))
                    {
                        treeView1.SelectedNode.Tag = id;
                    }
                }
            }
        }
    }
}
