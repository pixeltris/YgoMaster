using System;

namespace IL2CPP
{
    unsafe public class IL2List : IL2Base
    {
        public IL2List(IntPtr ptrNew) : base(ptrNew)
        {
            ptr = ptrNew;
        }

        public static IL2Class Instance_Class = Assembler.GetAssembly("mscorlib").GetClass("List`1", "System.Collections.Generic");
    }
    unsafe public class IL2List<T> : IL2List
    {
        public IL2List(IntPtr ptrNew)
            : base(ptrNew)
        {
            ptr = ptrNew;
        }

        private static IL2Method methodCountGet;
        public int Count
        {
            get
            {
                if (methodCountGet == null)
                {
                    methodCountGet = Instance_Class.GetProperty("Count").GetGetMethod();
                }
                return methodCountGet.Invoke(ptr).GetValueRef<int>();
            }
        }

        public IL2Method methodItemGet;
        public IL2Method methodItemSet;
        public IntPtr this[int index]
        {
            get
            {
                if (methodItemGet == null)
                {
                    methodItemGet = Instance_Class.GetProperty("Item").GetGetMethod();
                }
                IL2Object result = methodItemGet.Invoke(ptr, new IntPtr[] { new IntPtr(&index) });
                return result != null ? result.ptr : IntPtr.Zero;
            }
            /*set
            {
                if (methodItemSet == null)
                {
                    methodItemSet = Instance_Class.GetProperty("Item").GetSetMethod();
                }
                methodItemSet.Invoke(ptr, new IntPtr[] { new IntPtr(&index), value, methodItemSet.ptr });
            }*/
        }

        public IntPtr Get(int index)
        {
            return this[index];
        }

        public void Set(int index, IntPtr value)
        {
            if (methodItemSet == null)
            {
                methodItemSet = Instance_Class.GetProperty("Item").GetSetMethod();
            }
            methodItemSet.Invoke(ptr, new IntPtr[] { new IntPtr(&index), value, methodItemSet.ptr });
        }

        private static IL2Method methodClear;
        public void Clear()
        {
            if (methodClear == null)
            {
                methodClear = Instance_Class.GetMethod("Clear");
            }
            methodClear.Invoke(ptr);
        }

        private static IL2Method methodAdd;
        public void Add(IntPtr item)
        {
            if (methodAdd == null)
            {
                methodAdd = Instance_Class.GetMethod("Add", x => x.ReturnType.Name == typeof(void).FullName);
            }
            methodAdd.Invoke(ptr, new IntPtr[] { item, methodAdd.ptr });
        }

        private static IL2Method methodContainsObject;
        public bool Contains(IntPtr item)
        {
            if (methodContainsObject == null)
            {
                methodContainsObject = Instance_Class.GetMethod("Contains", x => x.GetParameters()[0].Type.Name != typeof(object).FullName);
            }
            return methodContainsObject.Invoke(ptr, new IntPtr[] { item, methodContainsObject.ptr }).GetValueRef<bool>();
        }

        private static IL2Method methodRemove;
        public bool Remove(IntPtr item)
        {
            if (methodRemove == null)
            {
                methodRemove = Instance_Class.GetMethod("Remove", x => x.ReturnType.Name == typeof(bool).FullName);
            }
            return methodRemove.Invoke(ptr, new IntPtr[] { item, methodRemove.ptr }).GetValueRef<bool>();
        }

        private static IL2Method methodToArray = null;
        public T[] ToArray()
        {
            if (methodToArray == null)
            {
                methodToArray = Instance_Class.GetMethod("ToArray");
            }

            IL2Object result = methodToArray.Invoke(ptr, new IntPtr[] { methodToArray.ptr });
            if (result == null)
                return new T[0];

            return result.UnboxArray<T>();
        }

        public static new IL2Class Instance_Class = IL2List.Instance_Class.MakeGenericType(new Type[] { typeof(T) });
    }

    // TODO: Remove this (it's a replacement for an earlier broken hack)
    public unsafe class IL2ListExplicit : IL2Base
    {
        static System.Collections.Generic.Dictionary<IntPtr, ListClassInfo> Classes = new System.Collections.Generic.Dictionary<IntPtr, ListClassInfo>();
        public class ListClassInfo
        {
            public IL2Class Class;
            public IL2Method MethodCtor;
            public IL2Method MethodItemGet;
            public IL2Method MethodItemSet;
            public IL2Method MethodCountGet;
            public IL2Method MethodAdd;
            public IL2Method MethodClear;
        }

        public ListClassInfo ClassInfo;

        public IL2ListExplicit(IntPtr ptrNew, IL2Class type, bool nullPtrMakeNew = false)
            : base(ptrNew)
        {
            if (!Classes.TryGetValue(type.ptr, out ClassInfo))
            {
                ClassInfo = new ListClassInfo();
                ClassInfo.Class = IL2List.Instance_Class.MakeGenericType(new IntPtr[] { type.IL2Typeof() });
                ClassInfo.MethodCtor = ClassInfo.Class.GetMethod(".ctor", x => x.GetParameters().Length == 0);
                ClassInfo.MethodItemGet = ClassInfo.Class.GetProperty("Item").GetGetMethod();
                ClassInfo.MethodItemSet = ClassInfo.Class.GetProperty("Item").GetSetMethod();
                ClassInfo.MethodCountGet = ClassInfo.Class.GetProperty("Count").GetGetMethod();
                ClassInfo.MethodAdd = ClassInfo.Class.GetMethod("Add");
                ClassInfo.MethodClear = ClassInfo.Class.GetMethod("Clear");
                Classes[type.ptr] = ClassInfo;
            }
            if (nullPtrMakeNew && ptr == IntPtr.Zero)
            {
                ptr = Import.Object.il2cpp_object_new(ClassInfo.Class.ptr);
                ClassInfo.MethodCtor.Invoke(ptr);
            }
        }

        public int Count
        {
            get { return ClassInfo.MethodCountGet.Invoke(ptr).GetValueRef<int>(); }
        }

        public IntPtr this[int index]
        {
            get
            {
                IL2Object obj = ClassInfo.MethodItemGet.Invoke(ptr, new IntPtr[] { new IntPtr(&index), ClassInfo.MethodItemGet.ptr });
                return obj != null ? obj.ptr : IntPtr.Zero;
            }
            set
            {
                ClassInfo.MethodItemSet.Invoke(ptr, new IntPtr[] { new IntPtr(&index), value, ClassInfo.MethodItemSet.ptr });
            }
        }

        public T GetRef<T>(int index) where T : struct
        {
            IL2Object obj = ClassInfo.MethodItemGet.Invoke(ptr, new IntPtr[] { new IntPtr(&index), ClassInfo.MethodItemGet.ptr });
            return obj != null ? obj.GetValueRef<T>() : default(T);
        }

        public void Add(IntPtr value)
        {
            ClassInfo.MethodAdd.Invoke(ptr, new IntPtr[] { value, ClassInfo.MethodAdd.ptr } );
        }

        public void Clear()
        {
            ClassInfo.MethodClear.Invoke(ptr, new IntPtr[] { ClassInfo.MethodAdd.ptr });
        }
    }
}
