using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;

namespace YgoMaster.Net
{
    abstract class NetMessage
    {
        public abstract void Read(BinaryReader reader);
        public abstract void Write(BinaryWriter writer);

        static Dictionary<uint, Type> messageTypesByOpcode = new Dictionary<uint, Type>();
        static Dictionary<Type, uint> messageOpcodesByType = new Dictionary<Type, uint>();

        static NetMessage()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.BaseType == typeof(NetMessage))
                {
                    using (MD5 md5 = MD5.Create())
                    {
                        for (int i = 0; i < int.MaxValue; i++)
                        {
                            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(type.FullName + (i >= 0 ? i.ToString() : string.Empty)));
                            uint opcode = BitConverter.ToUInt32(hash, 0);
                            if (!messageTypesByOpcode.ContainsKey(opcode))
                            {
                                messageTypesByOpcode.Add(opcode, type);
                                messageOpcodesByType.Add(type, opcode);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public static Type GetType(uint opcode)
        {
            Type type;
            messageTypesByOpcode.TryGetValue(opcode, out type);
            return type;
        }

        public static uint GetOpcode(Type type)
        {
            uint opcode;
            messageOpcodesByType.TryGetValue(type, out opcode);
            return opcode;
        }

        public static void SetupHandlers(Type type, Dictionary<Type, MethodInfo> handlers, int paramLen)
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == paramLen &&
                    (paramLen == 1 || parameters[0].ParameterType == typeof(NetClient)) &&
                    parameters[paramLen - 1].ParameterType.IsSubclassOf(typeof(NetMessage)))
                {
                    handlers[parameters[paramLen - 1].ParameterType] = method;
                }
            }
        }
    }

    static class NetMessageHandlers<T>
    {
        public static Dictionary<Type, MethodInfo> Handlers = new Dictionary<Type, MethodInfo>();

        static NetMessageHandlers()
        {
#if YGO_MASTER_CLIENT
            NetMessage.SetupHandlers(typeof(T), Handlers, 1);
#else
            NetMessage.SetupHandlers(typeof(T), Handlers, typeof(T) == typeof(NetServer) ? 2 : 1);
#endif
        }
    }
}
