using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Vulcan.EasyWebSocket.Uitl.Interfaces;
using Vulcan.EasyWebSocket.Uitl.Models;

namespace Vulcan.EasyWebSocket.Uitl.Stream
{
    public class Receive
    {
        private readonly WsClientBase _client;
        private readonly Thread _receiveMessage;
        private NetworkStream _stream;

        public event EventHandler<Package> ReceivePackege;

        public Receive(WsClientBase client)
        {
            _client = client;
            _receiveMessage = new Thread(Worker);
        }

        public void Start()
        {
            _stream = _client.Client.GetStream();
            _receiveMessage.Start();
        }

        private void Worker()
        {
            byte[] buffer = new byte[2];
            Package package = new Package();

            void callback(IAsyncResult ar)
            {
                _stream.EndRead(ar);

                Frame frame = new Frame
                {
                    Fin = (buffer[0] & 1 << 7) > 0,
                    Rsv1 = (buffer[0] & 1 << 6) > 0,
                    Rsv2 = (buffer[0] & 1 << 5) > 0,
                    Rsv3 = (buffer[0] & 1 << 4) > 0,
                    OpCode = (byte)(buffer[0] & 15),
                    Mask = (buffer[1] & 1 << 7) > 0,
                    Length = (byte)(buffer[1] & 127)
                };

                if (frame.Length <= 125)
                {
                    frame.Payload = new byte[frame.Length];

                    void getPayload(IAsyncResult ar)
                    {
                        _stream.EndRead(ar);
                    }

                    _stream.BeginRead(frame.Payload, 0, frame.Payload.Length, getPayload, ar.AsyncState);
                }
                else if (frame.Length == 126)
                {

                }
                else
                {

                }

                if (frame.Mask)
                {
                    frame.MaskKey = new byte[4];

                    void getPayload(IAsyncResult ar)
                    {
                        _stream.EndRead(ar);
                    }

                    _stream.BeginRead(frame.Payload, 0, frame.Payload.Length, getPayload, ar.AsyncState);
                }

                package.AddFrame(frame);

                if (frame.Fin)
                {
                    ReceivePackege?.Invoke(this, package);
                    package = new Package();
                }

                _stream.BeginRead(buffer, 0, buffer.Length, callback, ar.AsyncState);
            }

            _stream.BeginRead(buffer, 0, buffer.Length, callback, _stream);
        }
    }
}
