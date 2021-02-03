using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Vulcan.EasyWebSocket.Server.Client;

namespace Vulcan.EasyWebSocket.Server.Tcp
{
    public class TcpServer
    {
        private readonly TcpListener _tcpListener;
        private readonly Thread _receivedConnection;
        private readonly WsServer _server;

        private const string _shaKey = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        public TcpServer(WsServer server, IPEndPoint endPoint)
        {
            _server = server;
            _tcpListener = new TcpListener(endPoint);
            _receivedConnection = new Thread(ReceivedConnection);
        }

        public void Start()
        {
            _tcpListener.Start();
            _receivedConnection.Start();
        }

        public void ReceivedConnection()
        {
            void callback(IAsyncResult ar)
            {
                TcpClient client = _tcpListener.EndAcceptTcpClient(ar);

                Handshaking(client);

                _tcpListener.BeginAcceptTcpClient(callback, ar.AsyncState);
            }

            _tcpListener.BeginAcceptTcpClient(callback, _tcpListener);
        }

        public async void Handshaking(TcpClient client)
        {
            NetworkStream networkStream = client.GetStream();
            byte[] bufferReceive = new byte[client.Available];
            await networkStream.ReadAsync(bufferReceive, 0, bufferReceive.Length);

            string[] request = Encoding.ASCII.GetString(bufferReceive).Split(Environment.NewLine);

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("method", request[0]);

            for (int i = 1; i < request.Length - 2; i++)
            {
                if (request[i].Contains(':'))
                {
                    Console.WriteLine(request[i]);

                    string[] values = request[i].Split(':');

                    headers.Add(values[0].ToLower(), values[1].TrimStart());
                }
            }

            if (headers["method"].StartsWith("GET"))
            {
                string response = "HTTP/1.1 101 Switching Protocols" + Environment.NewLine;
                response += "Connection: Upgrade" + Environment.NewLine;
                response += "Upgrade: websocket" + Environment.NewLine;

                byte[] hash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(headers["Sec-WebSocket-Key"] + _shaKey));
                response += "Sec-WebSocket-Accept: " + Convert.ToBase64String(hash);

                response += Environment.NewLine + Environment.NewLine;

                Console.WriteLine();
                Console.WriteLine(response);

                byte[] bufferSend = Encoding.UTF8.GetBytes(response);

                await networkStream.WriteAsync(bufferSend, 0, bufferSend.Length);
            }

            WsClient wsClient = new WsClient(client);
            _server.OnClientConnect(wsClient);
        }
    }
}
