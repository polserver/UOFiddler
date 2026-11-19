/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.Collections.Generic;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>A node of the multieditor.xml tree: a group (union of its subgroups) or a subgroup.</summary>
    internal sealed class TileBrowserCategory
    {
        public TileBrowserCategory(string name, TileBrowserCategory parent)
        {
            Name = name;
            Parent = parent;
        }

        public string Name { get; }

        public TileBrowserCategory Parent { get; }

        public List<TileBrowserCategory> Children { get; } = new List<TileBrowserCategory>();

        public List<int> Tiles { get; } = new List<int>();

        public string Path => Parent == null ? Name : Parent.Path + " / " + Name;

        public override string ToString() => Name;
    }
}
