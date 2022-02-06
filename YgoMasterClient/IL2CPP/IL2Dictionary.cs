using System;
using System.Reflection;

namespace IL2CPP
{
	unsafe public class IL2Dictionary : IL2Base
	{
		public IL2Dictionary(IntPtr ptrNew) : base(ptrNew)
        {
			ptr = ptrNew;
        }

		public int Count
		{
			get { return Instance_Class.GetProperty("Count").GetGetMethod().Invoke(ptr).GetValueRef<int>(); }
		}

		public void Clear()
		{
			Instance_Class.GetMethod("Clear").Invoke(ptr, ex: false);
		}

		public static IL2Class Instance_Class = Assembler.GetAssembly("mscorlib").GetClass("Dictionary`2", "System.Collections.Generic");
	}

	unsafe public class IL2Dictionary<TKey, TValue> : IL2Dictionary
	{
		public IL2Dictionary(IntPtr ptrNew) : base(ptrNew)
        {
			ptr = ptrNew;
        }

		private static IL2Property propertyItem = null;
		public string this[string key]
		{
			get
            {
                IL2Object obj = Instance_Class.GetProperty("Item").GetGetMethod().Invoke(ptr, new IntPtr[] { new IL2String(key).ptr, propertyItem.GetGetMethod().ptr });
                return obj != null ? obj.GetValueObj<string>() : null;
            }
			set { Instance_Class.GetProperty("Item").GetSetMethod().Invoke(ptr, new IntPtr[] { new IL2String(key).ptr, new IL2String(value).ptr, propertyItem.GetSetMethod().ptr }); }
		}

		public int FindEntry(IntPtr key)
		{
			return Instance_Class.GetMethod("FindEntry").Invoke(ptr, new IntPtr[] { key }).GetValueRef<int>();
		}

		private static IL2Method methodAdd = null;
		public void Add(IntPtr key, IntPtr value)
		{
			if (methodAdd == null)
			{
				methodAdd = Instance_Class.GetMethod("Add");
				if (methodAdd == null)
					return;
			}
			methodAdd.Invoke(ptr, new IntPtr[] { key, value, methodAdd.ptr });
		}

		private static IL2Method methodRemove = null;
		public bool Remove(IntPtr key)
		{
			if (methodRemove == null)
			{
				methodRemove = Instance_Class.GetMethod("Remove");
				if (methodRemove == null)
                    return false;//default;
			}
			IL2Object result = methodRemove.Invoke(ptr, new IntPtr[] { key, methodRemove.ptr });
			if (result == null)
				return false;//default;

			return result.GetValueRef<bool>();
		}

		public static new IL2Class Instance_Class = IL2Dictionary.Instance_Class.MakeGenericType(new Type[] {typeof(TKey), typeof(TValue) });
	}

    public unsafe class IL2DictionaryExplicit : IL2Base
    {
        static System.Collections.Generic.Dictionary<Type, DictionaryClassInfo> Classes = new System.Collections.Generic.Dictionary<Type, DictionaryClassInfo>();
        public class DictionaryClassInfo
        {
            public IL2Class Class;
            public IL2Method MethodItemGet;
            public IL2Method MethodItemSet;
            public IL2Method MethodCountGet;
            public IL2Method MethodAdd;
            public IL2Method MethodRemove;
            public IL2Method MethodContainsKey;
            public IL2Method MethodClear;
        }

        public DictionaryClassInfo ClassInfo;

        public bool RequiresInit
        {
            get { return ClassInfo == null; }
        }

        public IL2DictionaryExplicit(IntPtr ptrNew) : base(ptrNew)
        {
            Classes.TryGetValue(GetType(), out ClassInfo);
        }

        protected void Init(IL2Class keyClass, IL2Class valueClass)
        {
            IntPtr keyType = keyClass.IL2Typeof();
            IntPtr valueType = valueClass.IL2Typeof();
            ClassInfo = new DictionaryClassInfo();
            ClassInfo.Class = IL2Dictionary.Instance_Class.MakeGenericType(new IntPtr[] { keyType, valueType });
            ClassInfo.MethodItemGet = ClassInfo.Class.GetProperty("Item").GetGetMethod();
            ClassInfo.MethodItemSet = ClassInfo.Class.GetProperty("Item").GetSetMethod();
            ClassInfo.MethodCountGet = ClassInfo.Class.GetProperty("Count").GetGetMethod();
            ClassInfo.MethodAdd = ClassInfo.Class.GetMethod("Add");
            ClassInfo.MethodRemove = ClassInfo.Class.GetMethod("Remove");
            ClassInfo.MethodContainsKey = ClassInfo.Class.GetMethod("ContainsKey");
            ClassInfo.MethodClear = ClassInfo.Class.GetMethod("Clear");
            Classes[GetType()] = ClassInfo;
        }

        public int Count
        {
            get { return ClassInfo.MethodCountGet.Invoke(ptr).GetValueRef<int>(); }
        }

        public void Clear()
        {
            ClassInfo.MethodClear.Invoke(ptr);
        }
    }

    // <primitive (int,long,byte,etc), System.Object>
    public unsafe class IL2Dictionary_Primitive_Object : IL2DictionaryExplicit
    {
        public IL2Dictionary_Primitive_Object(IntPtr ptrNew, Type primitiveType) : base(ptrNew)
        {
            if (RequiresInit)
            {
                IL2Class keyType = Assembler.GetAssembly("mscorlib").GetClass(primitiveType.Name, primitiveType.Namespace);
                IL2Class valueType = Assembler.GetAssembly("mscorlib").GetClass(typeof(object).Name, typeof(object).Namespace);
                Init(keyType, valueType);
            }
        }
    }

    public unsafe class IL2Dictionary_Int32_Object : IL2Dictionary_Primitive_Object
    {
        public IL2Dictionary_Int32_Object(IntPtr ptrNew, Type intType = null)
            : base(ptrNew, intType != null ? intType : typeof(int))
        {
        }

        public IntPtr this[int key]
        {
            get
            {
                IL2Object obj = ClassInfo.MethodItemGet.Invoke(ptr, new IntPtr[] { new IntPtr(&key) });
                return obj != null ? obj.ptr : IntPtr.Zero;
            }
            set
            {
                ClassInfo.MethodItemSet.Invoke(ptr, new IntPtr[] { new IntPtr(&key), value });
            }
        }

        public void Add(int key, IntPtr value)
        {
            ClassInfo.MethodAdd.Invoke(ptr, new IntPtr[] { new IntPtr(&key), value });
        }

        public bool Remove(int key)
        {
            return ClassInfo.MethodRemove.Invoke(ptr, new IntPtr[] { new IntPtr(&key) }).GetValueRef<bool>();
        }

        public bool ContainsKey(int key)
        {
            return ClassInfo.MethodContainsKey.Invoke(ptr, new IntPtr[] { new IntPtr(&key) }).GetValueRef<bool>();
        }
    }

    public unsafe class IL2Dictionary_UInt32_Object : IL2Dictionary_Int32_Object
    {
        public IL2Dictionary_UInt32_Object(IntPtr ptrNew)
            : base(ptrNew, typeof(uint))
        {
        }
    }
}
