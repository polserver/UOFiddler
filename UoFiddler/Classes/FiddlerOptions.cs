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
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Forms;

namespace UoFiddler.Classes
{
    public static class FiddlerOptions
    {
        public static List<ExternTool> ExternTools { get; private set; }

        /// <summary>
        /// Defines if an Update Check should be made on startup
        /// </summary>
        public static bool UpdateCheckOnStart { get; set; } = true;

        public static bool StoreFormState { get; set; }
        public static bool MaximisedForm { get; set; }
        public static Point FormPosition { get; set; }
        public static Size FormSize { get; set; }

        // TODO: unused?
        //public static string OutputPath { get; set; }

        private static void MoveFiles(IEnumerable<FileInfo> files, string path)
        {
            foreach (FileInfo file in files)
            {
                string destFileName = Path.Combine(path, file.Name);
                if (File.Exists(destFileName))
                {
                    continue;
                }

                file.CopyTo(destFileName);
            }
        }

        public static void Startup()
        {
            // Move xml files to AppData
            if (!Directory.Exists(Options.AppDataPath))
            {
                Directory.CreateDirectory(Options.AppDataPath);
            }

            string plugInPath = Path.Combine(Options.AppDataPath, "plugins");
            if (!Directory.Exists(plugInPath))
            {
                Directory.CreateDirectory(plugInPath);
            }

            DirectoryInfo di = new DirectoryInfo(Application.StartupPath);
            MoveFiles(di.GetFiles("Options_default.xml", SearchOption.TopDirectoryOnly), Options.AppDataPath);
            MoveFiles(di.GetFiles("Animationlist.xml", SearchOption.TopDirectoryOnly), Options.AppDataPath);
            MoveFiles(di.GetFiles("Multilist.xml", SearchOption.TopDirectoryOnly), Options.AppDataPath);

            di = new DirectoryInfo(Path.Combine(Application.StartupPath, "plugins"));
            MoveFiles(di.GetFiles("*.xml", SearchOption.TopDirectoryOnly), plugInPath);

            Load();

            if (UpdateCheckOnStart)
            {
                RunUpdater();
            }
        }

        private static void RunUpdater()
        {
            using (BackgroundWorker updater = new BackgroundWorker())
            {
                updater.DoWork += Updater_DoWork;
                updater.RunWorkerCompleted += Updater_RunWorkerCompleted;
                updater.RunWorkerAsync();
            }
        }

