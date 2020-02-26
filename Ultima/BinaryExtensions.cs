using System;
using System.IO;
using System.Text;

namespace Ultima
{
    public static class BinaryExtensions
    {
        public static string ReadString(this BinaryReader reader, int length)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (length < 0 || length > reader.BaseStream.Length + reader.BaseStream.Position)
            {
                throw new ArgumentException("Out of range.");
            }

            char[] buffer = new char[length];
            reader.Read(buffer, 0, length);
            return new string(buffer);
        }

        public static void WriteString(this BinaryWriter writer, string data)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            byte[] bytes = Encoding.ASCII.GetBytes(data);
            writer.Write(bytes);
        }
    }
}