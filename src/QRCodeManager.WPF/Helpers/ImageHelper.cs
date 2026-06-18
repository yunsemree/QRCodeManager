using System.IO;
using System.Windows.Media.Imaging;

namespace QRCodeManager.WPF.Helpers;

public static class ImageHelper
{
    public static BitmapImage ToBitmapImage(byte[] imageBytes)
    {
        using var stream = new MemoryStream(imageBytes);
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        return image;
    }

    public static BitmapImage? ToBitmapImageFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(filePath, UriKind.Absolute);
        image.EndInit();
        image.Freeze();
        return image;
    }
}
