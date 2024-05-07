// /***************************************************************************
//  *
//  * $Author: Turley
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ultima;
using static Ultima.Animdata;

namespace UoFiddler.Controls.Classes
{
    using AnimdataTable = Dictionary<int, AnimdataEntry>;

    public class ExportedAnimData
    {
        public static readonly int CurrentVersion = 1;
        public int Version { get; set; }
        public AnimdataTable Data { get; set; }

        public static ExportedAnimData FromFile(string fileName)
        {
            string jsonString = File.ReadAllText(fileName);

            var imported = JsonSerializer.Deserialize<ExportedAnimData>(jsonString) ??
                throw new Exception("Imported null value.");

            if (imported.Version != CurrentVersion)
            {
                throw new InvalidOperationException($"Unexpected version {imported.Version}, expected {CurrentVersion}");
            }

            return imported;
        }

        public static ExportedAnimData ToFile(string fileName, AnimdataTable entries, bool includeInvalidTiles, bool includeMissingAnimation)
        {
            bool Selector(int id)
            {
                return (Art.IsValidStatic(id) ||
                        includeInvalidTiles)  // Should export invalid tiles
                       && (((TileData.ItemTable[id].Flags & TileFlag.Animation) == 0) ||
                           includeMissingAnimation);  // Should export missing animations
            }

            var data = new ExportedAnimData
            {
                Version = 1,
                Data = (from entry in AnimData where Selector(entry.Key) select entry).ToDictionary()
            };

            string jsonString = JsonSerializer.Serialize(data);

            File.WriteAllText(fileName, jsonString);

            return data;
        }

        public int UpdateAnimdata(AnimdataTable dst, bool overwrite)
        {
            int count = 0;
            foreach (var (id, entry) in Data)
            {
                if (overwrite)
                {
                    dst[id] = entry;
                    count++;
                }
                else if (dst.TryAdd(id, entry))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
