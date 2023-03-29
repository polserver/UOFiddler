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
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.Forms
{
    public partial class ItemDetailForm : Form
    {
        public ItemDetailForm(int i)
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            Icon = Options.GetFiddlerIcon();
            _index = i;
            Data.AddBasicContextMenu();
        }

        private readonly int _index;
        private bool _partialHue;
        private bool _animate;
        private Timer _mTimer;
        private int _frame;
        private Animdata.AnimdataEntry _info;

        private int _hue = -1;

        /// <summary>
        /// Sets Hue
        /// </summary>
        public int Hue
        {
            get => _hue;
            set
            {
                _hue = value;

                if (_animate)
                {
                    return;
                }

                Bitmap hueBit = new Bitmap(Art.GetStatic(_index));
                if (_hue >= 0)
                {
                    Hue hue = Hues.List[_hue];
                    hue.ApplyTo(hueBit, _partialHue);
                }

                Graphic.Tag = hueBit;
                Graphic.Invalidate();
            }
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(SystemColors.Control);
            if (Graphic.Tag == null)
            {
                return;
            }

            Bitmap bit = (Bitmap)Graphic.Tag;
            e.Graphics.DrawImage(bit, (e.ClipRectangle.Width - bit.Width) / 2, 5);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            animateToolStripMenuItem.Visible = false;
            ItemData item = TileData.ItemTable[_index];
            Bitmap bit = Art.GetStatic(_index);

            Text = $"Item Detail 0x{_index:X} '{item.Name}'";

            if (bit == null)
            {
                splitContainer1.SplitterDistance = 10;
            }
            else
            {
                Size = new Size(300, bit.Size.Height + Data.Size.Height + 10);
                splitContainer1.SplitterDistance = bit.Size.Height + 10;
                Graphic.Size = new Size(300, bit.Size.Height + 10);
                Graphic.Tag = bit;
                Graphic.Invalidate();
            }

            Data.AppendText($"Name: {item.Name}\n");
            Data.AppendText(string.Format("Graphic: 0x{0:X4} ({0})\n", _index));
            Data.AppendText($"Height/Capacity: {item.Height}\n");
            Data.AppendText($"Weight: {item.Weight}\n");
            Data.AppendText($"Animation: {item.Animation}\n");
            Data.AppendText($"Quality/Layer/Light: {item.Quality}\n");
            Data.AppendText($"Quantity: {item.Quantity}\n");
            Data.AppendText($"Hue: {item.Hue}\n");
            Data.AppendText($"StackingOffset/Unk4: {item.StackingOffset}\n");
            Data.AppendText($"Flags: {item.Flags}\n");

            if ((item.Flags & TileFlag.PartialHue) != 0)
            {
                _partialHue = true;
            }

            if ((item.Flags & TileFlag.Animation) == 0)
            {
                return;
            }

            _info = Animdata.GetAnimData(_index);
            if (_info == null)
            {
                return;
            }

            animateToolStripMenuItem.Visible = true;
            Data.AppendText($"Animation FrameCount: {_info.FrameCount} Interval: {_info.FrameInterval}\n");
        }

        private void AnimTick(object sender, EventArgs e)
        {
            ++_frame;
            if (_frame >= _info.FrameCount)
            {
                _frame = 0;
            }

            Bitmap animBit = new Bitmap(Art.GetStatic(_index + _info.FrameData[_frame]));
            if (_hue >= 0)
            {
                Hue hue = Hues.List[_hue];
                hue.ApplyTo(animBit, _partialHue);
            }
            Graphic.Tag = animBit;
            Graphic.Invalidate();
        }

        private HuePopUpItemForm _showForm;

        private void OnClick_Hue(object sender, EventArgs e)
        {
            if (_showForm?.IsDisposed != false)
            {
                _showForm = new HuePopUpItemForm(UpdateSelectedHue, Hue);
            }
            else
            {
                _showForm.SetHue(Hue);
            }

            _showForm.TopMost = true;
            _showForm.Show();
        }

        private void UpdateSelectedHue(int selectedHue)
        {
            Hue = selectedHue;
        }

        private void OnClickAnimate(object sender, EventArgs e)
        {
            _animate = !_animate;
            if (_animate)
            {
                _mTimer = new Timer();
                _frame = -1;
                _mTimer.Interval = 100 * _info.FrameInterval;
                _mTimer.Tick += AnimTick;
                _mTimer.Start();
            }
            else
            {
                if (_mTimer.Enabled)
                {
                    _mTimer.Stop();
                }

                _mTimer.Dispose();
                _mTimer = null;
                Graphic.Tag = Art.GetStatic(_index);
                Graphic.Invalidate();
            }
        }

        private void OnClose(object sender, FormClosingEventArgs e)
        {
            if (_mTimer != null)
            {
                if (_mTimer.Enabled)
                {
                    _mTimer.Stop();
                }

                _mTimer.Dispose();
                _mTimer = null;
            }

            if (_showForm?.IsDisposed == false)
            {
                _showForm.Close();
            }
        }

        private void Extract_Image_ClickBmp(object sender, EventArgs e)
        {
            SaveImage(ImageFormat.Bmp);
        }

        private void Extract_Image_ClickTiff(object sender, EventArgs e)
        {
            SaveImage(ImageFormat.Tiff);
        }

        private void Extract_Image_ClickJpg(object sender, EventArgs e)
        {
            SaveImage(ImageFormat.Jpeg);
        }

        private void Extract_Image_ClickPng(object sender, EventArgs e)
        {
            SaveImage(ImageFormat.Png);
        }

        private void SaveImage(ImageFormat imageFormat)
        {
            if (!Art.IsValidStatic(_index))
            {
                return;
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"Item 0x{_index:X}.{fileExtension}");

            using (Bitmap bit = new Bitmap(Art.GetStatic(_index).Width, Art.GetStatic(_index).Height))
            {
                using (Graphics newGraphic = Graphics.FromImage(bit))
                {
                    newGraphic.Clear(Color.Transparent);
                    using (Bitmap huedBitmap = new Bitmap(Art.GetStatic(_index)))
                    {
                        if (_hue > 0)
                        {
                            Hue hue = Hues.List[_hue];
                            hue.ApplyTo(huedBitmap, _partialHue);
                        }

                        newGraphic.DrawImage(huedBitmap, 0, 0);
                    }
                }

                bit.Save(fileName, imageFormat);
            }

            MessageBox.Show($"Item saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnSizeChange(object sender, EventArgs e)
        {
            Graphic.Invalidate();
        }
    }
}