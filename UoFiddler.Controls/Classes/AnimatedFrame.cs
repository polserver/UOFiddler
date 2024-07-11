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

using System.Drawing;

namespace UoFiddler.Controls.Classes
{
    public sealed class AnimatedFrame(Bitmap bitmap, Point center)
    {
        public Point Center { get; set; } = center;
        public Bitmap Bitmap { get; set; } = bitmap;
    }
}
