using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Hashing;
using System.Runtime.InteropServices;

namespace Ultima.Helpers
{
    public static class Extensions
    {
        public static byte[] ToArray(this Bitmap bmp, PixelFormat? format = null)
        {
            if (bmp == null)
            {
                throw new ArgumentNullException(nameof(bmp));
            }

            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadOnly, format ?? bmp.PixelFormat);
            try
            {
                int size = data.Stride * bmp.Height;
                byte[] buffer = new byte[size];
                Marshal.Copy(data.Scan0, buffer, 0, size);
                return buffer;
            }
            finally
            {
                bmp.UnlockBits(data);
            }
        }

        /// <summary>
        /// Hashes a bitmap's pixel data using xxHash128. Replaces the old
        /// <c>bmp.ToArray().ToSha256()</c> pattern for Save-time deduplication:
        ///
        /// 1. Allocation-free — hashes from the locked <see cref="BitmapData.Scan0"/>
        ///    span directly. No <c>byte[]</c> copy of pixel data, no <c>byte[32]</c>
        ///    SHA256 output.
        /// 2. ~10× faster — xxHash128 hits 10–30 GB/s on modern CPUs vs SHA256's
        ///    1–3 GB/s, and we no longer pay the LockBits+memcpy that <c>ToArray</c>
        ///    did before hashing.
        /// 3. Returns a 128-bit struct that's a perfect <see cref="System.Collections.Generic.Dictionary{TKey,TValue}"/>
        ///    key for O(1) dedup, replacing the previous O(n²) linear scan over
        ///    32-byte SHA256 digests.
        ///
        /// This is not a cryptographic hash; do not use for security-sensitive
        /// comparisons. For dedup on bitmaps that the caller already
        /// produced/owns, xxHash128's collision probability is negligible
        /// (~2^-64 per pair) — perfectly adequate.
        /// </summary>
        public static UInt128 Hash128(this Bitmap bmp, PixelFormat? format = null)
        {
            if (bmp == null)
            {
                throw new ArgumentNullException(nameof(bmp));
            }

            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadOnly, format ?? bmp.PixelFormat);
            try
            {
                return Hash128(data);
            }
            finally
            {
                bmp.UnlockBits(data);
            }
        }

        /// <summary>
        /// xxHash128 over an already-locked <see cref="BitmapData"/>. Use this
        /// overload when the caller has already acquired a LockBits (Save
        /// hot path locks once for read, hashes, then encodes).
        /// </summary>
        public static unsafe UInt128 Hash128(this BitmapData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            int size = data.Stride * data.Height;
            var span = new ReadOnlySpan<byte>((void*)data.Scan0, size);
            return XxHash128.HashToUInt128(span);
        }
    }
}
