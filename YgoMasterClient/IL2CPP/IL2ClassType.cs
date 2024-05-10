using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace IL2CPP
{
    public class IL2ClassType : IL2Base
    {
        internal IL2ClassType(IntPtr ptr) : base(ptr)
        {
            base.ptr = ptr;
        }

        public string Name
        {
            get { return Marshal.PtrToStringAnsi(Import.Object.il2cpp_type_get_name(ptr)); }
        }
    }
}
