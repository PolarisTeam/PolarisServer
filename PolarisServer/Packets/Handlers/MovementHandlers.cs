using PolarisServer.Models;
using System;
using System.Runtime.InteropServices;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x04, 0x07)]
    public class MovementHandler : PacketHandler
    {
        #region implemented abstract members of PacketHandler

        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            PacketReader reader = new PacketReader(data);
            // This packet is "Compressed" basically.
            reader.ReadBytes(6); // Get past the junk
            // For simplicity's sake, read the 3 flag bytes into a big int
            byte[] flagBytes = reader.ReadBytes(3);
            uint dataFlags = flagBytes[0];
            dataFlags |= (uint)(flagBytes[1] << 8);
            dataFlags |= (uint)(flagBytes[2] << 16);

            PackedData theFlags = (PackedData)dataFlags;

            // Debug
            Logger.WriteInternal("[MOV] Movement packet from {0} contains {1} data.", context.Character.Name, theFlags);

            // TODO: Maybe do this better someday
            FullMovementData dstData = new FullMovementData();

            if(theFlags.HasFlag(PackedData.ENT1_ID))
            {
                dstData.entity1.ID = reader.ReadUInt64();
            }
            if(theFlags.HasFlag(PackedData.ENT1_TYPE))
            {
                dstData.entity1.EntityType = (EntityType)reader.ReadUInt16();
            }
            if(theFlags.HasFlag(PackedData.ENT1_A))
            {
                dstData.entity1.Unknown_A = reader.ReadUInt16();
            }
            if (theFlags.HasFlag(PackedData.ENT2_ID))
            {
                dstData.entity1.ID = reader.ReadUInt64();
            }
            if (theFlags.HasFlag(PackedData.ENT2_TYPE))
            {
                dstData.entity1.EntityType = (EntityType)reader.ReadUInt16();
            }
            if (theFlags.HasFlag(PackedData.ENT2_A))
            {
                dstData.entity1.Unknown_A = reader.ReadUInt16();
            }
            if (theFlags.HasFlag(PackedData.UNKNOWN1))
            {
                dstData.unknown1 = reader.ReadUInt32();
            }
            if(theFlags.HasFlag(PackedData.ROT_X))
            {
                dstData.rotation.x = reader.ReadUInt16();
            }
            if (theFlags.HasFlag(PackedData.ROT_Y))
            {
                dstData.rotation.y = reader.ReadUInt16();
            }
            if (theFlags.HasFlag(PackedData.ROT_Z))
            {
                dstData.rotation.z = reader.ReadUInt16();
            }
            if (theFlags.HasFlag(PackedData.ROT_W))
            {
                dstData.rotation.w = reader.ReadUInt16();
            }
            if (theFlags.HasFlag(PackedData.LAST_X))
            {
                dstData.lastPos.x = reader.ReadUInt16();
            }
            if (theFlags.HasFlag(PackedData.LAST_Y))
            {
                dstData.lastPos.y = reader.ReadUInt16();
            }
            if (theFlags.HasFlag(PackedData.LAST_Z))
            {
                dstData.lastPos.z = reader.ReadUInt16();
            }
            if (theFlags.HasFlag(PackedData.UNKNOWN4))
            {
                dstData.Unknown2 = reader.ReadUInt16();
            }
            if (theFlags.HasFlag(PackedData.CUR_X))
            {
                dstData.currentPos.x = reader.ReadUInt16();
                context.CurrentLocation.PosX = Helper.FloatFromHalfPrecision(dstData.currentPos.x);
            }
            if (theFlags.HasFlag(PackedData.CUR_Y))
            {
                dstData.currentPos.y = reader.ReadUInt16();
                context.CurrentLocation.PosY = Helper.FloatFromHalfPrecision(dstData.currentPos.y);
            }
            if (theFlags.HasFlag(PackedData.CUR_Z))
            {
                dstData.currentPos.z = reader.ReadUInt16();
                context.CurrentLocation.PosZ = Helper.FloatFromHalfPrecision(dstData.currentPos.z);
            }
            if (theFlags.HasFlag(PackedData.UNKNOWN5))
            {
                dstData.Unknown3 = reader.ReadUInt16();
            }
            if (theFlags.HasFlag(PackedData.UNKNOWN6))
            {
                if (theFlags.HasFlag(PackedData.UNKNOWN7))
                {
                    dstData.Unknown4 = reader.ReadByte();
                } else
                {
                    dstData.Unknown4 = reader.ReadUInt32();
                }
            }

            if (theFlags.HasFlag(PackedData.CUR_X) || theFlags.HasFlag(PackedData.CUR_Y) || theFlags.HasFlag(PackedData.CUR_Z))
            {
                Logger.WriteInternal("[MOV] Player moving! {0} -> ({1}, {2}, {3})", context.Character.Name, context.CurrentLocation.PosX,
                    context.CurrentLocation.PosY, context.CurrentLocation.PosZ);
            }
        }

        #endregion
    }

    [PacketHandlerAttr(0x04, 0x71)]
    public class MovementEndHandler : PacketHandler
    {
        #region implemented abstract members of PacketHandler

        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            PacketReader reader = new PacketReader(data);
            FullMovementData movData = reader.ReadStruct<FullMovementData>();

            if (movData.entity1.ID == 0 && movData.entity2.ID != 0)
                movData.entity1 = movData.entity2;

            // This could be simplified
            PacketWriter writer = new PacketWriter();
            writer.WriteStruct(movData);

            Logger.WriteInternal("[MOV] {0} stopped moving at ({1}, {2}, {3}) from ({4}, {5}, {6})", context.Character.Name,
                Helper.FloatFromHalfPrecision(movData.currentPos.x), Helper.FloatFromHalfPrecision(movData.currentPos.y),
                Helper.FloatFromHalfPrecision(movData.currentPos.z), Helper.FloatFromHalfPrecision(movData.lastPos.x),
                Helper.FloatFromHalfPrecision(movData.lastPos.y), Helper.FloatFromHalfPrecision(movData.lastPos.z));

            foreach (var c in Server.Instance.Clients)
            {
                if (c == context)
                    continue;

                if (c.Character == null)
                    continue;

                c.SendPacket(0x04, 0x71, 0x40, writer.ToArray());
            }
        }

        #endregion
    }


    [Flags]
    public enum PackedData : Int32
    {
        ENT1_ID = 1,
        ENT1_TYPE = 2,
        ENT1_A = 4,
        ENT2_ID = 8,
        ENT2_TYPE = 0x10,
        ENT2_A = 0x20,
        UNKNOWN1 = 0x40,
        ROT_X = 0x80,
        ROT_Y = 0x100,
        ROT_Z = 0x200,
        ROT_W = 0x400,
        LAST_X = 0x800,
        LAST_Y = 0x1000,
        LAST_Z = 0x2000,
        UNKNOWN4 = 0x4000,
        CUR_X = 0x8000,
        CUR_Y = 0x10000,
        CUR_Z = 0x20000,
        UNKNOWN5 = 0x40000,
        UNKNOWN6 = 0x80000,
        UNKNOWN7 = 0x100000
    }


    public struct PackedVec4
    {
        public UInt16 x, y, z, w;
    }

    public struct PackedVec3
    {
        public UInt16 x, y, z;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x38)]
    public struct FullMovementData
    {
        [FieldOffset(0x0)]
        public EntityHeader entity1;
        [FieldOffset(0xC)]
        public EntityHeader entity2;
        [FieldOffset(0x18)]
        public UInt32 unknown1;
        [FieldOffset(0x1C)]
        public PackedVec4 rotation;
        [FieldOffset(0x24)]
        public PackedVec3 lastPos;
        [FieldOffset(0x2A)]
        public UInt16 Unknown2; // This MAY be part of lastPos, as lastPos may be a Vec4?
        [FieldOffset(0x2C)]
        public PackedVec3 currentPos;
        [FieldOffset(0x32)]
        public UInt16 Unknown3; // This MAY be part of currentPos, as lastPos may be a Vec4?
        [FieldOffset(0x34)]
        public UInt32 Unknown4;
    }

}