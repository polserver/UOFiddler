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

namespace FiddlerControls
{
    public sealed class Options
    {
        private static int m_ArtItemSizeWidth = 48;
        private static int m_ArtItemSizeHeight = 48;
        private static bool m_ArtItemClip = true;
        private static bool m_DesignAlternative = false;
        private static string m_MapCmd = ".goforce";
        // {1} x {2} y {3} z {4} mapid {5} mapname
        private static string m_MapArgs = "{1} {2} {3} {4}";
        private static string[] m_MapNames = { "Felucca", "Trammel", "Ilshenar", "Malas", "Tokuno", "Ter Mur" };
        private static List<string> m_PluginsToLoad;
        private static Dictionary<string, bool> m_LoadedUltimaClass = new Dictionary<string, bool>()
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

        private static Dictionary<string, bool> m_ChangedUltimaClass = new Dictionary<string, bool>()
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

        private static Dictionary<int, bool> m_ChangedViewStates = new Dictionary<int, bool>()
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


        /// <summary>
        /// Definies Element Width in ItemShow
        /// </summary>
        public static int ArtItemSizeWidth
        {
            get { return m_ArtItemSizeWidth; }
            set { m_ArtItemSizeWidth = value; }
        }

        /// <summary>
        /// Definies Element Height in ItemShow
        /// </summary>
        public static int ArtItemSizeHeight
        {
            get { return m_ArtItemSizeHeight; }
            set { m_ArtItemSizeHeight = value; }
        }

        /// <summary>
        /// Definies if Element should be clipped or shrinked in ItemShow
        /// </summary>
        public static bool ArtItemClip
        {
            get { return m_ArtItemClip; }
            set { m_ArtItemClip = value; }
        }

        /// <summary>
        /// Definies if alternative Controls should be used 
        /// </summary>
        public static bool DesignAlternative
        {
            get { return m_DesignAlternative; }
            set { m_DesignAlternative = value; }
        }

        /// <summary>
        /// Definies the cmd to Send Client to Loc
        /// </summary>
        public static string MapCmd
        {
            get { return m_MapCmd; }
            set { m_MapCmd = value; }
        }

        /// <summary>
        /// Definies the args for Send Client
        /// </summary>
        public static string MapArgs
        {
            get { return m_MapArgs; }
            set { m_MapArgs = value; }
        }

        /// <summary>
        /// Definies the MapNames
        /// </summary>
        public static string[] MapNames
        {
            get { return m_MapNames; }
            set { m_MapNames = value; }
        }

        /// <summary>
        /// Definies which Plugins to load on startup
        /// </summary>
        public static List<string> PluginsToLoad
        {
            get { return m_PluginsToLoad; }
            set { m_PluginsToLoad = value; }
        }

        /// <summary>
        /// Definies which muls are loaded
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
        public static Dictionary<string, bool> LoadedUltimaClass
        {
            get { return m_LoadedUltimaClass; }
            set { m_LoadedUltimaClass = value; }
        }

        /// <summary>
        /// Definies which muls have unsaved changes
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
        public static Dictionary<string, bool> ChangedUltimaClass
        {
            get { return Options.m_ChangedUltimaClass; }
            set { Options.m_ChangedUltimaClass = value; }
        }

        /// <summary>
        /// Definies which tabs have been enabled and disabled
        /// </summary>
        public static Dictionary<int, bool> ChangedViewState
        {
            get { return Options.m_ChangedViewStates; }
            set { Options.m_ChangedViewStates = value; }
        }

        public static Icon GetFiddlerIcon()
        {
            return new Icon(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("UoFiddler.UOFiddler.ico"));
        }


        public static string AppDataPath { get; set; }
        public static string OutputPath { get; set; }

        public static string ProfileName { get; set; }

        static Options()
        {
            AppDataPath = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "UoFiddler");
        }
    }
}
