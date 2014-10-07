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
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using Ultima;

namespace ComparePlugin
{
    public partial class CompareTexture : UserControl
    {
        public CompareTexture()
        {
            InitializeComponent();
        }
        Dictionary<int, bool> m_Compare = new Dictionary<int, bool>();
        SHA256Managed shaM = new SHA256Managed();
        System.Drawing.ImageConverter ic = new System.Drawing.ImageConverter();

        private void OnLoad(object sender, EventArgs e)
        {
            listBoxOrg.BeginUpdate();
            listBoxOrg.Items.Clear();
            List<object> cache = new List<object>();
            for (int i = 0; i < 0x4000; i++)
            {
                cache.Add(i);
            }
            listBoxOrg.Items.AddRange(cache.ToArray());
            listBoxOrg.EndUpdate();
        }

        private void OnIndexChangedOrg(object sender, EventArgs e)
        {
            if ((listBoxOrg.SelectedIndex == -1) || (listBoxOrg.Items.Count < 1))
                return;

            int i = int.Parse(listBoxOrg.Items[listBoxOrg.SelectedIndex].ToString());
            if (listBoxSec.Items.Count > 0)
                listBoxSec.SelectedIndex = listBoxSec.Items.IndexOf(i);
            if (Textures.TestTexture(i))
            {
                Bitmap bmp = Textures.GetTexture(i);
                if (bmp != null)
                    pictureBoxOrg.BackgroundImage = bmp;
                else
                    pictureBoxOrg.BackgroundImage = null;
            }
            else
                pictureBoxOrg.BackgroundImage = null;
            listBoxOrg.Invalidate();
        }

