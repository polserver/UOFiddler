using System;
using System.Collections.Generic;
using System.IO;

namespace Ultima
{
    /// <summary>
    /// Contains translation tables used for mapping body values to file subsets.
    /// <seealso cref="Animations" />
    /// </summary>
    public static class BodyConverter
    {
        public static int[] Table1 { get; private set; }
        public static int[] Table2 { get; private set; }
        public static int[] Table3 { get; private set; }
        public static int[] Table4 { get; private set; }

        static BodyConverter()
        {
            Initialize();
        }

        /// <summary>
        /// Fills bodyconv.def Tables
        /// </summary>
        public static void Initialize()
        {
            string path = Files.GetFilePath("bodyconv.def");
            if (path == null)
            {
                return;
            }

            List<int> list1 = new List<int>();
            List<int> list2 = new List<int>();
            List<int> list3 = new List<int>();
            List<int> list4 = new List<int>();

            int max1 = 0;
            int max2 = 0;
            int max3 = 0;
            int max4 = 0;

            using (var ip = new StreamReader(path))
            {
                while (ip.ReadLine() is { } line)
                {
                    line = line.Trim();

                    if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("\""))
                    {
                        continue;
                    }

                    try
                    {
                        string[] split = line.Split(new [] {'\t', ' '}, StringSplitOptions.RemoveEmptyEntries);

                        bool hasOriginalBodyId = int.TryParse(split[0], out int original);
                        if (!hasOriginalBodyId)
                        {
                            // First item in array is not an int which means 
                            // it's probably wrong line to parse. So we skip it.
                            continue;
                        }

                        if (!int.TryParse(split[1], out int anim2))
                        {
                            anim2 = -1;
                        }

                        if (!int.TryParse(split[2], out int anim3))
                        {
                            anim3 = -1;
                        }

                        if (!int.TryParse(split[3], out int anim4))
                        {
                            anim4 = -1;
                        }

                        if (!int.TryParse(split[4], out int anim5))
                        {
                            anim5 = -1;
                        }

                        if (anim2 != -1)
                        {
                            if (anim2 == 68)
                            {
                                anim2 = 122;
                            }

                            if (original > max1)
                            {
                                max1 = original;
                            }

                            list1.Add(original);
                            list1.Add(anim2);
                        }

                        if (anim3 != -1)
                        {
                            if (original > max2)
                            {
                                max2 = original;
                            }

                            list2.Add(original);
                            list2.Add(anim3);
                        }

                        if (anim4 != -1)
                        {
                            if (original > max3)
                            {
                                max3 = original;
                            }

                            list3.Add(original);
                            list3.Add(anim4);
                        }

                        if (anim5 != -1)
                        {
                            if (original > max4)
                            {
                                max4 = original;
                            }

                            list4.Add(original);
                            list4.Add(anim5);
                        }
                    }
                    catch
                    {
                        // TODO: ignored?
                    }
                }
            }

            Table1 = new int[max1 + 1];

            for (int i = 0; i < Table1.Length; ++i)
            {
                Table1[i] = -1;
            }

            for (int i = 0; i < list1.Count; i += 2)
            {
                Table1[list1[i]] = list1[i + 1];
            }

            Table2 = new int[max2 + 1];

            for (int i = 0; i < Table2.Length; ++i)
            {
                Table2[i] = -1;
            }

            for (int i = 0; i < list2.Count; i += 2)
            {
                Table2[list2[i]] = list2[i + 1];
            }

            Table3 = new int[max3 + 1];

            for (int i = 0; i < Table3.Length; ++i)
            {
                Table3[i] = -1;
            }

            for (int i = 0; i < list3.Count; i += 2)
            {
                Table3[list3[i]] = list3[i + 1];
            }

            Table4 = new int[max4 + 1];

            for (int i = 0; i < Table4.Length; ++i)
            {
                Table4[i] = -1;
            }

            for (int i = 0; i < list4.Count; i += 2)
            {
                Table4[list4[i]] = list4[i + 1];
            }
        }

