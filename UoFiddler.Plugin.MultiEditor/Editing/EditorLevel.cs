/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace UoFiddler.Plugin.MultiEditor.Editing
{
    /// <summary>
    /// A floor of the multi: tiles from Floor up to (not including) Ceiling. BaseZ is the Z new
    /// tiles get on it. The lowest level also takes what lies below its base, the highest what
    /// lies above.
    /// </summary>
    internal readonly record struct EditorLevel(string Name, string ShortName, int Floor, int Ceiling, int BaseZ)
    {
        public string Description => Ceiling > sbyte.MaxValue ? $"{Name} (Z {BaseZ} and up)" : $"{Name} (Z {BaseZ} to {Ceiling - 1})";
    }
}
