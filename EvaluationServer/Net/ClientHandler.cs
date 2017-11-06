using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VitretTool.EvaluationServer {
    class ClientHandler {
        
        public static async void Listen(TcpClient client, Teams teams) {
            var ns = client.GetStream();
            var ep = (IPEndPoint)client.Client.RemoteEndPoint;

            byte[] buffer = new byte[16];

            Debug.WriteLine("Client {0}:{1} connected.", ep.Address.ToString(), ep.Port);
            Team team = null;

            try {

                int result = await ns.ReadAsync(buffer, 0, 16);
                while (result > 0) {

                    if (result != 16) {
                        Debug.WriteLine("Client {0}:{1} sent {2} bytes (expected 12).", ep.Address.ToString(), ep.Port, result);
                    } else {
                        Debug.WriteLine("Got query type {0} from client {1}:{2}.", Encoding.ASCII.GetString(buffer, 0, 4), ep.Address.ToString(), ep.Port);
                    }

                    int prefix = BitConverter.ToInt32(buffer, 0);
                    long id = BitConverter.ToInt64(buffer, 4);

                    if (prefix == 0x6e534256) { //VBSn
                        int namesize = BitConverter.ToInt32(buffer, 12);

                        if (namesize > 512) {
                            Debug.WriteLine("Client {0}:{1} sent {2} bytes long name, TERMINATING.", ep.Address.ToString(), ep.Port, namesize);
                            client.Client.Disconnect(false);
                            break;
                        }

                        byte[] nameBuffer = new byte[namesize];
                        result = await ns.ReadAsync(nameBuffer, 0, namesize);

                        if (result != namesize) {
                            Debug.WriteLine("Client {0}:{1} sent {2} bytes long name but declared {3}.", ep.Address.ToString(), ep.Port, result, namesize);
                            client.Client.Disconnect(false);
                            break;
                        }

                        string name = Encoding.Unicode.GetString(nameBuffer);

                        team = teams.CreateNewTeam(name);

                        byte[] bytes = new byte[2 * sizeof(long)];
                        Buffer.BlockCopy(new long[] { 0x00004b4f6e534256, team.Id }, 0, bytes, 0, bytes.Length);

                        await ns.WriteAsync(bytes, 0, bytes.Length);

                    } else if (prefix == 0x66534256) { //VBSf
                        int frameId = BitConverter.ToInt32(buffer, 12);

                        if (team == null) {
                            team = teams[id];
                            if (team == null) {
                                Debug.WriteLine("Client id {0} does not exit.", id.ToString());
                                client.Client.Disconnect(false);
                                break;
                            }
                        }
                        if (team.Id != id) {
                            Debug.WriteLine("Client id {0} does not match.", id.ToString());
                            client.Client.Disconnect(false);
                            break;
                        }

                        team.SubmitKeyframeId(frameId);

                        byte[] bytes = new byte[2 * sizeof(int)];
                        Buffer.BlockCopy(new int[] { 0x66534256, 0x00004b4f }, 0, bytes, 0, bytes.Length);

                        await ns.WriteAsync(bytes, 0, bytes.Length);

                    } else {
                        Debug.WriteLine(string.Format("Query type {0} does not exist.", Encoding.ASCII.GetString(buffer, 0, 4)));
                    }

                    result = await ns.ReadAsync(buffer, 0, 16);
                }

            } catch(System.IO.IOException) { }

            Debug.WriteLine("Client {0}:{1} disconnected.", ep.Address.ToString(), ep.Port);
        }

    }
}
