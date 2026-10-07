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
using UoFiddler.Controls.Forms;
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
        private Func<string, string?>? _localizationGetter;

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
                    
                    // 应用列头汉化
                    ApplyColumnHeaderLocalization();
                }

                if (!_loaded)
                {
                    ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                    _source.ListChanged += Source_ListChanged;
                }

                _loaded = true;
            }
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
            Options.ChangedUltimaClass["Skills"] = false;

            string message = _localizationGetter?.Invoke("Forms.SkillsControl.FileSavedMessage") ?? "Skills saved successfully.";
            string title = _localizationGetter?.Invoke("Forms.SkillsControl.FileSavedTitle") ?? "Save";
            FileSavedDialog.Show(FindForm(), Options.OutputPath, message, title);
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

        /// <summary>
        /// 设置本地化（从 MainForm 调用）
        /// </summary>
        public void SetLocalization(Func<string, string?> getLocalized)
        {
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        /// <summary>
        /// 应用列标题汉化（在数据绑定后调用）
        /// </summary>
        private void ApplyColumnHeaderLocalization()
        {
            // 设置表格列标题
            if (dataGridView1.Columns.Count > 0)
            {
                dataGridView1.Columns[0].HeaderText = _localizationGetter?.Invoke("Forms.SkillsControl.ColumnId") ?? "ID";
                if (dataGridView1.Columns.Count > 1)
                    dataGridView1.Columns[1].HeaderText = _localizationGetter?.Invoke("Forms.SkillsControl.ColumnIsAction") ?? "is Action";
                if (dataGridView1.Columns.Count > 2)
                    dataGridView1.Columns[2].HeaderText = _localizationGetter?.Invoke("Forms.SkillsControl.ColumnName") ?? "Name";
            }
        }

        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;

            // 菜单项汉化
            saveToolStripMenuItem.Text = _localizationGetter("Forms.SkillsControl.SaveButton") ?? "Save";
            addToolStripMenuItem.Text = _localizationGetter("Forms.SkillsControl.AddButton") ?? "Add";
            deleteToolStripMenuItem.Text = _localizationGetter("Forms.SkillsControl.DeleteButton") ?? "Delete";

            // 表格列标题
            ApplyColumnHeaderLocalization();
        }
    }
}
