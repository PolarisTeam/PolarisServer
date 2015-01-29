using System;
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
            var testObject = new PsoObject();
            var output = JsonConvert.SerializeObject(testObject);
            Console.Out.WriteLine(output);
        }
    }
}