namespace ViretTool.DataLayer.DataModel.Creation.IO
{
    internal abstract class DatasetFileWriter : DatasetIO
    {
        public abstract void WriteDataset(Dataset dataset);
    }
}