        public static void Save()
        {
            string fileName = Path.Combine(Options.AppDataPath, Options.ProfileName);

            XmlDocument dom = new XmlDocument();
            XmlDeclaration decl = dom.CreateXmlDeclaration("1.0", "utf-8", null);
            dom.AppendChild(decl);
            XmlElement sr = dom.CreateElement("Options");

            XmlComment comment = dom.CreateComment("Output Path");
            sr.AppendChild(comment);
            XmlElement elem = dom.CreateElement("OutputPath");
            elem.SetAttribute("path", Options.OutputPath);
            sr.AppendChild(elem);
            comment = dom.CreateComment("ItemSize controls the size of images in items tab");
            sr.AppendChild(comment);
            elem = dom.CreateElement("ItemSize");
            elem.SetAttribute("width", Options.ArtItemSizeWidth.ToString());
            elem.SetAttribute("height", Options.ArtItemSizeHeight.ToString());
            sr.AppendChild(elem);
            comment = dom.CreateComment("ItemClip images in items tab shrinked or clipped");
            sr.AppendChild(comment);
            elem = dom.CreateElement("ItemClip");
            elem.SetAttribute("active", Options.ArtItemClip.ToString());
            sr.AppendChild(elem);
            comment = dom.CreateComment("CacheData should mul entries be cached for faster load");
            sr.AppendChild(comment);
            elem = dom.CreateElement("CacheData");
            elem.SetAttribute("active", Files.CacheData.ToString());
            sr.AppendChild(elem);
            comment = dom.CreateComment("NewMapSize Felucca/Trammel width 7168?");
            sr.AppendChild(comment);
            elem = dom.CreateElement("NewMapSize");
            elem.SetAttribute("active", Map.Felucca.Width == 7168 ? true.ToString() : false.ToString());
            sr.AppendChild(elem);
            comment = dom.CreateComment("UseMapDiff should mapdiff files be used");
            sr.AppendChild(comment);
            elem = dom.CreateElement("UseMapDiff");
            elem.SetAttribute("active", Map.UseDiff.ToString());
            sr.AppendChild(elem);
            comment = dom.CreateComment("Alternative layout in item/landtile/texture tab?");
            sr.AppendChild(comment);
            elem = dom.CreateElement("AlternativeDesign");
            elem.SetAttribute("active", Options.DesignAlternative.ToString());
            sr.AppendChild(elem);
            comment = dom.CreateComment("Use Hashfile to speed up load?");
            sr.AppendChild(comment);
            elem = dom.CreateElement("UseHashFile");
            elem.SetAttribute("active", Files.UseHashFile.ToString());
            sr.AppendChild(elem);
            comment = dom.CreateComment("Should an Update Check be done on startup?");
            sr.AppendChild(comment);
            elem = dom.CreateElement("UpdateCheck");
            elem.SetAttribute("active", UpdateCheckOnStart.ToString());
            sr.AppendChild(elem);

            comment = dom.CreateComment("Defines the cmd to send Client to loc");
            sr.AppendChild(comment);
            comment = dom.CreateComment("{1} = x, {2} = y, {3} = z, {4} = mapid, {5} = mapname");
            sr.AppendChild(comment);
            elem = dom.CreateElement("SendCharToLoc");
            elem.SetAttribute("cmd", Options.MapCmd);
            elem.SetAttribute("args", Options.MapArgs);
            sr.AppendChild(elem);

            comment = dom.CreateComment("Defines the map names");
            sr.AppendChild(comment);
            elem = dom.CreateElement("MapNames");
            elem.SetAttribute("map0", Options.MapNames[0]);
            elem.SetAttribute("map1", Options.MapNames[1]);
            elem.SetAttribute("map2", Options.MapNames[2]);
            elem.SetAttribute("map3", Options.MapNames[3]);
            elem.SetAttribute("map4", Options.MapNames[4]);
            elem.SetAttribute("map5", Options.MapNames[5]);
            sr.AppendChild(elem);

            comment = dom.CreateComment("Extern Tools settings");
            sr.AppendChild(comment);

            if (ExternTools != null)
            {
                foreach (ExternTool tool in ExternTools)
                {
                    XmlElement externalToolElement = dom.CreateElement("ExternTool");
                    externalToolElement.SetAttribute("name", tool.Name);
                    externalToolElement.SetAttribute("path", tool.FileName);

                    for (int i = 0; i < tool.Args.Count; i++)
                    {
                        XmlElement argsElement = dom.CreateElement("Args");
                        argsElement.SetAttribute("name", tool.ArgsName[i]);
                        argsElement.SetAttribute("arg", tool.Args[i]);
                        externalToolElement.AppendChild(argsElement);
                    }
                    sr.AppendChild(externalToolElement);
                }
            }

            comment = dom.CreateComment("Loaded Plugins");
            sr.AppendChild(comment);
            if (Options.PluginsToLoad != null)
            {
                foreach (string plugIn in Options.PluginsToLoad)
                {
                    XmlElement xmlPlugin = dom.CreateElement("Plugin");
                    xmlPlugin.SetAttribute("name", plugIn);
                    sr.AppendChild(xmlPlugin);
                }
            }

            comment = dom.CreateComment("Path settings");
            sr.AppendChild(comment);
            elem = dom.CreateElement("RootPath");
            elem.SetAttribute("path", Files.RootDir);
            sr.AppendChild(elem);
            List<string> sorter = new List<string>(Files.MulPath.Keys);
            sorter.Sort();
            foreach (string key in sorter)
            {
                XmlElement path = dom.CreateElement("Paths");
                path.SetAttribute("key", key);
                path.SetAttribute("value", Files.MulPath[key]);
                sr.AppendChild(path);
            }
            dom.AppendChild(sr);

            comment = dom.CreateComment("Disabled Tab Views");
            sr.AppendChild(comment);
            foreach (KeyValuePair<int, bool> kvp in Options.ChangedViewState)
            {
                if (kvp.Value)
                {
                    continue;
                }

                XmlElement viewState = dom.CreateElement("TabView");
                viewState.SetAttribute("tab", kvp.Key.ToString());
                sr.AppendChild(viewState);
            }

            comment = dom.CreateComment("ViewState of the MainForm");
            sr.AppendChild(comment);
            elem = dom.CreateElement("ViewState");
            elem.SetAttribute("Active", StoreFormState.ToString());
            elem.SetAttribute("Maximised", MaximisedForm.ToString());
            elem.SetAttribute("PositionX", FormPosition.X.ToString());
            elem.SetAttribute("PositionY", FormPosition.Y.ToString());
            elem.SetAttribute("Height", FormSize.Height.ToString());
            elem.SetAttribute("Width", FormSize.Width.ToString());
            sr.AppendChild(elem);

            comment = dom.CreateComment("TileData Options");
            sr.AppendChild(comment);
            elem = dom.CreateElement("TileDataDirectlySaveOnChange");
            elem.SetAttribute("value", Options.TileDataDirectlySaveOnChange.ToString());
            sr.AppendChild(elem);

            dom.Save(fileName);
        }

