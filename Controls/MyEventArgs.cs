using System;

namespace FiddlerControls
{
    public class MyEventArgs : EventArgs
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