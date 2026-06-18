using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace QRCodeManager.WPF.Helpers;

public static class ClipboardHelper
{
    public static void CopyImage(BitmapImage image)
    {
        Clipboard.SetImage(image);
    }

    public static void CopyText(string text)
    {
        Clipboard.SetText(text);
    }

    public static async Task SavePngAsync(byte[] imageBytes, string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(filePath, imageBytes);
    }
}
