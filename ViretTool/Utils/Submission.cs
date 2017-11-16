using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

        public async void Send(int trecvidVideoId, int trecvidFrameId) {
            if (mClient == null) return;
            try {
                var list = new string[] { "Type=VBSf", "Name=" + TeamName, "VideoID=" + trecvidVideoId, "FrameID=" + trecvidFrameId };
                var content = new StringContent(string.Join("&", list));

                var response = await mClient.PostAsync(string.Format("http://{0}:{1}/", IP, Port), content);
                var responseString = await response.Content.ReadAsStringAsync();

                IsConnected = responseString == "VBSfOK";
            } catch (Exception) {
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
