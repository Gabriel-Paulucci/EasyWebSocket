using System;
using System.Net;
using System.Threading.Tasks;
using Vulcan.EasyWebSocket.Server;

namespace Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            WsServer server = new WsServer("127.0.0.1", 3000);
            await server.StartAsync();
        }
    }
}
