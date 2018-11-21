using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataLayer.DataIO.DataIOUtilities
{
    internal class FileHeaderUtilities
    {
        private const string DATETIME_FORMAT = "s"; // "yyyy'-'MM'-'dd'T'HH':'mm':'ss" (2008-04-10T06:30:00)


        public static byte[] EncodeDatasetID(string datasetName, DateTime creationTime)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(memoryStream))
            {
                writer.Write(datasetName);
                writer.Write(creationTime.ToString(DATETIME_FORMAT, CultureInfo.InvariantCulture));
                return memoryStream.ToArray();
            }
        }

        public static void DecodeDatasetID(byte[] datasetId, out string datasetName, out DateTime creationTime)
        {
            using (MemoryStream memoryStream = new MemoryStream(datasetId))
            using (BinaryReader reader = new BinaryReader(memoryStream))
            {
                datasetName = reader.ReadString();
                string dateTime = reader.ReadString();
                creationTime = DateTime.ParseExact(dateTime, DATETIME_FORMAT, CultureInfo.InvariantCulture);

                // TODO: check memory stream is empty
            }
        }
    }
}
