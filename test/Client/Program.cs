using System;
using System.Threading.Tasks;
using Vulcan.EasyWebSocket.Client;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            WsClient webSocket = new WsClient("ws://127.0.0.1:3000/ws/");
            await webSocket.StartAsync();
        }
    }
}
