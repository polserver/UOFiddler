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

using System.Windows.Forms;

namespace UoFiddler.Controls.Forms
{
    public partial class ProgressBarDialog : Form
    {
        public ProgressBarDialog()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        public ProgressBarDialog(int max, string desc, bool useFileSaveEvent = true)
        {
            InitializeComponent();
            Text = desc;
            progressBar.Maximum = max;
            progressBar.Minimum = 0;
            progressBar.Value = 0;
            progressBar.Step = 1;
            if (useFileSaveEvent)
            {
                Ultima.Files.FileSaveEvent += OnChangeEvent;
            }
            else
            {
                Classes.ControlEvents.ProgressChangeEvent += OnChangeEvent;
            }

            Show();
        }

        private void OnChangeEvent()
        {
            progressBar.PerformStep();
        }
    }
}
