using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ViretTool.DataLayer.DataModel.Creation.IO
{
    internal class DatasetBinaryFileReader : DatasetFileReader
    {
        public DatasetBinaryFileReader()
        {
        }


        public override Dataset ReadDataset(string inputFilePath)
        {
            using (BinaryReader reader = new BinaryReader(
                File.Open(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                byte[] datasetId = LoadAndCheckFileHeader(reader);

                // load item counts
                LoadDatasetItems(reader, out Video[] videos, out Shot[] shots, out Group[] groups, out Frame[] frames);

                // load mappings
                LoadItemMappings(reader, videos, shots, groups, frames);

                // TODO: load item attributes (move to separate files?)
                //LoadItemAttributes(reader, videos, shots, groups, frames);

                // instantiate dataset object
                Dataset dataset = new Dataset(datasetId, videos, shots, groups, frames);

                return dataset;
            }
        }
        
        
        private static byte[] LoadAndCheckFileHeader(BinaryReader reader)
        {
            byte[] datasetNameEncoded = ReadNullTerminatedStringBytes(reader);
            byte[] timestampEncoded = ReadNullTerminatedStringBytes(reader);

            byte[] fileTypeEncoded = ReadNullTerminatedStringBytes(reader);
            CheckFileType(fileTypeEncoded);

            int datasetVersion = reader.ReadInt32();
            CheckDatasetVersion(datasetVersion);

            byte[] datasetId = datasetNameEncoded.Concat(timestampEncoded).ToArray();
            return datasetId;
        }
        
        private static void CheckFileType(byte[] fileTypeEncoded)
        {
            string fileType = Encoding.UTF8.GetString(fileTypeEncoded);
            if (!fileType.Equals(DATASET_FILETYPE_ID))
            {
                throw new IOException(
                    string.Format("Invalid filetype: {0}, ({1} expected).", fileType, DATASET_FILETYPE_ID));
            }
        }

        private static void CheckDatasetVersion(int datasetVersion)
        {
            if (!datasetVersion.Equals(DATASET_VERSION))
            {
                throw new IOException(
                    string.Format("Invalid dataset version: {0}, ({1} expected).", datasetVersion, DATASET_VERSION));
            }
        }


        private static void LoadDatasetItems(BinaryReader reader, out Video[] videos, out Shot[] shots, out Group[] groups, out Frame[] frames)
        {
            int videoCount = reader.ReadInt32();
            int shotCount = reader.ReadInt32();
            int groupCount = reader.ReadInt32();
            int frameCount = reader.ReadInt32();

            videos = new Video[videoCount];
            shots = new Shot[shotCount];
            groups = new Group[groupCount];
            frames = new Frame[frameCount];
        }

        
        private static void LoadItemMappings(BinaryReader reader, 
            Video[] videos, Shot[] shots, Group[] groups, Frame[] frames)
        {
            LoadVideoShotMappings(reader, videos, shots);
            LoadVideoGroupMappings(reader, videos, groups);
            LoadVideoFrameMappings(reader, videos, frames);

            LoadShotFrameMappings(reader, shots, frames);
            LoadGroupFrameMappings(reader, groups, frames);
        }

        private static void LoadVideoShotMappings(BinaryReader reader, Video[] videos, Shot[] shots)
        {
            foreach (Video video in videos)
            {
                Shot[] shotMappings = LoadChildrenMappings(reader, video, shots);
                video.SetShotMappings(shotMappings);
            }
        }

        private static void LoadVideoGroupMappings(BinaryReader reader, Video[] videos, Group[] groups)
        {
            foreach (Video video in videos)
            {
                Group[] groupMappings = LoadChildrenMappings(reader, video, groups);
                video.SetGroupMappings(groupMappings);
            }
        }

        private static void LoadVideoFrameMappings(BinaryReader reader, Video[] videos, Frame[] frames)
        {
            foreach (Video video in videos)
            {
                Frame[] frameMappings = LoadChildrenMappings(reader, video, frames);
                video.SetFrameMappings(frameMappings);
            }
        }

        private static void LoadShotFrameMappings(BinaryReader reader, Shot[] shots, Frame[] frames)
        {
            foreach (Shot shot in shots)
            {
                Frame[] frameMappings = LoadChildrenMappings(reader, shot, frames);
                shot.SetFrameMappings(frameMappings);
            }
        }

        private static void LoadGroupFrameMappings(BinaryReader reader, Group[] groups, Frame[] frames)
        {
            foreach (Group group in groups)
            {
                Frame[] frameMappings = LoadChildrenMappings(reader, group, frames);
                group.SetFrameMappings(frameMappings);
            }
        }


        //private static void LoadItemAttributes(BinaryReader reader, Video[] videos, Shot[] shots, Group[] groups, Frame[] frames)
        //{
        //    throw new NotImplementedException();
        //}

        
        private static byte[] ReadNullTerminatedStringBytes(BinaryReader reader)
        {
            List<byte> bytes = new List<byte>();

            byte value;
            while ((value = reader.ReadByte()) != 0)
            {
                bytes.Add(value);
            }
            bytes.Add(0);

            return bytes.ToArray();
        }

        private static Child[] LoadChildrenMappings<Parent, Child>(
            BinaryReader reader, Parent parent, Child[] childrenCollection)
        {
            int childCount = reader.ReadInt32();
            Child[] childrenMappings = new Child[childCount];

            for (int iChild = 0; iChild < childCount; iChild++)
            {
                int childId = reader.ReadInt32();
                childrenMappings[iChild] = childrenCollection[childId];
            }

            return childrenMappings;   
        }
    }
}
