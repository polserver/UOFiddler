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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Ultima;

namespace FiddlerControls
{
    public partial class Gump : UserControl
    {
        public Gump()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            if (!Files.CacheData)
                Preload.Visible = false;
            ProgressBar.Visible = false;
        }

        private bool Loaded = false;
        private bool ShowFreeSlots = false;

        /// <summary>
        /// Reload when loaded (file changed)
        /// </summary>
        private void Reload()
        {
            if (Loaded)
                OnLoad(EventArgs.Empty);
        }

        protected override void OnLoad(EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Gumps"] = true;
            ShowFreeSlots = false;
            showFreeSlotsToolStripMenuItem.Checked = false;

            listBox.BeginUpdate();
            listBox.Items.Clear();
            List<object> cache = new List<object>();
            for (int i = 0; i < Gumps.GetCount(); ++i)
            {
                if (Gumps.IsValidIndex(i))
                    cache.Add((object)i);
            }
            listBox.Items.AddRange(cache.ToArray());
            listBox.EndUpdate();
            if (listBox.Items.Count > 0)
                listBox.SelectedIndex = 0;
            if (!Loaded)
            {
                FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
                FiddlerControls.Events.GumpChangeEvent += new FiddlerControls.Events.GumpChangeHandler(OnGumpChangeEvent);
            }
            Loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }
        private void OnGumpChangeEvent(object sender, int index)
        {
            if (!Loaded)
                return;
            if (sender.Equals(this))
                return;
            if (Gumps.IsValidIndex(index))
            {
                bool done = false;
                for (int i = 0; i < listBox.Items.Count; ++i)
                {
                    int j = int.Parse(listBox.Items[i].ToString());
                    if (j > index)
                    {
                        listBox.Items.Insert(i, index);
                        listBox.SelectedIndex = i;
                        done = true;
                        break;
                    }
                    if (j == index)
                    {
                        done = true;
                        break;
                    }
                }
                if (!done)
                    listBox.Items.Add(index);
            }
            else
            {
                for (int i = 0; i < listBox.Items.Count; ++i)
                {
                    int j = int.Parse(listBox.Items[i].ToString());
                    if (j == index)
                    {
                        listBox.Items.RemoveAt(i);
                        break;
                    }
                }
                listBox.Invalidate();
            }
        }

        static Brush BrushLightSteelBlue = Brushes.LightSteelBlue;
        static Brush BrushLightCoral = Brushes.LightCoral;
        static Brush BrushRed = Brushes.Red;
        static Brush BrushGray = Brushes.Gray;

        private void listBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;
            Brush fontBrush = BrushGray;

            int i = int.Parse(listBox.Items[e.Index].ToString());
            if (Gumps.IsValidIndex(i))
            {
                bool patched;
                Bitmap bmp = Gumps.GetGump(i, out patched);

                if (bmp != null)
                {
                    int width = bmp.Width > 100 ? 100 : bmp.Width;
                    int height = bmp.Height > 54 ? 54 : bmp.Height;

                    if (listBox.SelectedIndex == e.Index)
                        e.Graphics.FillRectangle(BrushLightSteelBlue, e.Bounds.X, e.Bounds.Y, 105, 60);
                    else if (patched)
                        e.Graphics.FillRectangle(BrushLightCoral, e.Bounds.X, e.Bounds.Y, 105, 60);

                    e.Graphics.DrawImage(bmp, new Rectangle(e.Bounds.X + 3, e.Bounds.Y + 3, width, height));
                }
                else
                    fontBrush = BrushRed;
            }
            else
            {
                if (listBox.SelectedIndex == e.Index)
                    e.Graphics.FillRectangle(BrushLightSteelBlue, e.Bounds.X, e.Bounds.Y, 105, 60);
                fontBrush = BrushRed;
            }

