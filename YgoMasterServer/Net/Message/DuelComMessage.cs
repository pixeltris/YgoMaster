using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster.Net.Message
{
    abstract class DuelComMessage : NetMessage
    {
        public ulong RunEffectSeq;

        public override void Read(BinaryReader reader)
        {
            RunEffectSeq = reader.ReadUInt64();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(RunEffectSeq);
        }
    }
}
