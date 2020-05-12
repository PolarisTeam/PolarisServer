using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PolarisServer.Models;

namespace PolarisServer.Packets.PSOPackets
{
    class LoginDataPacket : Packet
    {
        private string blockName, error;
        private uint userid;

        public LoginDataPacket(string blockName, string error, uint userid)
        {
            this.blockName = blockName;
            this.error = error;
            this.userid = userid;
        }

        public override byte[] Build()
        {
            var resp = new PacketWriter();
            resp.Write((uint)((userid == 0) ? 1 : 0)); // Status flag: 0=success, 1=error
            resp.WriteUtf16(error, 0x8BA4, 0xB6);

            if (userid == 0)
            {
                for (var i = 0; i < 0xEC; i++)
                    resp.Write((byte)0);
                return resp.ToArray();
            }

            // TODO: Explore this data! Some if it seems really important. (May contain level cap setting + more)

            resp.WriteStruct(new ObjectHeader(userid, EntityType.Player));
            resp.WriteFixedLengthUtf16(blockName, 0x20); // This is right
            // Set things to "default" values; Dunno these purposes yet.
            resp.Write(0x42700000); //0
            resp.Write(7);          //4
            resp.Write(0xA);        //8 - Level Cap!
            resp.Write(1);          //C
            resp.Write(0x41200000); //10
            resp.Write(0x40A00000); //14
            resp.Write(11);         //18
            resp.Write(0x3F800000); //1C (1 as a float)
            resp.Write(0x42960000); //20
            resp.Write(40);         //24
            resp.Write(0x41200000); //28
            resp.Write(1);          //2C?
            resp.Write(1120403456); //30

            //WHAT
            for (int i = 0; i < 10; i++)
            {
                resp.Write(1065353216);
            }
            //ARE
            for (int i = 0; i < 21; i++)
            {
                resp.Write(1120403456);
            }
            //THESE?
            resp.Write(0x91A2B);    //B0
            resp.Write(0x91A2B);    //B4

            resp.WriteBytes(0, 12);

            return resp.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0x11, 0x1, PacketFlags.PACKED);
        }
    }
}
