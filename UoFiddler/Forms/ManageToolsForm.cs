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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using UoFiddler.Classes;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Forms
{
    public partial class ManageToolsForm : Form
    {
        private readonly Action _loadExternToolStripMenuAction;

        public ManageToolsForm(Action loadExternToolStripMenuAction)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();

            _loadExternToolStripMenuAction = loadExternToolStripMenuAction;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (FiddlerOptions.ExternTools is null)
            {
                return;
            }

            listBoxTools.BeginUpdate();
            try
            {
                for (int i = 0; i < FiddlerOptions.ExternTools.Count; i++)
                {
                    listBoxTools.Items.Add(i);
                }
            }
            finally
            {
                listBoxTools.EndUpdate();
            }
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            _loadExternToolStripMenuAction.Invoke();
        }

        private void OnToolIndexChanged(object sender, EventArgs e)
        {
            if (listBoxTools.SelectedIndex >= 0)
            {
                listBoxArgs.BeginUpdate();
                listBoxArgs.Items.Clear();
                ExternTool tool = FiddlerOptions.ExternTools[listBoxTools.SelectedIndex];
                for (int i = 0; i < tool.Args.Count; i++)
                {
                    listBoxArgs.Items.Add(i);
                }
                listBoxArgs.EndUpdate();
                textBoxToolName.Text = tool.Name;
                textBoxToolFile.Text = tool.FileName;
                textBoxArgName.Text = "";
                textBoxArgParam.Text = "";
            }
            else
            {
                textBoxToolName.Text = "";
                textBoxToolFile.Text = "";
            }

            listBoxTools.Invalidate();
        }

        private void ToolDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
            {
                return;
            }

            Brush fontBrush = Brushes.Black;
            if (listBoxTools.SelectedIndex == e.Index)
            {
                e.Graphics.FillRectangle(Brushes.LightSteelBlue, e.Bounds);
            }

            e.Graphics.DrawString(FiddlerOptions.ExternTools[e.Index].FormatName(), Font, fontBrush, new Point(e.Bounds.X, e.Bounds.Y));
        }

        private void OnArgIndexChanged(object sender, EventArgs e)
        {
            if (listBoxArgs.SelectedIndex >= 0)
            {
                ExternTool tool = FiddlerOptions.ExternTools[listBoxTools.SelectedIndex];
                textBoxArgName.Text = tool.ArgsName[listBoxArgs.SelectedIndex];
                textBoxArgParam.Text = tool.Args[listBoxArgs.SelectedIndex];
            }
            else
            {
                textBoxArgName.Text = "";
                textBoxArgParam.Text = "";
            }
            listBoxArgs.Invalidate();
        }

        private void ArgDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
            {
                return;
            }

            Brush fontBrush = Brushes.Black;
            if (listBoxArgs.SelectedIndex == e.Index)
            {
                e.Graphics.FillRectangle(Brushes.LightSteelBlue, e.Bounds);
            }

            e.Graphics.DrawString(FiddlerOptions.ExternTools[listBoxTools.SelectedIndex].FormatArg(e.Index),
                Font, fontBrush, new Point(e.Bounds.X, e.Bounds.Y));
        }

        private void OnClickRemoveTool(object sender, EventArgs e)
        {
            if (listBoxTools.SelectedIndex < 0)
            {
                return;
            }

            FiddlerOptions.ExternTools.RemoveAt(listBoxTools.SelectedIndex);
            listBoxTools.BeginUpdate();
            listBoxTools.Items.Clear();
            for (int i = 0; i < FiddlerOptions.ExternTools.Count; i++)
            {
                listBoxTools.Items.Add(i);
            }
            listBoxTools.EndUpdate();
            listBoxArgs.Items.Clear();
            listBoxTools.Invalidate();
            textBoxToolName.Text = "";
            textBoxToolFile.Text = "";
        }

        private void OnClickRemoveArg(object sender, EventArgs e)
        {
            if (listBoxTools.SelectedIndex < 0 || listBoxArgs.SelectedIndex < 0)
            {
                return;
            }

            ExternTool tool = FiddlerOptions.ExternTools[listBoxTools.SelectedIndex];
            tool.Args.RemoveAt(listBoxArgs.SelectedIndex);
            tool.ArgsName.RemoveAt(listBoxArgs.SelectedIndex);
            listBoxArgs.BeginUpdate();
            listBoxArgs.Items.Clear();
            for (int i = 0; i < tool.Args.Count; i++)
            {
                listBoxArgs.Items.Add(i);
            }
            listBoxArgs.EndUpdate();
            listBoxArgs.Invalidate();
            textBoxArgName.Text = "";
            textBoxArgParam.Text = "";
        }

        private void OnClickSaveTool(object sender, EventArgs e)
        {
            if (listBoxTools.SelectedIndex < 0)
            {
                return;
            }

            ExternTool tool = FiddlerOptions.ExternTools[listBoxTools.SelectedIndex];
            tool.Name = textBoxToolName.Text;
            tool.FileName = textBoxToolFile.Text;
            listBoxTools.Invalidate();
        }

        private void OnClickChangeToolFile(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select program";
                dialog.CheckFileExists = true;

                if (!string.IsNullOrEmpty(textBoxToolFile.Text))
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(textBoxToolFile.Text);
                }

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxToolFile.Text = dialog.FileName;
                }
            }
        }

        private void OnAddTool(object sender, EventArgs e)
        {
            FiddlerOptions.ExternTools.Add(new ExternTool(textBoxToolName.Text, textBoxToolFile.Text));
            listBoxTools.Items.Add(FiddlerOptions.ExternTools.Count - 1);
            listBoxTools.SelectedIndex = FiddlerOptions.ExternTools.Count - 1;
        }

        private void OnClickSaveArg(object sender, EventArgs e)
        {
            if (listBoxTools.SelectedIndex < 0)
            {
                return;
            }

            ExternTool tool = FiddlerOptions.ExternTools[listBoxTools.SelectedIndex];
            if (listBoxArgs.SelectedIndex < 0)
            {
                return;
            }

            tool.Args[listBoxArgs.SelectedIndex] = textBoxArgParam.Text;
            tool.ArgsName[listBoxArgs.SelectedIndex] = textBoxArgName.Text;
            listBoxArgs.Invalidate();
        }

        private void OnAddArg(object sender, EventArgs e)
        {
            if (listBoxTools.SelectedIndex < 0)
            {
                return;
            }

            ExternTool tool = FiddlerOptions.ExternTools[listBoxTools.SelectedIndex];
            tool.Args.Add(textBoxArgParam.Text);
            tool.ArgsName.Add(textBoxArgName.Text);
            listBoxArgs.Items.Add(tool.Args.Count - 1);
            listBoxArgs.SelectedIndex = tool.Args.Count - 1;
        }
    }
}
