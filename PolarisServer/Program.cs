using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace PolarisServer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Arf");
			new Server ().Run ();
		}
	}
}
