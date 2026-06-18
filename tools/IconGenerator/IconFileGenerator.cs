using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace IconGenerator;

public static class IconFileGenerator
{
    private static readonly int[] IconSizes = [16, 32, 48, 256];

    public static void CreateFromPng(string pngPath, string icoPath)
    {
        var pngImages = new List<byte[]>();

        using (var source = new Bitmap(pngPath))
        {
            foreach (var size in IconSizes)
            {
                using var resized = new Bitmap(size, size, PixelFormat.Format32bppArgb);
                using (var graphics = Graphics.FromImage(resized))
                {
                    graphics.Clear(Color.Transparent);
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(source, 0, 0, size, size);
                }

                using var stream = new MemoryStream();
                resized.Save(stream, ImageFormat.Png);
                pngImages.Add(stream.ToArray());
            }
        }

        WriteIcoFile(icoPath, IconSizes, pngImages);
    }

    private static void WriteIcoFile(string icoPath, int[] sizes, IReadOnlyList<byte[]> pngImages)
    {
        using var stream = File.Create(icoPath);
        using var writer = new BinaryWriter(stream);

        writer.Write((ushort)0);
        writer.Write((ushort)1);
        writer.Write((ushort)pngImages.Count);

        var offset = 6 + 16 * pngImages.Count;

        for (var index = 0; index < pngImages.Count; index++)
        {
            var size = sizes[index];
            var png = pngImages[index];

            writer.Write(size >= 256 ? (byte)0 : (byte)size);
            writer.Write(size >= 256 ? (byte)0 : (byte)size);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((ushort)1);
            writer.Write((ushort)32);
            writer.Write((uint)png.Length);
            writer.Write((uint)offset);

            offset += png.Length;
        }

        foreach (var png in pngImages)
        {
            writer.Write(png);
        }
    }
}
