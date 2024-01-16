using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using YgoMaster;
using YgoMasterClient;

namespace IL2CPP
{
    static class ReflectionValidator
    {
        class SignatureInfo
        {
            public string Assembly;
            public string Namespace;
            public List<string> Class;
            public MemberType Type;
            public string Name;
            public string Signature;

            public SignatureInfo(string assembly, string namespaceName, List<string> classPath, MemberType type, string name, string sig)
            {
                Assembly = assembly;
                Namespace = namespaceName;
                Class = classPath;
                Type = type;
                Name = name;
                Signature = sig;
            }

            public SignatureInfo(IntPtr klass, MemberType type, string name, string sig)
            {
                Console.WriteLine("Found sig '" + sig + "'");

                IntPtr assembly = Import.Class.il2cpp_class_get_image(klass);
                if (assembly != IntPtr.Zero)
                {
                    Assembly = Path.GetFileNameWithoutExtension(Marshal.PtrToStringAnsi(Import.Image.il2cpp_image_get_name(assembly)));
                }

                Class = new List<string>();
                IntPtr k = klass;
                while (true)
                {
                    Class.Add(Marshal.PtrToStringAnsi(Import.Class.il2cpp_class_get_name(k)));
                    IntPtr declaringType = Import.Class.il2cpp_class_get_declaring_type(k);
                    if (declaringType == IntPtr.Zero)
                    {
                        Namespace = Marshal.PtrToStringAnsi(Import.Class.il2cpp_class_get_namespace(k));
                        break;
                    }
                    k = declaringType;
                }
                Class.Reverse();
                Type = type;
                Name = name;
                Signature = sig;
            }
        }

#pragma warning disable 0649
        public static bool IsDumping;
#pragma warning restore 0649
        static List<SignatureInfo> signatures = new List<SignatureInfo>();

        static string GetDumpPath()
        {
            return Path.Combine(Program.CurrentDir, "Docs", "ReflectionDump.json");
        }

