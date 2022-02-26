using System;
using System.Runtime.InteropServices;

namespace IL2CPP
{
    public static class Import
    {
        public static class Domain
        {
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_domain_get();
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_domain_get_assemblies(IntPtr domain, ref uint count);
        }
        
        public static class Assembly
        {
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_assembly_get_image(IntPtr assembly);
        }

        public static class Image
        {
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_image_get_name(IntPtr image);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static uint il2cpp_image_get_class_count(IntPtr image);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_image_get_class(IntPtr image, uint index);
        }

        
        public static class Handler
        {
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_gchandle_new(IntPtr obj, bool pinned);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static void il2cpp_gchandle_free(IntPtr handle);
        }
        
        public static class Exception
        {
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            unsafe public extern static void il2cpp_format_exception(IntPtr exception, void* message, int ourMessageBytes);

        }

        public static class Object
        {
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static string il2cpp_type_get_name(IntPtr type);

            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_object_unbox(IntPtr obj);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_value_box(IntPtr klass, IntPtr data);

            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_object_get_class(IntPtr str);

            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_string_new(string str);

            

            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_string_new_len(string str, int length);

            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_object_new(IntPtr klass);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_array_new(IntPtr elementTypeInfo, int length);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_array_class_get(IntPtr element_class, uint rank);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static int il2cpp_array_length(IntPtr pArray);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static uint il2cpp_array_get_byte_length(IntPtr pArray);
            /*unsafe public static IntPtr CreateNewObject<T>(T value, IL2Class type) where T : unmanaged
            {
                IntPtr result = il2cpp_object_new(type.ptr);
                *(T*)(result + 0x10) = value;
                return result;
            }*/
            unsafe public static IntPtr CreateNewObject<T>(T value, IL2Class type) where T : struct
            {
                IntPtr result = il2cpp_object_new(type.ptr);
                byte[] buffer = NativeUtils.StructToByteArray(value);
                Marshal.Copy(buffer, 0, result + 0x10, buffer.Length);
                return result;
            }
        }

        public static class Class
        {
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_class_get_name(IntPtr klass);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_class_get_namespace(IntPtr method);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static uint il2cpp_class_get_flags(IntPtr klass, ref uint flags);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_class_get_methods(IntPtr klass, ref IntPtr iter);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_class_get_method_from_name(IntPtr klass, IntPtr name, int argsCount);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_class_get_fields(IntPtr klass, ref IntPtr iter);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_class_get_nested_types(IntPtr klass, ref IntPtr iter);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_class_get_properties(IntPtr klass, ref IntPtr iter);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_class_get_interfaces(IntPtr klass, ref IntPtr iter);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_class_get_type(IntPtr klass);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_type_get_object(IntPtr type);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_class_get_parent(IntPtr type);

            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_class_from_system_type(IntPtr type);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_class_get_image(IntPtr klass);

            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_class_get_declaring_type(IntPtr klass);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static bool il2cpp_class_has_attribute(IntPtr klass, IntPtr attr_class);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static bool il2cpp_class_is_enum(IntPtr klass);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static int il2cpp_class_get_type_token(IntPtr method);

            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_class_from_type(IntPtr type);
        }

        public static class Property
        {
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_property_get_name(IntPtr property);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static uint il2cpp_property_get_flags(IntPtr property);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_property_get_get_method(IntPtr property);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_property_get_set_method(IntPtr property);
        }

        public static class Method
        {
            [DllImport("GameAssembly", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern IntPtr il2cpp_resolve_icall([MarshalAs(UnmanagedType.LPStr)] string name);

            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_method_get_name(IntPtr method);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_method_get_return_type(IntPtr method);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static uint il2cpp_method_get_flags(IntPtr method, ref uint flags);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static uint il2cpp_method_get_param_count(IntPtr method);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_method_get_param(IntPtr method, uint index);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_method_get_param_name(IntPtr method, uint index);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static int il2cpp_method_get_token(IntPtr method);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static bool il2cpp_method_has_attribute(IntPtr method, IntPtr attr_class);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_method_get_class(IntPtr method);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            unsafe public extern static IntPtr il2cpp_runtime_invoke(IntPtr method, IntPtr obj, void** param, IntPtr exc);

            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_object_get_virtual_method(IntPtr obj, IntPtr method);

            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_method_get_from_reflection(IntPtr method);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_method_get_object(IntPtr method, IntPtr refclass);
        }

        public static class Field
        {
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_field_get_name(IntPtr field);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static uint il2cpp_field_get_flags(IntPtr field, ref uint flags);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_field_get_type(IntPtr field);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static IntPtr il2cpp_field_get_parent(IntPtr field);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            unsafe public extern static IntPtr il2cpp_field_get_value_object(IntPtr field, IntPtr obj);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            unsafe public extern static void il2cpp_field_static_set_value(IntPtr field, IntPtr value);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            unsafe public extern static void il2cpp_field_set_value(IntPtr obj, IntPtr field, IntPtr value);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static int il2cpp_field_get_offset(IntPtr field);
            [DllImport("GameAssembly", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            public extern static bool il2cpp_field_has_attribute(IntPtr field, IntPtr attr_class);
        }
    }
}
