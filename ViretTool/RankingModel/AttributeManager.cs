using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataModel;

namespace ViretTool.RankingModel.AttributeModels
{
    public static class AttributeManager
    {
        private static DataModel.Dataset mDataset;

        private static SourceFileModel mSourceFileModel;
        private static DateTimeModel mDateTimeModel;

        public static void Initialize(DataModel.Dataset dataset)
        {
            mDataset = dataset;

            mSourceFileModel = new SourceFileModel(mDataset);
            mDateTimeModel = new DateTimeModel(mDataset);
        }


        public static string GetSourceFile(Frame frame)
        {
            if (frame.ID == -1) return null;
            return mSourceFileModel.GetSourceFile(frame);
        }

        public static DateTime GetDateTime(Frame frame)
        {
            if (frame.ID == -1) return new DateTime();
            return mDateTimeModel.GetDateTime(frame);
        }
    }
}
