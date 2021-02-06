using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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

            byte[] bufferResponse = new byte[_client.Available];

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
                _receiveMessage.Start();
            }
        }

        private void ReceiveMessage()
        {
            byte[] buffer = new byte[2];
            bool fin = false;
            bool rsv1 = false;
            bool rsv2 = false;
            bool rsv3 = false;
            byte opcode;
            bool mask = false;
            byte length;
            byte[] payload = null;

            void callback(IAsyncResult ar)
            {
                _stream.EndRead(ar);

                fin = (buffer[0] & 1 << 7) > 0;
                rsv1 = (buffer[0] & 1 << 6) > 0;
                rsv2 = (buffer[0] & 1 << 5) > 0;
                rsv3 = (buffer[0] & 1 << 4) > 0;
                opcode = (byte)(buffer[0] & 15);
                mask = (buffer[1] & 1 << 7) > 0;
                length = (byte)(buffer[1] & 127);

                if (length <= 125)
                {
                    payload = new byte[length];

                    void getPayload(IAsyncResult ar)
                    {
                        _stream.EndRead(ar);
                    }

                    _stream.BeginRead(payload, 0, payload.Length, getPayload, ar.AsyncState);
                }
                else if (length == 126)
                {

                }
                else
                {

                }

                Console.WriteLine(Encoding.UTF8.GetString(payload));

                _stream.BeginRead(buffer, 0, buffer.Length, callback, ar.AsyncState);
            }

            void reset()
            {
                fin = false;
                rsv1 = false;
                rsv2 = false;
                rsv3 = false;
                opcode = 0;
            }

            _stream.BeginRead(buffer, 0, buffer.Length, callback, _stream);
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
