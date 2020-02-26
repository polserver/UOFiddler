using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Ultima
{
    public sealed class UoSound
    {
        public string Name;
        public int Id;
        public byte[] Buffer;

        public UoSound(string name, int id, byte[] buff)
        {
            Name = name;
            Id = id;
            Buffer = buff;
        }
    };

    public static class Sounds
    {
        private static Dictionary<int, int> _mTranslations;
        private static FileIndex _mFileIndex;
        private static UoSound[] _mCache;
        private static bool[] _mRemoved;

        static Sounds()
        {
            Initialize();
        }

        /// <summary>
        /// Reads Sounds and def
        /// </summary>
        public static void Initialize()
        {
            _mCache = new UoSound[0xFFF];
            _mRemoved = new bool[0xFFF];
			_mFileIndex = new FileIndex("soundidx.mul", "sound.mul", "soundLegacyMUL.uop", 0xFFF, 8, ".dat", -1, false);
            Regex reg = new Regex(@"(\d{1,4}) \x7B(\d{1,4})\x7D (\d{1,3})", RegexOptions.Compiled);

            _mTranslations = new Dictionary<int, int>();

            string line;
            string path = Files.GetFilePath("Sound.def");
            if (path == null)
                return;
            using (StreamReader reader = new StreamReader(path))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (((line = line.Trim()).Length != 0) && !line.StartsWith("#"))
                    {
                        Match match = reg.Match(line);

                        if (match.Success)
                            _mTranslations.Add(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
                    }
                }
            }
        }

        /// <summary>
        /// Returns <see cref="UoSound"/> of ID
        /// </summary>
        /// <param name="soundId"></param>
        /// <returns></returns>
        public static UoSound GetSound(int soundId)
        {
            bool translated;
            return GetSound(soundId, out translated);
        }

        /// <summary>
        /// Returns <see cref="UoSound"/> of ID with bool translated in .def
        /// </summary>
        /// <param name="soundId"></param>
        /// <param name="translated"></param>
        /// <returns></returns>
        public static UoSound GetSound(int soundId, out bool translated)
        {
            translated = false;
            if (soundId < 0)
                return null;
            if (_mRemoved[soundId])
                return null;
            if (_mCache[soundId] != null)
                return _mCache[soundId];

            int length, extra;
            bool patched;
            Stream stream = _mFileIndex.Seek(soundId, out length, out extra, out patched);

            if ((_mFileIndex.Index[soundId].lookup < 0) || (length <= 0))
            {
                if (!_mTranslations.TryGetValue(soundId, out soundId))
                    return null;

                translated = true;
                stream = _mFileIndex.Seek(soundId, out length, out extra, out patched);
            }

            if (stream == null)
                return null;

            length -= 32;
            int[] waveHeader = WaveHeader(length);

            byte[] stringBuffer = new byte[32];
            byte[] buffer = new byte[length];

            stream.Read(stringBuffer, 0, 32);
            stream.Read(buffer, 0, length);
            stream.Close();

            byte[] resultBuffer = new byte[buffer.Length + (waveHeader.Length << 2)];

            Buffer.BlockCopy(waveHeader, 0, resultBuffer, 0, (waveHeader.Length << 2));
            Buffer.BlockCopy(buffer, 0, resultBuffer, (waveHeader.Length << 2), buffer.Length);

            string str = Encoding.ASCII.GetString(stringBuffer); // seems that the null terminator's not being properly recognized :/
            if (str.IndexOf('\0') > 0)
                str = str.Substring(0, str.IndexOf('\0'));
            UoSound sound = new UoSound(str, soundId, resultBuffer);

            if (Files.CacheData)
            {
                if (!translated) // no .def definition
                    _mCache[soundId] = sound;
            }

            return sound;
        }

        private static int[] WaveHeader(int length)
        {
            /* ====================
             * = WAVE File layout =
             * ====================
             * char[4] = 'RIFF' \
             * int - chunk size |- Riff Header
             * char[4] = 'WAVE' /
             * char[4] = 'fmt ' \
             * int - chunk size |
             * short - format	|
             * short - channels	|
             * int - samples p/s|- Format header
             * int - avg bytes	|
             * short - align	|
             * short - bits p/s /
             * char[4] - data	\
             * int - chunk size | - Data header
             * short[..] - data /
             * ====================
             * */
            return new int[] { 0x46464952, (length + 36), 0x45564157, 0x20746D66, 0x10, 0x010001, 0x5622, 0xAC44, 0x100002, 0x61746164, length };
        }

        /// <summary>
        /// Returns Soundname and tests if valid
        /// </summary>
        /// <param name="soundId"></param>
        /// <returns></returns>
        public static bool IsValidSound(int soundId, out string name, out bool translated)
        {
            translated = false;
            name = "";
            if (soundId < 0)
                return false;
            if (_mRemoved[soundId])
                return false;
            if (_mCache[soundId] != null)
            {
                name = _mCache[soundId].Name;
                return true;
            }

            int length, extra;
            bool patched;
            Stream stream = _mFileIndex.Seek(soundId, out length, out extra, out patched);

            if ((_mFileIndex.Index[soundId].lookup < 0) || (length <= 0))
            {
                if (!_mTranslations.TryGetValue(soundId, out soundId))
                    return false;
                else
                    translated = true;

                stream = _mFileIndex.Seek(soundId, out length, out extra, out patched);
            }
            if (stream == null)
                return false;

            byte[] stringBuffer = new byte[32];
            stream.Read(stringBuffer, 0, 32);
            stream.Close();
            name = Encoding.ASCII.GetString(stringBuffer); // seems that the null terminator's not being properly recognized :/
            if (name.IndexOf('\0') > 0)
                name = name.Substring(0, name.IndexOf('\0'));
            return true;
        }

        /// <summary>
        /// Returns length of SoundID
        /// </summary>
        /// <param name="soundId"></param>
        /// <returns></returns>
        public static double GetSoundLength(int soundId)
        {
            if (soundId < 0)
                return 0;
            if (_mRemoved[soundId])
                return 0;

            double len;
            if (_mCache[soundId] != null)
            {
                len = _mCache[soundId].Buffer.Length;
                len -= 44; //wavheaderlength
            }
            else
            {
                int length, extra;
                bool patched;
                Stream stream = _mFileIndex.Seek(soundId, out length, out extra, out patched);
                if ((_mFileIndex.Index[soundId].lookup < 0) || (length <= 0))
                {
                    if (!_mTranslations.TryGetValue(soundId, out soundId))
                        return 0;

                    stream = _mFileIndex.Seek(soundId, out length, out extra, out patched);
                }

                if (stream == null)
                    return 0;
                stream.Close();
                length -= 32; //mulheaderlength
                len = length;
            }
            len /= 0x5622; // Sample Rate
            len /= 2;
            return len;
        }

        public static void Add(int id, string name, string file)
        {
            using (FileStream wav = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] resultBuffer = new byte[wav.Length];
                wav.Seek(0, SeekOrigin.Begin);
                wav.Read(resultBuffer, 0, (int)wav.Length);

                resultBuffer = CheckAndFixWave(resultBuffer);

                _mCache[id] = new UoSound(name, id, resultBuffer);
                _mRemoved[id] = false;
            }
        }

        public static void Remove(int id)
        {
            _mRemoved[id] = true;
            _mCache[id] = null;
        }

        public static void Save(string path)
        {
            string idx = Path.Combine(path, "soundidx.mul");
            string mul = Path.Combine(path, "sound.mul");
            int headerlength = 44;
            using (FileStream fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write),
                              fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter binidx = new BinaryWriter(fsidx),
                                    binmul = new BinaryWriter(fsmul))
                {
                    for (int i = 0; i < _mCache.Length; ++i)
                    {
                        UoSound sound = _mCache[i];
                        if ((sound == null) && (!_mRemoved[i]))
                        {
                            bool trans;
                            sound = GetSound(i, out trans);
                            if (!trans)
                                _mCache[i] = sound;
                            else
                                sound = null;
                        }
                        if ((sound == null) || (_mRemoved[i]))
                        {
                            binidx.Write(-1); // lookup
                            binidx.Write(-1); // length
                            binidx.Write(-1); // extra
                        }
                        else
                        {
                            binidx.Write((int)fsmul.Position); //lookup
                            int length = (int)fsmul.Position;

                            byte[] b = new byte[32];
                            if (sound.Name != null)
                            {
                                byte[] bb = Encoding.Default.GetBytes(sound.Name);
                                if (bb.Length > 32)
                                    Array.Resize(ref bb, 32);
                                bb.CopyTo(b, 0);
                            }
                            binmul.Write(b);
                            using (MemoryStream m = new MemoryStream(sound.Buffer))
                            {
                                m.Seek(headerlength, SeekOrigin.Begin);
                                byte[] resultBuffer = new byte[m.Length - headerlength];
                                m.Read(resultBuffer, 0, (int)m.Length - headerlength);
                                binmul.Write(resultBuffer);
                            }

                            length = (int)fsmul.Position - length;
                            binidx.Write(length);
                            binidx.Write(i + 1);
                        }
                    }
                }
            }
        }

        public static void SaveSoundListToCsv(string fileName)
        {
            using (StreamWriter tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), Encoding.GetEncoding(1252)))
            {
                tex.WriteLine("ID;Name;Length");
                string name = "";
                bool translated = false;
                for (int i = 1; i <= 0xFFF; ++i)
                {
                    if (IsValidSound(i - 1, out name, out translated))
                    {
                        tex.Write($"0x{i:X3}");
                        tex.Write($";{name}");
                        tex.WriteLine($";{GetSoundLength(i - 1):f}");
                    }
                }
            }
        }

        public static bool IsRemovedSound(int soundId)
        {
            if (soundId < 0)
                return true;

            return _mRemoved[soundId];
        }

        private static byte[] CheckAndFixWave(byte[] inputBuffer)
        {
            if (inputBuffer.Length < 44)
            {
                throw new WaveFormatException("Invalid File.");
            }

            try
            {
                int croppedByteCount = 0;
                using (MemoryStream outputMemoryStream = new MemoryStream())
                {
                    using (BinaryWriter output = new BinaryWriter(outputMemoryStream))
                    {
                        using (MemoryStream inputMemoryStream = new MemoryStream(inputBuffer))
                        {
                            using (BinaryReader input = new BinaryReader(inputMemoryStream))
                            {
                                // RIFF HEADER
                                if (input.ReadString(4) != "RIFF")
                                {
                                    throw new WaveFormatException("RIFF Header not found.");
                                }
                                output.WriteString("RIFF");

                                uint riffLength = input.ReadUInt32();
                                output.Write(riffLength);
                                Debug.WriteLine("RIFF length: " + riffLength + " bytes");

                                if (input.ReadString(4) != "WAVE")
                                {
                                    throw new WaveFormatException("WAVE Format not found.");
                                }
                                output.WriteString("WAVE");

                                bool fmtWritten = false;
                                while (inputMemoryStream.Position < inputMemoryStream.Length)
                                {
                                    string currentHeader = input.ReadString(4);
                                    int currentLength = input.ReadInt32();

                                    /////////////////////////////////////////////////////////////////////////
                                    if (currentHeader == "fmt ")
                                    {
                                        if (currentLength != 16)
                                        {
                                            throw new WaveFormatException("FMT size not supported.");
                                        }

                                        output.WriteString(currentHeader);
                                        output.Write(currentLength);

                                        WaveFormat waveFormat = (WaveFormat)input.ReadInt16();
                                        if (waveFormat != WaveFormat.PCM)
                                        {
                                            throw new WaveFormatException("Format not supported: PCM expected but " + waveFormat + " given!");
                                        }
                                        output.Write((short)waveFormat);

                                        ushort channels = input.ReadUInt16();
                                        if (channels != 1)
                                        {
                                            throw new WaveFormatException("Only Mono supported, but " + channels + " channels given!");
                                        }
                                        output.Write(channels);

                                        uint frequency = input.ReadUInt32();
                                        if (frequency != 22050)
                                        {
                                            throw new WaveFormatException("Only 22050Hz supported, but " + frequency + "Hz given!");
                                        }
                                        output.Write(frequency);

                                        uint rate = input.ReadUInt32();
                                        output.Write(rate);
                                        Debug.WriteLine("Rate: " + rate + " byte/s");
                                        ushort align = input.ReadUInt16();
                                        output.Write(align);
                                        Debug.WriteLine("Align: " + align);

                                        ushort depth = input.ReadUInt16();
                                        if (depth != 16)
                                        {
                                            throw new WaveFormatException("Only 16 bits supported, but " + depth + " bits given!");
                                        }
                                        output.Write(depth);
                                        Debug.WriteLine("Depth: " + depth + " bits");

                                        fmtWritten = true;
                                    }
                                    /////////////////////////////////////////////////////////////////////////
                                    else if (currentHeader == "data")
                                    {
                                        if (!fmtWritten)
                                        {
                                            throw new WaveFormatException("Unexpected wave file order.");
                                        }

                                        output.WriteString(currentHeader);
                                        output.Write(currentLength);

                                        Debug.WriteLine("Data length: " + currentLength + " bytes");
                                        byte[] data = input.ReadBytes(currentLength);

                                        output.Write(data);
                                    }
                                    /////////////////////////////////////////////////////////////////////////
                                    else
                                    {
                                        croppedByteCount += currentLength + 8;
                                        input.ReadBytes(currentLength); // throw away
                                    }
                                }

                                // Adjust riff length
                                outputMemoryStream.Position = 4;
                                output.Write((uint)(riffLength - croppedByteCount));

                                return outputMemoryStream.ToArray();
                            }
                        }
                    }
                }
            }
            catch (EndOfStreamException ex)
            {
                throw new WaveFormatException("Unexpected Data end.", ex);
            }
        }
    }
}
