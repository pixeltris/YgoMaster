using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IL2CPP;
using System.Runtime.InteropServices;

// https://github.com/BlazeBest/BlazeEngine-IL2CPP/blob/0a0d86a3ce878c08cf847d1ae4b8da6f01b00409/BE4v/SDK/UnityEngine.CoreModule/Events/UnityAction.cs

namespace UnityEngine.Events
{
    public delegate void UnityAction();
    public delegate void UnityAction<T0>(T0 arg0);
    public delegate void UnityAction<T0, T1>(T0 arg0, T1 arg1);
    public delegate void UnityAction<T0, T1, T2>(T0 arg0, T1 arg1, T2 arg2);
    public delegate void UnityAction<T0, T1, T2, T3>(T0 arg0, T1 arg1, T2 arg2, T3 arg3);

    public static class _UnityAction
    {
        /// <summary>
        /// Returns il2cpp UnityAction
        /// </summary>
        public unsafe static IntPtr CreateUnityAction(Delegate function)
        {
            return UnityEngine.Events._UnityAction.CreateDelegate(function, IntPtr.Zero, UnityEngine.Events._UnityAction.Instance_Class);
        }

        /// <summary>
        /// Returns il2cpp Action
        /// </summary>
        public unsafe static IntPtr CreateAction(Delegate function)
        {
            return UnityEngine.Events._UnityAction.CreateDelegate(function, IntPtr.Zero, UnityEngine.Events._UnityAction.ActionClass);
        }

        // NOTE: Untested
        public unsafe static IntPtr CreateAction<T>(Delegate function)
        {
            IL2Class actionClass = typeof(Action<>).GetClass().MakeGenericType(new Type[] { typeof(T) });
            return UnityEngine.Events._UnityAction.CreateDelegate(function, IntPtr.Zero, actionClass);
        }

        public unsafe static IntPtr CreateDelegate<T>(Delegate function, T instance, IL2Class klass = null)
        {
            if (function == null)
            {
                return IntPtr.Zero;
            }
            if (klass == null)
            {
                klass = Instance_Class;
            }

            var obj = Import.Object.il2cpp_object_new(klass.ptr);
            if (instance == null || (typeof(T) == typeof(IntPtr) && (IntPtr)(object)instance == IntPtr.Zero))
            {
                var runtimeStaticMethod = Marshal.AllocHGlobal(8);
                *(IntPtr*)runtimeStaticMethod = function.Method.MethodHandle.GetFunctionPointer();
                *((IntPtr*)obj + 2) = function.Method.MethodHandle.GetFunctionPointer();
                *((IntPtr*)obj + 4) = IntPtr.Zero;
                *((IntPtr*)obj + 5) = runtimeStaticMethod;
                return obj;
            }

            IL2Method ctor = klass.GetMethod(".ctor");
            GCHandle handle1 = GCHandle.Alloc(instance);
            var runtimeMethod = Marshal.AllocHGlobal(80);
            //methodPtr
            *((IntPtr*)runtimeMethod) = function.Method.MethodHandle.GetFunctionPointer();
            byte paramCount = 0;
            var paramInfo = function.Method.GetParameters();
            if (paramInfo != null && paramInfo.Length > 0)
            {
                paramCount = (byte)paramInfo.Length;
            }
            //Parameter count
            *((byte*)runtimeMethod + 75) = paramCount; // 0 parameter_count

            //Slot (65535)
            *((byte*)runtimeMethod + 74) = 0xFF;
            *((byte*)runtimeMethod + 73) = 0xFF;

            *((IntPtr*)obj + 2) = function.Method.MethodHandle.GetFunctionPointer();
            *((IntPtr*)obj + 4) = obj;
            *((IntPtr*)obj + 5) = runtimeMethod;
            *((IntPtr*)obj + 7) = GCHandle.ToIntPtr(handle1);

            return obj;
        }

        public static IL2Class Instance_Class = Assembler.GetAssembly("UnityEngine.CoreModule").GetClass("UnityAction", "UnityEngine.Events");
        public static IL2Class ActionClass = typeof(Action).GetClass();
    }
}

/*namespace YgoMasterClient
{
    static unsafe class DelegateHelper
    {
        class DelegateInfo
        {
            public IL2Class Class;
            public IL2Method Ctor;
            //public IL2Method 
        }

        [StructLayout(LayoutKind.Sequential)]
        struct Il2CppMethodInfo
        {
            public IntPtr methodPointer;
            public IntPtr virtualMethodPointer;
            public IntPtr invoker_method;
            public IntPtr name; // const char*
            public IntPtr klass;
            public IntPtr return_type;
            public IntPtr parameters;
            public IntPtr someRtData;
            public IntPtr someGenericData;
            public uint token;
            public ushort flags;
            public ushort iflags;
            public ushort slot;
            public byte parameters_count;
            public byte extra_flags;
        }

        static Dictionary<IntPtr, IL2Method> invokers = new Dictionary<IntPtr, IL2Method>();

        static IL2Field fieldMethodPtr;
        static IL2Field fieldMethodInfo;
        static IL2Field fieldMethod;

        static DelegateHelper()
        {
            IL2Class delegateClassInfo = typeof(Delegate).GetClass();
            fieldMethodPtr = delegateClassInfo.GetField("method_ptr");
            fieldMethodInfo = delegateClassInfo.GetField("method_info");
            fieldMethod = delegateClassInfo.GetField("method");
        }

        public static IntPtr DelegateIl2cppDelegate(Delegate d, IL2Class klass)
        {
            IntPtr ptr = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(d);
            IntPtr obj = Import.Object.il2cpp_object_new(klass.ptr);
            IL2Method invoke;
            if (!invokers.TryGetValue(klass.ptr, out invoke))
            {
                invoke = invokers[klass.ptr] = klass.GetMethod("Invoke");
            }

            Il2CppMethodInfo* p = (Il2CppMethodInfo*)Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Il2CppMethodInfo)));
            *p = default(Il2CppMethodInfo);
            p->methodPointer = ptr;
            p->parameters_count = (byte)d.Method.GetParameters().Length;
            p->slot = ushort.MaxValue;

            fieldMethodPtr.SetValue(obj, ptr);
            fieldMethodInfo.SetValue(obj, invoke.ptr);
            fieldMethod.SetValue(obj, (IntPtr)p);

            //Import.Object.il2cpp_object_get_class
            return obj;
        }
    }
}
*/