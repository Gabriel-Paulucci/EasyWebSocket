using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vulcan.EasyWebSocket.Client
{
    public class WebSocket : IDisposable, IAsyncDisposable
    {
        private TcpClient client;
        private NetworkStream stream;
        private Thread receiveThread;
        public Uri Uri { get; }

        public WebSocket(string url)
        {
            client = new TcpClient();
            Uri = new Uri(url);
        }

        public async Task Open()
        {
            await client.ConnectAsync(Uri.Host, Uri.Port);
            stream = client.GetStream();
            SendHeaders();
            ReceiveThread();
        }

        private void SendHeaders()
        {
            string header = $"GET {Uri.PathAndQuery} HTTP/1.1\r\n";
            header += $"Host: {Uri.Host}:{Uri.Port}\r\n";
            header += "Accept: */*\r\n";
            header += "User-Agent: EasyWebSocket/dev_0.1\r\n";
            header += $"Sec-WebSocket-Key: {Convert.ToBase64String(Guid.NewGuid().ToByteArray())}\r\n";
            header += "Connection: Upgrade\r\n";
            header += "Sec-WebSocket-Version: 13\r\n";
            header += "Upgrade: websocket\r\n\r\n";

            stream.Write(Encoding.ASCII.GetBytes(header));
        }

        private void ReceiveThread()
        {
            receiveThread = new Thread(async () =>
            {
                while (true)
                {
                    byte[] buffer = new byte[4096];

                    await stream.ReadAsync(buffer, 0, buffer.Length);

                    string teste = Encoding.ASCII.GetString(buffer);

                    Console.WriteLine(teste);
                }
            });
            receiveThread.Start();
        }

        public void Dispose()
        {
            client?.Dispose();
            client = null;
            stream?.Dispose();
            stream = null;
        }

        public ValueTask DisposeAsync()
        {
            client?.Dispose();
            client = null;
            if (stream != null)
            {
                ValueTask valueTask = stream.DisposeAsync();
                stream = null;
                return valueTask;
            }
            return new ValueTask();
        }
    }
}
