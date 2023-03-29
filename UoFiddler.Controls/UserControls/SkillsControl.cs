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
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class SkillsControl : UserControl
    {
        public SkillsControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            _source = new BindingSource();
        }

        private bool _loaded;
        private static BindingSource _source;

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
            Options.LoadedUltimaClass["Skills"] = true;

            _source.DataSource = Skills.SkillEntries;
            dataGridView1.DataSource = _source;
            dataGridView1.Invalidate();

            if (dataGridView1.Columns.Count > 0)
            {
                dataGridView1.Columns[0].MinimumWidth = 40;
                dataGridView1.Columns[0].FillWeight = 10.82822F;
                dataGridView1.Columns[0].ReadOnly = true;
                dataGridView1.Columns[0].HeaderText = "ID";
                dataGridView1.Columns[1].MinimumWidth = 60;
                dataGridView1.Columns[1].FillWeight = 10.80126F;
                dataGridView1.Columns[1].ReadOnly = false;
                dataGridView1.Columns[1].HeaderText = "is Action";
                dataGridView1.Columns[2].FillWeight = 54.86799F;
                dataGridView1.Columns[2].ReadOnly = false;
                dataGridView1.Columns[3].Visible = false; // extraFlag
            }

            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                _source.ListChanged += Source_ListChanged;
            }

            _loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private static void Source_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            Options.ChangedUltimaClass["Skills"] = true;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            dataGridView1.CancelEdit();
            string path = Options.OutputPath;
            Skills.Save(path);
            MessageBox.Show($"Skills saved to {path}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Skills"] = false;
        }

        private void OnClickAdd(object sender, EventArgs e)
        {
            dataGridView1.CancelEdit();
            SkillInfo skill = new SkillInfo(Skills.SkillEntries.Count, "new skill", false, 0);
            _source.Add(skill);
            _source.MoveLast();
            dataGridView1.Invalidate();
        }

        private void OnClickDelete(object sender, EventArgs e)
        {
            dataGridView1.CancelEdit();
            if (dataGridView1.SelectedRows.Count <= 0)
            {
                return;
            }

            foreach (SkillInfo skill in Skills.SkillEntries)
            {
                if (skill.Index > dataGridView1.SelectedRows[0].Index)
                {
                    skill.Index--;
                }
            }
            _source.RemoveCurrent();
            dataGridView1.Invalidate();
        }
    }
}
