using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace YgoMaster
{
    class Engine
    {
        static PvpState tempState = new PvpState();

        class PvpState_PosTblEntry
        {
            public PvpNetValue<int> Pos = new PvpNetValue<int>();
            public PvpNetValue<ushort> UId = new PvpNetValue<ushort>();
            public PvpNetValue<ushort> Unk = new PvpNetValue<ushort>();
        }
        class PvpState_PosTbl
        {
            public List<PvpState_PosTblEntry> Entries = new List<PvpState_PosTblEntry>();

            public void Resize(int count)
            {
                while (Entries.Count < count)
                {
                    Entries.Add(new PvpState_PosTblEntry());
                }
            }

            public void Set(int index, int pos, ushort uid)
            {
                Resize(index + 1);
                Entries[index].Pos.Value = pos;
                Entries[index].UId.Value = uid;
            }

            public void Update(int index, int pos, ushort uid)
            {
                Entries[index].Pos.Update(pos);;
                Entries[index].UId.Update(uid);
            }
        }

        class PvpState_UidTblEntry
        {
            public PvpNetValue<ushort> UId = new PvpNetValue<ushort>();
            public PvpNetValue<ushort> Idx = new PvpNetValue<ushort>();
        }
        class PvpState_UidTbl
        {
            public List<PvpState_UidTblEntry> Entries = new List<PvpState_UidTblEntry>();

            public void Resize(int count)
            {
                while (Entries.Count < count)
                {
                    Entries.Add(new PvpState_UidTblEntry());
                }
            }

            public void Set(int index, ushort uid, ushort idx)
            {
                Resize(index + 1);
                Entries[index].UId.Value = uid;
                Entries[index].Idx.Value = idx;
            }

            public void Update(int index, ushort uid, ushort idx)
            {
                Entries[index].UId.Update(uid);
                Entries[index].Idx.Update(idx);
            }
        }

        class PvpState_CardProp
        {
            public PvpNetValue<ushort> cardId = new PvpNetValue<ushort>();
            public PvpNetValue<ushort> uniqueId = new PvpNetValue<ushort>();
            public PvpNetValue<ushort> flags = new PvpNetValue<ushort>();
        }
        class PvpState_BasicVal
        {
            public PvpNetValue<short> CardID = new PvpNetValue<short>();
            public PvpNetValue<short> EffectID = new PvpNetValue<short>();
            public PvpNetValue<int> Atk = new PvpNetValue<int>();
            public PvpNetValue<int> Def = new PvpNetValue<int>();
            public PvpNetValue<int> OrigAtk = new PvpNetValue<int>();
            public PvpNetValue<int> OrigDef = new PvpNetValue<int>();
            public PvpNetValue<short> Type = new PvpNetValue<short>();
            public PvpNetValue<short> Attr = new PvpNetValue<short>();
            public PvpNetValue<short> Element = new PvpNetValue<short>();
            public PvpNetValue<short> Level = new PvpNetValue<short>();
            public PvpNetValue<byte> Rank = new PvpNetValue<byte>();
            public PvpNetValue<byte> VoidMagic = new PvpNetValue<byte>();
            public PvpNetValue<byte> VoidTrap = new PvpNetValue<byte>();
            public PvpNetValue<byte> VoidMonst = new PvpNetValue<byte>();
        }
        class PvpState_UidBaseEntry
        {
            public PvpNetValue<uint> nCom = new PvpNetValue<uint>();
            public PvpNetValue<uint> nPos = new PvpNetValue<uint>();
            public PvpNetValue<ushort> wUid = new PvpNetValue<ushort>();
            public PvpState_CardProp stProp = new PvpState_CardProp();
            //public PvpNetValue<bool> isFace = new PvpNetValue<bool>();
            //public PvpNetValue<bool> isTurn = new PvpNetValue<bool>();
            public PvpState_BasicVal stBasicVal = new PvpState_BasicVal();
        }
        class PvpState_UidBase
        {
            public List<PvpState_UidBaseEntry> Entries = new List<PvpState_UidBaseEntry>();

            public void Resize(int count)
            {
                while (Entries.Count < count)
                {
                    Entries.Add(new PvpState_UidBaseEntry());
                }
            }
        }

        class PvpState_IconBase
        {
            public PvpNetValue<byte> player = new PvpNetValue<byte>();
            public PvpNetValue<byte> pos = new PvpNetValue<byte>();
            public PvpNetValue<byte> to_player = new PvpNetValue<byte>();
            public PvpNetValue<byte> to_pos = new PvpNetValue<byte>();
            public PvpNetValue<short> icon = new PvpNetValue<short>();
        }
        class PvpState_IconBases
        {
            public List<PvpState_IconBase> Entries = new List<PvpState_IconBase>();

            public void Resize(int count)
            {
                while (Entries.Count < count)
                {
                    Entries.Add(new PvpState_IconBase());
                }
            }
        }

        class PvpState
        {
            public NetPvpFieldMsgs FieldMsgs;
            public PvpState_DuelInfo DuelInfo = new PvpState_DuelInfo();
            public PvpState_PosTbl PosTbl = new PvpState_PosTbl();
            public PvpState_UidTbl UidTbl = new PvpState_UidTbl();
            public PvpState_UidBase UidBases = new PvpState_UidBase();
            public PvpNetValue<ushort>[] AttackFlags = new PvpNetValue<ushort>[7];// Find the size from PVP_DuelGetAttackTargetMask or PvpEngineData ctor
            public Dictionary<ushort, ushort> Rare = new Dictionary<ushort, ushort>();// <uid, cardStyleRarity?>
            public PvpNetValue<uint> summoningUid = new PvpNetValue<uint>();
            /// <summary>
            /// "posMask" is a dictionary (string,uint) where the string is string.Format("{0} {1}", index, commandId) and the value is the
            /// return value from DLL_DUELCOMGetPosMaskOfThisHand. As this is processed data (post serialization) we don't hold the "posMask"
            /// formatted data, and instead just hold an array of all values for serialization.
            /// </summary>
            public List<PvpNetValue<uint>> posMaskList = new List<PvpNetValue<uint>>();

            public PvpState()
            {
                FieldMsgs = new NetPvpFieldMsgs(this);

                for (int index = 0; index < 120; index++)
                {
                    // commandId / CommandType(SummonSp->Pendulum)
                    for (int commandId = 2; commandId <= 8; commandId++)
                    {
                        posMaskList.Add(new PvpNetValue<uint>());
                    }
                }
            }
        }

        class PvpState_PvpPosBase
        {
            public PvpNetValue<uint> nComBit = new PvpNetValue<uint>();
            public PvpNetValue<ushort> wMrk = new PvpNetValue<ushort>();
            public PvpNetValue<ushort> wTurnCounter = new PvpNetValue<ushort>();
            public PvpNetValue<byte> bTopIdx = new PvpNetValue<byte>();
            public PvpNetValue<byte> bEffectFlags = new PvpNetValue<byte>();
            public PvpNetValue<byte> bMonstOrgLevel = new PvpNetValue<byte>();
            public PvpNetValue<byte> bMonstOrgType = new PvpNetValue<byte>();
            public PvpNetValue<byte> bZoneAvailable = new PvpNetValue<byte>();
            public PvpNetValue<byte> bZoneAvailable2 = new PvpNetValue<byte>();
            public PvpNetValue<byte> bCardInBattle = new PvpNetValue<byte>();
            public PvpNetValue<byte> bNormalMonster = new PvpNetValue<byte>();
            public PvpNetValue<sbyte> bPendScale = new PvpNetValue<sbyte>();
            public PvpNetValue<sbyte> bPendOrgScale = new PvpNetValue<sbyte>();
            public PvpNetValue<sbyte> bMostRank = new PvpNetValue<sbyte>();
            public PvpNetValue<sbyte> bMostOrgRank = new PvpNetValue<sbyte>();
            public PvpNetValue<byte> bTrapMonster = new PvpNetValue<byte>();
            public PvpNetValue<byte> bTunerMonster = new PvpNetValue<byte>();
            public PvpNetValue<ushort> wOverlayNum = new PvpNetValue<ushort>();
            public PvpNetValue<ushort> wCardNum = new PvpNetValue<ushort>();
            public PvpNetValue<byte>[] bCounter;
            public PvpNetValue<byte> bFightable = new PvpNetValue<byte>();
            public PvpNetValue<ushort> wEquip = new PvpNetValue<ushort>();
            public PvpNetValue<ushort> wContinuous = new PvpNetValue<ushort>();
            public PvpNetValue<byte> bIsMagic = new PvpNetValue<byte>();
            public PvpNetValue<byte> bIsTrap = new PvpNetValue<byte>();
            public PvpNetValue<int> nShowParam = new PvpNetValue<int>();
            public PvpNetValue<uint> nCardParam = new PvpNetValue<uint>();
            public PvpNetValue<int> nCardDirectFlag = new PvpNetValue<int>();
            public PvpNetValue<int>[] nOtherEffect;

            public int CounterMax
            {
                get { return 66; }
            }

            public PvpState_PvpPosBase()
            {
                // TODO: Properly handle Engine.CounterTypeMax (66 is the default value)
                bCounter = new PvpNetValue<byte>[CounterMax];//Engine.CounterTypeMax];
                for (int i = 0; i < bCounter.Length; i++)
                {
                    bCounter[i] = new PvpNetValue<byte>();
                }

                nOtherEffect = new PvpNetValue<int>[32];
                for (int i = 0; i < nOtherEffect.Length; i++)
                {
                    nOtherEffect[i] = new PvpNetValue<int>();
                }
            }
        }

        class PvpState_DuelInfo
        {
            public PvpNetValue<bool> isQuick = new PvpNetValue<bool>();
            public PvpNetValue<uint> nTurnNum = new PvpNetValue<uint>();
            public PvpNetValue<uint> nCurrentPhase = new PvpNetValue<uint>();
            public PvpNetValue<uint> nCurrentStep = new PvpNetValue<uint>();
            public PvpNetValue<uint> nCurrentDmgStep = new PvpNetValue<uint>();
            public PvpNetValue<byte> bWhichTurnNow = new PvpNetValue<byte>();
            public PvpNetValue<uint> nMovablePhase = new PvpNetValue<uint>();
            public PvpNetValue<uint>[] nLP;
            public PvpNetValue<uint>[] nDoPutMonst;
            public PvpNetValue<bool>[] bDoSummon;
            public PvpNetValue<bool>[] bDoSpSummon;
            public PvpState_PvpPosBase[,] Pos;
            public PvpNetValue<ushort> wTblNum = new PvpNetValue<ushort>();
            public PvpNetValue<ushort> wIconNum = new PvpNetValue<ushort>();
            public PvpState_IconBases IconBases = new PvpState_IconBases();

            public PvpState_DuelInfo()
            {
                const int numSides = 2;
                nLP = new PvpNetValue<uint>[numSides];
                nDoPutMonst = new PvpNetValue<uint>[numSides];
                bDoSummon = new PvpNetValue<bool>[numSides];
                bDoSpSummon = new PvpNetValue<bool>[numSides];
                Pos = new PvpState_PvpPosBase[numSides, 19];
                for (int i = 0; i < numSides; i++)
                {
                    nLP[i] = new PvpNetValue<uint>();
                    nDoPutMonst[i] = new PvpNetValue<uint>();
                    bDoSummon[i] = new PvpNetValue<bool>();
                    bDoSpSummon[i] = new PvpNetValue<bool>();
                    for (int j = 0; j < Pos.GetLength(1); j++)
                    {
                        Pos[i, j] = new PvpState_PvpPosBase();
                    }
                }
            }
        }

        class NetPvpFieldMsgs
        {
            public PvpState State;
            public NetPvpFieldMsg_Prop Prop;
            public NetPvpFieldMsg_Pos Pos;
            public NetPvpFieldMsg_Uid Uid;
            public NetPvpFieldMsg_Vals Vals;
            public NetPvpFieldMsg_Icon Icon;
            public NetPvpFieldMsg_AttackFlags Attack;
            public NetPvpFieldMsg_Show Show;
            public NetPvpFieldMsg_Step Step;
            public NetPvpFieldMsg_SummoningUid SummoningUid;
            public NetPvpFieldMsg_PosMask PosMask;

            public NetPvpFieldMsgs(PvpState state)
            {
                State = state;
                Prop = new NetPvpFieldMsg_Prop(state);
                Pos = new NetPvpFieldMsg_Pos(state);
                Uid = new NetPvpFieldMsg_Uid(state);
                Vals = new NetPvpFieldMsg_Vals(state);
                Icon = new NetPvpFieldMsg_Icon(state);
                Attack = new NetPvpFieldMsg_AttackFlags(state);
                Show = new NetPvpFieldMsg_Show(state);
                Step = new NetPvpFieldMsg_Step(state);
                SummoningUid = new NetPvpFieldMsg_SummoningUid(state);
                PosMask = new NetPvpFieldMsg_PosMask(state);
            }
        }

        class NetPvpFieldMsg
        {
            public PvpState State;

            public NetPvpFieldMsg(PvpState state)
            {
                State = state;
            }

            public virtual void Serialize(NetPvpFieldSerializer serializer)
            {
            }

            // TODO: Move this function into serializer
            protected void Serialize(NetPvpFieldSerializer serializer, PvpNetValueBase val, string name)
            {
                if (serializer.IsRead)
                {
                    if (serializer.HasFlag())
                    {
                        val.Read(serializer.Reader);
                        if (serializer.ValueLog != null)
                        {
                            serializer.ValueLog.Add(name + ":" + val.GetValueObj());
                        }
                    }
                }
                else
                {
                    serializer.WriteFlag(val.HasChanged);
                    if (val.HasChanged)
                    {
                        val.Write(serializer.Writer);
                    }
                }
            }
        }

        class NetPvpFieldSerializer
        {
            public bool IsRead
            {
                get { return Reader != null; }
            }
            public BinaryReader Reader;
            public BinaryWriter Writer;
            public List<string> ValueLog;
            public long FlagsWriterOffset;
            public int FlagOffset;
            public byte CurrentByteFlags;
            public int N;

            public NetPvpFieldSerializer(BinaryReader reader, List<string> valueLog, int n = 0)
            {
                Reader = reader;
                ValueLog = valueLog;
                N = n;
            }

            public NetPvpFieldSerializer(BinaryWriter writer)
            {
                Writer = writer;
            }

            public bool HasFlag()
            {
                if (FlagOffset % 8 == 0)
                {
                    CurrentByteFlags = Reader.ReadByte();
                    FlagOffset = 0;
                }
                bool result = (CurrentByteFlags & (1 << FlagOffset)) != 0;
                FlagOffset++;
                return result;
            }

            public void WriteFlag(bool changed)
            {
                if ((FlagOffset % 8) == 0)
                {
                    WriteFlags(isFinal: false);
                    FlagOffset = 0;
                }
                CurrentByteFlags = (byte)(CurrentByteFlags | (1 << FlagOffset));
                FlagOffset++;
            }

            public void WriteFlags(bool isFinal)
            {
                if (FlagsWriterOffset >= 0)
                {
                    long tempOffset = Writer.BaseStream.Position;
                    Writer.BaseStream.Position = FlagsWriterOffset;
                    Writer.Write((byte)CurrentByteFlags);
                    Writer.BaseStream.Position = tempOffset;
                }
                if (!isFinal)
                {
                    FlagsWriterOffset = Writer.BaseStream.Position;
                    Writer.Write((byte)0);
                }
            }
        }

        class NetPvpFieldMsg_Pos : NetPvpFieldMsg
        {
            public NetPvpFieldMsg_Pos(PvpState state)
                : base(state)
            {
            }

            public override void Serialize(NetPvpFieldSerializer serializer)
            {
                Serialize(serializer, State.DuelInfo.wTblNum, "wTblNum");
                State.PosTbl.Resize(State.DuelInfo.wTblNum.Value);
                for (int i = 0; i < State.DuelInfo.wTblNum.Value; i++)
                {
                    Serialize(serializer, State.PosTbl.Entries[i].Pos, "PosTbl[" + i + "].Pos");
                    Serialize(serializer, State.PosTbl.Entries[i].UId, "PosTbl[" + i + "].UId");
                    Serialize(serializer, State.PosTbl.Entries[i].Unk, "PosTbl[" + i + "].Unk");
                }
            }
        }

        class NetPvpFieldMsg_Uid : NetPvpFieldMsg
        {
            public NetPvpFieldMsg_Uid(PvpState state)
                : base(state)
            {
            }

            public override void Serialize(NetPvpFieldSerializer serializer)
            {
                Serialize(serializer, State.DuelInfo.wTblNum, "wTblNum");
                State.UidTbl.Resize(State.DuelInfo.wTblNum.Value);
                for (int i = 0; i < State.DuelInfo.wTblNum.Value; i++)
                {
                    Serialize(serializer, State.UidTbl.Entries[i].UId, "UidTbl[" + i + "].UId");
                    Serialize(serializer, State.UidTbl.Entries[i].Idx, "UidTbl[" + i + "].Idx");
                }
            }
        }

        class NetPvpFieldMsg_Vals : NetPvpFieldMsg
        {
            public NetPvpFieldMsg_Vals(PvpState state)
                : base(state)
            {
            }

            public override void Serialize(NetPvpFieldSerializer serializer)
            {
                Serialize(serializer, State.DuelInfo.wTblNum, "wTblNum");
                State.UidBases.Resize(State.DuelInfo.wTblNum.Value);
                for (int i = 0; i < State.DuelInfo.wTblNum.Value; i++)
                {
                    Serialize(serializer, State.UidBases.Entries[i].nCom, "uidBases[" + i + "].nCom");
                    Serialize(serializer, State.UidBases.Entries[i].nPos, "uidBases[" + i + "].nPos");
                    Serialize(serializer, State.UidBases.Entries[i].wUid, "uidBases[" + i + "].wUid");
                    Serialize(serializer, State.UidBases.Entries[i].stProp.cardId, "uidBases[" + i + "].stProp.cardId");
                    Serialize(serializer, State.UidBases.Entries[i].stProp.uniqueId, "uidBases[" + i + "].stProp.uniqueId");
                    Serialize(serializer, State.UidBases.Entries[i].stProp.flags, "uidBases[" + i + "].stProp.flags");
                    /*if (serializer.IsRead) <<<VVV----- removed as it's determined based on the flags (not actually seperate packet values)
                    {
                        State.UidBases.Entries[i].isFace.UpdateValue((State.UidBases.Entries[i].stProp.flags.Value & 0x200) != 0);
                        State.UidBases.Entries[i].isTurn.UpdateValue((State.UidBases.Entries[i].stProp.flags.Value & 0x400) != 0);
                    }*/
                    Serialize(serializer, State.UidBases.Entries[i].stBasicVal.CardID, "uidBases[" + i + "].stBasicVal.CardID");
                    Serialize(serializer, State.UidBases.Entries[i].stBasicVal.EffectID, "uidBases[" + i + "].stBasicVal.EffectID");
                    Serialize(serializer, State.UidBases.Entries[i].stBasicVal.Atk, "uidBases[" + i + "].stBasicVal.Atk");
                    Serialize(serializer, State.UidBases.Entries[i].stBasicVal.Def, "uidBases[" + i + "].stBasicVal.Def");
                    Serialize(serializer, State.UidBases.Entries[i].stBasicVal.OrigAtk, "uidBases[" + i + "].stBasicVal.OrigAtk");
                    Serialize(serializer, State.UidBases.Entries[i].stBasicVal.OrigDef, "uidBases[" + i + "].stBasicVal.OrigDef");
                    Serialize(serializer, State.UidBases.Entries[i].stBasicVal.Type, "uidBases[" + i + "].stBasicVal.Type");
                    Serialize(serializer, State.UidBases.Entries[i].stBasicVal.Attr, "uidBases[" + i + "].stBasicVal.Attr");
                    Serialize(serializer, State.UidBases.Entries[i].stBasicVal.Element, "uidBases[" + i + "].stBasicVal.Element");
                    Serialize(serializer, State.UidBases.Entries[i].stBasicVal.Level, "uidBases[" + i + "].stBasicVal.Level");
                    Serialize(serializer, State.UidBases.Entries[i].stBasicVal.Rank, "uidBases[" + i + "].stBasicVal.Rank");
                    Serialize(serializer, State.UidBases.Entries[i].stBasicVal.VoidMagic, "uidBases[" + i + "].stBasicVal.VoidMagic");
                    Serialize(serializer, State.UidBases.Entries[i].stBasicVal.VoidTrap, "uidBases[" + i + "].stBasicVal.VoidTrap");
                    Serialize(serializer, State.UidBases.Entries[i].stBasicVal.VoidMonst, "uidBases[" + i + "].stBasicVal.VoidMonst");
                }
            }
        }

        class NetPvpFieldMsg_Icon : NetPvpFieldMsg
        {
            public NetPvpFieldMsg_Icon(PvpState state)
                : base(state)
            {
            }

            public override void Serialize(NetPvpFieldSerializer serializer)
            {
                Serialize(serializer, State.DuelInfo.wIconNum, "wIconNum");
                State.DuelInfo.IconBases.Resize(State.DuelInfo.wIconNum.Value);
                for (int i = 0; i < State.DuelInfo.wIconNum.Value; i++)
                {
                    Serialize(serializer, State.DuelInfo.IconBases.Entries[i].player, "IconBases[" + i + "].player");
                    Serialize(serializer, State.DuelInfo.IconBases.Entries[i].pos, "IconBases[" + i + "].pos");
                    Serialize(serializer, State.DuelInfo.IconBases.Entries[i].to_player, "IconBases[" + i + "].to_player");
                    Serialize(serializer, State.DuelInfo.IconBases.Entries[i].to_pos, "IconBases[" + i + "].to_pos");
                    Serialize(serializer, State.DuelInfo.IconBases.Entries[i].icon, "IconBases[" + i + "].icon");
                }
            }
        }

        class NetPvpFieldMsg_AttackFlags : NetPvpFieldMsg
        {
            public NetPvpFieldMsg_AttackFlags(PvpState state)
                : base(state)
            {
            }

            public override void Serialize(NetPvpFieldSerializer serializer)
            {
                for (int i = 0; i < State.AttackFlags.Length; i++)
                {
                    Serialize(serializer, State.AttackFlags[i], "AttackFlags[" + i + "]");
                }
            }
        }

        class NetPvpFieldMsg_Show : NetPvpFieldMsg
        {
            public NetPvpFieldMsg_Show(PvpState state)
                : base(state)
            {
            }

            public override void Serialize(NetPvpFieldSerializer serializer)
            {
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j <= 12; j++)
                    {
                        string posPrefix = "Pos[" + i + "," + j + "].";
                        Serialize(serializer, State.DuelInfo.Pos[i, j].nShowParam, posPrefix + "nShowParam");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].nCardParam, posPrefix + "nCardParam");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].nCardDirectFlag, posPrefix + "nCardDirectFlag");
                    }
                }
            }
        }

        class NetPvpFieldMsg_Step : NetPvpFieldMsg
        {
            public NetPvpFieldMsg_Step(PvpState state)
                : base(state)
            {
            }

            public override void Serialize(NetPvpFieldSerializer serializer)
            {
                Serialize(serializer, State.DuelInfo.nCurrentStep, "nCurrentStep");
                Serialize(serializer, State.DuelInfo.nCurrentDmgStep, "nCurrentDmgStep");
            }
        }

        class NetPvpFieldMsg_SummoningUid : NetPvpFieldMsg
        {
            public NetPvpFieldMsg_SummoningUid(PvpState state)
                : base(state)
            {
            }

            public override void Serialize(NetPvpFieldSerializer serializer)
            {
                Serialize(serializer, State.summoningUid, "summoningUid");
            }
        }

        class NetPvpFieldMsg_PosMask : NetPvpFieldMsg
        {
            public NetPvpFieldMsg_PosMask(PvpState state)
                : base(state)
            {
            }

            public override void Serialize(NetPvpFieldSerializer serializer)
            {
                for (int i = 0; i < State.posMaskList.Count; i++)
                {
                    Serialize(serializer, State.posMaskList[i], "posMaskList[" + i + "]");
                }
            }
        }

        class NetPvpFieldMsg_Prop : NetPvpFieldMsg
        {
            public NetPvpFieldMsg_Prop(PvpState state)
                : base(state)
            {
            }

            public override void Serialize(NetPvpFieldSerializer serializer)
            {
                Serialize(serializer, State.DuelInfo.isQuick, "isQuick");
                Serialize(serializer, State.DuelInfo.nTurnNum, "nTurnNum");
                Serialize(serializer, State.DuelInfo.nCurrentPhase, "nCurrentPhase");
                Serialize(serializer, State.DuelInfo.bWhichTurnNow, "bWhichTurnNow");
                Serialize(serializer, State.DuelInfo.nMovablePhase, "nMovablePhase");
                for (int i = 0; i < State.DuelInfo.nLP.Length; i++)
                {
                    Serialize(serializer, State.DuelInfo.nLP[i], "nLP[" + i + "]");
                    Serialize(serializer, State.DuelInfo.nDoPutMonst[i], "nDoPutMonst[" + i + "]");
                    Serialize(serializer, State.DuelInfo.bDoSummon[i], "bDoSummon[" + i + "]");
                    Serialize(serializer, State.DuelInfo.bDoSpSummon[i], "bDoSpSummon[" + i + "]");
                    for (int j = 0; j < State.DuelInfo.Pos.GetLength(1); j++)
                    {
                        string posPrefix = "Pos[" + i + "," + j + "].";
                        Serialize(serializer, State.DuelInfo.Pos[i, j].nComBit, posPrefix + "nComBit");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].wMrk, posPrefix + "wMrk");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].wTurnCounter, posPrefix + "wTurnCounter");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].bTopIdx, posPrefix + "bTopIdx");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].bEffectFlags, posPrefix + "bEffectFlags");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].bMonstOrgLevel, posPrefix + "bMonstOrgLevel");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].bMonstOrgType, posPrefix + "bMonstOrgType");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].bZoneAvailable, posPrefix + "bZoneAvailable");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].bZoneAvailable2, posPrefix + "bZoneAvailable2");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].bCardInBattle, posPrefix + "bCardInBattle");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].bNormalMonster, posPrefix + "bNormalMonster");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].bPendScale, posPrefix + "bPendScale");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].bPendOrgScale, posPrefix + "bPendOrgScale");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].bMostRank, posPrefix + "bMostRank");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].bMostOrgRank, posPrefix + "bMostOrgRank");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].bTrapMonster, posPrefix + "bTrapMonster");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].bTunerMonster, posPrefix + "bTunerMonster");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].wOverlayNum, posPrefix + "wOverlayNum");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].wCardNum, posPrefix + "wCardNum");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].bFightable, posPrefix + "bFightable");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].wEquip, posPrefix + "wEquip");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].wContinuous, posPrefix + "wContinuous");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].bIsMagic, posPrefix + "bIsMagic");
                        Serialize(serializer, State.DuelInfo.Pos[i, j].bIsTrap, posPrefix + "bIsTrap");
                        for (int k = 0; k < State.DuelInfo.Pos[i, j].bCounter.Length; k++)
                        {
                            Serialize(serializer, State.DuelInfo.Pos[i, j].bCounter[k], posPrefix + "bCounter[" + k + "]");
                        }
                        for (int k = 0; k < State.DuelInfo.Pos[i, j].nOtherEffect.Length; k++)
                        {
                            Serialize(serializer, State.DuelInfo.Pos[i, j].nOtherEffect[k], posPrefix + "nOtherEffect[" + k + "]");
                        }
                    }
                }
                Serialize(serializer, State.DuelInfo.wTblNum, "wTblNum");
                Serialize(serializer, State.DuelInfo.wIconNum, "wIconNum");
            }
        }

        abstract class PvpNetValueBase
        {
            public bool HasChanged;
            public abstract TypeCode Type { get; }
            public abstract object GetValueObj();
            public abstract TType GetValue<TType>();
            public abstract void SetValue<TType>(TType value);
            public abstract void UpdateValue<TType>(TType value);

            public void Read(BinaryReader br)
            {
                switch (Type)
                {
                    case TypeCode.Boolean: UpdateValue(br.ReadByte() > 0); break;
                    case TypeCode.SByte: UpdateValue(br.ReadSByte()); break;
                    case TypeCode.Byte: UpdateValue(br.ReadByte()); break;
                    case TypeCode.Int16: UpdateValue(IPAddress.NetworkToHostOrder(br.ReadInt16())); break;
                    case TypeCode.UInt16: UpdateValue((ushort)IPAddress.NetworkToHostOrder(br.ReadInt16())); break;
                    case TypeCode.Int32: UpdateValue(IPAddress.NetworkToHostOrder(br.ReadInt32())); break;
                    case TypeCode.UInt32: UpdateValue((uint)IPAddress.NetworkToHostOrder(br.ReadInt32())); break;
                    case TypeCode.Int64: UpdateValue(IPAddress.NetworkToHostOrder(br.ReadInt64())); break;
                    case TypeCode.UInt64: UpdateValue((ulong)IPAddress.NetworkToHostOrder(br.ReadInt64())); break;
                }
            }

            public void Write(BinaryWriter bw)
            {
                switch (Type)
                {
                    case TypeCode.Boolean: bw.Write((byte)(GetValue<bool>() ? 1 : 0)); break;
                    case TypeCode.SByte: bw.Write(GetValue<sbyte>()); break;
                    case TypeCode.Byte: bw.Write(GetValue<byte>()); break;
                    case TypeCode.Int16: bw.Write(IPAddress.HostToNetworkOrder(GetValue<short>())); break;
                    case TypeCode.UInt16: bw.Write(IPAddress.HostToNetworkOrder((short)GetValue<ushort>())); break;
                    case TypeCode.Int32: bw.Write(IPAddress.HostToNetworkOrder(GetValue<int>())); break;
                    case TypeCode.UInt32: bw.Write(IPAddress.HostToNetworkOrder((int)GetValue<uint>())); break;
                    case TypeCode.Int64: bw.Write(IPAddress.HostToNetworkOrder(GetValue<long>())); break;
                    case TypeCode.UInt64: bw.Write(IPAddress.HostToNetworkOrder((long)GetValue<ulong>())); break;
                }
            }
        }

        class PvpNetValue<T> : PvpNetValueBase where T : IConvertible
        {
            static TypeCode typeCode = System.Type.GetTypeCode(typeof(T));
            static CultureInfo culture = CultureInfo.InvariantCulture;

            public override TypeCode Type
            {
                get { return typeCode; }
            }

            T currentValue;
            public T Value
            {
                get { return currentValue; }
                set
                {
                    if (!currentValue.Equals(value))
                    {
                        currentValue = value;
                        HasChanged = true;
                    }
                }
            }

            public override object GetValueObj()
            {
                return currentValue;
            }

            public override TType GetValue<TType>()
            {
                return (TType)Convert.ChangeType(currentValue, typeof(TType));
            }

            public override void SetValue<TType>(TType value)
            {
                Value = (T)Convert.ChangeType(value, typeof(T));
            }

            public override void UpdateValue<TType>(TType value)
            {
                currentValue = (T)Convert.ChangeType(value, typeof(T));
            }

            public void Update(T value)
            {
                currentValue = value;
            }
        }

        private static bool checkFlag(BinaryReader br, List<byte> updateFlag, ref int count)
        {
            bool result = false;
            if (count % 8 == 0)
            {
                updateFlag.Add(br.ReadByte());
            }
            byte b = updateFlag[count / 8];
            if (((int)b & 1 << count % 8) > 0)
            {
                result = true;
            }
            count++;
            return result;
        }

        /// <summary>
        /// Engine.PvpUpdateEngineData
        /// </summary>
        public static bool ParseEngineData(PvpFieldType type, byte[] data, int n, List<string> entries)
        {
            bool handled = true;
            using (MemoryStream memoryStream = new MemoryStream(data))
            using (BinaryReader binaryReader = new BinaryReader(memoryStream))
            {
                switch (type)
                {
                    case PvpFieldType.Prop:
                        {
                            tempState.FieldMsgs.Prop.Serialize(new NetPvpFieldSerializer(binaryReader, entries));

                        }
                        break;
                    case PvpFieldType.Pos:
                        {
                            tempState.FieldMsgs.Pos.Serialize(new NetPvpFieldSerializer(binaryReader, entries));
                        }
                        break;
                    case PvpFieldType.Uid:
                        {
                            tempState.FieldMsgs.Uid.Serialize(new NetPvpFieldSerializer(binaryReader, entries));
                        }
                        break;
                    case PvpFieldType.Vals:
                        {
                            tempState.FieldMsgs.Vals.Serialize(new NetPvpFieldSerializer(binaryReader, entries));
                        }
                        break;
                    case PvpFieldType.Icon:
                        {
                            tempState.FieldMsgs.Icon.Serialize(new NetPvpFieldSerializer(binaryReader, entries));
                        }
                        break;
                    case PvpFieldType.Skill:
                        {
                            // These skills are speed duel skills (Duel Links). They are not used in Master Duel.
                            handled = false;
                        }
                        break;
                    case PvpFieldType.Rare:
                        {
                            tempState.Rare.Clear();
                            for (int i = 0; i < n; i++)
                            {
                                ushort uid = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                                ushort rare = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                                tempState.Rare[uid] = rare;
                            }
                        }
                        break;
                    case PvpFieldType.Attack:
                        {
                            tempState.FieldMsgs.Attack.Serialize(new NetPvpFieldSerializer(binaryReader, entries));
                        }
                        break;
                    case PvpFieldType.Show:
                        {
                            tempState.FieldMsgs.Show.Serialize(new NetPvpFieldSerializer(binaryReader, entries));
                        }
                        break;
                    case PvpFieldType.Step:
                        {
                            tempState.FieldMsgs.Step.Serialize(new NetPvpFieldSerializer(binaryReader, entries));
                        }
                        break;
                    case PvpFieldType.SummoningUid:
                        {
                            tempState.FieldMsgs.SummoningUid.Serialize(new NetPvpFieldSerializer(binaryReader, entries));
                        }
                        break;
                    case PvpFieldType.PosMask:
                        {
                            tempState.FieldMsgs.PosMask.Serialize(new NetPvpFieldSerializer(binaryReader, entries));
                        }
                        break;
                    default:
                        handled = false;
                        break;
                }
                if (handled && binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
                {
                    entries.Add("[INVALID_FIELD_DATA_LEN(" + binaryReader.BaseStream.Position + "," + binaryReader.BaseStream.Length + "]");
                }
                if (!handled)
                {
                    System.Diagnostics.Debug.WriteLine(YgoMasterInspectorHelper.ToHex(data));
                }
                return handled;
            }
        }

        /// <summary>
        /// Engine.ParseRecvData - adds PvpCommand instances to a queue which is then handled by Engine.PvpAct
        /// </summary>
        public static void ParseRecvData(byte[] recvData, List<string> entries)
        {
            Queue<PvpCommand> commands = new Queue<PvpCommand>();

            using (MemoryStream memoryStream = new MemoryStream(recvData))
            using (BinaryReader binaryReader = new BinaryReader(memoryStream))
            {
                while (binaryReader.BaseStream.Position < recvData.Length)
                {
                    long tempPos = binaryReader.BaseStream.Position;
                    byte[] remain = binaryReader.ReadBytes((int)(binaryReader.BaseStream.Length - binaryReader.BaseStream.Position));
                    binaryReader.BaseStream.Position = tempPos;
                    //System.Diagnostics.Debug.WriteLine(YgoMasterInspectorHelper.ToHex(remain));

                    PvpCommandType cmdType = (PvpCommandType)binaryReader.ReadByte();
                    int[] param = new int[4];
                    for (int i = 0; i < param.Length; i++)
                    {
                        param[i] = IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
                    }
                    object data = null;
                    bool addCommand = true;
                    switch (cmdType)
                    {
                        case PvpCommandType.Input:
                            {
                                // Not 100% sure if this is correct
                                MenuActType inputType = (MenuActType)param[1];
                                if (/*param[0] > 1 && */ inputType == MenuActType.Location)
                                {
                                    int mixValueArrayLen = IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
                                    int mixNum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
                                    uint[] mixValue = new uint[mixValueArrayLen];
                                    for (int i = 0; i < mixValueArrayLen; i++)
                                    {
                                        mixValue[i] = (uint)IPAddress.NetworkToHostOrder(binaryReader.ReadUInt32());
                                    }
                                    data = new object[]
                                        {
                                            mixNum,
                                            mixValue
                                        };
                                }
                            }
                            break;
                        case PvpCommandType.List:
                            data = PvpParseListData(binaryReader);
                            break;
                        case PvpCommandType.Dialog:
                            data = PvpParseDialogData(binaryReader);
                            break;
                        case PvpCommandType.Effect:// no "data"
                            break;
                        case PvpCommandType.Field:
                            {
                                PvpFieldType pvpFieldType = (PvpFieldType)param[1];
                                int fieldDataLen = param[0] >= 0 ? param[0] : 0;
                                byte[] fieldData = binaryReader.ReadBytes(fieldDataLen);
                                if (pvpFieldType <= PvpFieldType.End)
                                {
                                    data = new object[]
                                        {
                                            pvpFieldType,
                                            fieldData,
                                            param[2]
                                        };
                                }
                                else
                                {
                                    entries.Add("InvalidPvpFieldType: " + pvpFieldType);
                                }
                            }
                            break;
                        case PvpCommandType.Data:// no "data" (and not handled in PvpAct)
                            break;
                        case PvpCommandType.Fusion:
                            data = PvpParseFusionData(binaryReader);
                            break;
                        case PvpCommandType.Time:// no "data"
                            break;
                        case PvpCommandType.ListFrom:
                            {
                                int listFromCount = IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
                                int[] listFrom = new int[listFromCount];
                                for (int i = 0; i < listFrom.Length; i++)
                                {
                                    listFrom[i] = IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
                                }
                                data = listFrom;
                            }
                            break;
                        case PvpCommandType.FlipInfo:
                            {
                                addCommand = false;
                                int num3 = param[0];
                                if (num3 > 0)
                                {
                                    if (param[1] != -1 && param[2] != -1)
                                    {
                                        param[0] = param[1];
                                        param[1] = param[2];
                                        commands.Enqueue(new PvpCommand(cmdType, param, null));
                                    }
                                    else
                                    {
                                        entries.Add("FlipInfo(count): " + num3);
                                        for (int i = 0; i < num3; i++)
                                        {
                                            param[0] = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
                                            param[1] = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
                                            commands.Enqueue(new PvpCommand(cmdType, param, null));
                                        }
                                    }
                                }
                            }
                            break;
                        case PvpCommandType.FinishAttack:// no "data" (and no params / handler)
                            //Engine.s_instance.pvpWork.currentEngineData.attackFinish = true;
                            break;
                        case PvpCommandType.MrkList:
                            {
                                int mrkCount = param[0];
                                if (mrkCount > 0)
                                {
                                    ushort[] mrkList = new ushort[mrkCount];
                                    for (int i = 0; i < mrkList.Length; i++)
                                    {
                                        mrkList[i] = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                                    }
                                    data = mrkList;
                                }
                            }
                            break;
                        case PvpCommandType.FusionNeed:// no "data"
                            //Engine.s_instance.pvpWork.currentEngineData.syncNeed = (byte)param[0];
                            break;
                        case PvpCommandType.TunerLevel:// no "data"?
                            // TODO: Handle param logic
                            break;
                        case PvpCommandType.CutinActivate:// no "data"?
                            // TODO: Handle param logic
                            //Engine.s_instance.pvpWork.currentEngineData.effectIdAtChain = param[0];
                            break;
                        default:
                            entries.Add("[WARNING] didn't parse " + cmdType);
                            binaryReader.ReadBytes(param[3]);
                            break;
                    }
                    if (addCommand)
                    {
                        commands.Enqueue(new PvpCommand(cmdType, param, data));
                    }
                }
            }

            while (commands.Count > 0)
            {
                PvpCommand cmd = commands.Dequeue();
                switch (cmd.type)
                {
                    case PvpCommandType.Input:
                        {
                            //ViewType viewType = ViewType.WaitInput;
                            MenuActType inputType = (MenuActType)cmd.param[1];
                            StringBuilder mixStr = new StringBuilder();
                            if (cmd.data != null)
                            {
                                mixStr.Append("{");
                                mixStr.Append("mixNum:" + ((object[])cmd.data)[0]);
                                mixStr.Append(",mixValue:{" + string.Join(",", (int[])((object[])cmd.data)[1]) + "}");
                                mixStr.Append("}");
                            }
                            entries.Add("pvpCmd:" + cmd.type + " p0:" + cmd.param[0] + " inputType:" + inputType + " p2:" + cmd.param[2] + " p3:" + cmd.param[3] + " mix:" + mixStr.ToString());
                        }
                        break;
                    case PvpCommandType.Effect:
                        {
                            // p0 = id (the view type)
                            // p1,p2,p3 = param1,param2,param3 (on runEffectCallback)
                            ViewType viewType = (ViewType)cmd.param[0];
                            entries.Add("pvpCmd:" + cmd.type + " viewType:" + viewType + " p1:" + cmd.param[1] + " p2:" + cmd.param[2] + " p3:" + cmd.param[3]);
                        }
                        break;
                    case PvpCommandType.List:
                    case PvpCommandType.Dialog:
                    case PvpCommandType.Fusion:
                        {
                            entries.Add("pvpCmd:" + cmd.type + " p0:" + cmd.param[0] + " p1:" + cmd.param[1] + " p2:" + cmd.param[2] + " p3:" + cmd.param[3] + " data:" + cmd.data);
                        }
                        break;
                    case PvpCommandType.Time:
                        {
                            uint timeLeft = (uint)cmd.param[0];
                            uint timeTotal = (uint)cmd.param[1];
                            entries.Add("pvpCmd:" + cmd.type + " timeLeft:" + timeLeft + " timeTotal:" + timeTotal + " p2:" + cmd.param[2] + " p3:" + cmd.param[3]);
                        }
                        break;
                    case PvpCommandType.ListFrom:
                        {
                            int[] listFrom = cmd.data as int[];
                            string dataStr = "{" + (listFrom == null ? "null" : string.Join(",", listFrom)) + "}";
                            entries.Add("pvpCmd:" + cmd.type + " p0:" + cmd.param[0] + " p1:" + cmd.param[1] + " p2:" + cmd.param[2] + " p3:" + cmd.param[3] + " data:" + dataStr);
                        }
                        break;
                    case PvpCommandType.MrkList:
                        {
                            ushort[] mrkList = cmd.data as ushort[];
                            string dataStr = "{" + (mrkList == null ? "null" : string.Join(",", mrkList)) + "}";
                            entries.Add("pvpCmd:" + cmd.type + " p0:" + cmd.param[0] + " p1:" + cmd.param[1] + " p2:" + cmd.param[2] + " p3:" + cmd.param[3] + " data:" + dataStr);
                        }
                        break;
                    case PvpCommandType.Field:
                        {
                            string fieldDataStr = null;
                            object[] fieldData = cmd.data as object[];
                            if (fieldData != null)
                            {
                                fieldDataStr = YgoMasterInspectorHelper.ToHex(fieldData[1] as byte[]);
                                List<string> fieldStrings = new List<string>();
                                if (ParseEngineData((PvpFieldType)fieldData[0], fieldData[1] as byte[], (int)fieldData[2], fieldStrings))
                                {
                                    fieldDataStr = string.Join("|", fieldStrings);
                                }
                                else
                                {
                                    fieldDataStr += " ----- " + string.Join("|", fieldStrings);
                                }
                            }
                            entries.Add("pvpCmd:" + cmd.type + " fieldDataLen:" + cmd.param[0] + " pvpFieldType:" + (PvpFieldType)cmd.param[1] + " unk(p2):" + cmd.param[2] + " p3:" + cmd.param[3] + " fieldData:" + fieldDataStr);
                        }
                        break;
                    default:
                        entries.Add("pvpCmd:" + cmd.type + " p0:" + cmd.param[0] + " p1:" + cmd.param[1] + " p2:" + cmd.param[2] + " p3:" + cmd.param[3]);
                        break;
                }
            }
        }

        private static PvpListData PvpParseListData(BinaryReader br)
        {
            int mixValueArrayLen = IPAddress.NetworkToHostOrder(br.ReadInt32());
            PvpListData pvpListData = new PvpListData();
            pvpListData.listType = (ListType)IPAddress.NetworkToHostOrder(br.ReadInt16());
            pvpListData.selMax = br.ReadByte();
            pvpListData.selMin = br.ReadByte();
            pvpListData.itemMax = IPAddress.NetworkToHostOrder(br.ReadInt16());
            pvpListData.itemUids = new ushort[pvpListData.itemMax];
            pvpListData.itemAttributes = new uint[pvpListData.itemMax];
            pvpListData.itemIds = new uint[pvpListData.itemMax];
            for (int i = 0; i < pvpListData.itemMax; i++)
            {
                pvpListData.itemUids[i] = (ushort)IPAddress.NetworkToHostOrder(br.ReadInt16());
                pvpListData.itemAttributes[i] = (uint)IPAddress.NetworkToHostOrder(br.ReadInt32());
                pvpListData.itemIds[i] = (uint)IPAddress.NetworkToHostOrder(br.ReadInt32());
            }
            ////////////////////////
            pvpListData.mixNum = IPAddress.NetworkToHostOrder(br.ReadInt32());
            pvpListData.mixValue = new uint[mixValueArrayLen];
            for (int i = 0; i < mixValueArrayLen; i++)
            {
                pvpListData.mixValue[i] = (uint)IPAddress.NetworkToHostOrder(br.ReadInt32());
            }
            ////////////////////////
            // itemFrom is assigned based on the NEXT command (a PvpCommandType.ListFrom command, the data for it)
            // - Effect also does this
            return pvpListData;
        }

        private static PvpDialogData PvpParseDialogData(BinaryReader br)
        {
            PvpDialogData pvpDialogData = new PvpDialogData();
            pvpDialogData.posMaskSummon = (uint)IPAddress.NetworkToHostOrder(br.ReadInt32());
            int mixValueArrayLen = IPAddress.NetworkToHostOrder(br.ReadInt32());
            pvpDialogData.selMax = IPAddress.NetworkToHostOrder(br.ReadInt32());
            pvpDialogData.sel = new uint[pvpDialogData.selMax];
            pvpDialogData.dlgType = (DialogType)IPAddress.NetworkToHostOrder(br.ReadInt32());
            pvpDialogData.player = IPAddress.NetworkToHostOrder(br.ReadInt32());
            ////////////////////////
            pvpDialogData.mixNum = IPAddress.NetworkToHostOrder(br.ReadInt32());
            pvpDialogData.mixValue = new uint[mixValueArrayLen];
            for (int i = 0; i < mixValueArrayLen; i++)
            {
                pvpDialogData.mixValue[i] = (uint)IPAddress.NetworkToHostOrder(br.ReadInt32());
            }
            ////////////////////////
            for (int i = 0; i < pvpDialogData.selMax; i++)
            {
                pvpDialogData.sel[i] = (uint)IPAddress.NetworkToHostOrder(br.ReadInt32());
            }
            return pvpDialogData;
        }

        private static PvpFusionData PvpParseFusionData(BinaryReader br)
        {
            PvpFusionData pvpFusionData = new PvpFusionData();
            int num = IPAddress.NetworkToHostOrder(br.ReadInt32());
            pvpFusionData.material = new int[num];
            pvpFusionData.mrk = new int[num];
            for (int i = 0; i < num; i++)
            {
                pvpFusionData.material[i] = IPAddress.NetworkToHostOrder(br.ReadInt32());
            }
            for (int i = 0; i < num; i++)
            {
                pvpFusionData.mrk[i] = IPAddress.NetworkToHostOrder(br.ReadInt32());
            }
            return pvpFusionData;
        }

        public class PvpListData
        {
            public ListType listType;
            public int selMax;
            public int selMin;
            public short itemMax;
            public ushort[] itemUids;
            public uint[] itemAttributes;
            public int[] itemFrom;
            public uint[] itemIds;

            // Engine.PvpWork.mixValue/mixNum/mixvalQueue
            public int mixNum;
            public uint[] mixValue;

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                sb.Append("listType:" + listType);
                sb.Append(",selMax:" + selMax);
                sb.Append(",selMin:" + selMin);
                sb.Append(",itemMax:" + itemMax);
                sb.Append(",itemUids:{" + string.Join(",", itemUids) + "}");
                sb.Append(",itemAttributes:{" + string.Join(",", itemAttributes) + "}");
                sb.Append(",itemFrom:{" + (itemFrom == null ? "null" : string.Join(",", itemFrom)) + "}");
                sb.Append(",mixNum:" + mixNum);
                sb.Append(",mixValue:{" + string.Join(",", mixValue) + "}");
                sb.Append("}");
                return sb.ToString();
            }
        }

        public class PvpDialogData
        {
            public DialogType dlgType;
            public int player;
            public int selMax;
            public uint[] sel;
            public uint posMaskSummon;

            // Engine.PvpWork.mixValue/mixNum/mixvalQueue
            public int mixNum;
            public uint[] mixValue;

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                sb.Append("dlgType:" + dlgType);
                sb.Append(",player:" + player);
                sb.Append(",selMax:" + selMax);
                sb.Append(",sel:{" + string.Join(",", sel) + "}");
                sb.Append(",posMaskSummon:" + posMaskSummon);
                sb.Append(",mixNum:" + mixNum);
                sb.Append(",mixValue:{" + string.Join(",", mixValue) + "}");
                sb.Append("}");
                return sb.ToString();
            }
        }

        public class PvpFusionData
        {
            public int[] material;
            public int[] mrk;

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                sb.Append("material:{" + string.Join(",", material) + "}");
                sb.Append(",mixValue:{" + string.Join(",", mrk) + "}");
                sb.Append("}");
                return sb.ToString();
            }
        }

        public enum PvpFieldType
        {
            Prop = 1,
            Pos = 2,
            Uid = 3,
            Vals = 4,
            Icon = 5,
            Skill = 6,
            Rare = 7,
            Attack = 8,
            Show = 9,
            Step = 10,
            SummoningUid = 11,
            PosMask = 12,
            End = 13,
        }

        public class PvpCommand
        {
            public Engine.PvpCommandType type;
            public int[] param;
            public object data;

            public PvpCommand(Engine.PvpCommandType type, int[] param, object data)
            {
                this.param = new int[param.Length];
                Array.Copy(param, this.param, param.Length);
                this.type = type;
                this.data = data;
            }
        }

        public enum PvpCommandType
        {
            Input = 0,
            List = 1,
            Dialog = 2,
            Effect = 3,
            Field = 4,
            Data = 5,
            Fusion = 6,
            Time = 7,
            ListFrom = 8,
            FlipInfo = 9,
            FinishAttack = 10,
            MrkList = 11,
            FusionNeed = 12,
            TunerLevel = 13,
            CutinActivate = 14,
        }

        public enum ListType
        {
            Null = 0,
            Fusion = 1,
            Deck = 2,
            Grave = 3,
            Exclude = 4,
            View = 5,
            Select = 6,
            SelectMax = 38,
            Selectable = 39,
            SelectableMax = 71,
            SelUpTo = 72,
            SelUpToMax = 104,
            SelFree = 105,
            SelFreeMax = 137,
            BlindSelect = 138,
            SelAllCard = 139,
            SelAllDeck = 140,
            SelAllMonst = 141,
            SelAllMonst2 = 142,
            SelAllGadget = 143,
            SelAllIndeck = 144,
        }

        public enum DialogType
        {
            Ok,
            Info,
            Confirm,
            YesNo,
            Effect,
            Sort,
            Select,
            Phase,
            SelType,
            SelAttr,
            SelStand,
            SelCoin,
            SelDice,
            SelNum,
            Final,
            Result,
            Discard,
            Ritual,
            Update,
            Close
        }

        public enum CommandType
        {
            Attack = 0,
            Look = 1,
            SummonSp = 2,
            Action = 3,
            Summon = 4,
            Reverse = 5,
            SetMonst = 6,
            Set = 7,
            Pendulum = 8,
            TurnAtk = 9,
            TurnDef = 10,
            Surrender = 11,
            Decide = 12,
            Draw = 13,
        }

        public enum ViewType
        {
            Noop = -1,
            Null,
            DuelStart,
            DuelEnd,
            WaitFrame,
            WaitInput,
            PhaseChange,
            TurnChange,
            FieldChange,
            CursorSet,
            BgmUpdate,
            BattleInit,
            BattleSelect,
            BattleAttack,
            BattleRun,
            BattleEnd,
            LifeSet,
            LifeDamage,
            LifeReset,
            HandShuffle,
            HandShow,
            HandOpen,
            DeckShuffle,
            DeckReset,
            DeckFlipTop,
            GraveTop,
            CardLockon,
            CardMove,
            CardSwap,
            CardFlipTurn,
            CardCheat,
            CardSet,
            CardVanish,
            CardBreak,
            CardExplosion,
            CardExclude,
            CardHappen,
            CardDisable,
            CardEquip,
            CardIncTurn,
            CardUpdate,
            ManaSet,
            MonstDeathTurn,
            MonstShuffle,
            TributeSet,
            TributeReset,
            TributeRun,
            MaterialSet,
            MaterialReset,
            MaterialRun,
            TuningSet,
            TuningReset,
            TuningRun,
            ChainSet,
            ChainRun,
            RunSurrender,
            RunDialog,
            RunList,
            RunSummon,
            RunSpSummon,
            RunFusion,
            RunDetail,
            RunCoin,
            RunDice,
            RunYujyo,
            RunSpecialWin,
            RunVija,
            RunExtra,
            RunCommand,
            CutinDraw,
            CutinSummon,
            CutinFusion,
            CutinChain,
            CutinActivate,
            CutinSet,
            CutinReverse,
            CutinTurn,
            CutinFlip,
            CutinTurnEnd,
            CutinDamage,
            CutinBreak,
            CpuThinking,
            HandRundom,
            OverlaySet,
            OverlayReset,
            OverlayRun,
            CutinSuccess,
            ChainEnd,
            LinkSet,
            LinkReset,
            LinkRun,
            RunJanken,
            CutinCoinDice,
            ChainStep
        }

        public enum MenuActType
        {
            Null,
            DrawPhase,
            MainPhase,
            BattlePhase,
            CheckTiming,
            CheckChain,
            SummonChance,
            Location,
            Selection,
            LockOn
        }
    }

    class Pvp
    {
        public enum Command
        {
            ENTRY = 0,//c->s
            INIT = 1,
            WAIT = 2,
            READY = 3,//s->c
            /// <summary>
            /// Various commands (e.g. placing a card on the field)
            /// </summary>
            COMMAND = 4,//c->s
            EFFECT = 5,
            /// <summary>
            /// Used to cancel various things (e.g. prompts for action such as trap activation)
            /// </summary>
            CANCEL = 6,//c->s
            /// <summary>
            /// Dialog result
            /// </summary>
            RESULT = 7,//c->s
            DBGCMD = 8,
            CHEATCARD = 9,
            CHAT = 10,
            /// <summary>
            /// List selection response (most likely dialogs which are lists - see YgomGame.DuelYgomGame.Duel.EmotionalList)
            /// </summary>
            LIST = 11,//c->s
            /// <summary>
            /// Changing phase
            /// </summary>
            PHASE = 12,//c->s
            SKILL = 13,
            /// <summary>
            /// Response from an EXIT request? And / or getting kicked?
            /// </summary>
            LEAVE = 14,//s->c
            /// <summary>
            /// Requesting to leave (quit replay, replay error handler, SysAct error handler)
            /// </summary>
            EXIT = 15,//c->s
            /// <summary>
            /// Recovering from a disconnect / network hang?
            /// </summary>
            RECOVERY = 16,//s->c
            /// <summary>
            /// Number of watchers / spectators (single integer)
            /// </summary>
            WATCH = 17,//s->c
            /// <summary>
            /// Surrender request
            /// </summary>
            SURRENDER = 18,//c->s
            /// <summary>
            /// Latency to server (two integers)
            /// </summary>
            LATENCY = 19,//s->c
            SEND = 20,
            RECV = 21,
            /// <summary>
            /// How long you took to complete an action / input?
            /// </summary>
            TIME = 22,//c->s
            TURN = 23,
            /// <summary>
            /// Various bits of duel engine data
            /// </summary>
            DATA = 50,//s->c
            /// <summary>
            /// Replay data
            /// </summary>
            REPLAY = 60,//s->c
            /// <summary>
            /// Indicates time has ran out?
            /// </summary>
            TIMEUP = 97,//s->c
            /// <summary>
            /// Duel finished
            /// </summary>
            FINISH = 98,//s->c
            /// <summary>
            /// Polling for duel updates
            /// </summary>
            POLL = 99,//c->s
            /// <summary>
            /// Generic error?
            /// </summary>
            ERROR = 100,//s->c
            /// <summary>
            /// Generic fatal error?
            /// </summary>
            FATAL = 900,//s->c
            CONNECT = 1000,
            RECONNECT = 1001,
            CLOSE = 1003,
            PING = 1004,
            PONG = 1005,
            MATCH = 1006,
            DROP = 1007,
            MATCH_UPDATE = 1010,
            MATCH_LIST = 1011,
            INFO = 1012
        }
    }
}
