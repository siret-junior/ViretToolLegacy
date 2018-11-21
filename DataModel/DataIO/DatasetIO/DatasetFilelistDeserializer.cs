using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ViretTool.DataLayer.DataIO.DataIOUtilities;
using ViretTool.DataModel;

namespace ViretTool.DataLayer.DataIO.DatasetIO
{
    
    public class DatasetFilelistDeserializer
    {
        private List<Video> _videos;
        private List<Shot> _shots;
        private List<Group> _groups;
        private List<Frame> _frames;

        private List<List<Shot>> _videoShotMappings;
        private List<List<Group>> _videoGroupMappings;
        private List<List<Frame>> _videoFrameMappings;
        private List<List<Frame>> _shotFrameMappings;
        private List<List<Frame>> _groupFrameMappings;


        public virtual Dataset Deserialize(StreamReader reader, string datasetName)
        {
            ResetLists();
            
            while (!reader.EndOfStream)
            {
                string relativePath = reader.ReadLine();
                string filename = Path.GetFileName(relativePath);

                ParseFrameHeirarchy(filename,
                    out int videoId, 
                    out int shotId, 
                    out int shotStartFrame, 
                    out int shotEndFrame,
                    out int groupId, 
                    out int frameNumber,
                    out string extension);


                Video video = GetOrAppendVideo(videoId);
                
                Shot shot = GetOrAppendShot(video, shotId, shotStartFrame, shotEndFrame);

                Group group = GetOrAppendGroup(video, groupId);

                Frame frame = AppendFrame(video, shot, group, frameNumber);

            }

            // TODO: set mappings
            SetVideoShotMappings();
            SetVideoGroupMappings();
            SetVideoFrameMappings();
            SetShotFrameMappings();
            SetGroupFrameMappings();

            byte[] datasetId = FileHeaderUtilities.EncodeDatasetID(datasetName, DateTime.Now);

            Dataset dataset = new Dataset(datasetId, 
                _videos.ToArray(), _shots.ToArray(), _groups.ToArray(), _frames.ToArray());
            return dataset;
        }
        

        private Video GetOrAppendVideo(int videoId)
        {
            int lastVideoId = _videos.Count - 1;

            if (lastVideoId == videoId)
            {
                // this is the last added video
                return _videos[videoId];
            }
            else if (lastVideoId + 1 == videoId)
            {
                // this is a newly added video
                Video video = new Video(videoId);
                _videos.Add(video);

                // prepare for children
                _videoShotMappings.Add(new List<Shot>());
                _videoGroupMappings.Add(new List<Group>());
                _videoFrameMappings.Add(new List<Frame>());

                return video;
            }
            else
            {
                // IDs do not increment sequentially!
                // create missing videos
                int missingVideoId = lastVideoId + 1;
                while (missingVideoId != videoId)
                {
                    Video video = GetOrAppendVideo(missingVideoId++);
                    Shot shot = GetOrAppendShot(video, 0, 0, 0);
                    Group group = GetOrAppendGroup(video, 0);
                    //Frame frame = AppendFrame(video, shot, group, 0);
                }
                // try to append the video again
                return GetOrAppendVideo(videoId);
                
                //throw new ArgumentException(string.Format(
                //    "Input video IDs do not increment sequentially: last {0}, current: {1}", lastVideoId, videoId));
            }
        }

        private Shot GetOrAppendShot(Video video, int shotId, int shotStartFrame, int shotEndFrame)
        {
            List<Shot> videoShots = _videoShotMappings[video.Id];

            int lastShotId = videoShots.Count - 1;

            if (lastShotId == shotId)
            {
                // this is the last added shot
                return videoShots[shotId];
            }
            else if (lastShotId + 1 == shotId)
            {
                // this is a newly added shot
                Shot shot = new Shot(_shots.Count, shotId, shotStartFrame, shotEndFrame);
                _shots.Add(shot);

                // append to parent
                videoShots.Add(shot);

                // prepare for children
                _shotFrameMappings.Add(new List<Frame>());

                return shot;
            }
            else if (lastShotId + 1 < shotId)
            {
                // TODO: missing shot fix
                return GetOrAppendShot(video, lastShotId + 1, shotEndFrame, shotEndFrame);
            }
            else
            {
                // IDs do not increment sequentially!
                throw new ArgumentException(string.Format(
                    "Input shot IDs do not increment sequentially: last {0}, current: {1}", lastShotId, shotId));
            }
        }

