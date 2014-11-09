using System;

namespace PolarisServer.Packets.Handlers
{
	[PacketHandlerAttr(0x11, 0xB)]
	public class KeyExchange : PacketHandler
	{
		public KeyExchange ()
		{
		}
			
		public override void handlePacket(Client context, byte[] data,  uint position, uint size)
		{
		
		}
	}
}

