using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Vulcan.EasyWebSocket.Server.Types;

namespace Vulcan.EasyWebSocket.Server.Tcp
{
    public class TcpServer
    {
        private readonly TcpListener _tcpListener;
        private readonly Thread _receivedConnection;
        private readonly WsServer _server;

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

                ReceiveHandshaking(client);

                _tcpListener.BeginAcceptTcpClient(callback, ar.AsyncState);
            }

            _tcpListener.BeginAcceptTcpClient(callback, _tcpListener);
        }

        public async void ReceiveHandshaking(TcpClient client)
        {
            NetworkStream networkStream = client.GetStream();
            byte[] buffer = new byte[client.Available];
            await networkStream.ReadAsync(buffer, 0, buffer.Length);

            ReadHeaders();

            void ReadHeaders()
            {
                HeapString bufferRequest = new HeapString();

                bufferRequest.Set(Encoding.ASCII.GetString(buffer));

                Span<HeapString> request = stackalloc HeapString[0];

                Span<Header> headers = stackalloc Header[request.Length - 2];

                headers[0].Set("Method", request[0]);

                for (int i = 1; i < request.Length; i++)
                {
                    if (request[i].Contains(':'))
                    {
                        Span<string> value = request[i].Split(':');

                        headers[i].Set(value[0], value[1]);
                    }
                }
            }
        }
    }
}