        private static void Load()
        {
            string fileName = Path.Combine(Options.AppDataPath, "Options_default.xml");
            if (!File.Exists(fileName))
            {
                return;
            }

            LoadProfile profile = new LoadProfile
            {
                TopMost = true
            };
            profile.ShowDialog();

            //loadProfile(FileName);
        }

        public static void LoadProfile(string filename)
        {
            string fileName = Path.Combine(Options.AppDataPath, filename);
            if (!File.Exists(fileName))
            {
                return;
            }

            XmlDocument dom = new XmlDocument();
            dom.Load(fileName);
            XmlElement xOptions = dom["Options"];
            XmlElement elem = (XmlElement)xOptions?.SelectSingleNode("OutputPath");
            if (elem != null)
            {
                Options.OutputPath = elem.GetAttribute("path");
                if (!Directory.Exists(Options.OutputPath))
                {
                    Options.OutputPath = Options.AppDataPath;
                }
            }
            else
            {
                Options.OutputPath = Options.AppDataPath;
            }

            elem = (XmlElement)xOptions.SelectSingleNode("ItemSize");
            if (elem != null)
            {
                Options.ArtItemSizeWidth = int.Parse(elem.GetAttribute("width"));
                Options.ArtItemSizeHeight = int.Parse(elem.GetAttribute("height"));
            }

            elem = (XmlElement)xOptions.SelectSingleNode("ItemClip");
            if (elem != null)
            {
                Options.ArtItemClip = bool.Parse(elem.GetAttribute("active"));
            }

            elem = (XmlElement)xOptions.SelectSingleNode("CacheData");
            if (elem != null)
            {
                Files.CacheData = bool.Parse(elem.GetAttribute("active"));
            }

            elem = (XmlElement)xOptions.SelectSingleNode("NewMapSize");
            if (elem != null && bool.Parse(elem.GetAttribute("active")))
            {
                Map.Felucca.Width = 7168;
                Map.Trammel.Width = 7168;
            }

            elem = (XmlElement)xOptions.SelectSingleNode("UseMapDiff");
            if (elem != null)
            {
                Map.StartUpSetDiff(bool.Parse(elem.GetAttribute("active")));
            }

            elem = (XmlElement)xOptions.SelectSingleNode("AlternativeDesign");
            if (elem != null)
            {
                Options.DesignAlternative = bool.Parse(elem.GetAttribute("active"));
            }

            elem = (XmlElement)xOptions.SelectSingleNode("UseHashFile");
            if (elem != null)
            {
                Files.UseHashFile = bool.Parse(elem.GetAttribute("active"));
            }

            elem = (XmlElement)xOptions.SelectSingleNode("UpdateCheck");
            if (elem != null)
            {
                UpdateCheckOnStart = bool.Parse(elem.GetAttribute("active"));
            }

            elem = (XmlElement)xOptions.SelectSingleNode("SendCharToLoc");
            if (elem != null)
            {
                Options.MapCmd = elem.GetAttribute("cmd");
                Options.MapArgs = elem.GetAttribute("args");
            }

            elem = (XmlElement)xOptions.SelectSingleNode("MapNames");
            if (elem != null)
            {
                Options.MapNames[0] = elem.GetAttribute("map0");
                Options.MapNames[1] = elem.GetAttribute("map1");
                Options.MapNames[2] = elem.GetAttribute("map2");
                Options.MapNames[3] = elem.GetAttribute("map3");
                Options.MapNames[4] = elem.GetAttribute("map4");
                Options.MapNames[5] = elem.GetAttribute("map5");
            }

            ExternTools = new List<ExternTool>();
            foreach (XmlElement xTool in xOptions.SelectNodes("ExternTool"))
            {
                string name = xTool.GetAttribute("name");
                string file = xTool.GetAttribute("path");
                ExternTool tool = new ExternTool(name, file);
                foreach (XmlElement xArg in xTool.SelectNodes("Args"))
                {
                    string argName = xArg.GetAttribute("name");
                    string arg = xArg.GetAttribute("arg");
                    tool.Args.Add(arg);
                    tool.ArgsName.Add(argName);
                }
                ExternTools.Add(tool);
            }

            foreach (XmlElement xPlug in xOptions.SelectNodes("Plugin"))
            {
                string name = xPlug.GetAttribute("name");
                Options.PluginsToLoad.Add(name);
            }

            elem = (XmlElement)xOptions.SelectSingleNode("RootPath");
            if (elem != null)
            {
                Files.RootDir = elem.GetAttribute("path");
            }

            foreach (XmlElement xPath in xOptions.SelectNodes("Paths"))
            {
                string key = xPath.GetAttribute("key");
                Files.MulPath[key] = xPath.GetAttribute("value");
            }

            foreach (XmlElement xTab in xOptions.SelectNodes("TabView"))
            {
                int viewTab = Convert.ToInt32(xTab.GetAttribute("tab"));
                Options.ChangedViewState[viewTab] = false;
            }

            elem = (XmlElement)xOptions.SelectSingleNode("ViewState");
            if (elem != null)
            {
                StoreFormState = bool.Parse(elem.GetAttribute("Active"));
                MaximisedForm = bool.Parse(elem.GetAttribute("Maximised"));
                FormPosition = new Point(int.Parse(elem.GetAttribute("PositionX")), int.Parse(elem.GetAttribute("PositionY")));
                FormSize = new Size(int.Parse(elem.GetAttribute("Width")), int.Parse(elem.GetAttribute("Height")));
            }

            elem = (XmlElement)xOptions.SelectSingleNode("TileDataDirectlySaveOnChange");
            Options.TileDataDirectlySaveOnChange = elem != null && (elem.GetAttribute("value") ?? "").Equals("true", StringComparison.OrdinalIgnoreCase);

            Files.CheckForNewMapSize();
        }

