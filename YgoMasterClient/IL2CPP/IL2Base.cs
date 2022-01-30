//using SharpDisasm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2CPP
{
    public class IL2Base
    {
        public IntPtr ptr { get; set; }
        public IL2Base(IntPtr ptr)
        {
            this.ptr = ptr;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return this == null;
            IL2Base b = obj as IL2Base;
            if (b != null)
            {
                return b.ptr == ptr;
            }
            return false;
        }
        public T MonoCast<T>()
        {
            return ptr.MonoCast<T>();
        }
        public override int GetHashCode()
        {
            return ptr.GetHashCode();
        }
        public static bool operator !=(IL2Base x, IL2Base y)
        {
            return !(x == y);
        }
        public static bool operator ==(IL2Base x, IL2Base y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
            return x.ptr == y.ptr;
        }
        /*unsafe public Disassembler GetDisassembler(int @size = 0x1000)
        {
            return new Disassembler(*(IntPtr*)ptr, @size, ArchitectureMode.x86_64, unchecked((ulong)(*(IntPtr*)ptr).ToInt64()), true, Vendor.Intel);
        }*/
    }
}
