using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Vulcan.EasyWebSocket.Uitl.Interfaces;
using Vulcan.EasyWebSocket.Uitl.Models;
using Vulcan.EasyWebSocket.Uitl.Stream;

namespace Vulcan.EasyWebSocket.Client
{
    public class WsClient : WsClientBase, IDisposable
    {
        private NetworkStream _stream;
        private readonly Uri _uri;
        private readonly string _guid;
        private const string _shaKey = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        private readonly Receive _receive;

        public WsClient(string url)
        {
            Client = new TcpClient();
            _uri = new Uri(url);
            _guid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            _receive = new Receive(this);
        }

        private async void Init()
        {
            await Client.ConnectAsync(_uri.Host, _uri.Port);
            _stream = Client.GetStream();
            Handshaking();
            _receive.ReceivePackege += ReceivePackege;
        }

        private void ReceivePackege(object sender, Package e)
        {
            e.Process();
            Console.WriteLine(Encoding.UTF8.GetString(e.Data));
        }

        private void Handshaking()
        {
            string request = $"GET {_uri.PathAndQuery} HTTP/1.1" + Environment.NewLine;
            request += $"Host: {_uri.Host}:{_uri.Port}" + Environment.NewLine;
            request += "Accept: */*" + Environment.NewLine;
            request += "User-Agent: EasyWebSocket/Client/dev_0.1" + Environment.NewLine;
            request += $"Sec-WebSocket-Key: {_guid}" + Environment.NewLine;
            request += "Sec-WebSocket-Version: 13" + Environment.NewLine;
            request += "Connection: Upgrade" + Environment.NewLine;
            request += "Upgrade: websocket" + Environment.NewLine + Environment.NewLine;

            Console.WriteLine(request);

            byte[] bufferRequest = Encoding.UTF8.GetBytes(request);

            _stream.BeginWrite(bufferRequest, 0, bufferRequest.Length, (ar) =>
            {
                _stream.EndWrite(ar);
            }, null);

            byte[] bufferResponse = new byte[Client.Available];

            _stream.BeginRead(bufferResponse, 0, bufferResponse.Length, (ar) =>
            {
                _stream.EndRead(ar);
            }, null);

            string[] response = Encoding.UTF8.GetString(bufferResponse).Split(Environment.NewLine);

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("method", response[0]);

            Console.WriteLine(response[0]);

            for (int i = 1; i < response.Length - 2; i++)
            {
                if (response[i].Contains(':'))
                {
                    Console.WriteLine(response[i]);

                    string[] values = response[i].Split(':');

                    headers.Add(values[0].ToLower(), values[1].TrimStart());
                }
            }

            byte[] hash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(_guid + _shaKey));

            if (headers["sec-websocket-accept"].Equals(Convert.ToBase64String(hash)))
            {
                _receive.Start();
            }
        }

        public void Start()
        {
            Init();
        }

        public async Task StartAsync()
        {
            Init();
            await Task.Delay(-1);
        }

        public void Dispose()
        {
            Client?.Dispose();
            Client = null;
            _stream?.Dispose();
            _stream = null;
        }
    }
}
