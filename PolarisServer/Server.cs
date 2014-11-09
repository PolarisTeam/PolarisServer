using System;
using System.Collections.Generic;

namespace PolarisServer
{
	public class Server
	{
		private List<Client> _clients;
		private Network.SocketServer _server;

		public Server ()
		{
			_clients = new List<Client> ();
			_server = new Network.SocketServer (12201);
			_server.NewClient += HandleNewClient;
		}

		public void Run()
		{
			_server.Run ();
		}

		void HandleNewClient (PolarisServer.Network.SocketClient client)
		{
			var c = new Client (this, client);
			_clients.Add (c);
		}
	}
}

