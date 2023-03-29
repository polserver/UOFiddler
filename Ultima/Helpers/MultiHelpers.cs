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

using System.IO;

namespace Ultima.Helpers
{
    public static class MultiHelpers
    {
        public static string ReadUOAString(BinaryReader bin)
        {
            byte flag = bin.ReadByte();

            return flag == 0 ? null : bin.ReadString();
        }
    }
}