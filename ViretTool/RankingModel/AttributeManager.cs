using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataModel;

namespace ViretTool.RankingModel.AttributeModels
{
    public class AttributeManager
    {
        private DataModel.Dataset mDataset;

        private SourceFileModel mSourceFileModel;
        private DateTimeModel mDateTimeModel;

        public AttributeManager(DataModel.Dataset dataset)
        {
            mDataset = dataset;

            mSourceFileModel = new SourceFileModel(mDataset);
            mDateTimeModel = new DateTimeModel(mDataset);
        }


        public string GetSourceFile(Frame frame)
        {
            return mSourceFileModel.GetSourceFile(frame);
        }

        public DateTime GetDateTime(Frame frame)
        {
            return mDateTimeModel.GetDateTime(frame);
        }
    }
}
