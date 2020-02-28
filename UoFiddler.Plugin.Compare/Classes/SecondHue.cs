using System.IO;
using Ultima;

namespace UoFiddler.Plugin.Compare.Classes
{
    internal static class SecondHue
    {
        public static Hue[] List { get; private set; }

        /// <summary>
        /// Reads hues.mul and fills <see cref="List"/>
        /// </summary>
        public static void Initialize(string path)
        {
            int index = 0;

            List = new Hue[3000];

            if (path != null)
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryReader bin = new BinaryReader(fs);

                    int blockCount = (int)fs.Length / 708;

                    if (blockCount > 375)
                    {
                        blockCount = 375;
                    }

                    for (int i = 0; i < blockCount; ++i)
                    {
                        bin.ReadInt32();

                        for (int j = 0; j < 8; ++j, ++index)
                        {
                            List[index] = new Hue(index, bin);
                        }
                    }
                }
            }

            for (; index < List.Length; ++index)
            {
                List[index] = new Hue(index);
            }
        }

        // TODO: unused method?
        // public static Hue GetHue(int index)
        // {
        //     index &= 0x3FFF;
        //
        //     if (index >= 0 && index < 3000)
        //     {
        //         return List[index];
        //     }
        //
        //     return List[0];
        // }
    }
}
