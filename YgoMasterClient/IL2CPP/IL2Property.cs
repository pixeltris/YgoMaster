using System;
using System.Runtime.InteropServices;

namespace IL2CPP
{
    public class IL2Property : IL2Base
    {
        internal IL2Property(IntPtr ptr) : base(ptr)
        {
            base.ptr = ptr;
        }

        private string szName;
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(szName))
                    szName = Marshal.PtrToStringAnsi(Import.Property.il2cpp_property_get_name(ptr));
                return szName;
            }
            set
            {
                szName = value;
                if (GetGetMethod() != null)
                    GetGetMethod().Name = "get_" + value;
                if (GetSetMethod() != null)
                    GetSetMethod().Name = "set_" + value;
            }
        }

        public IL2BindingFlags Flags { get { return (IL2BindingFlags)Import.Property.il2cpp_property_get_flags(ptr); } }
        public bool HasFlag(IL2BindingFlags flag) { return ((Flags & flag) != 0); }
        public bool Instance
        {
            get { return GetGetMethod() != null && GetGetMethod().Instance;}
        }

        public IL2Method GetGetMethod()
        {
            if (getMethod == null)
            {
                IntPtr method = Import.Property.il2cpp_property_get_get_method(ptr);
                if (method != IntPtr.Zero)
                    getMethod = new IL2Method(method);
            }
            return getMethod;
        }
        private IL2Method getMethod;
        public IL2Method GetSetMethod()
        {
            if (setMethod == null)
            {
                IntPtr method = Import.Property.il2cpp_property_get_set_method(ptr);
                if (method != IntPtr.Zero)
                    setMethod = new IL2Method(method);
            }
            return setMethod;
        }
        private IL2Method setMethod;

        public bool IsStatic
        {
            get
            {
                IL2Method method = GetGetMethod();
                if (method != null && method.IsStatic)
                {
                    return true;
                }
                method = GetSetMethod();
                if (method != null && method.IsStatic)
                {
                    return true;
                }
                return false;
            }
        }
        public bool IsPublic
        {
            get
            {
                IL2Method method = GetGetMethod();
                if (method != null && method.IsPublic)
                {
                    return true;
                }
                method = GetSetMethod();
                if (method != null && method.IsPublic)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
