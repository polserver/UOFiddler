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
using System.IO;
using System.Windows.Forms;
using Serilog;
using UoFiddler.Classes;
using UoFiddler.Controls.UserControls;
using UoFiddler.Forms;

namespace UoFiddler
{
    internal class FiddlerAppContext : ApplicationContext
    {
        private readonly ILogger _logger;

        internal FiddlerAppContext(ILogger logger)
        {
            _logger = logger;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ApplicationExit += OnApplicationExit;

            FiddlerOptions.SetLogger(_logger);
            FiddlerOptions.Startup();

            _logger.Information("Starting loading profile form...");
            var profile = new LoadProfile { TopMost = true };
            var profileResult = profile.ShowDialog();
            if (profileResult == DialogResult.Cancel)
            {
                _logger.Information("No profile loaded... exiting.");
                return;
            }

            if (FiddlerOptions.UpdateCheckOnStart)
            {
                _logger.Information("Update check. Current version is {currentVersion}", FiddlerOptions.AppVersion);
                UpdateRunner.RunAsync(FiddlerOptions.RepositoryOwner, FiddlerOptions.RepositoryName, FiddlerOptions.AppVersion, false).GetAwaiter().GetResult();
            }

            _logger.Information("Starting main form...");
            MainForm = new MainForm();
            MainForm.Show();
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            FiddlerOptions.SaveProfile();
            Map.SaveMapOverlays();
            _logger.Information("UOFiddler - Application exit.");
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
            string fiddlerAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UoFiddler");
            string logFileName = Path.Combine(fiddlerAppDataPath, "log", "uo-fiddler-log.txt");

            var logger = new LoggerConfiguration()
                                .MinimumLevel.Information()
                                .WriteTo.File(logFileName,
                                              rollingInterval: RollingInterval.Day,
                                              rollOnFileSizeLimit: true,
                                              fileSizeLimitBytes: 44040192)
                                .CreateLogger();

            logger.Information("UOFiddler - Application start.");

            try
            {
                FiddlerAppContext fiddlerAppContext = new FiddlerAppContext(logger);
                Application.Run(fiddlerAppContext);
            }
            catch (Exception err)
            {
                Clipboard.SetDataObject(err.ToString(), true);
                logger.Fatal("UOFiddler - unhandled exception caught!");
                logger.Fatal("{err}", err);
                Application.Run(new ExceptionForm(err));
                logger.Fatal("UOFiddler - unhandled exception - Application exit.");
            }
        }
    }
}