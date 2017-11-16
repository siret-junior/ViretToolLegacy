using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VitretTool.EvaluationServer {

    class Server {

        public int Port { get; private set; }
        public IPAddress IP { get; private set; }

        private Teams mTeams;
        private HttpListener mListener;

        public Server(IPAddress ip, int port, Teams teams) {
            IP = ip;
            Port = port;
            mTeams = teams;
            mListener = new HttpListener();
            mListener.Prefixes.Add(string.Format("http://+:{1}/", IP, Port));
        }

        public async void Listen() {
            // netsh http add urlacl url=http://+:9999/ user=Tom
            // netsh http show urlacl
            //https://stackoverflow.com/questions/14962334/httplistenerexception-access-denied-for-non-admins
            mListener.Start();

            while (true) {
                var context = await mListener.GetContextAsync();

                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                ClientHandler.Respond(request, response, mTeams);
            }
        }
        
    }

}
