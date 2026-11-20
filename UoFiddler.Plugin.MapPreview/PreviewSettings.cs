/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Globalization;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.MapPreview.Rendering;

namespace UoFiddler.Plugin.MapPreview
{
    /// <summary>
    /// How a preview is made: the area around the clicked tile and what is drawn. Kept in the
    /// profile (<see cref="Options.PluginSettings"/>), so each profile has its own.
    /// </summary>
    public sealed class PreviewSettings
    {
        /// <summary>
        /// The square areas offered, in tiles a side. The largest is about 11,000 pixels across at
        /// 100% and around a hundred thousand sprites in a town: under a second to open.
        /// </summary>
        public static readonly int[] AreaSizes = { 32, 48, 64, 96, 128, 192, 256 };

        /// <summary>How far one move of the area goes, in tiles: a nudge up to a jump.</summary>
        public static readonly int[] MoveSteps = { 1, 8, 16, 32 };

        private const string Prefix = "MapPreview.";

        private static PreviewSettings _current;

        /// <summary>Raised after settings were changed and stored.</summary>
        public static event Action Changed;

        /// <summary>The profile's settings, read on first use.</summary>
        public static PreviewSettings Current => _current ??= Load();

        public int AreaWidth { get; set; } = 64;

        public int AreaHeight { get; set; } = 64;

        public bool ShowLand { get; set; } = true;

        public bool ShowStatics { get; set; } = true;

        public bool Stretch { get; set; } = true;

        public bool Lighting { get; set; } = true;

        public bool Hues { get; set; } = true;

        public bool HideRoofs { get; set; }

        /// <summary>Cut everything above the ground at the clicked tile on every preview, as the client does indoors.</summary>
        public bool AutoRoofCut { get; set; }

        /// <summary>Max Z hides land too, to show statics under the terrain. Set on the preview's toolbar.</summary>
        public bool CutLand { get; set; }

        /// <summary>Tiles the area moves per arrow, one of <see cref="MoveSteps"/>.</summary>
        public int MoveStep { get; set; } = 8;

        public PreviewSettings Clone() => (PreviewSettings)MemberwiseClone();

        public PreviewOptions ToOptions(int? maxZ) => new PreviewOptions
        {
            ShowLand = ShowLand,
            ShowStatics = ShowStatics,
            Stretch = Stretch,
            Lighting = Lighting,
            Hues = Hues,
            HideRoofs = HideRoofs,
            MaxZ = maxZ,
            CutLand = CutLand
        };

        /// <summary>Makes these the current settings and puts them in the profile, which the application saves on exit.</summary>
        public void Apply()
        {
            _current = this;

            Write(nameof(AreaWidth), AreaWidth.ToString(CultureInfo.InvariantCulture));
            Write(nameof(AreaHeight), AreaHeight.ToString(CultureInfo.InvariantCulture));
            Write(nameof(ShowLand), ShowLand.ToString());
            Write(nameof(ShowStatics), ShowStatics.ToString());
            Write(nameof(Stretch), Stretch.ToString());
            Write(nameof(Lighting), Lighting.ToString());
            Write(nameof(Hues), Hues.ToString());
            Write(nameof(HideRoofs), HideRoofs.ToString());
            Write(nameof(AutoRoofCut), AutoRoofCut.ToString());
            Write(nameof(CutLand), CutLand.ToString());
            Write(nameof(MoveStep), MoveStep.ToString(CultureInfo.InvariantCulture));

            Changed?.Invoke();
        }

        /// <summary>Reads the profile's settings; anything missing or unreadable keeps its default.</summary>
        private static PreviewSettings Load()
        {
            var settings = new PreviewSettings();
            settings.AreaWidth = NearestAreaSize(ReadInt(nameof(AreaWidth), settings.AreaWidth));
            settings.AreaHeight = NearestAreaSize(ReadInt(nameof(AreaHeight), settings.AreaHeight));
            settings.ShowLand = ReadBool(nameof(ShowLand), settings.ShowLand);
            settings.ShowStatics = ReadBool(nameof(ShowStatics), settings.ShowStatics);
            settings.Stretch = ReadBool(nameof(Stretch), settings.Stretch);
            settings.Lighting = ReadBool(nameof(Lighting), settings.Lighting);
            settings.Hues = ReadBool(nameof(Hues), settings.Hues);
            settings.HideRoofs = ReadBool(nameof(HideRoofs), settings.HideRoofs);
            settings.AutoRoofCut = ReadBool(nameof(AutoRoofCut), settings.AutoRoofCut);
            settings.CutLand = ReadBool(nameof(CutLand), settings.CutLand);
            int step = ReadInt(nameof(MoveStep), settings.MoveStep);
            settings.MoveStep = Array.IndexOf(MoveSteps, step) >= 0 ? step : 8;
            return settings;
        }

        /// <summary>The offered size closest to <paramref name="size"/>, for sizes saved before the list existed.</summary>
        public static int NearestAreaSize(int size)
        {
            int best = AreaSizes[0];
            foreach (int candidate in AreaSizes)
            {
                if (Math.Abs(candidate - size) < Math.Abs(best - size))
                {
                    best = candidate;
                }
            }

            return best;
        }

        private static void Write(string name, string value)
        {
            Options.PluginSettings[Prefix + name] = value;
        }

        private static int ReadInt(string name, int fallback)
        {
            return Options.PluginSettings.TryGetValue(Prefix + name, out string text) &&
                   int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
                ? value
                : fallback;
        }

        private static bool ReadBool(string name, bool fallback)
        {
            return Options.PluginSettings.TryGetValue(Prefix + name, out string text) && bool.TryParse(text, out bool value)
                ? value
                : fallback;
        }
    }
}
