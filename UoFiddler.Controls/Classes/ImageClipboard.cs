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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UoFiddler.Controls.Classes
{
    /// <summary>
    /// Moves single images between the graphic tabs and the system clipboard.
    /// </summary>
    public static class ImageClipboard
    {
        /// <summary>
        /// Clipboard format name Photoshop, GIMP, Paint.NET and Krita read and write. It is the only
        /// one of the flavours we handle that carries an alpha channel - CF_DIB flattens it.
        /// </summary>
        private const string PngFormat = "PNG";

        private static readonly string[] _acceptedDropExtensions =
        {
            ".png", ".bmp", ".tif", ".tiff", ".jpg", ".jpeg", ".gif"
        };

        /// <summary>
        /// True when the clipboard holds something we could paste as an image.
        /// </summary>
        public static bool ContainsImage()
        {
            try
            {
                IDataObject data = Clipboard.GetDataObject();
                if (data == null)
                {
                    return false;
                }

                return data.GetDataPresent(PngFormat)
                       || data.GetDataPresent(DataFormats.Bitmap)
                       || data.GetDataPresent(DataFormats.Dib)
                       || GetDroppedImagePath(data) != null;
            }
            catch (ExternalException)
            {
                // Another process has the clipboard open. Treat it as "nothing to paste" rather than
                // popping a dialog from a context menu Opening handler.
                return false;
            }
        }

        /// <summary>
        /// Puts <paramref name="source"/> on the clipboard as both a DIB (so Paint, Word and chat
        /// clients see it) and a PNG stream (so image editors keep the transparent areas).
        /// </summary>
        public static bool TryCopy(Bitmap source, out string error)
        {
            error = null;

            if (source == null)
            {
                error = "There is no image to copy.";
                return false;
            }

            Bitmap flattened = null;
            MemoryStream png = null;

            try
            {
                // The tabs hand us 16bppArgb1555 bitmaps straight out of the SDK cache; other
                // processes cope far better with a plain 32bpp copy.
                flattened = ToArgb32(source);

                png = new MemoryStream();
                flattened.Save(png, ImageFormat.Png);
                png.Position = 0;

                DataObject data = new DataObject();
                data.SetImage(flattened);
                data.SetData(PngFormat, false, png);

                // copy: true flushes everything to the OS clipboard now, so the bitmap and the
                // stream can be disposed as soon as this returns.
                Clipboard.SetDataObject(data, true);
                return true;
            }
            catch (ExternalException)
            {
                error = "The clipboard is in use by another program. Try again.";
                return false;
            }
            finally
            {
                png?.Dispose();
                flattened?.Dispose();
            }
        }

        /// <summary>
        /// Reads an image off the clipboard as a 32bppArgb bitmap owned by the caller, or null with a
        /// message in <paramref name="error"/>.
        /// </summary>
        public static Bitmap TryPaste(out string error)
        {
            error = null;

            IDataObject data;
            try
            {
                data = Clipboard.GetDataObject();
            }
            catch (ExternalException)
            {
                error = "The clipboard is in use by another program. Try again.";
                return null;
            }

            if (data == null)
            {
                error = "The clipboard does not contain an image.";
                return null;
            }

            // PNG first - it is the only flavour that survives with transparency intact.
            try
            {
                if (data.GetDataPresent(PngFormat) && data.GetData(PngFormat) is Stream pngStream)
                {
                    using (pngStream)
                    using (Image png = Image.FromStream(pngStream))
                    {
                        return ToArgb32(png);
                    }
                }
            }
            catch (Exception ex) when (ex is ArgumentException || ex is ExternalException)
            {
                // Malformed PNG flavour - fall through and try the DIB.
            }

            try
            {
                Image dib = Clipboard.GetImage();
                if (dib != null)
                {
                    using (dib)
                    {
                        Bitmap bitmap = ToArgb32(dib);

                        // Clipboard DIBs routinely arrive with the alpha byte left at zero, which
                        // would otherwise read as a fully transparent image. Nothing useful is ever
                        // wholly transparent, so treat that as "no alpha information".
                        MakeOpaqueIfFullyTransparent(bitmap);
                        return bitmap;
                    }
                }
            }
            catch (ExternalException)
            {
                // Fall through to the file drop.
            }

            string path = GetDroppedImagePath(data);
            if (path != null)
            {
                try
                {
                    using (Image fromFile = Image.FromFile(path))
                    {
                        return ToArgb32(fromFile);
                    }
                }
                catch (Exception ex) when (ex is OutOfMemoryException || ex is IOException || ex is ArgumentException)
                {
                    error = $"'{Path.GetFileName(path)}' could not be read as an image.";
                    return null;
                }
            }

            error = "The clipboard does not contain an image.";
            return null;
        }

        private static Bitmap ToArgb32(Image source)
        {
            Bitmap result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(result))
            {
                // SourceCopy so the alpha channel arrives byte for byte rather than being blended
                // against the transparent backdrop.
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.DrawImageUnscaled(source, 0, 0);
            }

            return result;
        }

        private static unsafe void MakeOpaqueIfFullyTransparent(Bitmap bitmap)
        {
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            try
            {
                int delta = data.Stride >> 2;
                uint* line = (uint*)data.Scan0;

                for (int y = 0; y < bitmap.Height; ++y, line += delta)
                {
                    for (int x = 0; x < bitmap.Width; ++x)
                    {
                        if ((line[x] & 0xFF000000) != 0)
                        {
                            return;
                        }
                    }
                }

                line = (uint*)data.Scan0;
                for (int y = 0; y < bitmap.Height; ++y, line += delta)
                {
                    for (int x = 0; x < bitmap.Width; ++x)
                    {
                        line[x] |= 0xFF000000;
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        private static string GetDroppedImagePath(IDataObject data)
        {
            if (!data.GetDataPresent(DataFormats.FileDrop))
            {
                return null;
            }

            if (data.GetData(DataFormats.FileDrop) is not string[] files)
            {
                return null;
            }

            foreach (string file in files)
            {
                string extension = Path.GetExtension(file);

                foreach (string accepted in _acceptedDropExtensions)
                {
                    if (string.Equals(extension, accepted, StringComparison.OrdinalIgnoreCase))
                    {
                        return file;
                    }
                }
            }

            return null;
        }
    }
}
