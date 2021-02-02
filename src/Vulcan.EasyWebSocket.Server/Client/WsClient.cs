using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Vulcan.EasyWebSocket.Server.Client
{
    public class WsClient
    {
        private readonly TcpClient _client;

        public WsClient(TcpClient client)
        {
            _client = client;
        }
    }
}
