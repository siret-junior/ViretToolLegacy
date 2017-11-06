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
        private TcpListener mListener;

        public Server(IPAddress ip, int port, Teams teams) {
            IP = ip;
            Port = port;
            mTeams = teams;
            mListener = new TcpListener(IP, Port);
        }

        public async void Listen() {
            mListener.Start();

            while (true) {
                TcpClient client = await mListener.AcceptTcpClientAsync();

                ClientHandler.Listen(client, mTeams);
            }
        }
        
    }

}
