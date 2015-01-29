using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PolarisServer.Packets;

namespace PolarisServer.Models
{
    class PSOObject
    {
        struct PSOObjectThing
        {
            public UInt32 data;
        }

        EntityHeader Header { get; set; }
        MysteryPositions Position { get; set; }
        string Name { get; set; }
        UInt32 ThingFlag { get; set; }
        PSOObjectThing[] things { get; set; }

        public byte[] GenerateSpawnBlob()
        {
            PacketWriter writer = new PacketWriter();
            writer.WriteStruct(Header);
            writer.WriteStruct(Position);
            writer.Seek(2, SeekOrigin.Current); // Padding I guess...
            writer.WriteFixedLengthASCII(Name, 0x34);
            writer.Write(ThingFlag);
            writer.Write(things.Length);
            foreach (PSOObjectThing thing in things)
            {
                writer.WriteStruct(thing);
            }

            return writer.ToArray();

        }
    }
}
