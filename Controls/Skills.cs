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

namespace FiddlerControls
{
    public partial class Skills : UserControl
    {
        public Skills()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            source = new BindingSource();
        }

        private bool Loaded = false;
        private static BindingSource source;

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
            Options.LoadedUltimaClass["Skills"] = true;

            source.DataSource = Ultima.Skills.SkillEntries;
            dataGridView1.DataSource = source;
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
            if (!Loaded)
            {
                FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
                source.ListChanged += new System.ComponentModel.ListChangedEventHandler(source_ListChanged);
            }
            Loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void source_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
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
            string path = FiddlerControls.Options.OutputPath;
            Ultima.Skills.Save(path);
            MessageBox.Show(
                String.Format("Skills saved to {0}", path),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Skills"] = false;
        }

        private void OnClickAdd(object sender, EventArgs e)
        {
            dataGridView1.CancelEdit();
            Ultima.SkillInfo skill = new Ultima.SkillInfo(Ultima.Skills.SkillEntries.Count, "new skill", false, 0);
            source.Add(skill);
            source.MoveLast();
            dataGridView1.Invalidate();
        }

        private void OnClickDelete(object sender, EventArgs e)
        {
            dataGridView1.CancelEdit();
            if (dataGridView1.SelectedRows.Count > 0)
            {
                foreach (Ultima.SkillInfo skill in Ultima.Skills.SkillEntries)
                {
                    if (skill.Index > dataGridView1.SelectedRows[0].Index)
                        skill.Index--;
                }
                source.RemoveCurrent();
                dataGridView1.Invalidate();
            }
        }
    }
}
