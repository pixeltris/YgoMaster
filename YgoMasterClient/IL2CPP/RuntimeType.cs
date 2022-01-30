using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2CPP
{
    public class RuntimeType : IL2Base
    {
        public RuntimeType(IntPtr ptr) : base(ptr)
        {
            base.ptr = ptr;
        }

        public IntPtr MakeGenericType(Type[] types)
        {
            return MakeGenericType(types.Select(x => x.IL2Typeof()).ToArray());
        }
        public IntPtr MakeGenericType(IntPtr[] intPtrs)
        {
            return Import.Class.il2cpp_class_from_system_type(Instance_Class.GetMethod("MakeGenericType").Invoke(
                new IL2Class(ptr).IL2Typeof(),
                new IntPtr[]
                {
                    intPtrs.ArrayToIntPtr(Assembler.GetAssembly("mscorlib").GetClass("Type", "System"))
                }).ptr);
        }

        public static IL2Class Instance_Class = Assembler.GetAssembly("mscorlib").GetClass("RuntimeType", "System");
    }
}
