/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.Drawing;

namespace UoFiddler.Plugin.MultiEditor.Rendering
{
    /// <summary>
    /// Colour change of a sprite: rgb * Mul + Add, alpha * MulA.
    /// </summary>
    internal readonly record struct SpriteTint(float MulR, float MulG, float MulB, float MulA, float AddR, float AddG, float AddB)
    {
        public static readonly SpriteTint None = new SpriteTint(1f, 1f, 1f, 1f, 0f, 0f, 0f);

        public static SpriteTint Solid(Color color)
        {
            return new SpriteTint(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f, 0f, 0f, 0f);
        }

        public static SpriteTint Alpha(float alpha)
        {
            return new SpriteTint(1f, 1f, 1f, alpha, 0f, 0f, 0f);
        }
    }
}