        public static void FinishDumping()
        {
            if (!IsDumping)
            {
                return;
            }
            List<object> json = new List<object>();
            foreach (SignatureInfo signature in signatures)
            {
                json.Add(new Dictionary<string, object>()
                {
                    { "Assembly", signature.Assembly },
                    { "Namespace", signature.Namespace },
                    { "Class", signature.Class },
                    { "Type", signature.Type },
                    { "Name", signature.Name },
                    { "Signature", signature.Signature },
                });
            }
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(GetDumpPath()));
            }
            catch
            {
            }
            File.WriteAllText(GetDumpPath(), MiniJSON.Json.Serialize(json));
            Console.WriteLine("Dumped to '" + GetDumpPath() + "'");
        }

        public static void ValidateDump()
        {
            ClearLog();
            signatures.Clear();

            List<object> data = MiniJSON.Json.Deserialize(File.ReadAllText(GetDumpPath())) as List<object>;
            foreach (object entry in data)
            {
                Dictionary<string, object> dict = entry as Dictionary<string, object>;
                string assembly = Utils.GetValue<string>(dict, "Assembly");
                string namespaceName = Utils.GetValue<string>(dict, "Namespace");
                List<string> classPath = Utils.GetValueTypeList<string>(dict, "Class");
                MemberType type = Utils.GetValue<MemberType>(dict, "Type");
                string name = Utils.GetValue<string>(dict, "Name");
                string sig = Utils.GetValue<string>(dict, "Signature");
                signatures.Add(new SignatureInfo(assembly, namespaceName, classPath, type, name, sig));
            }

            int validatedMembers = 0;
            foreach (SignatureInfo signature in signatures)
            {
                IL2Assembly assembly = Assembler.GetAssembly(signature.Assembly);
                if (assembly == null)
                {
                    Log("Failed to find asssembly '" + assembly.Name + "' for '" + string.Join(".", signature.Class) + "' - '" + signature.Signature + "'");
                    continue;
                }
                IL2Class klass = assembly.GetClass(signature.Class[0], signature.Namespace);
                if (klass == null)
                {
                    Log("Failed to type '" + signature.Class[0] + "' in assembly '" + signature.Assembly + "'");
                    continue;
                }
                for (int i = 1; i < signature.Class.Count; i++)
                {
                    klass = klass.GetNestedType(signature.Class[i]);
                    if (klass == null)
                    {
                        Log("Failed to nested type '" + string.Join(".", signature.Class) + "' in assembly '" + signature.Assembly + "'");
                        break;
                    }
                }
                if (klass == null)
                {
                    continue;
                }
                switch (signature.Type)
                {
                    case MemberType.Field:
                        {
                            IL2Field field = klass.GetField(signature.Name);
                            if (field == null)
                            {
                                Log("Failed to find field '" + signature.Name + "' in type '" + string.Join(".", signature.Class) + "' / assembly '" + signature.Assembly + "'");
                            }
                            else if (GetSignature(field) != signature.Signature)
                            {
                                Log("Signature of field '" + signature.Name + "' in type '" + string.Join(".", signature.Class) + "' / assembly '" + signature.Assembly + "'" +
                                    " does not match. Found: '" + GetSignature(field) + "' Expected: '" + signature.Signature + "'");
                            }
                            else
                            {
                                validatedMembers++;
                            }
                        }
                        break;
                    case MemberType.Property:
                        {
                            IL2Property property = klass.GetProperty(signature.Name);
                            if (property == null)
                            {
                                Log("Failed to find property '" + signature.Name + "' in type '" + string.Join(".", signature.Class) + "' / assembly '" + signature.Assembly + "'");
                            }
                            else if (GetSignature(property) != signature.Signature)
                            {
                                Log("Signature of property '" + signature.Name + "' in type '" + string.Join(".", signature.Class) + "' / assembly '" + signature.Assembly + "'" +
                                    " does not match. Found: '" + GetSignature(property) + "' Expected: '" + signature.Signature + "'");
                            }
                            else
                            {
                                validatedMembers++;
                            }
                        }
                        break;
                    case MemberType.Method:
                        {
                            IL2Method[] methods = klass.GetMethods().Where(x => x.Name == signature.Name).ToArray();
                            if (methods.FirstOrDefault(x => x.GetSignature() == signature.Signature) != null)
                            {
                                validatedMembers++;
                            }
                            else if (methods.Length == 0)
                            {
                                Log("Failed to find method '" + signature.Name + "' in type '" + string.Join(".", signature.Class) + "' / assembly '" + signature.Assembly + "'");
                            }
                            else
                            {
                                Log("Signature of method '" + signature.Name + "' in type '" + string.Join(".", signature.Class) + "' / assembly '" + signature.Assembly + "'" +
                                    " does not match. Expected: '" + signature.Signature + "' Found: " + string.Join(Environment.NewLine, methods.Select(x => x.GetSignature())));
                            }
                        }
                        break;
                }
            }
            Log("Validated " + validatedMembers + " members");
        }

        static string GetSignature(IL2Property property)
        {
            StringBuilder sig = new StringBuilder();
            if (property.IsPublic)
            {
                sig.Append("public ");
            }
            if (property.IsStatic)
            {
                sig.Append("static ");
            }
            sig.Append(property.GetGetMethod().ReturnType.Name);
            return sig.ToString();
        }

        static string GetSignature(IL2Field field)
        {
            StringBuilder sig = new StringBuilder();
            if (field.IsPublic)
            {
                sig.Append("public ");
            }
            if (field.IsStatic)
            {
                sig.Append("static ");
            }
            sig.Append(field.ReflectedType.Name);
            return sig.ToString();
        }

        public static IL2Property Add(IL2Property property)
        {
            if (IsDumping)
            {
                signatures.Add(new SignatureInfo(Import.Property.il2cpp_property_get_parent(property.ptr), MemberType.Property, property.Name, GetSignature(property)));
            }
            return property;
        }

        public static IL2Field Add(IL2Field field)
        {
            if (IsDumping)
            {
                signatures.Add(new SignatureInfo(Import.Field.il2cpp_field_get_parent(field.ptr), MemberType.Field, field.Name, GetSignature(field)));
            }
            return field;
        }

        public static IL2Method Add(IL2Method method)
        {
            if (IsDumping)
            {
                signatures.Add(new SignatureInfo(Import.Method.il2cpp_method_get_class(method.ptr), MemberType.Method, method.Name, method.GetSignature()));
            }
            return method;
        }

        static string GetLogPath()
        {
            return Path.Combine(Program.CurrentDir, "ReflectionValidator.txt");
        }

        static void ClearLog()
        {
            File.WriteAllText(GetLogPath(), string.Empty);
        }

        static void Log(string str)
        {
            File.AppendAllText(GetLogPath(), str + Environment.NewLine);
            Console.WriteLine(str);
        }

        enum MemberType
        {
            Method,
            Field,
            Property
        }
    }
}