            e.Graphics.DrawString(String.Format("0x{0:X}", i), Font, fontBrush,
                new PointF((float)105,
                e.Bounds.Y + ((e.Bounds.Height / 2) -
                (e.Graphics.MeasureString(String.Format("0x{0:X}", i), Font).Height / 2))));
        }

        private void listBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 60;
        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex == -1)
                return;

            int i = int.Parse(listBox.Items[listBox.SelectedIndex].ToString());
            if (Gumps.IsValidIndex(i))
            {
                Bitmap bmp = Gumps.GetGump(i);
                if (bmp != null)
                {
                    pictureBox.BackgroundImage = bmp;
                    IDLabel.Text = String.Format("ID: 0x{0:X} ({1})", i, i);
                    SizeLabel.Text = String.Format("Size: {0},{1}", bmp.Width, bmp.Height);
                }
                else
                    pictureBox.BackgroundImage = null;
            }
            else
                pictureBox.BackgroundImage = null;
            listBox.Invalidate();
        }

        private void onClickReplace(object sender, EventArgs e)
        {
            if (listBox.SelectedItems.Count == 1)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = false;
                    dialog.Title = "Choose image file to replace";
                    dialog.CheckFileExists = true;
                    dialog.Filter = "image files (*.tiff;*.bmp)|*.tiff;*.bmp";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Bitmap bmp = new Bitmap(dialog.FileName);
                        if (dialog.FileName.Contains(".bmp"))
                            bmp = Utils.ConvertBmp(bmp);
                        int i = int.Parse(listBox.Items[listBox.SelectedIndex].ToString());
                        Gumps.ReplaceGump(i, bmp);
                        FiddlerControls.Events.FireGumpChangeEvent(this, i);
                        listBox.Invalidate();
                        listBox_SelectedIndexChanged(this, EventArgs.Empty);
                        Options.ChangedUltimaClass["Gumps"] = true;
                    }
                }
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            DialogResult result =
                        MessageBox.Show("Are you sure? Will take a while", "Save",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                Gumps.Save(FiddlerControls.Options.OutputPath);
                Cursor.Current = Cursors.Default;
                MessageBox.Show(
                    String.Format("Saved to {0}", FiddlerControls.Options.OutputPath),
                    "Save",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
                Options.ChangedUltimaClass["Gumps"] = false;
            }
        }

        private void onClickRemove(object sender, EventArgs e)
        {
            int i = int.Parse(listBox.Items[listBox.SelectedIndex].ToString());
            DialogResult result =
                        MessageBox.Show(String.Format("Are you sure to remove {0}", i), "Remove",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                Gumps.RemoveGump(i);
                FiddlerControls.Events.FireGumpChangeEvent(this, i);
                if (!ShowFreeSlots)
                    listBox.Items.RemoveAt(listBox.SelectedIndex);
                pictureBox.BackgroundImage = null;
                listBox.Invalidate();
                Options.ChangedUltimaClass["Gumps"] = true;
            }
        }

        private void onClickFindFree(object sender, EventArgs e)
        {
            int id = int.Parse(listBox.Items[listBox.SelectedIndex].ToString());
            ++id;
            for (int i = listBox.SelectedIndex + 1; i < listBox.Items.Count; ++i, ++id)
            {
                if (id < int.Parse(listBox.Items[i].ToString()))
                {
                    listBox.SelectedIndex = i;
                    break;
                }
                else if (ShowFreeSlots)
                {
                    if (!Gumps.IsValidIndex(int.Parse(listBox.Items[i].ToString())))
                    {
                        listBox.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void onTextChanged_InsertAt(object sender, EventArgs e)
        {
            int index;
            if (Utils.ConvertStringToInt(InsertText.Text, out index, 0, Gumps.GetCount()))
            {
                if (Gumps.IsValidIndex(index))
                    InsertText.ForeColor = Color.Red;
                else
                    InsertText.ForeColor = Color.Black;
            }
            else
                InsertText.ForeColor = Color.Red;
        }

        private void onKeydown_InsertText(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            int index;
            if (Utils.ConvertStringToInt(InsertText.Text, out index, 0, Gumps.GetCount()))
            {
                if (Gumps.IsValidIndex(index))
                    return;
                contextMenuStrip1.Close();
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = false;
                    dialog.Title = String.Format("Choose image file to insert at 0x{0:X}", index);
                    dialog.CheckFileExists = true;
                    dialog.Filter = "image files (*.tiff;*.bmp)|*.tiff;*.bmp";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Bitmap bmp = new Bitmap(dialog.FileName);
                        if (dialog.FileName.Contains(".bmp"))
                            bmp = Utils.ConvertBmp(bmp);
                        Gumps.ReplaceGump(index, bmp);
                        FiddlerControls.Events.FireGumpChangeEvent(this, index);
                        bool done = false;
                        for (int i = 0; i < listBox.Items.Count; ++i)
                        {
                            int j = int.Parse(listBox.Items[i].ToString());
                            if (j > index)
                            {
                                listBox.Items.Insert(i, index);
                                listBox.SelectedIndex = i;
                                done = true;
                                break;
                            }
                            else if (ShowFreeSlots)
                            {
                                if (j == i)
                                {
                                    listBox.SelectedIndex = i;
                                    done = true;
                                    break;
                                }
                            }
                        }
                        if (!done)
                        {
                            listBox.Items.Add(index);
                            listBox.SelectedIndex = listBox.Items.Count - 1;
                        }
                        Options.ChangedUltimaClass["Gumps"] = true;
                    }
                }
            }
        }

        private void extract_Image_ClickBmp(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            int i = int.Parse(listBox.Items[listBox.SelectedIndex].ToString());
            string FileName = Path.Combine(path, String.Format("Gump {0}.bmp", i));
            Bitmap bit = new Bitmap(Gumps.GetGump(i));
            if (bit != null)
                bit.Save(FileName, ImageFormat.Bmp);
            bit.Dispose();
            MessageBox.Show(
                String.Format("Gump saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void extract_Image_ClickTiff(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            int i = int.Parse(listBox.Items[listBox.SelectedIndex].ToString());
            string FileName = Path.Combine(path, String.Format("Gump {0}.tiff", i));
            Bitmap bit = new Bitmap(Gumps.GetGump(i));
            if (bit != null)
                bit.Save(FileName, ImageFormat.Tiff);
            bit.Dispose();
            MessageBox.Show(
                String.Format("Gump saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void extract_Image_ClickJpg(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            int i = int.Parse(listBox.Items[listBox.SelectedIndex].ToString());
            string FileName = Path.Combine(path, String.Format("Gump {0}.jpg", i));
            Bitmap bit = new Bitmap(Gumps.GetGump(i));
            if (bit != null)
                bit.Save(FileName, ImageFormat.Jpeg);
            bit.Dispose();
            MessageBox.Show(
                String.Format("Gump saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClick_SaveAllBmp(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    for (int i = 0; i < listBox.Items.Count; ++i)
                    {
                        int index = int.Parse(listBox.Items[i].ToString());
                        if (index >= 0)
                        {
                            string FileName = Path.Combine(dialog.SelectedPath, String.Format("Gump {0}.bmp", index));
                            Bitmap bit = new Bitmap(Gumps.GetGump(index));
                            if (bit != null)
                                bit.Save(FileName, ImageFormat.Bmp);
                            bit.Dispose();
                        }
                    }
                    MessageBox.Show(String.Format("All Gumps saved to {0}", dialog.SelectedPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void OnClick_SaveAllTiff(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    for (int i = 0; i < listBox.Items.Count; ++i)
                    {
                        int index = int.Parse(listBox.Items[i].ToString());
                        if (index >= 0)
                        {
                            string FileName = Path.Combine(dialog.SelectedPath, String.Format("Gump {0}.tiff", index));
                            Bitmap bit = new Bitmap(Gumps.GetGump(index));
                            if (bit != null)
                                bit.Save(FileName, ImageFormat.Tiff);
                            bit.Dispose();
                        }
                    }
                    MessageBox.Show(String.Format("All Gumps saved to {0}", dialog.SelectedPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void OnClick_SaveAllJpg(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    for (int i = 0; i < listBox.Items.Count; ++i)
                    {
                        int index = int.Parse(listBox.Items[i].ToString());
                        if (index >= 0)
                        {
                            string FileName = Path.Combine(dialog.SelectedPath, String.Format("Gump {0}.jpg", index));
                            Bitmap bit = new Bitmap(Gumps.GetGump(index));
                            if (bit != null)
                                bit.Save(FileName, ImageFormat.Jpeg);
                            bit.Dispose();
                        }
                    }
                    MessageBox.Show(String.Format("All Gumps saved to {0}", dialog.SelectedPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void OnClickShowFreeSlots(object sender, EventArgs e)
        {
            ShowFreeSlots = !ShowFreeSlots;
            if (ShowFreeSlots)
            {
                listBox.BeginUpdate();
                listBox.Items.Clear();
                List<object> cache = new List<object>();
                for (int i = 0; i < Gumps.GetCount(); ++i)
                {
                    cache.Add((object)i);
                }
                listBox.Items.AddRange(cache.ToArray());
                listBox.EndUpdate();
                if (listBox.Items.Count > 0)
                    listBox.SelectedIndex = 0;
            }
            else
                OnLoad(null);
        }

        #region Preloader
        private void OnClickPreload(object sender, EventArgs e)
        {
            if (PreLoader.IsBusy)
                return;
            ProgressBar.Minimum = 1;
            ProgressBar.Maximum = Gumps.GetCount();
            ProgressBar.Step = 1;
            ProgressBar.Value = 1;
            ProgressBar.Visible = true;
            PreLoader.RunWorkerAsync();
        }

        private void PreLoaderDoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < Gumps.GetCount(); ++i)
            {
                Gumps.GetGump(i);
                PreLoader.ReportProgress(1);
            }
        }

        private void PreLoaderProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.PerformStep();
        }

        private void PreLoaderCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressBar.Visible = false;
        }
        #endregion
    }
}
