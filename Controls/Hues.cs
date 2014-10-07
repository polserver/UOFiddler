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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Ultima;

namespace FiddlerControls
{
    public partial class Hues : UserControl
    {
        public Hues()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            pictureBox.MouseWheel += new MouseEventHandler(OnMouseWheel);
            refmarker = this;
        }

        private const int ITEMHEIGHT = 20;
        private int selected = 0;
        private bool Loaded = false;
        private int row;
        private Hues refmarker;

        /// <summary>
        /// Sets Selected Hue
        /// </summary>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                if (Loaded)
                {
                    if (Ultima.Hues.List.Length > 0)
                        pictureBox.Invalidate();
                }
            }
        }

        /// <summary>
        /// Reload when loaded (file changed)
        /// </summary>
        private void Reload()
        {
            if (!Loaded)
                return;
            selected = 0;
            OnLoad(this, EventArgs.Empty);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Options.LoadedUltimaClass["Hues"] = true;
            if ((Parent.GetType() == typeof(HuePopUpItem)) || (Parent.GetType() == typeof(HuePopUp)) || (Parent.GetType() == typeof(HuePopUpDress)))
            {
                pictureBox.MouseDoubleClick -= new System.Windows.Forms.MouseEventHandler(this.OnMouseDoubleClick);
                pictureBox.ContextMenu = new ContextMenu();
            }


            vScrollBar.Maximum = Ultima.Hues.List.Length;
            vScrollBar.Minimum = 0;
            vScrollBar.Value = 0;
            vScrollBar.SmallChange = 1;
            vScrollBar.LargeChange = 10;
            if (selected > 0)
                vScrollBar.Value = selected;
            pictureBox.Invalidate();
            if (!Loaded)
            {
                FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
                FiddlerControls.Events.HueChangeEvent += new FiddlerControls.Events.HueChangeHandler(OnHueChangeEvent);
            }
            Loaded = true;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void OnHueChangeEvent()
        {
            pictureBox.Invalidate();
        }

        private int GetIndex(int y)
        {
            int value = vScrollBar.Value + y;
            if (Ultima.Hues.List.Length > value)
                return value;
            else
                return -1;
        }

        private void onPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            for (int y = 0; y <= row; ++y)
            {
                int index = GetIndex(y);
                if (index >= 0)
                {
                    Rectangle rect = new Rectangle(0, y * ITEMHEIGHT, 200, ITEMHEIGHT);
                    if (index == selected)
                        e.Graphics.FillRectangle(SystemBrushes.Highlight, rect);
                    else
                        e.Graphics.FillRectangle(SystemBrushes.Window, rect);

                    float size = ((float)(pictureBox.Width - 200)) / 32;
                    Hue hue = Ultima.Hues.List[index];
                    Rectangle stringrect = new Rectangle(3, y * ITEMHEIGHT, pictureBox.Width, ITEMHEIGHT);
                    e.Graphics.DrawString(String.Format("{0,-5} {1,-7} {2}", hue.Index + 1, String.Format("(0x{0:X})", hue.Index + 1), hue.Name), Font, Brushes.Black, stringrect);

                    for (int i = 0; i < hue.Colors.Length; ++i)
                    {
                        Rectangle rectangle = new Rectangle(200 + ((int)Math.Round((double)(i * size))), y * ITEMHEIGHT, (int)Math.Round((double)(size + 1f)), ITEMHEIGHT);
                        e.Graphics.FillRectangle(new SolidBrush(hue.GetColor(i)), rectangle);
                    }
                }
            }
        }

        private void onScroll(object sender, ScrollEventArgs e)
        {
            pictureBox.Invalidate();
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                if (vScrollBar.Value < vScrollBar.Maximum)
                {
                    vScrollBar.Value++;
                    pictureBox.Invalidate();
                }
            }
            else
            {
                if (vScrollBar.Value > 1)
                {
                    vScrollBar.Value--;
                    pictureBox.Invalidate();
                }
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            row = pictureBox.Height / ITEMHEIGHT;
            if (!Loaded)
                return;
            pictureBox.Invalidate();
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            pictureBox.Focus();
            Point m = PointToClient(Control.MousePosition);
            int index = GetIndex(m.Y / ITEMHEIGHT);
            if (index >= 0)
                Selected = index;
        }

        private void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            Point m = PointToClient(Control.MousePosition);
            int index = GetIndex(m.Y / ITEMHEIGHT);
            if (index >= 0)
                Selected = index;
            new HueEdit(index, refmarker).Show();
        }

        #region ContextMenu
        private void OnClickSave(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            Ultima.Hues.Save(path);
            MessageBox.Show(
                String.Format("Hue saved to {0}", path),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Hues"] = false;
        }

        private void OnTextChangedReplace(object sender, EventArgs e)
        {
            int index;
            if (Utils.ConvertStringToInt(ReplaceText.Text, out index, 1, 3000))
                ReplaceText.ForeColor = Color.Black;
            else
                ReplaceText.ForeColor = Color.Red;
        }

        private void OnKeyDownReplace(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int index;
                if (Utils.ConvertStringToInt(ReplaceText.Text, out index, 1, 3000))
                {
                    contextMenuStrip1.Close();
                    Ultima.Hues.List[selected] = Ultima.Hues.List[index - 1];
                    pictureBox.Invalidate();
                }
            }
        }

        private void OnExport(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string FileName = Path.Combine(path, String.Format("Hue {0}.txt", selected + 1));
            Ultima.Hues.List[selected].Export(FileName);
            MessageBox.Show(String.Format("Hue saved to {0}", FileName), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnImport(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = "Choose txt file to import";
            dialog.CheckFileExists = true;
            dialog.Filter = "txt files (*.txt)|*.txt";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Ultima.Hues.List[selected].Import(dialog.FileName);
                Options.ChangedUltimaClass["Hues"] = true;
                FiddlerControls.Events.FireHueChangeEvent();
            }
        }
        #endregion

        /// <summary>
        /// Print a nice border
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int borderWidth = 1;

            Color borderColor = VisualStyleInformation.TextControlBorder;
            if (borderColor == null)
                borderColor = Color.LightBlue;
            ControlPaint.DrawBorder(e.Graphics, e.ClipRectangle, borderColor,
                      borderWidth, ButtonBorderStyle.Solid, borderColor, borderWidth,
                      ButtonBorderStyle.Solid, borderColor, borderWidth, ButtonBorderStyle.Solid,
                      borderColor, borderWidth, ButtonBorderStyle.Solid);
        }
    }
}
