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
            PacketHandlers.loadPacketHandlers();
        }
        [Test]
        public void TestLoginLookup()
        {
            Assert.IsNotNull(PacketHandlers.getHandlerFor(0x11, 0x0));
        }
        [Test]
        public void TestAllHandlers()
        {
            foreach (PacketHandler p in PacketHandlers.getLoadedHandlers())
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
                Assert.IsNotNull(jp);
                byte[] jpArr = new byte[sizeof(Character.JobParam)];
                IntPtr ptr = Marshal.AllocHGlobal(sizeof(Character.JobParam));

                Marshal.StructureToPtr(jp, ptr, true);
                Marshal.Copy(ptr, jpArr, 0, sizeof(Character.JobParam));
                Marshal.FreeHGlobal(ptr);

                foreach (byte b in jpArr)
                {
                    Assert.AreEqual(0, b);
                }
                Assert.AreEqual(sizeof(Character.JobParam), jpArr.Length);
            }
        }

    }

    [TestFixture]
    public class WriterTests
    {
        private PolarisServer.Packets.Writer writer;

        [SetUp]
        public void Setup()
        {
            writer = new PolarisServer.Packets.Writer();
        }

        [Test]
        public unsafe void TestStructureWrite()
        {
            var structureSize = sizeof(Character.JobParam);
            Character.JobParam jp = new Character.JobParam();
            jp.entries.entry0.level = 7;
            writer.WriteStruct(jp, structureSize);
            byte[] structArray = writer.ToArray();
            Assert.AreEqual(structureSize, structArray.Length);
            Assert.AreEqual(7, structArray[8]);
        }
    }
}

