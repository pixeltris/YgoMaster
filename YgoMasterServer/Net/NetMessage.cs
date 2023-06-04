using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net
{
    abstract class NetMessage
    {
        public abstract NetMessageType Type { get; }

        public virtual void Read(BinaryReader reader)
        {
        }

        public virtual void Write(BinaryWriter writer)
        {
        }
    }
}
