using System;
using System.IO;
using ViretTool.DataModel;

namespace ViretTool.DataLayer.DataIO.DatasetIO
{
    public class DatasetBinaryFormatter
    {
        public const string DATASET_FILETYPE_ID = "Dataset";
        public const int DATASET_VERSION = 0;

        private static readonly Lazy<DatasetBinaryFormatter> lazy =
                new Lazy<DatasetBinaryFormatter>(() => new DatasetBinaryFormatter());

        public static DatasetBinaryFormatter Instance { get { return lazy.Value; } }
        

        private DatasetBinaryFormatter()
        {
        }
        

        public void Serialize(Stream serializationStream, Dataset dataset)
        {
            DatasetBinarySerializer.Serialize(serializationStream, dataset);
        }
        
        public Dataset Deserialize(Stream serializationStream)
        {
            return DatasetBinaryDeserializer.Deserialize(serializationStream);
        }
    }
}
