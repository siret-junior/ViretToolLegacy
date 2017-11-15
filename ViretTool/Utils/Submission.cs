using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.Utils {
    class Submission {

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

        public void Connect(string ip, int port, string teamName = null) {

        }

        public void Send(int trecvidVideoId, int trecvidFrameId) {

        }

        public void Disconnect() {
            // TODO disconnect

            Port = 0;
            IsConnected = false;
            IP = new IPAddress(new byte[] { 0, 0, 0, 0 });
        }

    }
}
