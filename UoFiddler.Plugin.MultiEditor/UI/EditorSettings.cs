/***************************************************************************
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
using System.Globalization;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// What the user can customise in the multi editor. Kept in the profile
    /// (<see cref="Options.PluginSettings"/>), so each profile has its own.
    /// </summary>
    internal sealed class EditorSettings
    {
        private const string _prefix = "MultiEditor.";

        /// <summary>Prefix the settings were saved under while the editor was a separate plugin.</summary>
        private const string _oldPrefix = "MultiEditorGL.";

        public static EditorSettings Current { get; private set; } = new EditorSettings();

        /// <summary>Raised after settings were changed and stored.</summary>
        public static event Action Changed;

        /// <summary>The background the editor shipped with before it followed the theme.</summary>
        private static readonly Color _oldDefaultBackground = Color.FromArgb(30, 30, 34);

        // Colours. Alpha is the opacity of planes and lines over the tiles.

        /// <summary>The canvas background the user chose, or empty to follow the application's light or dark theme.</summary>
        public Color Background { get; set; } = Color.Empty;

        /// <summary>The background to draw: the chosen one, else dark on a dark theme and light grey on a light one.</summary>
        public Color EffectiveBackground => Background.A != 0 ? Background : ThemeBackground;

        /// <summary>Text colour that reads on <see cref="EffectiveBackground"/>.</summary>
        public Color BackgroundText => EffectiveBackground.GetBrightness() < 0.5f ? Color.Gainsboro : Color.FromArgb(60, 60, 60);

        private static Color ThemeBackground =>
            SystemColors.Window.GetBrightness() < 0.5f ? _oldDefaultBackground : Color.FromArgb(206, 210, 216);

        public Color WorkPlane { get; set; } = Color.FromArgb(150, 40, 200, 40);

        public Color FloorPlane { get; set; } = Color.FromArgb(150, 40, 80, 220);

        public Color Grid { get; set; } = Color.FromArgb(150, 110, 110, 110);

        public Color Outline { get; set; } = Color.FromArgb(200, 150, 150, 150);

        public Color SelectionBox { get; set; } = Color.DodgerBlue;

        // How a new document starts.
        public bool StartWithWorkPlane { get; set; } = true;

        public bool StartWithFloorPlane { get; set; }

        public bool StartWithGrid { get; set; }

        public bool HideFloorsAbove { get; set; } = true;

        /// <summary>Ground under a previewed multi: 0 none, 1 grass, 2 dirt, 3 sand, 4 snow.</summary>
        public int PreviewTerrain { get; set; } = 1;

        // Layout.
        public bool ShowTileBrowser { get; set; } = true;

        public bool ShowInspector { get; set; } = true;

        public int TileBrowserWidth { get; set; } = 340;

        public int InspectorWidth { get; set; } = 300;

        /// <summary>Folder the Designs page lists UO Architect designs from.</summary>
        public string DesignLibraryFolder { get; set; } = string.Empty;

        /// <summary>Folder the Import from folder window scans.</summary>
        public string ImportFolder { get; set; } = string.Empty;

        /// <summary>Whether Import from folder also scans the folders below.</summary>
        public bool ImportRecursive { get; set; } = true;

        /// <summary>Height of the pieces list under the House page's styles.</summary>
        public int HousePiecesHeight { get; set; } = 250;

        public EditorSettings Clone() => (EditorSettings)MemberwiseClone();

        /// <summary>Reads the profile's settings; anything missing or unreadable keeps its default.</summary>
        public static void Load()
        {
            var settings = new EditorSettings();
            foreach (System.Reflection.PropertyInfo property in Properties())
            {
                if (!Options.PluginSettings.TryGetValue(_prefix + property.Name, out string text) &&
                    !Options.PluginSettings.TryGetValue(_oldPrefix + property.Name, out text))
                {
                    continue;
                }

                if (property.PropertyType == typeof(string))
                {
                    property.SetValue(settings, text);
                }
                else if (property.PropertyType == typeof(Color))
                {
                    if (int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int argb))
                    {
                        property.SetValue(settings, Color.FromArgb(argb));
                    }
                }
                else if (property.PropertyType == typeof(bool))
                {
                    if (bool.TryParse(text, out bool flag))
                    {
                        property.SetValue(settings, flag);
                    }
                }
                else if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int number))
                {
                    property.SetValue(settings, number);
                }
            }

            // A profile saved before the background followed the theme holds the old dark default,
            // which nobody chose.
            if (settings.Background.ToArgb() == _oldDefaultBackground.ToArgb())
            {
                settings.Background = Color.Empty;
            }

            settings.TileBrowserWidth = Math.Clamp(settings.TileBrowserWidth, 200, 900);
            settings.InspectorWidth = Math.Clamp(settings.InspectorWidth, 200, 600);
            Current = settings;
        }

        /// <summary>Makes these the current settings and puts them in the profile, which the application saves on exit.</summary>
        public void Apply()
        {
            Current = this;
            foreach (System.Reflection.PropertyInfo property in Properties())
            {
                object value = property.GetValue(this);
                string text = value switch
                {
                    Color color => color.ToArgb().ToString("X8", CultureInfo.InvariantCulture),
                    bool flag => flag.ToString(),
                    _ => Convert.ToString(value, CultureInfo.InvariantCulture)
                };
                Options.PluginSettings[_prefix + property.Name] = text;
            }

            Changed?.Invoke();
        }

        private static IEnumerable<System.Reflection.PropertyInfo> Properties()
        {
            foreach (System.Reflection.PropertyInfo property in typeof(EditorSettings).GetProperties(
                         System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                if (property.CanWrite)
                {
                    yield return property;
                }
            }
        }
    }
}
