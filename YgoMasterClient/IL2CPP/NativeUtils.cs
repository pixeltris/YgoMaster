using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;

namespace IL2CPP
{
    public static class NativeUtils
    {
        unsafe public static IntPtr ArrayToIntPtr(this IEnumerable<IntPtr> array, IL2Class typeobject = null)
        {
            return ArrayToIntPtr(array.ToArray(), typeobject);
        }
    
        unsafe public static IntPtr ArrayToIntPtr(this IntPtr[] array, IL2Class typeobject = null)
        {
            if (typeobject == null)
                typeobject = Assembler.GetAssembly("mscorlib").GetClass("Object", "System");

            int length = array.Count();
            IntPtr result = Import.Object.il2cpp_array_new(typeobject.ptr, length);
            if (typeobject == IL2SystemClass.Byte)
            {
                for (int i = 0; i < length; i++)
                {
                    *(IntPtr*)((IntPtr)((long*)result + 4) + i * sizeof(byte)) = array[i];
                }
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    *(IntPtr*)((IntPtr)((long*)result + 4) + i * IntPtr.Size) = array[i];
                }
            }
            return result;
        }

        public static T[] IntPtrToStructureArray<T>(IntPtr ptr, uint len)
        {
            IntPtr iter = ptr;
            T[] array = new T[len];
            for (uint i = 0; i < len; i++)
            {
                array[i] = (T)Marshal.PtrToStructure(iter, typeof(T));
                iter += Marshal.SizeOf(typeof(T));
            }
            return array;
        }
        public static IntPtr[] IL2ObjecToIntPtr(IL2Object[] objtbl)
        {
            return objtbl.Select(x => x.ptr).ToArray();
        }
        public static bool IsUnmanaged(this Type type)
        {
            // primitive, pointer or enum -> true
            if (type.IsPrimitive || type.IsPointer || type.IsEnum)
                return true;

            // not a struct -> false
            if (!type.IsValueType)
                return false;

            // otherwise check recursively
            return type
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .All(f => IsUnmanaged(f.FieldType));
        }

        // https://github.com/pixeltris/Lotd/blob/36c8a54d4fa58345974957c0bb061b2878ee9353/Lotd/NativeScript/MemTools.PInvoke.cs#L762
        public static byte[] StructToByteArray<T>(T value) where T : struct
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            StructToByteArray(value, buffer, 0);
            return buffer;
        }
        public static void StructToByteArray<T>(T value, byte[] buffer, int index) where T : struct
        {
            int structSize = Marshal.SizeOf(typeof(T));
            IntPtr ptr = Marshal.AllocHGlobal(structSize);
            Marshal.StructureToPtr(value, ptr, false);
            Marshal.Copy(ptr, buffer, index, structSize);
            Marshal.FreeHGlobal(ptr);
        }
        public static T[] StructsFromByteArray<T>(byte[] buffer) where T : struct
        {
            int structSize = Marshal.SizeOf(typeof(T));
            Debug.Assert(buffer.Length % buffer.Length == 0);

            T[] result = new T[buffer.Length / structSize];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = StructFromByteArray<T>(buffer, i * structSize);
            }
            return result;
        }
        public static T StructFromByteArray<T>(byte[] value) where T : struct
        {
            return StructFromByteArray<T>(value, 0);
        }
        private static T StructFromByteArray<T>(byte[] value, int index) where T : struct
        {
            int structSize = Marshal.SizeOf(typeof(T));
            IntPtr ptr = Marshal.AllocHGlobal(structSize);
            Marshal.Copy(value, index, ptr, structSize);
            return (T)Marshal.PtrToStructure(ptr, typeof(T));
        }
        public static T PtrToStruct<T>(IntPtr ptr)
        {
            return (T)Marshal.PtrToStructure(ptr, typeof(T));
        }

        public unsafe static string BuildMessage(IntPtr exception)
        {
            byte[] ourMessageBytes = new byte[65536];
            fixed (byte* message = &ourMessageBytes[0])
            {
                Import.Exception.il2cpp_format_exception(exception, (void*)message, ourMessageBytes.Length);
                string text = System.Text.Encoding.UTF8.GetString(ourMessageBytes, 0, Array.IndexOf<byte>(ourMessageBytes, 0));
                return text;
            }
        }

        /*public static void RedPrefix(this string message, string prefix)
        {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(prefix);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("] " + message);
        }
    
        public static void GreenPrefix(this string message, string prefix)
        {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(prefix);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("] " + message);
        }

        public static void WriteMessage(this string message, string prefix)
        {
            Console.WriteLine($"[{prefix}] {message}");
        }

        public static void WriteMessage(this string message)
        {
            Console.WriteLine(message);
        }*/
    }
}
