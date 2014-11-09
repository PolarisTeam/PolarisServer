using PolarisServer.Packets.Handlers;
using NUnit.Framework;
using System;

namespace PolarisTests
{
	[TestFixture]
	public class Test
	{
		[SetUp]
		public void Setup()
		{
			PacketHandlers.loadPacketHandlers ();
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
}

