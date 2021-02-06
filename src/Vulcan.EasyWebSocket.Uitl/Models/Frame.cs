using System;
using System.Collections.Generic;
using System.Text;

namespace Vulcan.EasyWebSocket.Uitl.Models
{
    public class Frame
    {
        public bool Fin = false;
        public bool Rsv1 = false;
        public bool Rsv2 = false;
        public bool Rsv3 = false;
        public byte OpCode;
        public bool Mask = false;
        public byte Length;
        public byte[] Payload;
        public byte[] MaskKey;
    }
}
