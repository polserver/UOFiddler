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