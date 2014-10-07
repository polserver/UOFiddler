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

namespace FiddlerControls
{
    public partial class ItemDetail : Form
    {
        public ItemDetail(int i)
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            index = i;
        }

        public int index;
        private int defHue = -1;
        private bool partialHue = false;
        private bool animate = false;
        private Timer m_Timer;
        int frame;
        Animdata.Data info;

        /// <summary>
        /// Sets Hue
        /// </summary>
        public int DefHue
        {
            get { return defHue; }
            set
            {
                defHue = value;
                if (!animate)
                {
                    Bitmap huebit = new Bitmap(Ultima.Art.GetStatic(index));
                    if (defHue >= 0)
                    {
                        Hue hue = Ultima.Hues.List[defHue];
                        hue.ApplyTo(huebit, partialHue);
                    }
                    Graphic.Tag = huebit;
                    Graphic.Invalidate();
                }
            }
        }

        private void onPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(SystemColors.Control);
            if (Graphic.Tag!=null)
            {
                Bitmap bit=(Bitmap)Graphic.Tag;
                e.Graphics.DrawImage(bit,(e.ClipRectangle.Width-bit.Width)/2,5);
            }
        }

        private void SetPicture(Bitmap bit)
        {
            Bitmap newbit = new Bitmap(this.Graphic.Size.Width, this.Graphic.Size.Height);
            Graphics newgraph = Graphics.FromImage(newbit);
            newgraph.DrawImage(bit, (this.Graphic.Size.Width - bit.Width) / 2, 5);
            this.Graphic.Image = newbit;
            newgraph.Dispose();
        }

        private void onLoad(object sender, EventArgs e)
        {
            this.animateToolStripMenuItem.Visible = false;
            Ultima.ItemData item = Ultima.TileData.ItemTable[index];
            Bitmap bit = Ultima.Art.GetStatic(index);

            this.Text = String.Format("Item Detail 0x{0:X} '{1}'", index, item.Name);
            if (bit == null)
                this.splitContainer1.SplitterDistance = 10;
            else
            {
                this.Size = new System.Drawing.Size(300, bit.Size.Height + this.Data.Size.Height + 10);
                this.splitContainer1.SplitterDistance = bit.Size.Height + 10;
                this.Graphic.Size = new System.Drawing.Size(300, bit.Size.Height + 10);
                Graphic.Tag = bit;
                Graphic.Invalidate();
            }

            this.Data.AppendText(String.Format("Name: {0}\n", item.Name));
            this.Data.AppendText(String.Format("Graphic: 0x{0:X4} ({0})\n", index));
            this.Data.AppendText(String.Format("Height/Capacity: {0}\n", item.Height));
            this.Data.AppendText(String.Format("Weight: {0}\n", item.Weight));
            this.Data.AppendText(String.Format("Animation: {0}\n", item.Animation));
            this.Data.AppendText(String.Format("Quality/Layer/Light: {0}\n", item.Quality));
            this.Data.AppendText(String.Format("Quantity: {0}\n", item.Quantity));
            this.Data.AppendText(String.Format("Hue: {0}\n", item.Hue));
            this.Data.AppendText(String.Format("StackingOffset/Unk4: {0}\n", item.StackingOffset));
            this.Data.AppendText(String.Format("Flags: {0}\n", item.Flags));
            if ((item.Flags & TileFlag.PartialHue) != 0)
                partialHue = true;
            if ((item.Flags & TileFlag.Animation) != 0)
            {
                info = Animdata.GetAnimData(index);
                if (info != null)
                {
                    this.animateToolStripMenuItem.Visible = true;
                    this.Data.AppendText(String.Format("Animation FrameCount: {0} Interval: {1}\n", info.FrameCount, info.FrameInterval));
                }
            }
        }

        private void AnimTick(object sender, EventArgs e)
        {
            ++frame;
            if (frame >= info.FrameCount)
                frame = 0;

            Bitmap animbit = new Bitmap(Ultima.Art.GetStatic(index + info.FrameData[frame]));
            if (defHue >= 0)
            {
                Hue hue = Ultima.Hues.List[defHue];
                hue.ApplyTo(animbit, partialHue);
            }
            Graphic.Tag = animbit;
            Graphic.Invalidate();
        }

        private HuePopUpItem showform = null;
        private void OnClick_Hue(object sender, EventArgs e)
        {
            if ((showform == null) || (showform.IsDisposed))
                showform = new HuePopUpItem(this, DefHue);
            else
                showform.SetHue(DefHue);
            showform.TopMost = true;
            showform.Show();
        }

        private void OnClickAnimate(object sender, EventArgs e)
        {
            animate = !animate;
            if (animate)
            {
                m_Timer = new Timer();
                frame = -1;
                m_Timer.Interval = 100 * info.FrameInterval;
                m_Timer.Tick += new EventHandler(AnimTick);
                m_Timer.Start();
            }
            else
            {
                if (m_Timer.Enabled)
                    m_Timer.Stop();

                m_Timer.Dispose();
                m_Timer = null;
                Graphic.Tag = Ultima.Art.GetStatic(index);
                Graphic.Invalidate();
            }
        }

        private void onClose(object sender, FormClosingEventArgs e)
        {
            if (m_Timer != null)
            {
                if (m_Timer.Enabled)
                    m_Timer.Stop();

                m_Timer.Dispose();
                m_Timer = null;
            }
            if ((showform != null) && (!showform.IsDisposed))
                showform.Close();
        }

        private void extract_Image_ClickBmp(object sender, EventArgs e)
        {
            if (!Art.IsValidStatic(index))
                return;
            string path = FiddlerControls.Options.OutputPath;
            string FileName = Path.Combine(path, String.Format("Item 0x{0:X}.bmp", index));
            Bitmap bit = new Bitmap(Ultima.Art.GetStatic(index).Width, Ultima.Art.GetStatic(index).Height);
            Graphics newgraph = Graphics.FromImage(bit);
            newgraph.Clear(Color.Transparent);
            Bitmap huebit = new Bitmap(Ultima.Art.GetStatic(index));
            if (defHue > 0)
            {
                Hue hue = Ultima.Hues.List[defHue];
                hue.ApplyTo(huebit, partialHue);
            }
            newgraph.DrawImage(huebit, 0, 0);
            bit.Save(FileName, ImageFormat.Bmp);
            MessageBox.Show(
                String.Format("Item saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void extract_Image_ClickTiff(object sender, EventArgs e)
        {
            if (!Art.IsValidStatic(index))
                return;
            string path = FiddlerControls.Options.OutputPath;
            string FileName = Path.Combine(path, String.Format("Item 0x{0:X}.tiff", index));
            Bitmap bit = new Bitmap(Ultima.Art.GetStatic(index).Width, Ultima.Art.GetStatic(index).Height);
            Graphics newgraph = Graphics.FromImage(bit);
            newgraph.Clear(Color.Transparent);
            Bitmap huebit = new Bitmap(Ultima.Art.GetStatic(index));
            if (defHue > 0)
            {
                Hue hue = Ultima.Hues.List[defHue];
                hue.ApplyTo(huebit, partialHue);
            }
            newgraph.DrawImage(huebit, 0, 0);
            bit.Save(FileName, ImageFormat.Tiff);
            MessageBox.Show(
                String.Format("Item saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnSizeChange(object sender, EventArgs e)
        {
            Graphic.Invalidate();
        }

        
    }
}