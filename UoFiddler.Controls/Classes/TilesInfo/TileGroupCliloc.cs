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

using Ultima;

namespace UoFiddler.Controls.Classes.TilesInfo
{
    public static class TileGroupCliloc
    {
        private static StringList _list;
        private static bool _attempted;

        public static string Lookup(int tid)
        {
            if (tid == 0)
            {
                return null;
            }

            if (!_attempted)
            {
                _attempted = true;
                try
                {
                    _list = new StringList("enu", false);
                }
                catch
                {
                    _list = null;
                }
            }

            return _list?.GetString(tid);
        }

        public static void Reset()
        {
            _list = null;
            _attempted = false;
        }
    }
}
