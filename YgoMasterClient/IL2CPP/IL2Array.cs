using System;
using System.Runtime.InteropServices;

namespace IL2CPP
{
    public class IL2Array<T> : IL2Base where T : struct// where T : unmanaged
	{
		public IL2Array(int length, IL2Class typeobject = null) : base(IntPtr.Zero)
		{
			if (typeobject == null)
				typeobject = Assembler.GetAssembly("mscorlib").GetClass("Object", "System");

			ptr = Import.Object.il2cpp_array_new(typeobject.ptr, length);
		}

		public IL2Array(IntPtr ptr) : base(ptr)
		{
			this.ptr = ptr;
		}

		unsafe public T this[int index]
		{
			get
			{
				if (index < 0 || index >= Length)
				{
					throw new ArgumentOutOfRangeException();
				}
                return NativeUtils.PtrToStruct<T>(((IntPtr)((long*)ptr + 4) + index * Marshal.SizeOf(typeof(T))));
				//return *(T*)((IntPtr)((long*)ptr + 4) + index * sizeof(T));
			}
			set
			{
				if (index < 0 || index >= Length)
				{
					throw new ArgumentOutOfRangeException();
				}
                byte[] buffer = NativeUtils.StructToByteArray(value);
                Marshal.Copy(buffer, 0, ((IntPtr)((long*)ptr + 4) + index * Marshal.SizeOf(typeof(T))), buffer.Length);
				//*(IntPtr*)((IntPtr)((long*)ptr + 4) + index * sizeof(T)) = new IntPtr(&value);
			}
		}

		public int Length
		{
			get
			{
				int result;
				if (typeof(T) == typeof(byte))
				{
					result = (int)Import.Object.il2cpp_array_get_byte_length(ptr);
				}
				else
				{
					result = Import.Object.il2cpp_array_length(ptr);
				}
				return result;
			}
		}

		public T[] ToArray()
        {
			int len = Length;
			T[] result = new T[Length];
			for(int i = 0;i< len; i++)
            {
				result[i] = this[i];
			}
			return result;
        }

        public unsafe byte[] ToByteArray()
        {
            byte[] result = new byte[Length * Marshal.SizeOf(typeof(T))];
            Marshal.Copy((IntPtr)((long*)ptr + 4), result, 0, result.Length);
            return result;
        }
	}
}