        /// <summary>
        /// Checks to see if <paramref name="body" /> is contained within the mapping table.
        /// </summary>
        /// <returns>True if it is, false if not.</returns>
        public static bool Contains(int body)
        {
            if (Table1 != null && body >= 0 && body < Table1.Length && Table1[body] != -1)
            {
                return true;
            }

            if (Table2 != null && body >= 0 && body < Table2.Length && Table2[body] != -1)
            {
                return true;
            }

            if (Table3 != null && body >= 0 && body < Table3.Length && Table3[body] != -1)
            {
                return true;
            }

            if (Table4 != null && body >= 0 && body < Table4.Length && Table4[body] != -1)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Attempts to convert <paramref name="body" /> to a body index relative to a file subset, specified by the return value.
        /// </summary>
        /// <returns>
        ///     A value indicating a file subset:
        ///     <list type="table">
        ///         <listheader>
        ///             <term>Return Value</term>
        ///             <description>File Subset</description>
        ///         </listheader>
        ///         <item>
        ///             <term>1</term>
        ///             <description>Anim.mul, Anim.idx (Standard)</description>
        ///         </item>
        ///         <item>
        ///             <term>2</term>
        ///             <description>Anim2.mul, Anim2.idx (LBR)</description>
        ///         </item>
        ///         <item>
        ///             <term>3</term>
        ///             <description>Anim3.mul, Anim3.idx (AOS)</description>
        ///         </item>
        ///         <item>
        ///             <term>4</term>
        ///             <description>Anim4.mul, Anim4.idx (SE)</description>
        ///         </item>
        ///         <item>
        ///             <term>5</term>
        ///             <description>Anim5.mul, Anim5.idx (ML)</description>
        ///         </item>
        ///     </list>
        /// </returns>
        public static int Convert(ref int body)
        {
            if (Table1 != null && body >= 0 && body < Table1.Length)
            {
                int val = Table1[body];

                if (val != -1)
                {
                    body = val;
                    return 2;
                }
            }

            if (Table2 != null && body >= 0 && body < Table2.Length)
            {
                int val = Table2[body];

                if (val != -1)
                {
                    body = val;
                    return 3;
                }
            }

            if (Table3 != null && body >= 0 && body < Table3.Length)
            {
                int val = Table3[body];

                if (val != -1)
                {
                    body = val;
                    return 4;
                }
            }

            if (Table4 != null && body >= 0 && body < Table4.Length)
            {
                int val = Table4[body];
                if (val == -1)
                {
                    return 1;
                }

                body = val;
                return 5;
            }

            return 1;
        }

        /// <summary>
        /// Converts backward
        /// </summary>
        /// <param name="fileType"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static int GetTrueBody(int fileType, int index)
        {
            switch (fileType)
            {
                case 1:
                default:
                {
                    return index;
                }
                case 2:
                {
                    if (Table1 != null && index >= 0)
                    {
                        for (int i = 0; i < Table1.Length; ++i)
                        {
                            if (Table1[i] == index)
                            {
                                return i;
                            }
                        }
                    }
                    break;
                }
                case 3:
                {
                    if (Table2 != null && index >= 0)
                    {
                        for (int i = 0; i < Table2.Length; ++i)
                        {
                            if (Table2[i] == index)
                            {
                                return i;
                            }
                        }
                    }
                    break;
                }
                case 4:
                {
                    if (Table3 != null && index >= 0)
                    {
                        for (int i = 0; i < Table3.Length; ++i)
                        {
                            if (Table3[i] == index)
                            {
                                return i;
                            }
                        }
                    }
                    break;
                }
                case 5:
                {
                    if (Table4 != null && index >= 0)
                    {
                        for (int i = 0; i < Table4.Length; ++i)
                        {
                            if (Table4[i] == index)
                            {
                                return i;
                            }
                        }
                    }
                    break;
                }
            }
            return -1;
        }
    }
}