        /// <summary>
        /// Checks polserver forum for updates
        /// </summary>
        /// <returns></returns>
        public static string[] CheckForUpdate(out string error)
        {
            StringBuilder sb = new StringBuilder();
            byte[] buf = new byte[8192];

            error = "";
            string[] match;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://uofiddler.polserver.com/latestversion");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream resStream = response.GetResponseStream();

                int count = 0;
                do
                {
                    if (resStream != null)
                    {
                        count = resStream.Read(buf, 0, buf.Length);
                    }

                    if (count == 0)
                    {
                        continue;
                    }

                    string tempString = Encoding.ASCII.GetString(buf, 0, count);
                    sb.Append(tempString);
                }
                while (count > 0);

                match = sb.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None);

                response.Close();
                resStream?.Dispose();
            }
            catch (Exception e)
            {
                match = null;
                error = e.Message;
            }

            return match;
        }

        private static void Updater_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = CheckForUpdate(out string error);

            if (e.Result == null)
            {
                throw new Exception(error);
            }
        }

        private static void Updater_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show($"Error:\n{e.Error}", "Check for Update");
                return;
            }

            string[] match = (string[])e.Result;
            if (match != null)
            {
                if (!VersionCheck(match[0]))
                {
                    return;
                }

                DialogResult result = MessageBox.Show(
                    $"{string.Format("A new version was found: {1}\nYour version: {0}", Forms.MainForm.Version, match[0])}\n\nDownload now?",
                    "Check for Update",
                    MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    DownloadFile(match[1]);
                }
            }
            else
            {
                MessageBox.Show("Failed to get version info", "Check for Update");
            }
        }

        public static bool VersionCheck(string newVersionParam)
        {
            Version.TryParse(Forms.MainForm.Version, out Version currentVersion);
            Version.TryParse(newVersionParam, out Version newVersion);

            return newVersion > currentVersion;
        }

        private static void DownloadFile(string file)
        {
            string fileName = Path.Combine(Options.OutputPath, file.Trim());
            using (WebClient web = new WebClient())
            {
                web.DownloadFileCompleted += OnDownloadFileCompleted;
                web.DownloadFileAsync(new Uri(
                    $"http://downloads.polserver.com/browser.php?download=./Projects/uofiddler/{file}"), fileName);
            }
        }

        private static void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show($"An error occurred while downloading UOFiddler\n{e.Error.Message}",
                    "Updater");
                return;
            }
            MessageBox.Show("Finished Download", "Updater");
        }
    }
}
