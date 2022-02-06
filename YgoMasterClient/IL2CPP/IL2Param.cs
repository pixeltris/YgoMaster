using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2CPP
{
    public class IL2Param : IL2Base
    {
        public string Name { get; private set; }
        internal IL2Param(IntPtr ptr, string name) : base(ptr)
        {
            base.ptr = ptr;

            Name = name;
        }
        public IL2ClassType Type { get { return new IL2ClassType(ptr); } }
        //public IL2ClassType ReturnType { get { return new IL2ClassType(ptr); } }
    }
}
