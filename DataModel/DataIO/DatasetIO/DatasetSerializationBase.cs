using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataLayer.DataIO.DatasetIO
{
    internal abstract class DatasetSerializationBase
    {
        public const string DATASET_FILETYPE_ID = "Dataset";
        public const int DATASET_VERSION = 0;
    }
}
