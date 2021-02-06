using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Vulcan.EasyWebSocket.Uitl.Models
{
    public class Package
    {
        private readonly Collection<Frame> _frames;
        private int Length;

        public byte[] Data { get; private set; }

        public Package()
        {
            _frames = new Collection<Frame>();
        }

        public void AddFrame(Frame frame)
        {
            _frames.Add(frame);
            Length += frame.Payload.Length;
        }

        public void Process()
        {
            Data = new byte[Length];
            int index = 0;

            for (int i = 0; i < _frames.Count; i++)
            {
                Array.Copy(_frames[i].Payload, 0, Data, 0, _frames[i].Length);
                index += _frames[i].Length;
            }
        }
    }
}
