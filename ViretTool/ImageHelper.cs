using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace ViretTool
{
    class ImageHelper
    {
        public static BitmapSource StreamToImage(byte[] JPGThumbnail)
        {
            BitmapImage BI = new BitmapImage();
            
            BI.BeginInit();
            BI.StreamSource = new System.IO.MemoryStream(JPGThumbnail);
            BI.EndInit();

            return BI;
        }

        public static byte[] ResizeAndStoreImageToRGBByteArray(BitmapSource img, int width, int height)
        {
            BitmapSource bmp = new TransformedBitmap(img, new ScaleTransform(width / img.Width, height / img.Height));

            if (bmp.Format != PixelFormats.Rgb24)
                bmp = new FormatConvertedBitmap(bmp, PixelFormats.Rgb24, null, 0);

            byte[] data = new byte[3 * width * height];
            bmp.CopyPixels(data, 3 * width, 0);
            
            return data;
        }
    }
}
