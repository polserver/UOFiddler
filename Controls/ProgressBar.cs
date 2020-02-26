using System.Windows.Forms;

namespace FiddlerControls
{
    public partial class ProgressBar : Form
    {
        public ProgressBar()
        {
            InitializeComponent();
        }

        public ProgressBar(int max, string desc, bool useFileSaveEvent = true)
        {
            InitializeComponent();
            Text = desc;
            progressBar1.Maximum = max;
            progressBar1.Minimum = 0;
            progressBar1.Value = 0;
            progressBar1.Step = 1;
            if (useFileSaveEvent)
            {
                Ultima.Files.FileSaveEvent += OnChangeEvent;
            }
            else
            {
                FiddlerControls.Events.ProgressChangeEvent += OnChangeEvent;
            }

            Show();
        }

        private void OnChangeEvent()
        {
            progressBar1.PerformStep();
        }
    }
}
