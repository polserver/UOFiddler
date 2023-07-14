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
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;
using Serilog;
using Ultima.Helpers;

namespace UoFiddler.Classes
{
    public static class FiddlerOptions
    {
        public static List<ExternTool> ExternTools { get; private set; }

        public static Version AppVersion => typeof(FiddlerOptions).Assembly.GetName().Version;

        public static ILogger Logger { get; private set; }

        internal static void SetLogger(ILogger logger)
        {
            Logger = logger;
            Options.SetLogger(logger);
        }

        /// <summary>
        /// Defines if an Update Check should be made on startup
        /// </summary>
        public static bool UpdateCheckOnStart { get; set; }

        public static string RepositoryOwner { get; } = "polserver";
        public static string RepositoryName { get; } = "UOFiddler";

        public static bool StoreFormState { get; set; }
        public static bool MaximisedForm { get; set; }
        public static Point FormPosition { get; set; }
        public static Size FormSize { get; set; }

        private static void MoveFiles(IEnumerable<FileInfo> files, string path)
        {
            foreach (FileInfo file in files)
            {
                string destFileName = Path.Combine(path, file.Name);
                if (File.Exists(destFileName))
                {
                    Logger.Information("MoveFiles. File exists. Skipping: {File}", destFileName);
                    continue;
                }

                Logger.Information("MoveFiles. Copying file: {File}", destFileName);
                file.CopyTo(destFileName);
            }
        }

        public static void Startup()
        {
            if (!Directory.Exists(Options.AppDataPath))
            {
                Logger.Information("Creating main app data path {AppDataPath}", Options.AppDataPath);
                Directory.CreateDirectory(Options.AppDataPath);
            }

            string plugInPath = Path.Combine(Options.AppDataPath, "plugins");
            if (!Directory.Exists(plugInPath))
            {
                Logger.Information("Creating app data plugin {AppDataPath}", plugInPath);
                Directory.CreateDirectory(plugInPath);
            }

            DirectoryInfo di = new DirectoryInfo(Application.StartupPath);
            MoveFiles(di.GetFiles("Options_default.xml", SearchOption.TopDirectoryOnly), Options.AppDataPath);
            MoveFiles(di.GetFiles("Animationlist.xml", SearchOption.TopDirectoryOnly), Options.AppDataPath);
            MoveFiles(di.GetFiles("Multilist.xml", SearchOption.TopDirectoryOnly), Options.AppDataPath);

            di = new DirectoryInfo(Path.Combine(Application.StartupPath, "plugins"));
            MoveFiles(di.GetFiles("*.xml", SearchOption.TopDirectoryOnly), plugInPath);

            string fileName = Path.Combine(Options.AppDataPath, "Options_default.xml");
            if (!File.Exists(fileName))
            {
                Logger.Fatal("Can't find default profile file: {FileName}", fileName);
                throw new FileNotFoundException($"Can't load default profile file {fileName}", "Options_default.xml");
            }
        }

        public static void SaveProfile()
        {
            if (Options.ProfileName is null)
            {
                Logger.Warning("SaveProfile - ProfileName is null!");
                return;
            }

            string fileName = Path.Combine(Options.AppDataPath, Options.ProfileName);
            Logger.Information("SaveProfile - start {Filename}", fileName);

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
            // + Colors
            comment = dom.CreateComment("Focus tile color for tile views");
            sr.AppendChild(comment);
            elem = dom.CreateElement("TileFocusColor");
            elem.SetAttribute("value", ColorTranslator.ToHtml(Options.TileFocusColor));
            sr.AppendChild(elem);

            comment = dom.CreateComment("Selected tile color for tile views");
            sr.AppendChild(comment);
            elem = dom.CreateElement("TileSelectionColor");
            elem.SetAttribute("value", ColorTranslator.ToHtml(Options.TileSelectionColor));
            sr.AppendChild(elem);

            comment = dom.CreateComment("Use tile background color as tile view background color");
            sr.AppendChild(comment);
            elem = dom.CreateElement("OverrideBackgroundColorFromTile");
            elem.SetAttribute("active", Options.OverrideBackgroundColorFromTile.ToString());
            sr.AppendChild(elem);

            comment = dom.CreateComment("Remove tile border in tile views");
            sr.AppendChild(comment);
            elem = dom.CreateElement("RemoveTileBorder");
            elem.SetAttribute("active", Options.RemoveTileBorder.ToString());
            sr.AppendChild(elem);
            // - Colors
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
            comment = dom.CreateComment("Offset Sound Ids by 1 (POL specific setting)");
            sr.AppendChild(comment);
            elem = dom.CreateElement("PolSoundIdOffset");
            elem.SetAttribute("active", Options.PolSoundIdOffset.ToString());
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
                    Logger.Information("SaveProfile - saving plugin {PlugIn}", plugIn);
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
            Logger.Information("SaveProfile - done {Filename}", fileName);
        }

        public static void LoadProfile(string filename)
        {
            Logger.Information("LoadProfile - start: {Filename}", filename);

            string fileName = Path.Combine(Options.AppDataPath, filename);
            if (!File.Exists(fileName))
            {
                Logger.Warning("LoadProfile: profile file doesn't exist: {Filename}", filename);
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

            elem = (XmlElement)xOptions.SelectSingleNode("TileFocusColor");
            if (elem != null)
            {
                Options.TileFocusColor = ColorTranslator.FromHtml(elem.GetAttribute("value"));
            }

            elem = (XmlElement)xOptions.SelectSingleNode("TileSelectionColor");
            if (elem != null)
            {
                Options.TileSelectionColor = ColorTranslator.FromHtml(elem.GetAttribute("value"));
            }

            elem = (XmlElement)xOptions.SelectSingleNode("OverrideBackgroundColorFromTile");
            if (elem != null)
            {
                Options.OverrideBackgroundColorFromTile = bool.Parse(elem.GetAttribute("active"));
            }

            elem = (XmlElement)xOptions.SelectSingleNode("RemoveTileBorder");
            if (elem != null)
            {
                Options.RemoveTileBorder = bool.Parse(elem.GetAttribute("active"));
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

            elem = (XmlElement)xOptions.SelectSingleNode("PolSoundIdOffset");
            if (elem != null)
            {
                Options.PolSoundIdOffset = bool.Parse(elem.GetAttribute("active"));
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
                Logger.Information("LoadProfile: adding plugin to load: {PluginName}", name);
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

            MapHelper.CheckForNewMapSize();

            Logger.Information("LoadProfile - done: {Filename}", filename);
        }
    }
}
