using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace IL2CPP
{
    public static class IL2Utils
    {
        public static T ResolveICall<T>(string signature)// where T : Delegate
        {
            var icallPtr = Import.Method.il2cpp_resolve_icall(signature);
            if (icallPtr != IntPtr.Zero)
                return (T)(object)Marshal.GetDelegateForFunctionPointer(icallPtr, typeof(T));
            return default(T);
        }

        public static bool CheckIsObfus(string text)
        {
            if (text.Length < 30) return false;
            if (string.IsNullOrEmpty(text)) return true;
            text = text.Replace(text[0] + "" + text[1], string.Empty);
            if (string.IsNullOrEmpty(text)) return true;
            text = text.Replace(text[0] + "" + text[1], string.Empty);
            return string.IsNullOrEmpty(text);
        }

        public static IL2Class FindClass_ByPropertyName(this IL2Class[] klass, string @name)
        {
            int length = klass.Length;
            for (int i = 0; i < length; i++)
            {
                if (klass[i].GetProperty(@name) != null)
                    return klass[i];
            }
            return null;
        }
        
        public static IL2Class FindClass_ByMethodName(this IL2Class[] klass, string @name)
        {
            int length = klass.Length;
            for (int i = 0; i < length; i++)
            {
                if (klass[i].GetMethod(@name) != null)
                    return klass[i];
            }
            return null;
        }
        
        public static IL2Class FindClass_ByFieldName(this IL2Class[] klass, string @name)
        {
            int length = klass.Length;
            for (int i = 0; i < length; i++)
            {
                if (klass[i].GetField(@name) != null)
                    return klass[i];
            }
            return null;
        }

        public static IL2Class FindClass_ByNesestTypedName(this IL2Class[] klass, string @name)
        {
            int length = klass.Length;
            for (int i = 0; i < length; i++)
            {
                if (klass[i].GetNestedType(@name) != null)
                    return klass[i];
            }
            return null;
        }
    }
}

