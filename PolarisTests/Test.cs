using PolarisServer.Packets.Handlers;
using PolarisServer.Models;
using PolarisServer.Packets;
using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

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
            foreach (PacketHandler p in PacketHandlers.GetLoadedHandlers())
            {
                Assert.IsNotNull(p);
                Assert.IsInstanceOf(typeof(PacketHandler), p, "Loaded PacketHandler is not a Packet Handler!");
            }
        }
    }

    [TestFixture]
    public class UnsafeTests
    {
        Character.JobParam jp;

        [Test]
        public void checkJobParam()
        {
            unsafe
            {
                var size = Marshal.SizeOf(typeof(Character.JobParam));
                Assert.IsNotNull(jp);
                byte[] jpArr = new byte[size];
                IntPtr ptr = Marshal.AllocHGlobal(size);

                Marshal.StructureToPtr(jp, ptr, true);
                Marshal.Copy(ptr, jpArr, 0, size);
                Marshal.FreeHGlobal(ptr);

                foreach (byte b in jpArr)
                {
                    Assert.AreEqual(0, b);
                }
                Assert.AreEqual(size, jpArr.Length);
            }
        }

    }

    [TestFixture]
    public class WriterTests
    {
        private PolarisServer.Packets.PacketWriter writer;

        [SetUp]
        public void Setup()
        {
            writer = new PolarisServer.Packets.PacketWriter();
        }

        [Test]
        public unsafe void TestStructureWrite()
        {
            var structureSize = Marshal.SizeOf(typeof(Character.JobParam));
            Character.JobParam jp = new Character.JobParam();
            jp.entries.hunter.level = 7;
            writer.WriteStruct(jp);
            byte[] structArray = writer.ToArray();
            Assert.AreEqual(structureSize, structArray.Length);
            Assert.AreEqual(7, structArray[12]);
        }
    }
}
