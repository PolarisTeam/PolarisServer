using System;
using System.IO;
using PolarisServer.Packets;

namespace PolarisServer.Models
{
    public class PsoObject
    {
        public EntityHeader Header { get; set; }
        public MysteryPositions Position { get; set; }
        public string Name { get; set; }
        public UInt32 ThingFlag { get; set; }
        public PsoObjectThing[] Things { get; set; }

        public byte[] GenerateSpawnBlob()
        {
            var writer = new PacketWriter();
            writer.WriteStruct(Header);
            writer.Write(Position);
            writer.Seek(2, SeekOrigin.Current); // Padding I guess...
            writer.WriteFixedLengthAscii(Name, 0x34);
            writer.Write(ThingFlag);
            writer.Write(Things.Length);
            foreach (var thing in Things)
            {
                writer.WriteStruct(thing);
            }

            return writer.ToArray();
        }

        public struct PsoObjectThing
        {
            public UInt32 Data;
        }
    }
}