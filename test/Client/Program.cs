using System;
using System.Threading.Tasks;
using Vulcan.EasyWebSocket.Client;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            WebSocket webSocket = new WebSocket("ws://127.0.0.1:3000/ws/");
            await webSocket.Open();

            await Task.Delay(-1);
        }
    }
}
