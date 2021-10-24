using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Ultima
{
    public sealed class UoSound
    {
        public string Name;
        public int Id;
        public readonly byte[] Buffer;

        public UoSound(string name, int id, byte[] buff)
        {
            Name = name;
            Id = id;
            Buffer = buff;
        }
    };

    public static class Sounds
    {
        private static Dictionary<int, int> _translations;
        private static FileIndex _fileIndex;
        private static UoSound[] _cache;
        private static bool[] _removed;

        static Sounds()
        {
            Initialize();
        }

        /// <summary>
        /// Reads Sounds and def
        /// </summary>
        public static void Initialize()
        {
            _cache = new UoSound[0xFFF];
            _removed = new bool[0xFFF];
            _fileIndex = new FileIndex("soundidx.mul", "sound.mul", "soundLegacyMUL.uop", 0xFFF, 8, ".dat", -1, false);
            var reg = new Regex(@"(\d{1,4}) \x7B(\d{1,4})\x7D (\d{1,3})", RegexOptions.Compiled);

            _translations = new Dictionary<int, int>();

            string path = Files.GetFilePath("Sound.def");
            if (path == null)
            {
                return;
            }

            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if ((line = line.Trim()).Length == 0 || line.StartsWith("#"))
                    {
                        continue;
                    }

                    Match match = reg.Match(line);

                    if (match.Success)
                    {
                        _translations.Add(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
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
            return GetSound(soundId, out bool _);
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
            {
                return null;
            }

            if (_removed[soundId])
            {
                return null;
            }

            if (_cache[soundId] != null)
            {
                return _cache[soundId];
            }

            Stream stream = _fileIndex.Seek(soundId, out int length, out int _, out bool _);

            if (_fileIndex.Index[soundId].Lookup < 0 || length <= 0)
            {
                if (!_translations.TryGetValue(soundId, out soundId))
                {
                    return null;
                }

                translated = true;
                stream = _fileIndex.Seek(soundId, out length, out int _, out bool _);
            }

            if (stream == null)
            {
                return null;
            }

            length -= 32;
            int[] waveHeader = WaveHeader(length);

            var stringBuffer = new byte[32];
            var buffer = new byte[length];

            stream.Read(stringBuffer, 0, 32);
            stream.Read(buffer, 0, length);
            stream.Close();

            var resultBuffer = new byte[buffer.Length + (waveHeader.Length << 2)];

            Buffer.BlockCopy(waveHeader, 0, resultBuffer, 0, waveHeader.Length << 2);
            Buffer.BlockCopy(buffer, 0, resultBuffer, waveHeader.Length << 2, buffer.Length);

            string str = Encoding.ASCII.GetString(stringBuffer);
            // seems that the null terminator's not being properly recognized :/
            if (str.IndexOf('\0') > 0)
            {
                str = str.Substring(0, str.IndexOf('\0'));
            }

            var sound = new UoSound(str, soundId, resultBuffer);

            if (!Files.CacheData)
            {
                return sound;
            }

            if (!translated) // no .def definition
            {
                _cache[soundId] = sound;
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
            return new[] { 0x46464952, length + 36, 0x45564157, 0x20746D66, 0x10, 0x010001, 0x5622, 0xAC44, 0x100002, 0x61746164, length };
        }

        /// <summary>
        /// Returns Sound name and tests if valid
        /// </summary>
        /// <param name="soundId"></param>
        /// <param name="name"></param>
        /// <param name="translated"></param>
        /// <returns></returns>
        public static bool IsValidSound(int soundId, out string name, out bool translated)
        {
            translated = false;
            name = "";

            if (soundId < 0)
            {
                return false;
            }

            if (_removed[soundId])
            {
                return false;
            }

            if (_cache[soundId] != null)
            {
                name = _cache[soundId].Name;
                return true;
            }

            Stream stream = _fileIndex.Seek(soundId, out int length, out _, out _);

            if (_fileIndex.Index[soundId].Lookup < 0 || length <= 0)
            {
                if (!_translations.TryGetValue(soundId, out soundId))
                {
                    return false;
                }

                translated = true;

                stream = _fileIndex.Seek(soundId, out _, out _, out _);
            }

            if (stream == null)
            {
                return false;
            }

            var stringBuffer = new byte[32];
            stream.Read(stringBuffer, 0, 32);
            stream.Close();
            name = Encoding.ASCII.GetString(stringBuffer); // seems that the null terminator's not being properly recognized :/
            if (name.IndexOf('\0') > 0)
            {
                name = name.Substring(0, name.IndexOf('\0'));
            }

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
            {
                return 0;
            }

            if (_removed[soundId])
            {
                return 0;
            }

            double len;

            if (_cache[soundId] != null)
            {
                len = _cache[soundId].Buffer.Length;
                len -= 44; // wavheaderlength
            }
            else
            {
                Stream stream = _fileIndex.Seek(soundId, out int length, out int _, out bool _);
                if (_fileIndex.Index[soundId].Lookup < 0 || length <= 0)
                {
                    if (!_translations.TryGetValue(soundId, out soundId))
                    {
                        return 0;
                    }

                    stream = _fileIndex.Seek(soundId, out length, out int _, out bool _);
                }

                if (stream == null)
                {
                    return 0;
                }

                stream.Close();
                length -= 32; // mulheaderlength
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

                _cache[id] = new UoSound(name, id, resultBuffer);
                _removed[id] = false;
            }
        }

        public static void Remove(int id)
        {
            _removed[id] = true;
            _cache[id] = null;
        }

        public static void Save(string path)
        {
            string idx = Path.Combine(path, "soundidx.mul");
            string mul = Path.Combine(path, "sound.mul");

            const int headerLength = 44;

            using (var fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var binidx = new BinaryWriter(fsidx))
            using (var binmul = new BinaryWriter(fsmul))
            {
                for (int i = 0; i < _cache.Length; ++i)
                {
                    UoSound sound = _cache[i];

                    if (sound == null && !_removed[i])
                    {
                        sound = GetSound(i, out bool trans);

                        if (!trans)
                        {
                            _cache[i] = sound;
                        }
                        else
                        {
                            sound = null;
                        }
                    }

                    if (sound == null || _removed[i])
                    {
                        binidx.Write(-1); // lookup
                        binidx.Write(-1); // length
                        binidx.Write(-1); // extra
                    }
                    else
                    {
                        binidx.Write((int)fsmul.Position); // lookup
                        var length = (int)fsmul.Position;

                        var b = new byte[32];
                        if (sound.Name != null)
                        {
                            byte[] bb = Encoding.ASCII.GetBytes(sound.Name);
                            if (bb.Length > 32)
                            {
                                Array.Resize(ref bb, 32);
                            }

                            bb.CopyTo(b, 0);
                        }

                        binmul.Write(b);
                        using (var m = new MemoryStream(sound.Buffer))
                        {
                            m.Seek(headerLength, SeekOrigin.Begin);
                            var resultBuffer = new byte[m.Length - headerLength];
                            m.Read(resultBuffer, 0, (int)m.Length - headerLength);
                            binmul.Write(resultBuffer);
                        }

                        length = (int)fsmul.Position - length;
                        binidx.Write(length);
                        binidx.Write(i + 1);
                    }
                }
            }
        }

        public static void SaveSoundListToCsv(string fileName, int soundIdOffset = 0)
        {
            using (var tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), Encoding.GetEncoding(1252)))
            {
                tex.WriteLine("ID;Name;Length");

                for (int i = 0; i < 0xFFF; ++i)
                {
                    if (!IsValidSound(i, out string name, out bool _))
                    {
                        continue;
                    }

                    tex.Write("0x{0:X3}", i + soundIdOffset);
                    tex.Write(";{0}", name);
                    tex.WriteLine(";{0:f}", GetSoundLength(i));
                }
            }
        }

        public static bool IsRemovedSound(int soundId)
        {
            return soundId < 0 || _removed[soundId];
        }

        private static byte[] CheckAndFixWave(byte[] inputBuffer)
        {
            if (inputBuffer.Length < 44)
            {
                throw new WaveFormatException("Invalid File.");
            }

            try
            {
                var croppedByteCount = 0;

                using (var outputMemoryStream = new MemoryStream())
                using (var output = new BinaryWriter(outputMemoryStream))
                using (var inputMemoryStream = new MemoryStream(inputBuffer))
                using (var input = new BinaryReader(inputMemoryStream))
                {
                    // RIFF HEADER
                    if (input.ReadString(4) != "RIFF")
                    {
                        throw new WaveFormatException("RIFF Header not found.");
                    }

                    output.WriteString("RIFF");

                    var riffLength = input.ReadUInt32();
                    output.Write(riffLength);

                    Debug.WriteLine("RIFF length: " + riffLength + " bytes");

                    if (input.ReadString(4) != "WAVE")
                    {
                        throw new WaveFormatException("WAVE Format not found.");
                    }

                    output.WriteString("WAVE");

                    var fmtWritten = false;
                    while (inputMemoryStream.Position < inputMemoryStream.Length)
                    {
                        var currentHeader = input.ReadString(4);
                        var currentLength = input.ReadInt32();

                        if (currentHeader == "fmt ")
                        {
                            if (currentLength != 16)
                            {
                                throw new WaveFormatException("FMT size not supported.");
                            }

                            output.WriteString(currentHeader);
                            output.Write(currentLength);

                            var waveFormat = (WaveFormat)input.ReadInt16();
                            if (waveFormat != WaveFormat.PCM)
                            {
                                throw new WaveFormatException("Format not supported: PCM expected but " + waveFormat + " given!");
                            }

                            output.Write((short)waveFormat);

                            var channels = input.ReadUInt16();
                            if (channels != 1)
                            {
                                throw new WaveFormatException("Only Mono supported, but " + channels + " channels given!");
                            }

                            output.Write(channels);

                            var frequency = input.ReadUInt32();
                            if (frequency != 22050)
                            {
                                throw new WaveFormatException("Only 22050Hz supported, but " + frequency + "Hz given!");
                            }

                            output.Write(frequency);

                            var rate = input.ReadUInt32();
                            output.Write(rate);
                            Debug.WriteLine("Rate: " + rate + " byte/s");
                            var align = input.ReadUInt16();
                            output.Write(align);
                            Debug.WriteLine("Align: " + align);

                            var depth = input.ReadUInt16();
                            if (depth != 16)
                            {
                                throw new WaveFormatException(
                                    "Only 16 bits supported, but " + depth + " bits given!");
                            }

                            output.Write(depth);
                            Debug.WriteLine("Depth: " + depth + " bits");

                            fmtWritten = true;
                        }
                        else if (currentHeader == "data")
                        {
                            if (!fmtWritten)
                            {
                                throw new WaveFormatException("Unexpected wave file order.");
                            }

                            output.WriteString(currentHeader);
                            output.Write(currentLength);

                            Debug.WriteLine("Data length: " + currentLength + " bytes");
                            var data = input.ReadBytes(currentLength);

                            output.Write(data);
                        }
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
            catch (EndOfStreamException ex)
            {
                throw new WaveFormatException("Unexpected Data end.", ex);
            }
        }
    }
}
