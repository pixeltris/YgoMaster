using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2CPP
{
    public class IL2String : IL2Base
    {
        public IL2String(IntPtr ptr) : base(ptr)
        {
            base.ptr = ptr;
        }
        unsafe public IL2String(string str) : base(IntPtr.Zero)
        {
            if (str == null)
            {
                ptr = IntPtr.Zero;
                return;
            }
            while (ptr == IntPtr.Zero || ToString() != str)
            {
                int length = str.Length;
                ptr = Import.Object.il2cpp_string_new(string.Empty.PadRight(length, '\u0001'));
                for (int i = 0; i < length; i++)
                {
                    *(char*)(ptr + 0x14 + (0x2 * i)) = str[i];
                }
            }
        }

        unsafe public override string ToString()
        {
            if (ptr == IntPtr.Zero)
                return null;

            return new string((char*)ptr.ToPointer() + 10);
        }
        private bool isStatic = false;
        private IntPtr handleStatic = IntPtr.Zero;
        public bool Static
        {
            get { return isStatic; }
            set
            {
                isStatic = value;
                if (value)
                {
                    if (handleStatic == IntPtr.Zero)
                    {
                        handleStatic = Import.Handler.il2cpp_gchandle_new(ptr, true);
                    }
                }
                else
                {
                    if (handleStatic != IntPtr.Zero)
                    {
                        Import.Handler.il2cpp_gchandle_free(handleStatic);
                        handleStatic = IntPtr.Zero;
                    }
                }
            }
        }

    }
}
