using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VitretTool.EvaluationServer {
    class ClientHandler {

        public static void Respond(HttpListenerRequest request, HttpListenerResponse response, Teams teams, VBSTasks tasks) {
            string postData;

            using (var stream = new StreamReader(request.InputStream, request.ContentEncoding)) {
                postData = stream.ReadToEnd();
            }

            Debug.WriteLine(postData);

            var post = new Dictionary<string, string>();

            foreach (var item in postData.Split('&')) {
                var p = item.Split('=');
                if (p.Length != 2) continue;
                post.Add(p[0], p[1]);
            }

            if (!post.ContainsKey("Type") || !post.ContainsKey("Name")) {
                response.OutputStream.Close();
                return;
            }

            Team team;

            switch (post["Type"]) {
                case "VBSt":
                    response.ContentLength64 = 10;
                    response.ContentEncoding = Encoding.ASCII;
                    response.OutputStream.Write(BitConverter.GetBytes(tasks.CurrentTask != null ? tasks.CurrentTask.TaskId : -1), 0, 4);
                    response.OutputStream.Write(new byte[] { 0x56, 0x42, 0x53, 0x74, 0x4f, 0x4b }, 0, 6);
                    break;
                case "VBSn":
                    team = teams.CreateNewTeam(post["Name"]);
                    if (team == null) {
                        response.OutputStream.Close();
                        return;
                    }

                    response.ContentLength64 = 6;
                    response.ContentEncoding = Encoding.ASCII;
                    response.OutputStream.Write(new byte[] { 0x56, 0x42, 0x53, 0x6e, 0x4f, 0x4b }, 0, 6);
                    break;
                case "VBSf":
                    team = teams[post["Name"]];
                    if (team == null) {
                        response.OutputStream.Close();
                        return;
                    }

                    int frameId, videoId;
                    if (!post.ContainsKey("FrameID") || !post.ContainsKey("VideoID")
                        || !int.TryParse(post["FrameID"], out frameId) || !int.TryParse(post["VideoID"], out videoId)) {
                        response.OutputStream.Close();
                        return;
                    }

                    team.SubmitVideoFrame(videoId, frameId);

                    response.ContentLength64 = 6;
                    response.OutputStream.Write(new byte[] { 0x56, 0x42, 0x53, 0x66, 0x4f, 0x4b }, 0, 6);
                    break;
                default:
                    break;
            }

            response.OutputStream.Close();
        }

    }
}