/*


00007fff`3172a240 GameAssembly!il2cpp_class_get_static_field_data (<no parameter info>)
00007fff`3172a390 GameAssembly!il2cpp_register_debugger_agent_transport (<no parameter info>)
00007fff`3172a390 GameAssembly!il2cpp_debugger_set_agent_options (<no parameter info>)
00007fff`3172a390 GameAssembly!il2cpp_gc_set_external_wbarrier_tracker (<no parameter info>)
00007fff`3172a390 GameAssembly!il2cpp_gc_set_external_allocation_tracker (<no parameter info>)
00007fff`3172a390 GameAssembly!il2cpp_custom_attrs_free (<no parameter info>)
00007fff`318215c0 GameAssembly!il2cpp_gc_has_strict_wbarriers (<no parameter info>)
00007fff`31966e20 GameAssembly!il2cpp_object_header_size (<no parameter info>)
00007fff`31966e20 GameAssembly!il2cpp_offset_of_array_bounds_in_array_object_header (<no parameter info>)
00007fff`31966e20 GameAssembly!il2cpp_allocation_granularity (<no parameter info>)
00007fff`319f2580 GameAssembly!il2cpp_add_internal_call (<no parameter info>)
00007fff`319f2590 GameAssembly!il2cpp_alloc (<no parameter info>)
00007fff`319f25a0 GameAssembly!il2cpp_array_class_get (<no parameter info>)
00007fff`319f25b0 GameAssembly!il2cpp_array_element_size (<no parameter info>)
00007fff`319f25c0 GameAssembly!il2cpp_array_get_byte_length (<no parameter info>)
00007fff`319f25d0 GameAssembly!il2cpp_array_length (<no parameter info>)
00007fff`319f25e0 GameAssembly!il2cpp_array_new (<no parameter info>)
00007fff`319f25f0 GameAssembly!il2cpp_array_new_full (<no parameter info>)
00007fff`319f2600 GameAssembly!il2cpp_array_new_specific (<no parameter info>)
00007fff`319f2610 GameAssembly!il2cpp_array_object_header_size (<no parameter info>)
00007fff`319f2620 GameAssembly!il2cpp_object_get_class (<no parameter info>)
00007fff`319f2620 GameAssembly!il2cpp_class_get_image (<no parameter info>)
00007fff`319f2620 GameAssembly!il2cpp_assembly_get_image (<no parameter info>)
00007fff`319f2620 GameAssembly!il2cpp_property_get_parent (<no parameter info>)
00007fff`319f2620 GameAssembly!il2cpp_image_get_filename (<no parameter info>)
00007fff`319f2620 GameAssembly!il2cpp_image_get_name (<no parameter info>)
00007fff`319f2620 GameAssembly!il2cpp_field_get_name (<no parameter info>)
00007fff`319f2630 GameAssembly!il2cpp_bounded_array_class_get (<no parameter info>)
00007fff`319f2640 GameAssembly!il2cpp_capture_memory_snapshot (<no parameter info>)
00007fff`319f2650 GameAssembly!il2cpp_class_array_element_size (<no parameter info>)
00007fff`319f2660 GameAssembly!il2cpp_class_enum_basetype (<no parameter info>)
00007fff`319f2670 GameAssembly!il2cpp_class_for_each (<no parameter info>)
00007fff`319f2680 GameAssembly!il2cpp_class_from_il2cpp_type (<no parameter info>)
00007fff`319f2680 GameAssembly!il2cpp_class_from_type (<no parameter info>)
00007fff`319f2690 GameAssembly!il2cpp_class_from_name (<no parameter info>)
00007fff`319f26a0 GameAssembly!il2cpp_class_from_system_type (<no parameter info>)
00007fff`319f26b0 GameAssembly!il2cpp_class_get_assemblyname (<no parameter info>)
00007fff`319f26c0 GameAssembly!il2cpp_class_get_bitmap (<no parameter info>)
00007fff`319f26e0 GameAssembly!il2cpp_class_get_bitmap_size (<no parameter info>)
00007fff`319f26f0 GameAssembly!il2cpp_class_get_data_size (<no parameter info>)
00007fff`319f2700 GameAssembly!il2cpp_class_get_declaring_type (<no parameter info>)
00007fff`319f2710 GameAssembly!il2cpp_class_get_element_class (<no parameter info>)
00007fff`319f2720 GameAssembly!il2cpp_class_get_events (<no parameter info>)
00007fff`319f2730 GameAssembly!il2cpp_class_get_field_from_name (<no parameter info>)
00007fff`319f2740 GameAssembly!il2cpp_class_get_fields (<no parameter info>)
00007fff`319f2750 GameAssembly!il2cpp_class_get_flags (<no parameter info>)
00007fff`319f2760 GameAssembly!il2cpp_class_get_interfaces (<no parameter info>)
00007fff`319f2770 GameAssembly!il2cpp_class_get_method_from_name (<no parameter info>)
00007fff`319f2780 GameAssembly!il2cpp_class_get_methods (<no parameter info>)
00007fff`319f2790 GameAssembly!il2cpp_method_get_name (<no parameter info>)
00007fff`319f2790 GameAssembly!il2cpp_class_get_name (<no parameter info>)
00007fff`319f2790 GameAssembly!il2cpp_method_get_from_reflection (<no parameter info>)
00007fff`319f2790 GameAssembly!il2cpp_property_get_get_method (<no parameter info>)
00007fff`319f2790 GameAssembly!il2cpp_field_get_parent (<no parameter info>)
00007fff`319f2790 GameAssembly!il2cpp_image_get_assembly (<no parameter info>)
00007fff`319f27a0 GameAssembly!il2cpp_property_get_set_method (<no parameter info>)
00007fff`319f27a0 GameAssembly!il2cpp_class_get_namespace (<no parameter info>)
00007fff`319f27a0 GameAssembly!il2cpp_method_get_class (<no parameter info>)
00007fff`319f27a0 GameAssembly!il2cpp_method_get_declaring_type (<no parameter info>)
00007fff`319f27b0 GameAssembly!il2cpp_class_get_nested_types (<no parameter info>)
00007fff`319f27c0 GameAssembly!il2cpp_class_get_parent (<no parameter info>)
00007fff`319f27d0 GameAssembly!il2cpp_class_get_properties (<no parameter info>)
00007fff`319f27e0 GameAssembly!il2cpp_class_get_property_from_name (<no parameter info>)
00007fff`319f27f0 GameAssembly!il2cpp_class_get_rank (<no parameter info>)
00007fff`319f2800 GameAssembly!il2cpp_class_get_type (<no parameter info>)
00007fff`319f2810 GameAssembly!il2cpp_class_get_type_token (<no parameter info>)
00007fff`319f2820 GameAssembly!il2cpp_class_get_userdata_offset (<no parameter info>)
00007fff`319f2830 GameAssembly!il2cpp_class_has_attribute (<no parameter info>)
00007fff`319f2840 GameAssembly!il2cpp_class_has_parent (<no parameter info>)
00007fff`319f2850 GameAssembly!il2cpp_class_has_references (<no parameter info>)
00007fff`319f2860 GameAssembly!il2cpp_class_instance_size (<no parameter info>)
00007fff`319f2870 GameAssembly!il2cpp_class_is_abstract (<no parameter info>)
00007fff`319f2880 GameAssembly!il2cpp_class_is_assignable_from (<no parameter info>)
00007fff`319f2890 GameAssembly!il2cpp_class_is_blittable (<no parameter info>)
00007fff`319f28a0 GameAssembly!il2cpp_class_is_enum (<no parameter info>)
00007fff`319f28b0 GameAssembly!il2cpp_class_is_generic (<no parameter info>)
00007fff`319f28c0 GameAssembly!il2cpp_class_is_inflated (<no parameter info>)
00007fff`319f28d0 GameAssembly!il2cpp_class_is_interface (<no parameter info>)
00007fff`319f28e0 GameAssembly!il2cpp_class_is_subclass_of (<no parameter info>)
00007fff`319f28f0 GameAssembly!il2cpp_class_is_valuetype (<no parameter info>)
00007fff`319f2900 GameAssembly!il2cpp_class_num_fields (<no parameter info>)
00007fff`319f2910 GameAssembly!il2cpp_class_set_userdata (<no parameter info>)
00007fff`319f2920 GameAssembly!il2cpp_class_value_size (<no parameter info>)
00007fff`319f2930 GameAssembly!il2cpp_current_thread_get_frame_at (<no parameter info>)
00007fff`319f2940 GameAssembly!il2cpp_current_thread_get_stack_depth (<no parameter info>)
00007fff`319f2960 GameAssembly!il2cpp_current_thread_get_top_frame (<no parameter info>)
00007fff`319f2970 GameAssembly!il2cpp_current_thread_walk_frame_stack (<no parameter info>)
00007fff`319f2980 GameAssembly!il2cpp_custom_attrs_construct (<no parameter info>)
00007fff`319f2990 GameAssembly!il2cpp_custom_attrs_from_class (<no parameter info>)
00007fff`319f29b0 GameAssembly!il2cpp_custom_attrs_from_method (<no parameter info>)
00007fff`319f29d0 GameAssembly!il2cpp_custom_attrs_get_attr (<no parameter info>)
00007fff`319f29e0 GameAssembly!il2cpp_custom_attrs_has_attr (<no parameter info>)
00007fff`319f29f0 GameAssembly!il2cpp_debug_get_method_info (<no parameter info>)
00007fff`319f2a00 GameAssembly!il2cpp_domain_assembly_open (<no parameter info>)
00007fff`319f2a10 GameAssembly!il2cpp_domain_get (<no parameter info>)
00007fff`319f2a20 GameAssembly!il2cpp_domain_get_assemblies (<no parameter info>)
00007fff`319f2a50 GameAssembly!il2cpp_exception_from_name_msg (<no parameter info>)
00007fff`319f2a60 GameAssembly!il2cpp_field_get_flags (<no parameter info>)
00007fff`319f2a70 GameAssembly!il2cpp_field_get_offset (<no parameter info>)
00007fff`319f2a80 GameAssembly!il2cpp_field_get_type (<no parameter info>)
00007fff`319f2a80 GameAssembly!il2cpp_property_get_name (<no parameter info>)
00007fff`319f2a90 GameAssembly!il2cpp_field_get_value (<no parameter info>)
00007fff`319f2aa0 GameAssembly!il2cpp_field_get_value_object (<no parameter info>)
00007fff`319f2ab0 GameAssembly!il2cpp_field_has_attribute (<no parameter info>)
00007fff`319f2ac0 GameAssembly!il2cpp_field_is_literal (<no parameter info>)
00007fff`319f2ad0 GameAssembly!il2cpp_field_set_value (<no parameter info>)
00007fff`319f2ae0 GameAssembly!il2cpp_field_set_value_object (<no parameter info>)
00007fff`319f2af0 GameAssembly!il2cpp_field_static_get_value (<no parameter info>)
00007fff`319f2b00 GameAssembly!il2cpp_field_static_set_value (<no parameter info>)
00007fff`319f2b10 GameAssembly!il2cpp_format_exception (<no parameter info>)
00007fff`319f2ba0 GameAssembly!il2cpp_format_stack_trace (<no parameter info>)
00007fff`319f2c30 GameAssembly!il2cpp_free (<no parameter info>)
00007fff`319f2c40 GameAssembly!il2cpp_free_captured_memory_snapshot (<no parameter info>)
00007fff`319f2c50 GameAssembly!il2cpp_gc_collect (<no parameter info>)
00007fff`319f2c60 GameAssembly!il2cpp_gc_collect_a_little (<no parameter info>)
00007fff`319f2c70 GameAssembly!il2cpp_gc_disable (<no parameter info>)
00007fff`319f2c80 GameAssembly!il2cpp_gc_enable (<no parameter info>)
00007fff`319f2c90 GameAssembly!il2cpp_gc_foreach_heap (<no parameter info>)
00007fff`319f2cc0 GameAssembly!il2cpp_gc_get_heap_size (<no parameter info>)
00007fff`319f2cd0 GameAssembly!il2cpp_gc_get_max_time_slice_ns (<no parameter info>)
00007fff`319f2ce0 GameAssembly!il2cpp_gc_get_used_size (<no parameter info>)
00007fff`319f2cf0 GameAssembly!il2cpp_gc_is_disabled (<no parameter info>)
00007fff`319f2d00 GameAssembly!il2cpp_gc_is_incremental (<no parameter info>)
00007fff`319f2d10 GameAssembly!il2cpp_gc_set_max_time_slice_ns (<no parameter info>)
00007fff`319f2d20 GameAssembly!il2cpp_gc_wbarrier_set_field (<no parameter info>)
00007fff`319f2d30 GameAssembly!il2cpp_gchandle_foreach_get_target (<no parameter info>)
00007fff`319f2d60 GameAssembly!il2cpp_gchandle_free (<no parameter info>)
00007fff`319f2d70 GameAssembly!il2cpp_gchandle_get_target (<no parameter info>)
00007fff`319f2d80 GameAssembly!il2cpp_gchandle_new (<no parameter info>)
00007fff`319f2d90 GameAssembly!il2cpp_gchandle_new_weakref (<no parameter info>)
00007fff`319f2da0 GameAssembly!il2cpp_get_corlib (<no parameter info>)
00007fff`319f2db0 GameAssembly!il2cpp_get_exception_argument_null (<no parameter info>)
00007fff`319f2dc0 GameAssembly!il2cpp_image_get_class (<no parameter info>)
00007fff`319f2dd0 GameAssembly!il2cpp_image_get_class_count (<no parameter info>)
00007fff`319f2de0 GameAssembly!il2cpp_image_get_entry_point (<no parameter info>)
00007fff`319f2df0 GameAssembly!il2cpp_init (<no parameter info>)
00007fff`319f2e20 GameAssembly!il2cpp_init_utf16 (<no parameter info>)
00007fff`319f2eb0 GameAssembly!il2cpp_is_debugger_attached (<no parameter info>)
00007fff`319f2ec0 GameAssembly!il2cpp_is_vm_thread (<no parameter info>)
00007fff`319f2ed0 GameAssembly!il2cpp_method_get_flags (<no parameter info>)
00007fff`319f2f00 GameAssembly!il2cpp_method_get_object (<no parameter info>)
00007fff`319f2f10 GameAssembly!il2cpp_method_get_param (<no parameter info>)
00007fff`319f2f20 GameAssembly!il2cpp_method_get_param_count (<no parameter info>)
00007fff`319f2f30 GameAssembly!il2cpp_method_get_param_name (<no parameter info>)
00007fff`319f2f40 GameAssembly!il2cpp_method_get_return_type (<no parameter info>)
00007fff`319f2f50 GameAssembly!il2cpp_method_get_token (<no parameter info>)
00007fff`319f2f60 GameAssembly!il2cpp_method_has_attribute (<no parameter info>)
00007fff`319f2f70 GameAssembly!il2cpp_method_is_generic (<no parameter info>)
00007fff`319f2f80 GameAssembly!il2cpp_method_is_inflated (<no parameter info>)
00007fff`319f2f90 GameAssembly!il2cpp_method_is_instance (<no parameter info>)
00007fff`319f2fa0 GameAssembly!il2cpp_monitor_enter (<no parameter info>)
00007fff`319f2fb0 GameAssembly!il2cpp_monitor_exit (<no parameter info>)
00007fff`319f2fc0 GameAssembly!il2cpp_monitor_pulse (<no parameter info>)
00007fff`319f2fd0 GameAssembly!il2cpp_monitor_pulse_all (<no parameter info>)
00007fff`319f2fe0 GameAssembly!il2cpp_monitor_try_enter (<no parameter info>)
00007fff`319f2ff0 GameAssembly!il2cpp_monitor_try_wait (<no parameter info>)
00007fff`319f3000 GameAssembly!il2cpp_monitor_wait (<no parameter info>)
00007fff`319f3010 GameAssembly!il2cpp_object_get_size (<no parameter info>)
00007fff`319f3020 GameAssembly!il2cpp_object_get_virtual_method (<no parameter info>)
00007fff`319f3030 GameAssembly!il2cpp_object_new (<no parameter info>)
00007fff`319f3050 GameAssembly!il2cpp_object_unbox (<no parameter info>)
00007fff`319f3060 GameAssembly!il2cpp_offset_of_array_length_in_array_object_header (<no parameter info>)
00007fff`319f3070 GameAssembly!il2cpp_override_stack_backtrace (<no parameter info>)
00007fff`319f3080 GameAssembly!il2cpp_profiler_install (<no parameter info>)
00007fff`319f3090 GameAssembly!il2cpp_profiler_install_allocation (<no parameter info>)
00007fff`319f30a0 GameAssembly!il2cpp_profiler_install_enter_leave (<no parameter info>)
00007fff`319f30b0 GameAssembly!il2cpp_profiler_install_fileio (<no parameter info>)
00007fff`319f30c0 GameAssembly!il2cpp_profiler_install_gc (<no parameter info>)
00007fff`319f30d0 GameAssembly!il2cpp_profiler_install_thread (<no parameter info>)
00007fff`319f30e0 GameAssembly!il2cpp_profiler_set_events (<no parameter info>)
00007fff`319f30f0 GameAssembly!il2cpp_property_get_flags (<no parameter info>)
00007fff`319f3100 GameAssembly!il2cpp_raise_exception (<no parameter info>)
00007fff`319f3110 GameAssembly!il2cpp_register_log_callback (<no parameter info>)
00007fff`319f3120 GameAssembly!il2cpp_resolve_icall (<no parameter info>)
00007fff`319f3130 GameAssembly!il2cpp_runtime_class_init (<no parameter info>)
00007fff`319f3140 GameAssembly!il2cpp_runtime_invoke (<no parameter info>)
00007fff`319f3160 GameAssembly!il2cpp_runtime_invoke_convert_args (<no parameter info>)
00007fff`319f3180 GameAssembly!il2cpp_runtime_object_init (<no parameter info>)
00007fff`319f3190 GameAssembly!il2cpp_runtime_object_init_exception (<no parameter info>)
00007fff`319f31a0 GameAssembly!il2cpp_runtime_unhandled_exception_policy_set (<no parameter info>)
00007fff`319f31b0 GameAssembly!il2cpp_set_commandline_arguments (<no parameter info>)
00007fff`319f31c0 GameAssembly!il2cpp_set_commandline_arguments_utf16 (<no parameter info>)
00007fff`319f31d0 GameAssembly!il2cpp_set_config (<no parameter info>)
00007fff`319f31e0 GameAssembly!il2cpp_set_config_dir (<no parameter info>)
00007fff`319f31f0 GameAssembly!il2cpp_set_config_utf16 (<no parameter info>)
00007fff`319f3200 GameAssembly!il2cpp_set_data_dir (<no parameter info>)
00007fff`319f3210 GameAssembly!il2cpp_set_default_thread_affinity (<no parameter info>)
00007fff`319f3220 GameAssembly!il2cpp_set_find_plugin_callback (<no parameter info>)
00007fff`319f3230 GameAssembly!il2cpp_set_memory_callbacks (<no parameter info>)
00007fff`319f3240 GameAssembly!il2cpp_set_temp_dir (<no parameter info>)
00007fff`319f3250 GameAssembly!il2cpp_shutdown (<no parameter info>)
00007fff`319f3260 GameAssembly!il2cpp_start_gc_world (<no parameter info>)
00007fff`319f3270 GameAssembly!il2cpp_stats_dump_to_file (<no parameter info>)
00007fff`319f37b0 GameAssembly!il2cpp_stats_get_value (<no parameter info>)
00007fff`319f3830 GameAssembly!il2cpp_stop_gc_world (<no parameter info>)
00007fff`319f3840 GameAssembly!il2cpp_string_chars (<no parameter info>)
00007fff`319f3850 GameAssembly!il2cpp_string_intern (<no parameter info>)
00007fff`319f3860 GameAssembly!il2cpp_string_is_interned (<no parameter info>)
00007fff`319f3870 GameAssembly!il2cpp_string_length (<no parameter info>)
00007fff`319f3880 GameAssembly!il2cpp_string_new (<no parameter info>)
00007fff`319f3880 GameAssembly!il2cpp_string_new_wrapper (<no parameter info>)
00007fff`319f3890 GameAssembly!il2cpp_string_new_len (<no parameter info>)
00007fff`319f38a0 GameAssembly!il2cpp_string_new_utf16 (<no parameter info>)
00007fff`319f38b0 GameAssembly!il2cpp_thread_attach (<no parameter info>)
00007fff`319f38c0 GameAssembly!il2cpp_thread_current (<no parameter info>)
00007fff`319f38d0 GameAssembly!il2cpp_thread_detach (<no parameter info>)
00007fff`319f38e0 GameAssembly!il2cpp_thread_get_all_attached_threads (<no parameter info>)
00007fff`319f38f0 GameAssembly!il2cpp_thread_get_frame_at (<no parameter info>)
00007fff`319f3900 GameAssembly!il2cpp_thread_get_stack_depth (<no parameter info>)
00007fff`319f3910 GameAssembly!il2cpp_thread_get_top_frame (<no parameter info>)
00007fff`319f3920 GameAssembly!il2cpp_thread_walk_frame_stack (<no parameter info>)
00007fff`319f3930 GameAssembly!il2cpp_type_equals (<no parameter info>)
00007fff`319f3940 GameAssembly!il2cpp_type_get_assembly_qualified_name (<no parameter info>)
00007fff`319f39e0 GameAssembly!il2cpp_type_get_attrs (<no parameter info>)
00007fff`319f39f0 GameAssembly!il2cpp_type_get_class_or_element_class (<no parameter info>)
00007fff`319f3a00 GameAssembly!il2cpp_type_get_name (<no parameter info>)
00007fff`319f3aa0 GameAssembly!il2cpp_type_get_name_chunked (<no parameter info>)
00007fff`319f3ab0 GameAssembly!il2cpp_type_get_object (<no parameter info>)
00007fff`319f3ac0 GameAssembly!il2cpp_type_get_type (<no parameter info>)
00007fff`319f3ad0 GameAssembly!il2cpp_type_is_byref (<no parameter info>)
00007fff`319f3ae0 GameAssembly!il2cpp_type_is_pointer_type (<no parameter info>)
00007fff`319f3af0 GameAssembly!il2cpp_type_is_static (<no parameter info>)
00007fff`319f3b00 GameAssembly!il2cpp_unhandled_exception (<no parameter info>)
00007fff`319f3b10 GameAssembly!il2cpp_unity_install_unitytls_interface (<no parameter info>)
00007fff`319f3b20 GameAssembly!il2cpp_unity_liveness_calculation_begin (<no parameter info>)
00007fff`319f3b30 GameAssembly!il2cpp_unity_liveness_calculation_end (<no parameter info>)
00007fff`319f3b40 GameAssembly!il2cpp_unity_liveness_calculation_from_root (<no parameter info>)
00007fff`319f3b50 GameAssembly!il2cpp_unity_liveness_calculation_from_statics (<no parameter info>)
00007fff`319f3b60 GameAssembly!il2cpp_value_box (<no parameter info>)
*/