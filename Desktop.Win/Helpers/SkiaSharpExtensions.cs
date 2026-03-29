using SkiaSharp;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Remotely.Desktop.Win.Helpers;

public static class SkiaSharpExtensions
{
    public static SKBitmap ToSKBitmap(this Bitmap bitmap)
    {
        var width = bitmap.Width;
        var height = bitmap.Height;
        var skBitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        var bmpData = bitmap.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);

        try
        {
            var pixels = skBitmap.GetPixels();
            if (pixels != IntPtr.Zero)
            {
                var bytesToCopy = width * height * 4;
                unsafe
                {
                    Buffer.MemoryCopy(bmpData.Scan0.ToPointer(), pixels.ToPointer(), bytesToCopy, bytesToCopy);
                }
            }
        }
        finally
        {
            bitmap.UnlockBits(bmpData);
        }

        return skBitmap;
    }
}
