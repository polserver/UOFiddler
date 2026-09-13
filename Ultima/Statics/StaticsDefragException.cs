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

namespace Ultima.Statics
{
    /// <summary>
    /// Raised when a statics defrag run cannot continue. Unlike the routine it replaces, the
    /// defragmenter never swallows an error and never silently emits a partial file.
    /// </summary>
    public sealed class StaticsDefragException : Exception
    {
        public StaticsDefragException(string message) : base(message)
        {
        }

        public StaticsDefragException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}