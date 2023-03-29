/***************************************************************************
 *
 * $Author: Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;

namespace UoFiddler.Controls.Classes
{
    public sealed class MyEventArgs : EventArgs
    {
        public enum Types
        {
            Common = 0,
            ForceReload
        }

        public Types Type { get; }

        public MyEventArgs(Types type) { Type = type; }
    }
}