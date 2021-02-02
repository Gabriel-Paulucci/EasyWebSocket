using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Vulcan.EasyWebSocket.Server.Client;
using Vulcan.EasyWebSocket.Server.Tcp;

namespace Vulcan.EasyWebSocket.Server
{
    public class WsServer
    {
        private readonly TcpServer _tcpServer;
        private readonly IPEndPoint _endPoint;
        private List<WsClient> clients;

        public WsServer(string ip, int port)
        {
            _endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            _tcpServer = new TcpServer(this, _endPoint);
        }

        public void Start()
        {
            _tcpServer.Start();
        }

        public async Task StartAsync()
        {
            _tcpServer.Start();
            await Task.Delay(-1);
        }

        public void OnClientConnect(WsClient client)
        {
            clients.Add(client);
        }
    }
}
