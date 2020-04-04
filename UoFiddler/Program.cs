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
using UoFiddler.Classes;
using UoFiddler.Controls.UserControls;
using UoFiddler.Forms;

namespace UoFiddler
{
    internal class FiddlerAppContext : ApplicationContext
    {
        internal FiddlerAppContext()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ApplicationExit += OnApplicationExit;

            FiddlerOptions.Startup();

            var profile = new LoadProfile { TopMost = true };
            var profileResult = profile.ShowDialog();
            if (profileResult == DialogResult.Cancel)
            {
                return;
            }

            if (FiddlerOptions.UpdateCheckOnStart)
            {
                UpdateRunner.RunAsync(FiddlerOptions.RepositoryOwner, FiddlerOptions.RepositoryName, FiddlerOptions.AppVersion, false).GetAwaiter().GetResult();
            }

            MainForm = new MainForm();
            MainForm.Show();
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            FiddlerOptions.Save();
            Map.SaveMapOverlays();
        }
    }

    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            try
            {
                FiddlerAppContext fiddlerAppContext = new FiddlerAppContext();
                Application.Run(fiddlerAppContext);
            }
            catch (Exception err)
            {
                Clipboard.SetDataObject(err.ToString(), true);
                Application.Run(new ExceptionForm(err));
            }
        }
    }
}