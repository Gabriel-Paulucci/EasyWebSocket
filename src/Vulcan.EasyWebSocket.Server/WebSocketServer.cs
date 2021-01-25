using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Vulcan.EasyWebSocket.Server
{
    public class WebSocketServer
    {
        private TcpListener listener;

        public WebSocketServer(string ip, int port)
        {
            listener = new TcpListener(IPAddress.Parse(ip), port);
        }

        public void Start()
        {
            listener.Start();
            listener.BeginAcceptTcpClient(Callback, listener);
        }

        public void Callback(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(ar);
            listener.BeginAcceptTcpClient(Callback, ar);
        }
    }
}
