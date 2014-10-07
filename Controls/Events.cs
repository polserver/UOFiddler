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

namespace FiddlerControls
{
    public sealed class Events
    {
        public delegate void MapDiffChangeHandler();
        public delegate void MapNameChangeHandler();
        public delegate void MapSizeChangeHandler();
        public delegate void FilePathChangeHandler();
        public delegate void MultiChangeHandler(object sender, int id);
        public delegate void HueChangeHandler();
        public delegate void ItemChangeHandler(object sender, int id);
        public delegate void LandTileChangeHandler(object sender, int id);
        public delegate void TextureChangeHandler(object sender, int id);
        public delegate void GumpChangeHandler(object sender, int id);
        public delegate void TileDataChangeHandler(object sender, int id);
        public delegate void AlwaysOnTopChangeHandler(bool value);
        public delegate void ProgressChangeHandler();

        /// <summary>
        /// Fired when map diff file usage is switched
        /// </summary>
        public static event MapDiffChangeHandler MapDiffChangeEvent;
        /// <summary>
        /// Fired when map names where changed
        /// </summary>
        public static event MapNameChangeHandler MapNameChangeEvent;
        /// <summary>
        /// Fired when map size has changed
        /// </summary>
        public static event MapSizeChangeHandler MapSizeChangeEvent;
        /// <summary>
        /// Fired on reload files
        /// </summary>
        public static event FilePathChangeHandler FilePathChangeEvent;
        /// <summary>
        /// Fired when Multi Id changed
        /// </summary>
        public static event MultiChangeHandler MultiChangeEvent;
        /// <summary>
        /// Fired when Hues changed
        /// </summary>
        public static event HueChangeHandler HueChangeEvent;
        /// <summary>
        /// Fired when ArtItems changed
        /// </summary>
        public static event ItemChangeHandler ItemChangeEvent;
        /// <summary>
        /// Fired when LandTile changed
        /// </summary>
        public static event LandTileChangeHandler LandTileChangeEvent;
        /// <summary>
        /// Fired when Texture changed
        /// </summary>
        public static event TextureChangeHandler TextureChangeEvent;
        /// <summary>
        /// Fired when Gump changed
        /// </summary>
        public static event GumpChangeHandler GumpChangeEvent;
        /// <summary>
        /// Fired when Tiledata changed
        /// </summary>
        public static event TileDataChangeHandler TileDataChangeEvent;
        /// <summary>
        /// Fired when AlwaysOnTop changed
        /// </summary>
        public static event AlwaysOnTopChangeHandler AlwaysOnTopChangeEvent;
        /// <summary>
        /// Fired when Progressbar should be changed
        /// </summary>
        public static event ProgressChangeHandler ProgressChangeEvent;


        public static void FireMapDiffChangeEvent()
        {
            if (MapDiffChangeEvent != null)
                MapDiffChangeEvent();
        }
        public static void FireMapNameChangeEvent()
        {
            if (MapNameChangeEvent != null)
                MapNameChangeEvent();
        }
        public static void FireMapSizeChangeEvent()
        {
            if (MapSizeChangeEvent != null)
                MapSizeChangeEvent();
        }
        public static void FireFilePathChangeEvent()
        {
            if (FilePathChangeEvent != null)
                FilePathChangeEvent();
        }
        public static void FireMultiChangeEvent(object sender, int id)
        {
            if (MultiChangeEvent != null)
                MultiChangeEvent(sender, id);
        }
        public static void FireHueChangeEvent()
        {
            if (HueChangeEvent != null)
                HueChangeEvent();
        }
        public static void FireItemChangeEvent(object sender, int id)
        {
            if (ItemChangeEvent != null)
                ItemChangeEvent(sender, id);
        }
        public static void FireLandTileChangeEvent(object sender, int id)
        {
            if (LandTileChangeEvent != null)
                LandTileChangeEvent(sender, id);
        }
        public static void FireTextureChangeEvent(object sender, int id)
        {
            if (TextureChangeEvent != null)
                TextureChangeEvent(sender, id);
        }
        public static void FireGumpChangeEvent(object sender, int id)
        {
            if (GumpChangeEvent != null)
                GumpChangeEvent(sender, id);
        }
        public static void FireTileDataChangeEvent(object sender, int id)
        {
            if (TileDataChangeEvent != null)
                TileDataChangeEvent(sender, id);
        }
        public static void FireAlwaysOnTopChangeEvent(bool value)
        {
            if (AlwaysOnTopChangeEvent != null)
                AlwaysOnTopChangeEvent(value);
        }
        public static void FireProgressChangeEvent()
        {
            if (ProgressChangeEvent != null)
                ProgressChangeEvent();
        }
    }
}
