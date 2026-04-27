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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UoFiddler.Classes;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.UserControls;
using UoFiddler.Forms;

namespace UoFiddler
{
    internal sealed class FiddlerAppContext : ApplicationContext
    {
        private readonly ILogger<FiddlerAppContext> _logger;

        internal FiddlerAppContext(IServiceProvider services)
        {
            AppLog.Initialize(services.GetRequiredService<ILoggerFactory>());
            _logger = services.GetRequiredService<ILogger<FiddlerAppContext>>();

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ApplicationExit += OnApplicationExit;

            FiddlerOptions.Startup();

            _logger.LogInformation("Starting loading profile form...");
            var profile = new LoadProfileForm(services.GetRequiredService<ILogger<LoadProfileForm>>()) { TopMost = true };
            var profileResult = profile.ShowDialog();
            if (profileResult == DialogResult.Cancel)
            {
                _logger.LogInformation("No profile loaded... exiting");
                return;
            }

            if (FiddlerOptions.UpdateCheckOnStart)
            {
                _logger.LogInformation("Update check. Current version is {Version}", FiddlerOptions.AppVersion);
                UpdateRunner.RunAsync(FiddlerOptions.RepositoryOwner, FiddlerOptions.RepositoryName, FiddlerOptions.AppVersion, false).GetAwaiter().GetResult();
            }

            _logger.LogInformation("Starting main form...");
            MainForm = new MainForm(services.GetRequiredService<ILogger<MainForm>>())
            {
                Text = $"{Application.ProductName} (Profile: {Options.ProfileName.Replace("Options_", "").Replace(".xml", "")})"
            };
            MainForm.Show();
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            FiddlerOptions.SaveProfile();
            MapControl.SaveMapOverlays();
            _logger.LogInformation("UOFiddler - Application exit");
        }
    }
}
