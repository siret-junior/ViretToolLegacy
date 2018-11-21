using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataModel;

namespace ViretTool.DataLayer.DataIO.DatasetIO
{
    internal class DatasetBinarySerializer : DatasetSerializationBase
    {
        public DatasetBinarySerializer()
        {
        }


        public static void Serialize(Stream serializationStream, Dataset dataset)
        {
            using (BinaryWriter writer = new BinaryWriter(serializationStream))
            {
                StoreFileHeader(writer, dataset);
                StoreDatasetItems(writer, dataset);
                StoreItemMappings(writer, dataset);
            }
        }


        private static void StoreFileHeader(BinaryWriter writer, Dataset dataset)
        {
            writer.Write(dataset.DatasetId.Length);
            writer.Write(dataset.DatasetId);    // dataset name + extraction time
            writer.Write(DATASET_FILETYPE_ID);
            writer.Write(DATASET_VERSION);
        }

        private static void StoreDatasetItems(BinaryWriter writer, Dataset dataset)
        {
            writer.Write(dataset.Videos.Count);
            writer.Write(dataset.Shots.Count);
            writer.Write(dataset.Groups.Count);
            writer.Write(dataset.Frames.Count);
        }

        private static void StoreItemMappings(BinaryWriter writer, Dataset dataset)
        {
            StoreVideoShotMappings(writer, dataset);
            StoreVideoGroupMappings(writer, dataset);
            StoreVideoFrameMappings(writer, dataset);

            StoreShotFrameMappings(writer, dataset);

            StoreGroupFrameMappings(writer, dataset);
        }


        private static void StoreVideoShotMappings(BinaryWriter writer, Dataset dataset)
        {
            foreach (Video video in dataset.Videos)
            {
                writer.Write(video.Shots.Count);

                foreach (Shot shot in video.Shots)
                {
                    writer.Write(shot.Id);
                }
            }
        }

        private static void StoreVideoGroupMappings(BinaryWriter writer, Dataset dataset)
        {
            foreach (Video video in dataset.Videos)
            {
                writer.Write(video.Groups.Count);

                foreach (Group group in video.Groups)
                {
                    writer.Write(group.Id);
                }
            }
        }

        private static void StoreVideoFrameMappings(BinaryWriter writer, Dataset dataset)
        {
            foreach (Video video in dataset.Videos)
            {
                writer.Write(video.Frames.Count);

                foreach (Frame frame in video.Frames)
                {
                    writer.Write(frame.Id);
                }
            }
        }

        private static void StoreShotFrameMappings(BinaryWriter writer, Dataset dataset)
        {
            foreach (Shot shot in dataset.Shots)
            {
                writer.Write(shot.Frames.Count);

                foreach (Frame frame in shot.Frames)
                {
                    writer.Write(frame.Id);
                }
            }
        }

        private static void StoreGroupFrameMappings(BinaryWriter writer, Dataset dataset)
        {
            foreach (Group group in dataset.Groups)
            {
                writer.Write(group.Frames.Count);

                foreach (Frame frame in group.Frames)
                {
                    writer.Write(frame.Id);
                }
            }
        }
    }
}
