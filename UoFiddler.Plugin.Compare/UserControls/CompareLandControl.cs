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
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.Compare.Classes;

namespace UoFiddler.Plugin.Compare.UserControls
{
    public partial class CompareLandControl : UserControl
    {
        public CompareLandControl()
        {
            InitializeComponent();
        }

        private readonly Dictionary<int, bool> _mCompare = new Dictionary<int, bool>();
        private readonly SHA256 _sha256 = SHA256.Create();
        private readonly ImageConverter _ic = new ImageConverter();

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
            if (listBoxOrg.SelectedIndex == -1 || listBoxOrg.Items.Count < 1)
            {
                return;
            }

            int i = int.Parse(listBoxOrg.Items[listBoxOrg.SelectedIndex].ToString());
            if (listBoxSec.Items.Count > 0)
            {
                listBoxSec.SelectedIndex = listBoxSec.Items.IndexOf(i);
            }

            pictureBoxOrg.BackgroundImage = Art.IsValidLand(i)
                ? Art.GetLand(i)
                : null;

            listBoxOrg.Invalidate();
        }

        private void DrawitemOrg(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
            {
                return;
            }

            Brush fontBrush = Brushes.Gray;

            int i = int.Parse(listBoxOrg.Items[e.Index].ToString());
            if (listBoxOrg.SelectedIndex == e.Index)
            {
                e.Graphics.FillRectangle(Brushes.LightSteelBlue, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            }

            if (!Art.IsValidLand(i))
            {
                fontBrush = Brushes.Red;
            }
            else if (listBoxSec.Items.Count > 0)
            {
                if (!Compare(i))
                {
                    fontBrush = Brushes.Blue;
                }
            }

            e.Graphics.DrawString($"0x{i:X}", Font, fontBrush,
                new PointF(5,
                e.Bounds.Y + ((e.Bounds.Height / 2) -
                (e.Graphics.MeasureString($"0x{i:X}", Font).Height / 2))));
        }

        private void MeasureOrg(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 13;
        }

        private void OnClickLoadSecond(object sender, EventArgs e)
        {
            if (textBoxSecondDir.Text == null)
            {
                return;
            }

            string path = textBoxSecondDir.Text;
            string file = Path.Combine(path, "art.mul");
            string file2 = Path.Combine(path, "artidx.mul");
            if (File.Exists(file) && File.Exists(file2))
            {
                SecondArt.SetFileIndex(file2, file);
                LoadSecond();
            }
        }

        private void LoadSecond()
        {
            _mCompare.Clear();
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
            {
                return;
            }

            Brush fontBrush = Brushes.Gray;

            int i = int.Parse(listBoxOrg.Items[e.Index].ToString());
            if (listBoxSec.SelectedIndex == e.Index)
            {
                e.Graphics.FillRectangle(Brushes.LightSteelBlue, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            }

            if (!SecondArt.IsValidLand(i))
            {
                fontBrush = Brushes.Red;
            }
            else if (!Compare(i))
            {
                fontBrush = Brushes.Blue;
            }

            e.Graphics.DrawString($"0x{i:X}", Font, fontBrush,
                new PointF(5,
                e.Bounds.Y + ((e.Bounds.Height / 2) -
                (e.Graphics.MeasureString($"0x{i:X}", Font).Height / 2))));
        }

        private void MeasureSec(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 13;
        }

        private void OnIndexChangedSec(object sender, EventArgs e)
        {
            if (listBoxSec.SelectedIndex == -1 || listBoxSec.Items.Count < 1)
            {
                return;
            }

            int i = int.Parse(listBoxSec.Items[listBoxSec.SelectedIndex].ToString());
            listBoxOrg.SelectedIndex = listBoxOrg.Items.IndexOf(i);
            pictureBoxSec.BackgroundImage = SecondArt.IsValidLand(i) ? SecondArt.GetLand(i) : null;

            listBoxSec.Invalidate();
        }

        private bool Compare(int index)
        {
            if (_mCompare.ContainsKey(index))
            {
                return _mCompare[index];
            }

            Bitmap bitorg = Art.GetLand(index);
            Bitmap bitsec = SecondArt.GetLand(index);
            if (bitorg == null && bitsec == null)
            {
                _mCompare[index] = true;
                return true;
            }
            if (bitorg == null || bitsec == null
                               || bitorg.Size != bitsec.Size)
            {
                _mCompare[index] = false;
                return false;
            }

            byte[] btImage1 = new byte[1];
            btImage1 = (byte[])_ic.ConvertTo(bitorg, btImage1.GetType());
            byte[] btImage2 = new byte[1];
            btImage2 = (byte[])_ic.ConvertTo(bitsec, btImage2.GetType());

            string hash1String = BitConverter.ToString(_sha256.ComputeHash(btImage1));
            string hash2String = BitConverter.ToString(_sha256.ComputeHash(btImage2));

            bool res = hash1String == hash2String;
            _mCompare[index] = res;

            return res;
        }

        private void OnChangeShowDiff(object sender, EventArgs e)
        {
            if (_mCompare.Count < 1)
            {
                if (!checkBox1.Checked)
                {
                    return;
                }

                MessageBox.Show("Second Land file is not loaded!");
                checkBox1.Checked = false;
                return;
            }

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
                    {
                        cache.Add(i);
                    }
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
        }

        private void ExportAsBmp(object sender, EventArgs e)
        {
            if (listBoxSec.SelectedIndex == -1)
            {
                return;
            }

            int i = int.Parse(listBoxSec.Items[listBoxSec.SelectedIndex].ToString());
            if (!SecondArt.IsValidLand(i))
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Landtile(Sec) 0x{i:X}.bmp");
            SecondArt.GetLand(i).Save(fileName, ImageFormat.Bmp);
            MessageBox.Show(
                $"Landtile saved to {fileName}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void ExportAsTiff(object sender, EventArgs e)
        {
            if (listBoxSec.SelectedIndex == -1)
            {
                return;
            }

            int i = int.Parse(listBoxSec.Items[listBoxSec.SelectedIndex].ToString());
            if (!SecondArt.IsValidLand(i))
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Landtile(Sec) 0x{i:X}.tiff");
            SecondArt.GetLand(i).Save(fileName, ImageFormat.Tiff);
            MessageBox.Show(
                $"Landtile saved to {fileName}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void BrowseOnClick(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory containing the art files";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxSecondDir.Text = dialog.SelectedPath;
                }
            }
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            if (listBoxSec.SelectedIndex == -1)
            {
                return;
            }

            int i = int.Parse(listBoxSec.Items[listBoxSec.SelectedIndex].ToString());
            if (!SecondArt.IsValidLand(i))
            {
                return;
            }

            Bitmap copy = new Bitmap(SecondArt.GetLand(i));
            Art.ReplaceLand(i, copy);
            Options.ChangedUltimaClass["Art"] = true;
            ControlEvents.FireLandTileChangeEvent(this, i);
            _mCompare[i] = true;
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
            {
                listBoxOrg.Items.Add(i);
            }

            listBoxOrg.EndUpdate();
            listBoxOrg.Invalidate();
            listBoxSec.Invalidate();
            OnIndexChangedOrg(this, null);
        }
    }
}
