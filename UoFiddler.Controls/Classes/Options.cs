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

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Serilog;

namespace UoFiddler.Controls.Classes
{
    public static class Options
    {
        /// <summary>
        /// Logger instance
        /// </summary>
        public static ILogger Logger { get; private set; }

        public static void SetLogger(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Defines Element Width in ItemShow
        /// </summary>
        public static int ArtItemSizeWidth { get; set; } = 48;

        /// <summary>
        /// Defines Element Height in ItemShow
        /// </summary>
        public static int ArtItemSizeHeight { get; set; } = 48;

        /// <summary>
        /// Defines if Element should be clipped or shrunken in ItemShow
        /// </summary>
        public static bool ArtItemClip { get; set; } = true;

        /// <summary>
        /// Defines if the right panel into the Sounds Tab
        /// should be toggled on
        /// </summary>
        public static bool RightPanelInSoundsTab { get; set; } = true;

        /// <summary>
        /// Offsets the sound ids in Sound tab by 1 (POL specific setting)
        /// </summary>
        public static bool PolSoundIdOffset { get; set; }

        /// <summary>
        /// Defines the cmd to Send Client to Loc
        /// </summary>
        public static string MapCmd { get; set; } = ".goforce";

        /// <summary>
        /// Defines the args for Send Client
        /// </summary>
        public static string MapArgs { get; set; } = "{1} {2} {3} {4}";

        /// <summary>
        /// Defines the MapNames
        /// </summary>
        public static string[] MapNames { get; set; } = { "Felucca", "Trammel", "Ilshenar", "Malas", "Tokuno", "Ter Mur" };

        /// <summary>
        /// Defines which Plugins to load on startup
        /// </summary>
        public static List<string> PluginsToLoad { get; set; } = new List<string>();

        /// <summary>
        /// Defines which muls are loaded
        /// <para>
        /// <list type="bullet">
        /// <item>Animations</item>
        /// <item>Animdata</item>
        /// <item>Art</item>
        /// <item>ASCIIFont</item>
        /// <item>Gumps</item>
        /// <item>Hues</item>
        /// <item>Light</item>
        /// <item>Map</item>
        /// <item>Multis</item>
        /// <item>Skills</item>
        /// <item>Sound</item>
        /// <item>Speech</item>
        /// <item>StringList</item>
        /// <item>Texture</item>
        /// <item>TileData</item>
        /// <item>UnicodeFont</item>
        /// <item>RadarColor</item>
        /// <item>AnimationEdit</item>
        /// </list>
        /// </para>
        /// </summary>
        public static Dictionary<string, bool> LoadedUltimaClass { get; set; } = new Dictionary<string, bool>
        {
            {"Animations",false},
            {"Animdata", false},
            {"Art", false},
            {"ASCIIFont", false},
            {"UnicodeFont", false},
            {"Gumps", false},
            {"Hues", false},
            {"Light", false},
            {"Map", false},
            {"Multis", false},
            {"Skills", false},
            {"Sound", false},
            {"Speech", false},
            {"StringList", false},
            {"Texture", false},
            {"TileData", false},
            {"RadarColor",false},
            {"AnimationEdit",false},
            {"SkillGrp",false}
        };

        /// <summary>
        /// Defines which muls have unsaved changes
        /// <para>
        /// <list type="bullet">
        /// <item>Animations</item>
        /// <item>Animdata</item>
        /// <item>Art</item>
        /// <item>ASCIIFont</item>
        /// <item>Gumps</item>
        /// <item>Hues</item>
        /// <item>Light</item>
        /// <item>Map</item>
        /// <item>Multis</item>
        /// <item>Skills</item>
        /// <item>Sound</item>
        /// <item>Speech</item>
        /// <item>StringList</item>
        /// <item>Texture</item>
        /// <item>TileData</item>
        /// <item>UnicodeFont</item>
        /// <item>RadarColor</item>
        /// </list>
        /// </para>
        /// </summary>
        public static Dictionary<string, bool> ChangedUltimaClass { get; set; } = new Dictionary<string, bool>
        {
            {"Animations",false},
            {"Animdata", false},
            {"Art", false},
            {"ASCIIFont", false},
            {"UnicodeFont", false},
            {"Gumps", false},
            {"Hues", false},
            {"Light", false},
            {"Map", false},
            {"Multis", false},
            {"Skills", false},
            {"Sound", false},
            {"Speech", false},
            {"CliLoc", false},
            {"Texture", false},
            {"TileData", false},
            {"RadarColor",false},
            {"SkillGrp",false}
        };

        /// <summary>
        /// Defines which tabs have been enabled and disabled
        /// </summary>
        public static Dictionary<int, bool> ChangedViewState { get; set; } = new Dictionary<int, bool>
        {
            {0, true},
            {1, true},
            {2, true},
            {3, true},
            {4, true},
            {5, true},
            {6, true},
            {7, true},
            {8, true},
            {9, true},
            {10, true},
            {11, true},
            {12, true},
            {13, true},
            {14, true},
            {15, true},
            {16, true},
            {17, true},
            {18, true},
            {19, true},
            {20, true}
        };

        public static Icon GetFiddlerIcon()
        {
            return new Icon(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("UoFiddler.Resources.UOFiddlerIcon.ico"));
        }

        public static bool TileDataDirectlySaveOnChange { get; set; }
        public static string AppDataPath { get; set; }
        public static string OutputPath { get; set; }
        public static string ProfileName { get; set; }
        public static Color TileFocusColor { get; set; } = Color.DarkRed;
        public static Color TileSelectionColor { get; set; } = Color.DodgerBlue;
        public static bool OverrideBackgroundColorFromTile { get; set; }
        public static bool RemoveTileBorder { get; set; }

        static Options()
        {
            AppDataPath = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "UoFiddler");
        }
    }
}
