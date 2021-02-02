using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Vulcan.EasyWebSocket.Server.Types;

namespace Vulcan.EasyWebSocket.Server.Tcp
{
    public unsafe struct Header
    {
        public HeapString Key;
        public HeapString Value;

        public void Dispose()
        {
            Key.Dispose();
            Value.Dispose();
        }

        public void Set(string key, string value)
        {
            Key.Set(key);
            Value.Set(value);
        }
    }
}
