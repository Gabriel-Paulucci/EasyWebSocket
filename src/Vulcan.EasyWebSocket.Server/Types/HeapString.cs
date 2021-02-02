using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Vulcan.EasyWebSocket.Server.Types
{
    public unsafe struct HeapString
    {
        private char* _value;
        private int _length;

        public void Set(string value)
        {
            fixed (char* span = value)
            {
                _value = (char*)Marshal.AllocHGlobal(sizeof(char) * value.Length).ToPointer();
                _length = value.Length;
                for (int i = 0; i < _length; i++)
                {
                    _value[i] = span[i];
                }
            }
        }

        public string Get()
        {
            char[] value = new char[_length];
            for (int i = 0; i < _length; i++)
            {
                value[0] = _value[0];
            }

            return value.ToString();
        }

        public Span<HeapString> Split()
        {
            Span<HeapString> span = 
        }

        public void Dispose()
        {
            _value = (char*)IntPtr.Zero;
        }
    }
}
