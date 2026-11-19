/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using Ultima;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// One multi read from an import file. UO Architect designs read their items only when
    /// <see cref="Components"/> is first used, so a large batch lists quickly.
    /// </summary>
    internal sealed class ImportedMulti
    {
        private readonly Func<MultiComponentList> _load;
        private MultiComponentList _components;

        public ImportedMulti(string name, Multis.ImportType format, string file, int tileCount, MultiComponentList components)
        {
            Name = name;
            Format = format;
            File = file;
            TileCount = tileCount;
            _components = components;
        }

        public ImportedMulti(string name, Multis.ImportType format, string file, int tileCount, Func<MultiComponentList> load)
        {
            Name = name;
            Format = format;
            File = file;
            TileCount = tileCount;
            _load = load;
        }

        /// <summary>The tab title: the file name, or file and entry for one multi out of several.</summary>
        public string Name { get; }

        public Multis.ImportType Format { get; }

        /// <summary>Full path of the file it came from.</summary>
        public string File { get; }

        public int TileCount { get; }

        /// <summary>The multi's tiles. Throws when the file cannot be read.</summary>
        public MultiComponentList Components => _components ??= _load();
    }
}
