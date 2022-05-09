using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;

namespace IL2CPP
{
    public class IL2Method : IL2Base
    {
        internal IL2Method(IntPtr ptr)
            : base(ptr)
        {
            base.ptr = ptr;
        }

        private string szName;
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(szName))
                    szName = Marshal.PtrToStringAnsi(Import.Method.il2cpp_method_get_name(ptr));
                return szName;
            }
            set
            {
                szName = value;
            }
        }

        public int Token { get { return Import.Method.il2cpp_method_get_token(ptr); } }
        public bool IsAbstract { get { return HasFlag(IL2BindingFlags.METHOD_ABSTRACT); } }
        public bool IsStatic { get { return HasFlag(IL2BindingFlags.METHOD_STATIC); } }
        public bool IsPrivate { get { return HasFlag(IL2BindingFlags.METHOD_PRIVATE); } }
        public bool IsPublic { get { return HasFlag(IL2BindingFlags.METHOD_PUBLIC); } }

        public bool Instance { get { return IsStatic && GetParameters().Length == 0 && ReturnType.Name == ReflectedType.FullName; } }

        public IL2Class ReflectedType { get { return new IL2Class(Import.Method.il2cpp_method_get_class(ptr)); } }

        public IL2ClassType ReturnType { get { return new IL2ClassType(Import.Method.il2cpp_method_get_return_type(ptr)); } }

        public IL2Param[] GetParameters()
        {
            if (Parameters == null)
            {
                Parameters = new List<IL2Param>();
                uint param_count = Import.Method.il2cpp_method_get_param_count(ptr);
                for (uint i = 0; i < param_count; i++)
                    Parameters.Add(new IL2Param(Import.Method.il2cpp_method_get_param(ptr, i), Marshal.PtrToStringAnsi(Import.Method.il2cpp_method_get_param_name(ptr, i))));
            }

            return Parameters.ToArray();
        }
        private List<IL2Param> Parameters = null;

        public string GetSignature()
        {
            StringBuilder result = new StringBuilder();
            if (IsPublic)
            {
                result.Append("public ");
            }
            if (IsStatic)
            {
                result.Append("static ");
            }
            result.Append(ReturnType.Name + " " + Name + "(" + string.Join(", ", GetParameters().Select(x => x.Type.Name + " " + x.Name)) + ")");
            return result.ToString();
        }

        public bool HasAttribute(IL2Class klass)
        {
            if (klass == null) return false;
            return Import.Method.il2cpp_method_has_attribute(ptr, klass.ptr);
        }

        public IL2BindingFlags Flags
        {
            get
            {
                uint f = 0;
                return (IL2BindingFlags)Import.Method.il2cpp_method_get_flags(ptr, ref f);
            }
        }

        public bool HasFlag(IL2BindingFlags flag) { return ((Flags & flag) != 0); }

        public IL2Object Invoke()
        {
            return Invoke(IntPtr.Zero, new IntPtr[] { IntPtr.Zero });
        }
        public IL2Object Invoke(IntPtr obj, bool isVirtual = false, bool ex = true)
        {
            return Invoke(obj, new IntPtr[] { IntPtr.Zero }, isVirtual: isVirtual, ex: ex);
        }
        public IL2Object Invoke(params IntPtr[] paramtbl)
        {
            return Invoke(IntPtr.Zero, paramtbl);
        }
        public IL2Object Invoke(IL2Object obj)
        {
            return Invoke(obj.ptr, new IntPtr[] { IntPtr.Zero });
        }
        public IL2Object Invoke(params IL2Object[] paramtbl)
        {
            return Invoke(IntPtr.Zero, NativeUtils.IL2ObjecToIntPtr(paramtbl));
        }
        public IL2Object Invoke(IntPtr obj, IL2Object[] paramtbl, bool isVirtual = false, bool ex = true)
        {
            return Invoke(obj, NativeUtils.IL2ObjecToIntPtr(paramtbl), isVirtual, ex);
        }
        public IL2Object Invoke(IL2Object obj, IntPtr[] paramtbl, bool isVirtual = false, bool ex = true)
        {
            return Invoke(obj.ptr, paramtbl, isVirtual, ex);
        }
        public IL2Object Invoke(IntPtr obj, IntPtr[] paramtbl, bool isVirtual = false, bool ex = true)
        {
            IntPtr returnval = InvokeMethod(ptr, obj, paramtbl, isVirtual, ex);
            if (returnval != IntPtr.Zero)
                return new IL2Object(returnval);
            return null;
        }

        unsafe public static IntPtr InvokeMethod(IntPtr method, IntPtr obj, IntPtr[] paramtbl, bool isVirtual = false, bool ex = true)
        {
            if (method == IntPtr.Zero)
                return IntPtr.Zero;
            IntPtr[] intPtrArray;
            IntPtr returnval = IntPtr.Zero;
            intPtrArray = ((paramtbl != null) ? paramtbl : new IntPtr[0]);
            IntPtr intPtr = Marshal.AllocHGlobal(intPtrArray.Length * sizeof(void*));
            try
            {
                void** pointerArray = (void**)intPtr.ToPointer();
                for (int i = 0; i < intPtrArray.Length; i++)
                    pointerArray[i] = intPtrArray[i].ToPointer();

                IntPtr @m = isVirtual ? Import.Method.il2cpp_object_get_virtual_method(obj, method) : method;

                IntPtr err = IntPtr.Zero;
                returnval = Import.Method.il2cpp_runtime_invoke(@m, obj, pointerArray, new IntPtr(&err));
                if (err != IntPtr.Zero && ex)
                {
                    Console.WriteLine("Error: " + NativeUtils.BuildMessage(err));
                    Console.WriteLine("Src: " + new IL2Method(@m).Name);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(intPtr);
            }
            return returnval;
        }

        public IL2Method MakeGenericMethod(Type[] types)
        {
            return MakeGenericMethod(types.Select(x => x.IL2Typeof()).ToArray());
        }
        public IL2Method MakeGenericMethod(IntPtr[] intPtrs)
        {
            if (intPtrs == null || intPtrs.Length == 0)
            {
                return null;
            }
            IntPtr intPtrArrayPtr = intPtrs.ArrayToIntPtr(Assembler.GetAssembly("mscorlib").GetClass("Type", "System"));
            if (intPtrArrayPtr == IntPtr.Zero)
            {
                return null;
            }
            IntPtr monoMethodInfo = Import.Method.il2cpp_method_get_object(ptr, this.ReflectedType.ptr);
            if (monoMethodInfo == IntPtr.Zero)
            {
                return null;
            }
            IL2Method makeGenericMethodMethod = monoMethodClassInfo.GetMethod("MakeGenericMethod");
            if (makeGenericMethodMethod == null)
            {
                return null;
            }
            IL2Object methodInfo = makeGenericMethodMethod.Invoke(monoMethodInfo, new IntPtr[] { intPtrArrayPtr });
            if (methodInfo == null)
            {
                return null;
            }
            IntPtr result = Import.Method.il2cpp_method_get_from_reflection(methodInfo.ptr);
            if (result == IntPtr.Zero)
            {
                return null;
            }
            return new IL2Method(result);
        }

        static IL2Class monoMethodClassInfo = Assembler.GetAssembly("mscorlib").GetClass("MonoMethod", "System.Reflection");
    }
}
