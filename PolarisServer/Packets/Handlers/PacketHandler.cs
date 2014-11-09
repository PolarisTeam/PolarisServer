using System;
using System.Reflection;
using System.Linq;

namespace PolarisServer.Packets.Handlers
{
	public class PacketHandlerAttr : Attribute
	{
		public uint type, subtype;
		public PacketHandlerAttr(uint type, uint subtype)
		{
			this.type = type;
			this.subtype = subtype;
		}


	}


	public abstract class PacketHandler
	{
		public abstract void handlePacket(Client context, byte[] data,  uint position, uint size);
	}

	public static class PacketHandlers
	{
		/// <summary>
		/// Gets and creates a PacketHandler for a given packet type and subtype.
		/// TODO: Creating new instances all the time can't be a good thing. Move this over to some big list or something.
		/// </summary>
		/// <returns>An instance of a PacketHandler or null</returns>
		/// <param name="typeA">Type a.</param>
		/// <param name="typeB">Type b.</param>
		/// 
		public static PacketHandler getHandlerFor(uint typeA, uint typeB)
		{
			var classes = from t in Assembly.GetExecutingAssembly ().GetTypes ()
					where t.IsClass && t.Namespace == "PolarisServer.Packets.Handlers" && t.IsSubclassOf (typeof(PacketHandler))
			              select t;
			foreach (Type t in classes.ToList()) 
			{
				Attribute[] attrs = (Attribute[])t.GetCustomAttributes (typeof(PacketHandlerAttr), false);
				foreach (Attribute attr in attrs) 
				{
					if (attr is PacketHandlerAttr) 
					{
						PacketHandlerAttr handlerAttr = (PacketHandlerAttr)attr;
						if (handlerAttr.type == typeA && handlerAttr.subtype == typeB)
							return (PacketHandler)Activator.CreateInstance (t);
					}
				}
			}


			return null;

		}
	}
}