        private Group GetOrAppendGroup(Video video, int groupId)
        {
            List<Group> videoGroups = _videoGroupMappings[video.Id];

            int lastGroupId = videoGroups.Count - 1;
            
            if (lastGroupId == groupId)
            {
                // this is the last added group
                return videoGroups[groupId];
            }
            else if (lastGroupId + 1 == groupId)
            {
                // this is a newly added shot
                Group group = new Group(_groups.Count);
                _groups.Add(group);

                // append to parent
                videoGroups.Add(group);

                // prepare for children
                _groupFrameMappings.Add(new List<Frame>());

                return group;
            }
            //else if (lastGroupId < 0)
            //{
            //    // this is an empty group
            //    Group group = new Group(_groups.Count);
            //    _groups.Add(group);

            //    // append to parent
            //    videoGroups.Add(group);

            //    // prepare for children
            //    _groupFrameMappings.Add(new List<Frame>());

            //    return group;
            //}
            else
            {
                // IDs do not increment sequentially!
                throw new ArgumentException(string.Format(
                    "Input group IDs do not increment sequentially: last {0}, current: {1}", lastGroupId, groupId));
            }
        }

        private Frame AppendFrame(Video video, Shot shot, Group group, int frameNumber)
        {
            Frame frame = new Frame(_frames.Count, frameNumber);
            _frames.Add(frame);

            // append to parents
            _videoFrameMappings[video.Id].Add(frame);
            _shotFrameMappings[shot.Id].Add(frame);
            _groupFrameMappings[group.Id].Add(frame);

            return frame;
        }



        private void SetVideoShotMappings()
        {
            foreach (Video video in _videos)
            {
                Shot[] shotMappings = _videoShotMappings[video.Id].ToArray();
                video.SetShotMappings(shotMappings);
            }
        }

        private void SetVideoGroupMappings()
        {
            foreach (Video video in _videos)
            {
                Group[] groupMappings = _videoGroupMappings[video.Id].ToArray();
                video.SetGroupMappings(groupMappings);
            }
        }

        private void SetVideoFrameMappings()
        {
            foreach (Video video in _videos)
            {
                Frame[] frameMappings = _videoFrameMappings[video.Id].ToArray();
                video.SetFrameMappings(frameMappings);
            }
        }

        private void SetShotFrameMappings()
        {
            foreach (Shot shot in _shots)
            {
                Frame[] frameMappings = _shotFrameMappings[shot.Id].ToArray();
                shot.SetFrameMappings(frameMappings);
            }
        }

        private void SetGroupFrameMappings()
        {
            foreach (Group group in _groups)
            {
                Frame[] frameMappings = _groupFrameMappings[group.Id].ToArray();
                group.SetFrameMappings(frameMappings);
            }
        }

        private void ResetLists()
        {
            _videos = new List<Video>();
            _shots = new List<Shot>();
            _groups = new List<Group>();
            _frames = new List<Frame>();

            _videoShotMappings = new List<List<Shot>>();
            _videoGroupMappings = new List<List<Group>>();
            _videoFrameMappings = new List<List<Frame>>();
            _shotFrameMappings = new List<List<Frame>>();
            _groupFrameMappings = new List<List<Frame>>();
        }



        
        private static readonly System.Text.RegularExpressions.Regex _tokenFormatRegex
            = new System.Text.RegularExpressions.Regex(
            @"^[Vv](?<videoId>[0-9]+)"
            + @"_"
            + @"[Ss](?<shotId>[0-9]+)"
                + @"\("
                + @"[Ff](?<shotStartFrame>[0-9]+)"
                + @"-"
                + @"[Ff](?<shotEndFrame>[0-9]+)"
                + @"\)"
            + @"_"
            + @"[Gg](?<groupId>[0-9]+)"
            + @"_"
            + @"[Ff](?<frameNumber>[0-9]+)"
            + @"\.(?<extension>.*)$",
            System.Text.RegularExpressions.RegexOptions.ExplicitCapture);


        private static void ParseFrameHeirarchy(string inputString,
            out int videoId, 
            out int shotId, 
            out int shotStartFrame,
            out int shotEndFrame,
            out int groupId,
            out int frameNumber, 
            out string extension)
        {
            System.Text.RegularExpressions.Match match = _tokenFormatRegex.Match(inputString);
            if (!match.Success)
            {
                throw new ArgumentException("Unknown interaction token format: " + inputString);
            }

            videoId = int.Parse(match.Groups["videoId"].Value);
            shotId = int.Parse(match.Groups["shotId"].Value);
            shotStartFrame = int.Parse(match.Groups["shotStartFrame"].Value);
            shotEndFrame = int.Parse(match.Groups["shotEndFrame"].Value);
            groupId = int.Parse(match.Groups["groupId"].Value);
            frameNumber = int.Parse(match.Groups["frameNumber"].Value);
            extension = match.Groups["extension"].Value;

            // TODO: other checks
            if (frameNumber < shotStartFrame || frameNumber > shotEndFrame)
            {
                throw new ArgumentException("Frame number not in shot range!");
            }
        }
    }
}
