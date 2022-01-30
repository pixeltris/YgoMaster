using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace IL2CPP
{
    public class IL2Assembly : IL2Base
    {

        public string Name { get; private set; }
        private List<IL2Class> ClassList = new List<IL2Class>();
        internal IL2Assembly(IntPtr ptr) : base(ptr)
        {
            base.ptr = ptr;
            Name = Path.GetFileNameWithoutExtension(Marshal.PtrToStringAnsi(Import.Image.il2cpp_image_get_name(ptr)));

            // Map out Classes
            uint count = Import.Image.il2cpp_image_get_class_count(ptr);
            for (uint i = 0; i < count; i++)
            {
                IL2Class klass = new IL2Class(Import.Image.il2cpp_image_get_class(ptr, i));
                if (klass.DeclaringType == null)
                    ClassList.Add(klass);
            }
        }
        public IL2Class[] GetClasses()
        {
            return ClassList.ToArray();
        }
        public IL2Class[] GetClasses(IL2BindingFlags flags)
        {
            return GetClasses().Where(x => x.HasFlag(flags)).ToArray();
        }
        public IL2Class GetClass(IntPtr ptr)
        {
            return GetClasses().Where(x => x.ptr == ptr).FirstOrDefault();
        }
        public IL2Class GetClass(string name)
        {
            return GetClass(name, null);
        }
        public IL2Class GetClass(string name, IL2BindingFlags flags)
        {
            return GetClass(name, null, flags);
        }
        public IL2Class GetClass(string name, string name_space)
        {
            IL2Class returnval = null;
            foreach (IL2Class type in GetClasses())
            {
                if (type.Name.Equals(name) && (string.IsNullOrEmpty(name_space) || (!string.IsNullOrEmpty(type.Namespace) && type.Namespace.Equals(name_space))))
                {
                    returnval = type;
                    break;
                }
                else
                {
                    foreach (IL2Class nestedtype in type.GetNestedTypes())
                    {
                        if (nestedtype.Name.Equals(name) && (string.IsNullOrEmpty(name_space) || (!string.IsNullOrEmpty(nestedtype.Namespace) && nestedtype.Namespace.Equals(name_space))))
                        {
                            returnval = nestedtype;
                            break;
                        }
                    }
                    if (returnval != null)
                        break;
                }
            }
            return returnval;
        }
        public IL2Class GetClass(string name, string name_space, IL2BindingFlags flags)
        {
            IL2Class returnval = null;
            foreach (IL2Class type in GetClasses())
            {
                if (type.Name.Equals(name) && (string.IsNullOrEmpty(name_space) || (!string.IsNullOrEmpty(type.Namespace) && type.Namespace.Equals(name_space))) && type.HasFlag(flags))
                {
                    returnval = type;
                    break;
                }
                /*
                else
                {
                    foreach (IL2Class nestedtype in type.GetNestedTypes())
                    {
                        if (nestedtype.Name.Equals(name) && (string.IsNullOrEmpty(nestedtype.Namespace) || nestedtype.Namespace.Equals(name_space)) && nestedtype.HasFlag(flags))
                        {
                            returnval = nestedtype;
                            break;
                        }
                    }
                    if (returnval != null)
                        break;
                }
                */
            }
            return returnval;
        }
    }
}
