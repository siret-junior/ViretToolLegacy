using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.RankingModel.AttributeModels
{
    class DateTimeModel
    {
        private readonly DataModel.Dataset mDataset;

        /// <summary>
        /// A thumbnail based signature in RGB format, stored as a 1D byte array.
        /// </summary>
        private List<DateTime> mDateTimes;

        private const string dateTimeFormat = "yyyy-MM-dd_HH:mm:ss.FF";

        public DateTimeModel(DataModel.Dataset dataset)
        {
            mDataset = dataset;
            mDateTimes = new List<DateTime>(dataset.Frames.Count);

            string inputFile = dataset.GetFileNameByExtension("-dateTimes.txt");
            LoadDateTimes(inputFile);
        }

        private void LoadDateTimes(string inputFile)
        {
            using (StreamReader reader = new StreamReader(inputFile))
            {
                while (!reader.EndOfStream)
                {
                    mDateTimes.Add(
                        DateTime.ParseExact(reader.ReadLine(), dateTimeFormat, CultureInfo.InvariantCulture));
                }
            }
        }


        public DateTime GetDateTime(DataModel.Frame frame)
        {
            return mDateTimes[frame.ID];
        }
    }
}
