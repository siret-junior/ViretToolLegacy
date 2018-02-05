using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BasicClient;

namespace ViretTool.Utils {
    class Submission {
        private HttpClient mClient;

        public Submission() {
            IP = new IPAddress(new byte[] { 0, 0, 0, 0 });
        }

        public bool IsConnected { get; private set; }

        /// <summary>
        /// Never null, can be always shown by IP.ToString().
        /// </summary>
        public IPAddress IP { get; private set; }

        public int Port { get; private set; }

        /// <summary>
        /// Null if not set.
        /// </summary>
        public string TeamName { get; private set; }

        public async void Connect(string ip, int port, string teamName = null) {
            mClient = new HttpClient();
            IPAddress ip_;
            if (!IPAddress.TryParse(ip, out ip_)) return;

            IP = ip_;
            mClient = new HttpClient();
            Port = port;
            TeamName = teamName;

            try {
                var list = new string[] { "Type=VBSn", "Name=" + TeamName };
                var content = new StringContent(string.Join("&", list));

                var response = await mClient.PostAsync(string.Format("http://{0}:{1}/", IP, Port), content);
                var responseString = await response.Content.ReadAsStringAsync();

                IsConnected = responseString == "VBSnOK";
            } catch (Exception) {
                IsConnected = false;
            }
        }

        /// <returns>TaskId as int or -1 if error or no task available</returns>
        public async Task<int> GetCurrentTaskId() {
            if (mClient == null) return -1;
            try {
                var list = new string[] { "Type=VBSt", "Name=" + TeamName };
                var content = new StringContent(string.Join("&", list));

                var response = await mClient.PostAsync(string.Format("http://{0}:{1}/", IP, Port), content);
                var responseBytes = await response.Content.ReadAsByteArrayAsync();

                if (responseBytes.Length != 10) return -1;
                return BitConverter.ToInt32(responseBytes, 0);
            } catch (Exception) {
                IsConnected = false;
                return -1;
            }
        }

        public async void Send(int trecvidVideoId, int trecvidFrameId) {
            string browsingString = VBSLogger.AppendTimeAndGetLogString();
            

            if (mClient == null) return;
            try {
                //var list = new string[] { "Type=VBSf", "Name=" + TeamName, "VideoID=" + trecvidVideoId, "FrameID=" + trecvidFrameId };

                const int TEAM_ID = 6;
                const int TRECVID_VIDEO_OFFSET = 35345;

                //string[] list = new string[] {
                ////team =[your team id]
                //    "team=" + TEAM_ID,
                ////video =[id of the video according to the TRECVID 2016 data set(35345 - 39937)]
                //    "video=" + trecvidVideoId,
                ////frame =[zero - based frame number(this frame must be inside the target segment in order to be rated as correct)]
                ////shot =[master shot id(one - based) in accordance with the TRECVID master shot reference(msb)(only for AVS tasks)]
                //    "frame=" + trecvidFrameId,
                ////iseq =[sequence of actions that led to the submission, collected for logging purposes (see instructions)]
                //    "iseq=" + browsingString };

                string list =
                    //team =[your team id]
                    "team=" + TEAM_ID + "&" +
                    //video =[id of the video according to the TRECVID 2016 data set(35345 - 39937)]
                    "video=" + (trecvidVideoId + TRECVID_VIDEO_OFFSET) + "&" +
                    //frame =[zero - based frame number(this frame must be inside the target segment in order to be rated as correct)]
                    //shot =[master shot id(one - based) in accordance with the TRECVID master shot reference(msb)(only for AVS tasks)]
                    "frame=" + trecvidFrameId + "&" +
                    //iseq =[sequence of actions that led to the submission, collected for logging purposes (see instructions)]
                    "iseq=" + browsingString;

                //var content = new StringContent(string.Join("&", list));

                const string DEMO_VBS_URL = "http://demo2.itec.aau.at:80/vbs/submit?";
                const string VBS_URL = "http://10.10.10.43:80/vbs/submit?";

                //var response = await mClient.PostAsync(/*string.Format("http://{0}:{1}/", IP, Port)*/VBS_URL, content);
                //var responseString = await response.Content.ReadAsStringAsync();

                string URI = VBS_URL + list;
                var response = await mClient.GetAsync(URI);

                Logger.LogInfo(this, "Submission: " + URI);

                //will throw an exception if not successful
                //response.EnsureSuccessStatusCode();

                //string content = await response.Content.ReadAsStringAsync();
                //return await Task.Run(() = &gt; JsonObject.Parse(content));

                //IsConnected = responseString == "VBSfOK";
            } catch (Exception ex) {
                string msg = ex.Message;
                IsConnected = false;
            }
        }

        public void Disconnect() {
            if (mClient != null) mClient.Dispose();
            mClient = null;

            Port = 0;
            IsConnected = false;
            IP = new IPAddress(new byte[] { 0, 0, 0, 0 });
        }

    }
}
