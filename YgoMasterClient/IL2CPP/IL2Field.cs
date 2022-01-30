using System;
using System.Runtime.InteropServices;

namespace IL2CPP
{
    public class IL2Field : IL2Base
    {
        internal IL2Field(IntPtr ptr) : base(ptr)
        {
            base.ptr = ptr;
        }

        private string szName;
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(szName))
                    szName = Marshal.PtrToStringAnsi(Import.Field.il2cpp_field_get_name(ptr));
                return szName;
            }
            set
            {
                szName = value;
            }
        }

        public IL2ClassType ReturnType { get { return new IL2ClassType(Import.Field.il2cpp_field_get_type(ptr)); } }
        public int Token { get { return Import.Field.il2cpp_field_get_offset(ptr); } }

        public IL2BindingFlags Flags
        {
            get
            {
                uint flags = 0;
                return (IL2BindingFlags)Import.Field.il2cpp_field_get_flags(ptr, ref flags);
            }
        }
        public bool HasFlag(IL2BindingFlags flag) { return ((Flags & flag) != 0); }

        public bool IsStatic { get { return HasFlag(IL2BindingFlags.FIELD_STATIC); } }
        public bool IsPrivate { get { return HasFlag(IL2BindingFlags.FIELD_PRIVATE); } }
        public bool IsPublic { get { return HasFlag(IL2BindingFlags.FIELD_PUBLIC); } }

        public bool Instance { get { return IsStatic && ReturnType.Name == ReflectedType.FullName; } }
        public IL2Class ReflectedType { get { return new IL2Class(Import.Field.il2cpp_field_get_parent(ptr)); } }

        public bool HasAttribute(IL2Class klass)
        {
            if (klass == null) return false;
            return Import.Field.il2cpp_field_has_attribute(ptr, klass.ptr);
        }

        public IL2Object GetValue()
        {
            return GetValue(IntPtr.Zero);
        }
        public IL2Object GetValue(IntPtr obj)
        {
            IntPtr returnval = IntPtr.Zero;
            if (HasFlag(IL2BindingFlags.FIELD_STATIC))
                returnval = Import.Field.il2cpp_field_get_value_object(ptr, IntPtr.Zero);
            else
                returnval = Import.Field.il2cpp_field_get_value_object(ptr, obj);
            if (returnval != IntPtr.Zero)
                return new IL2Object(returnval);
            return null;
        }
        public void SetValue(IntPtr value)
        {
            SetValue(IntPtr.Zero, value);
        }
        public void SetValue(IntPtr obj, IntPtr value)
        {
            if (HasFlag(IL2BindingFlags.FIELD_STATIC))
                Import.Field.il2cpp_field_static_set_value(ptr, value);
            else
                Import.Field.il2cpp_field_set_value(obj, ptr, value);
        }
    }
}
