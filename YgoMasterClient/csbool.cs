using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace YgoMasterClient
{
    /// <summary>
    /// Used for bool interop between C# and IL2CPP
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct csbool
    {
        private byte val;
        public bool Value
        {
            get { return val != 0; }
            set { val = (byte)(value ? 1 : 0); }
        }

        public csbool(byte value)
        {
            val = (byte)(value == 0 ? 0 : 1);
        }

        public csbool(bool value)
        {
            val = (byte)(value ? 1 : 0);
        }

        public static implicit operator csbool(bool value)
        {
            return new csbool(value);
        }

        public static implicit operator bool(csbool value)
        {
            return value.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
