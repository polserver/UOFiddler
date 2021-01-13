using System;
using System.Collections.Generic;

namespace UoFiddler.Controls.UserControls.TileView
{
    public class CacheItemEventArgs : EventArgs
    {
        public CacheItemEventArgs(List<int> indices)
        {
            Indices = indices;
        }

        public List<int> Indices { get; }

        public bool Success;
    }

}
