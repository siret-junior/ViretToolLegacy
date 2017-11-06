using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ViretTool.DataModel {
    class ImageHelper {
        public static BitmapSource StreamToImage(byte[] JPGThumbnail) {
            BitmapImage BI = new BitmapImage();

            BI.BeginInit();
            BI.StreamSource = new System.IO.MemoryStream(JPGThumbnail);
            BI.EndInit();

            return BI;
        }
    }
}
