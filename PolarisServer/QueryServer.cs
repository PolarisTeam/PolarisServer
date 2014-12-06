using System;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;

using PolarisServer.Packets;
using PolarisServer.Models;

namespace PolarisServer
{
    public enum QueryMode
    {
        ShipList,
        BlockBalance
    }

    public class QueryServer
    {
        public static List<Thread> runningServers = new List<Thread>();

        QueryMode mode;
        int port;

        public QueryServer(QueryMode mode, int port)
        {
            this.mode = mode;
            this.port = port;
            Thread queryThread = new Thread(new ThreadStart(Run));
            queryThread.Start();
            runningServers.Add(queryThread);
            Logger.WriteInternal("[---] Started a new QueryServer on port " + port);
        }

        private delegate void OnConnection(Socket server);

        private void Run()
        {
            OnConnection c;
            switch (mode)
            {
                default:
                    c = DoShipList;
                    break;
                case QueryMode.BlockBalance:
                    c = DoBlockBalance;
                    break;
                case QueryMode.ShipList:
                    c = DoShipList;
                    break;
            }

            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Blocking = true;
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, this.port);
            serverSocket.Bind(ep); // TODO: Custom bind address.
            serverSocket.Listen(5);
            while (true)
            {
                Socket newConnection = serverSocket.Accept();
                c(newConnection);
            }
        }

        private unsafe void DoShipList(Socket socket)
        {
            PacketWriter writer = new PacketWriter();
            List<ShipEntry> entries = new List<ShipEntry>();

            for (int i = 1; i < 11; i++)
            {
                ShipEntry entry = new ShipEntry();
                entry.order = (ushort)i;
                entry.number = (uint)i;
                entry.status = ShipStatus.Online;
                entry.name = String.Format("Ship{0:0#}", i);
				entry.ip = PolarisApp.BindAddress.GetAddressBytes();
                entries.Add(entry);
            }
            writer.WriteStruct(new PacketHeader(Marshal.SizeOf(typeof(ShipEntry)) * entries.Count + 12, 0x11, 0x3D, 0x4, 0x0));
            writer.WriteMagic((uint)entries.Count, 0xE418, 81);
            foreach (ShipEntry entry in entries)
                writer.WriteStruct(entry);

            writer.Write((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            writer.Write((Int32)1);

            socket.Send(writer.ToArray());
            socket.Close();

        }

        private void DoBlockBalance(Socket socket)
        {
            PacketWriter writer = new PacketWriter();
            writer.WriteStruct(new PacketHeader(0x90, 0x11, 0x2C, 0x0, 0x0));
            writer.Write(new byte[0x64 - 8]);
            writer.Write(PolarisApp.BindAddress.GetAddressBytes());
            writer.Write((UInt16)12205);
            writer.Write(new byte[0x90 - 0x6A]);

            socket.Send(writer.ToArray());
            socket.Close();
        }
    }
}
