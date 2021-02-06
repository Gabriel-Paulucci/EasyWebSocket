using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Vulcan.EasyWebSocket.Uitl.Models;

namespace Vulcan.EasyWebSocket.Uitl.Interfaces
{
    public abstract class WsClientBase
    {
        public TcpClient Client { get; set; }
    }
}
