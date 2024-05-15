using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ultima;
using static Ultima.Animdata;

namespace UoFiddler.Controls.Classes
{
    using AnimDataDict = Dictionary<int, AnimdataEntry>;

    public enum ExportSelection
    {
        All = 0,
        IncludeMissingTileFlag = 1,
        OnlyValid = 2
    }

    public class ExportedAnimData
    {
        public static readonly int CurrentVersion = 1;
        public int Version { get; set; }
        public AnimDataDict Data { get; set; }

        public static ExportedAnimData FromFile(string fileName)
        {
            string jsonString = File.ReadAllText(fileName);

            var imported = JsonSerializer.Deserialize<ExportedAnimData>(jsonString) ??
                throw new Exception("Imported null value.");

            if (imported.Version != CurrentVersion)
            {
                throw new InvalidOperationException($"Unexpected version {imported.Version}, expected {CurrentVersion}.");
            }

            return imported;
        }

        public static ExportedAnimData ToFile(string fileName, AnimDataDict entries, ExportSelection selection)
        {
            bool ExportSelector(int id)
            {
                if (Art.IsValidStatic(id))
                {
                    if (TileData.ItemTable[id].Flags.HasFlag(TileFlag.Animation) || selection == ExportSelection.IncludeMissingTileFlag)
                    {
                        return true;
                    }
                }

                return selection == ExportSelection.All;
            }

            var data = new ExportedAnimData
            {
                Version = 1,
                Data = (from entry in entries where ExportSelector(entry.Key) select entry).ToDictionary()
            };

            string jsonString = JsonSerializer.Serialize(data);

            File.WriteAllText(fileName, jsonString);

            return data;
        }

        public int UpdateAnimdata(AnimDataDict dst, bool overwrite)
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
