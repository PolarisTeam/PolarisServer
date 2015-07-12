using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PolarisServer.Packets;

namespace PolarisServer.Models
{
    public class PSOObject
    {
        public struct PSOObjectThing
        {
            public UInt32 Data;
        }

        public EntityHeader Header { get; set; }
        public PSOLocation Position { get; set; }
        public string Name { get; set; }
        public UInt32 ThingFlag { get; set; }
        public PSOObjectThing[] Things { get; set; }

        public byte[] GenerateSpawnBlob()
        {
            PacketWriter writer = new PacketWriter();
            writer.WriteStruct(Header);
            writer.Write(Position);
            writer.Seek(2, SeekOrigin.Current); // Padding I guess...
            writer.WriteFixedLengthASCII(Name, 0x34);
            writer.Write(ThingFlag);
            writer.Write(Things.Length);
            foreach (PSOObjectThing thing in Things)
            {
                writer.WriteStruct(thing);
            }

            return writer.ToArray();

        }

        internal static PSOObject FromPacketBin(byte[] v)
        {
            PacketReader reader = new PacketReader(v);
            PSOObject obj = new PSOObject();
            reader.ReadStruct<PacketHeader>(); //Skip over header
            obj.Header = reader.ReadStruct<EntityHeader>();
            obj.Position = reader.ReadEntityPosition();
            reader.ReadUInt16(); // Seek 2
            obj.Name = reader.ReadFixedLengthAscii(0x34);
            obj.ThingFlag = reader.ReadUInt32();
            uint thingCount = reader.ReadUInt32();
            obj.Things = new PSOObjectThing[thingCount];
            for(int i = 0; i < thingCount; i++)
            {
                obj.Things[i] = reader.ReadStruct<PSOObjectThing>();
            }

            return obj;
        }
    }
}
