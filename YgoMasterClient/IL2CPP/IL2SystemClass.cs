using System;
using System.Runtime.InteropServices;

namespace IL2CPP
{
    public static class IL2SystemClass
    {
        public static IL2Class Byte = Assembler.GetAssembly("mscorlib").GetClass(typeof(byte).Name, typeof(byte).Namespace);
        public static IL2Class Int32 = Assembler.GetAssembly("mscorlib").GetClass(typeof(int).Name, typeof(int).Namespace);
        public static IL2Class String = Assembler.GetAssembly("mscorlib").GetClass(typeof(string).Name, typeof(string).Namespace);
    }
}
