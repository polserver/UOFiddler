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
using System.Text;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using UoFiddler.Forms;

namespace UoFiddler
{
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

            var serilogLogger = new LoggerConfiguration()
                                .MinimumLevel.Information()
                                .WriteTo.File(logFileName,
                                              fileSizeLimitBytes: 44040192,
                                              rollingInterval: RollingInterval.Day,
                                              rollOnFileSizeLimit: true)
                                .CreateLogger();

            serilogLogger.Information("UOFiddler - Application start");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var services = new ServiceCollection();
            services.AddLogging(b => b.AddSerilog(serilogLogger, dispose: true));
            using var serviceProvider = services.BuildServiceProvider();

            try
            {
                FiddlerAppContext fiddlerAppContext = new(serviceProvider);
                Application.Run(fiddlerAppContext);
            }
            catch (Exception err)
            {
                Clipboard.SetDataObject(err.ToString(), true);
                serilogLogger.Fatal(err, "UOFiddler - unhandled exception caught!");
                Application.Run(new ExceptionForm(err));
                serilogLogger.Fatal("UOFiddler - unhandled exception - Application exit");
            }
        }
    }
}
