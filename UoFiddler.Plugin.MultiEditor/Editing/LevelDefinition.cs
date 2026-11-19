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
    /// <summary>How the user splits a design into levels: a name and the Z the level starts at.</summary>
    internal readonly record struct LevelDefinition(string Name, int StartZ);
}
