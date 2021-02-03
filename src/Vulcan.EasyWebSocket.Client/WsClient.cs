using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vulcan.EasyWebSocket.Client
{
    public class WsClient : IDisposable
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private readonly Uri _uri;
        private readonly Thread _receiveMessage;
        private readonly string _guid;
        private const string _shaKey = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        public WsClient(string url)
        {
            _uri = new Uri(url);
            _client = new TcpClient();
            _receiveMessage = new Thread(ReceiveMessage);
            _guid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        private async void Init()
        {
            await _client.ConnectAsync(_uri.Host, _uri.Port);
            _stream = _client.GetStream();
            Handshaking();
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

        private async void Handshaking()
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

            await _stream.WriteAsync(bufferRequest, 0, bufferRequest.Length);

            byte[] bufferResponse = new byte[_client.Available];

            await _stream.ReadAsync(bufferResponse, 0, bufferResponse.Length);

            string[] response = Encoding.UTF8.GetString(bufferResponse).Split(Environment.NewLine);

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("method", response[0]);

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

            if (!headers["sec-websocket-accept"].Equals(Convert.ToBase64String(hash)))
            {
                Console.WriteLine("erro");
            }

            _receiveMessage.Start();
        }

        private async void ReceiveMessage()
        {
            while (true)
            {
                byte[] buffer = new byte[4096];

                await _stream.ReadAsync(buffer, 0, buffer.Length);

                string teste = Encoding.ASCII.GetString(buffer);

                Console.WriteLine(teste);
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
            _client = null;
            _stream?.Dispose();
            _stream = null;
        }
    }
}
