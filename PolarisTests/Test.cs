using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using NUnit.Framework;
using PolarisServer.Models;
using PolarisServer.Packets;
using PolarisServer.Packets.Handlers;

namespace PolarisTests
{
    [TestFixture]
    public class ReflectionTests
    {
        [SetUp]
        public void Setup()
        {
            PacketHandlers.LoadPacketHandlers();
        }

        [Test]
        public void TestLoginLookup()
        {
            Assert.IsNotNull(PacketHandlers.GetHandlerFor(0x11, 0x0));
        }

        [Test]
        public void TestAllHandlers()
        {
            foreach (var p in PacketHandlers.GetLoadedHandlers())
            {
                Assert.IsNotNull(p);
                Assert.IsInstanceOf(typeof (PacketHandler), p, "Loaded PacketHandler is not a Packet Handler!");
            }
        }
    }

    [TestFixture]
    public class UnsafeTests
    {
        private readonly Character.JobParam _jp = new Character.JobParam();

        [Test]
        public void CheckJobParam()
        {
            var size = Marshal.SizeOf(typeof (Character.JobParam));
            Assert.IsNotNull(_jp);
            var jpArr = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(_jp, ptr, true);
            Marshal.Copy(ptr, jpArr, 0, size);
            Marshal.FreeHGlobal(ptr);

            foreach (var b in jpArr)
            {
                Assert.AreEqual(0, b);
            }
            Assert.AreEqual(size, jpArr.Length);
        }
    }

    [TestFixture]
    public class WriterTests
    {
        private PacketWriter _writer;

        [SetUp]
        public void Setup()
        {
            _writer = new PacketWriter();
        }

        [Test]
        public void TestStructureWrite()
        {
            var structureSize = Marshal.SizeOf(typeof (Character.JobParam));
            var jp = new Character.JobParam();
            jp.entries.hunter.level = 7;
            _writer.WriteStruct(jp);
            var structArray = _writer.ToArray();
            Assert.AreEqual(structureSize, structArray.Length);
            Assert.AreEqual(7, structArray[12]);
        }
    }

    [TestFixture]
    public class JsonTests
    {
        [Test]
        public void TestObjectSerialize()
        {
            var testObject = new PSOObject
            {
                Name = "testobj",
                Header = new ObjectHeader {ID = 1337, EntityType = EntityType.Object},
                Position = new PSOLocation
                {
                    RotX = (float) 3.3,
                    RotY = (float) 3.3,
                    RotZ = (float) 3.3,
                    RotW = (float) 3.3,
                    PosX = (float) 3.3,
                    PosY = (float) 3.3,
                    PosZ = (float) 3.3
                },
                ThingFlag = 4,
                Things = new PSOObject.PSOObjectThing[2]
            };

            var thingData = BitConverter.ToUInt32(new byte[] {0xff, 0xff, 0xff, 0xff}, 0);
            testObject.Things[0] = new PSOObject.PSOObjectThing {Data = thingData};
            var output = JsonConvert.SerializeObject(testObject);
            Console.Out.WriteLine(output);
        }
    }

    [TestFixture]
    public class DataTests
    {
        [Test]
        public void TestShiftEnum()
        {
            byte[] bytes = { 0x1, 0x1, 0x1 };
            uint dataFlags = bytes[0];
            dataFlags |= (uint)(bytes[1] << 8);
            dataFlags |= (uint)(bytes[2] << 16);

            Assert.AreEqual((PackedData.ENT1_ID|PackedData.ROT_Y|PackedData.UNK_Y), 
                (PackedData) dataFlags);
            Console.Out.WriteLine((PackedData)dataFlags);
        }

      
    }
}