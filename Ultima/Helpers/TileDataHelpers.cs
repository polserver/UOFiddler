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
using System.Globalization;
using System.Text;

namespace Ultima.Helpers
{
    public static class TileDataHelpers
    {
        private static readonly byte[] _stringBuffer = new byte[20];

        public static unsafe string ReadNameString(byte* buffer)
        {
            int count;
            for (count = 0; count < 20 && *buffer != 0; ++count)
            {
                _stringBuffer[count] = *buffer++;
            }

            return Encoding.ASCII.GetString(_stringBuffer, 0, count);
        }

        public static string ReadNameString(byte[] buffer, int len)
        {
            int count;
            for (count = 0; count < 20 && buffer[count] != 0; ++count)
            {
                // TODO: this loop is weird
                //;
            }

            return Encoding.ASCII.GetString(buffer, 0, count);
        }

        /// <summary>
        /// Reads a NUL-padded ASCII name from the start of <paramref name="buffer"/>,
        /// up to 20 bytes. Lets callers avoid pinning + Marshal.PtrToStructure.
        /// </summary>
        public static string ReadNameString(ReadOnlySpan<byte> buffer)
        {
            int max = Math.Min(20, buffer.Length);
            int count = 0;
            while (count < max && buffer[count] != 0)
            {
                count++;
            }

            return Encoding.ASCII.GetString(buffer.Slice(0, count));
        }

        public static int ConvertStringToInt(string text)
        {
            int result;
            if (text.Contains("0x"))
            {
                string convert = text.Replace("0x", "");
                int.TryParse(convert, NumberStyles.HexNumber, null, out result);
            }
            else
            {
                int.TryParse(text, NumberStyles.Integer, null, out result);
            }

            return result;
        }
    }
}