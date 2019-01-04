namespace ViretTool.DataLayer.DataModel.Creation.IO
{
    internal abstract class DatasetFileReader : DatasetIO
    {
        public abstract Dataset ReadDataset(string inputFilePath);
    }
}