        private void DrawitemOrg(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;

            Brush fontBrush = Brushes.Gray;

            int i = int.Parse(listBoxOrg.Items[e.Index].ToString());
            if (listBoxOrg.SelectedIndex == e.Index)
                e.Graphics.FillRectangle(Brushes.LightSteelBlue, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            if (!Textures.TestTexture(i))
                fontBrush = Brushes.Red;
            else if (listBoxSec.Items.Count > 0)
            {
                if (!Compare(i))
                    fontBrush = Brushes.Blue;
            }

            e.Graphics.DrawString(String.Format("0x{0:X}", i), Font, fontBrush,
                new PointF((float)5,
                e.Bounds.Y + ((e.Bounds.Height / 2) -
                (e.Graphics.MeasureString(String.Format("0x{0:X}", i), Font).Height / 2))));
        }

        private void MeasureOrg(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 13;
        }

        private void OnClickLoadSecond(object sender, EventArgs e)
        {
            if (textBoxSecondDir.Text == null)
                return;
            string path = textBoxSecondDir.Text;
            string file = Path.Combine(path, "texmaps.mul");
            string file2 = Path.Combine(path, "texidx.mul");
            if ((File.Exists(file)) && (File.Exists(file2)))
            {
                SecondTexture.SetFileIndex(file2, file);
                LoadSecond();
            }
        }

        private void LoadSecond()
        {
            m_Compare.Clear();
            listBoxSec.BeginUpdate();
            listBoxSec.Items.Clear();
            List<object> cache = new List<object>();
            for (int i = 0; i < 0x4000; i++)
            {
                cache.Add(i);
            }
            listBoxSec.Items.AddRange(cache.ToArray());
            listBoxSec.EndUpdate();
        }

        private void DrawItemSec(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;

            Brush fontBrush = Brushes.Gray;

            int i = int.Parse(listBoxOrg.Items[e.Index].ToString());
            if (listBoxSec.SelectedIndex == e.Index)
                e.Graphics.FillRectangle(Brushes.LightSteelBlue, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            if (!SecondTexture.IsValidTexture(i))
                fontBrush = Brushes.Red;
            else if (!Compare(i))
                fontBrush = Brushes.Blue;

            e.Graphics.DrawString(String.Format("0x{0:X}", i), Font, fontBrush,
                new PointF((float)5,
                e.Bounds.Y + ((e.Bounds.Height / 2) -
                (e.Graphics.MeasureString(String.Format("0x{0:X}", i), Font).Height / 2))));
        }

        private void MeasureSec(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 13;
        }

        private void OnIndexChangedSec(object sender, EventArgs e)
        {
            if ((listBoxSec.SelectedIndex == -1) || (listBoxSec.Items.Count < 1))
                return;

            int i = int.Parse(listBoxSec.Items[listBoxSec.SelectedIndex].ToString());
            listBoxOrg.SelectedIndex = listBoxOrg.Items.IndexOf(i);
            if (SecondTexture.IsValidTexture(i))
            {
                Bitmap bmp = SecondTexture.GetTexture(i);
                if (bmp != null)
                    pictureBoxSec.BackgroundImage = bmp;
                else
                    pictureBoxSec.BackgroundImage = null;
            }
            else
                pictureBoxSec.BackgroundImage = null;
            listBoxSec.Invalidate();
        }

        private bool Compare(int index)
        {
            if (m_Compare.ContainsKey(index))
                return m_Compare[index];
            Bitmap bitorg = Textures.GetTexture(index);
            Bitmap bitsec = SecondTexture.GetTexture(index);
            if ((bitorg == null) && (bitsec == null))
            {
                m_Compare[index] = true;
                return true;
            }
            if (((bitorg == null) || (bitsec == null))
                || (bitorg.Size != bitsec.Size))
            {
                m_Compare[index] = false;
                return false;
            }

            byte[] btImage1 = new byte[1];
            btImage1 = (byte[])ic.ConvertTo(bitorg, btImage1.GetType());
            byte[] btImage2 = new byte[1];
            btImage2 = (byte[])ic.ConvertTo(bitsec, btImage2.GetType());

            string hash1string = BitConverter.ToString(shaM.ComputeHash(btImage1));
            string hash2string = BitConverter.ToString(shaM.ComputeHash(btImage2));

            bool res;
            if (hash1string != hash2string)
                res = false;
            else
                res = true;

            m_Compare[index] = res;
            return res;
        }

        private void OnChangeShowDiff(object sender, EventArgs e)
        {
            if (m_Compare.Count < 1)
            {
                if (checkBox1.Checked)
                {
                    MessageBox.Show("Second Texture file is not loaded!");
                    checkBox1.Checked = false;
                }
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            listBoxOrg.BeginUpdate();
            listBoxSec.BeginUpdate();
            listBoxOrg.Items.Clear();
            listBoxSec.Items.Clear();
            List<object> cache = new List<object>();
            if (checkBox1.Checked)
            {
                for (int i = 0; i < 0x4000; i++)
                {
                    if (!Compare(i))
                        cache.Add(i);
                }
            }
            else
            {
                for (int i = 0; i < 0x4000; i++)
                {
                    cache.Add(i);
                }
            }
            listBoxOrg.Items.AddRange(cache.ToArray());
            listBoxSec.Items.AddRange(cache.ToArray());
            listBoxOrg.EndUpdate();
            listBoxSec.EndUpdate();
            Cursor.Current = Cursors.Default;
        }

        private void ExportAsBmp(object sender, EventArgs e)
        {
            if (listBoxSec.SelectedIndex == -1)
                return;
            int i = int.Parse(listBoxSec.Items[listBoxSec.SelectedIndex].ToString());
            if (!SecondTexture.IsValidTexture(i))
                return;
            string path = FiddlerControls.Options.OutputPath;
            string FileName = Path.Combine(path, String.Format("Texture(Sec) 0x{0:X}.bmp", i));
            SecondTexture.GetTexture(i).Save(FileName, ImageFormat.Bmp);
            MessageBox.Show(
                String.Format("Texture saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void ExportAsTiff(object sender, EventArgs e)
        {
            if (listBoxSec.SelectedIndex == -1)
                return;
            int i = int.Parse(listBoxSec.Items[listBoxSec.SelectedIndex].ToString());
            if (!SecondTexture.IsValidTexture(i))
                return;
            string path = FiddlerControls.Options.OutputPath;
            string FileName = Path.Combine(path, String.Format("Texture(Sec) 0x{0:X}.tiff", i));
            SecondTexture.GetTexture(i).Save(FileName, ImageFormat.Tiff);
            MessageBox.Show(
                String.Format("Texture saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void BrowseOnClick(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory containing the texture files";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                    textBoxSecondDir.Text = dialog.SelectedPath;
            }
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            if (listBoxSec.SelectedIndex == -1)
                return;
            int i = int.Parse(listBoxSec.Items[listBoxSec.SelectedIndex].ToString());
            if (!SecondTexture.IsValidTexture(i))
                return;
            Bitmap copy = new Bitmap(SecondTexture.GetTexture(i));
            Ultima.Textures.Replace(i, copy);
            FiddlerControls.Options.ChangedUltimaClass["Texture"] = true;
            FiddlerControls.Events.FireTextureChangeEvent(this, i);
            m_Compare[i] = true;
            listBoxOrg.BeginUpdate();
            bool done = false;
            for (int id = 0; id < 0x4000; id++)
            {
                if (id > i)
                {
                    listBoxOrg.Items.Insert(id, i);
                    done = true;
                    break;
                }
                if (id == i)
                {
                    done = true;
                    break;
                }
            }
            if (!done)
                listBoxOrg.Items.Add(i);
            listBoxOrg.EndUpdate();
            listBoxOrg.Invalidate();
            listBoxSec.Invalidate();
            OnIndexChangedOrg(this, null);
        }

        private void CopyAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 0x4000; i++)
            {
                if (!SecondTexture.IsValidTexture(i))
                    continue;
                else if (!Compare(i))
                {
                    Bitmap copy = new Bitmap(SecondTexture.GetTexture(i));
                    Ultima.Textures.Replace(i, copy);
                    FiddlerControls.Events.FireTextureChangeEvent(this, i);
                }
            }

            m_Compare.Clear();
            listBoxOrg.BeginUpdate();
            listBoxOrg.Items.Clear();
            List<object> cache = new List<object>();
            for (int i = 0; i < 0x4000; i++)
            {
                cache.Add(i);
            }
            listBoxOrg.Items.AddRange(cache.ToArray());
            listBoxOrg.EndUpdate();
        }

        private void CopyToLeft_Click(object sender, MouseEventArgs e)
        {
            OnClickCopy(sender, e);
        }

        private void CopyAddOnly_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 0x4000; i++)
            {
                if (!SecondTexture.IsValidTexture(i))
                    continue;
                else if (!Textures.TestTexture(i))
                {
                    Bitmap copy = new Bitmap(SecondTexture.GetTexture(i));
                    Ultima.Textures.Replace(i, copy);
                    FiddlerControls.Events.FireTextureChangeEvent(this, i);
                }
            }

            m_Compare.Clear();
            listBoxOrg.BeginUpdate();
            listBoxOrg.Items.Clear();
            List<object> cache = new List<object>();
            for (int i = 0; i < 0x4000; i++)
            {
                cache.Add(i);
            }
            listBoxOrg.Items.AddRange(cache.ToArray());
            listBoxOrg.EndUpdate();
        }
    }
}

