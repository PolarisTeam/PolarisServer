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
        SHIP_LIST,
        BLOCK_BALANCE
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
            Console.WriteLine("[---] Started a new QueryServer on port {0}", port);
        }

        private delegate void OnConnection(Socket server);

        private void Run()
        {
            OnConnection c;
            switch(mode)
            {
                default:
                    c = doShipList;
                    break;
                case QueryMode.BLOCK_BALANCE:
                    c = doBlockBalance;
                    break;
                case QueryMode.SHIP_LIST:
                    c = doShipList;
                    break;
            }

            Socket serverSocket = new Socket(AddressFamily.InterNetwork ,SocketType.Stream, ProtocolType.Tcp);
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

        private unsafe void doShipList(Socket s)
        {
            PacketWriter w = new PacketWriter();
            List<ShipEntry> entries = new List<ShipEntry>();

            for (int i = 1; i < 11; i++)
            {
                ShipEntry entry = new ShipEntry();
                entry.order = (ushort)i;
                entry.number = (uint)i;
                entry.status = ShipStatus.SHIP_ONLINE;
                entry.name = String.Format("Ship{0:0#}", i);
                entry.ip = IPAddress.Loopback.GetAddressBytes();
                entries.Add(entry);
            }
            w.WriteStruct(new PacketHeader(Marshal.SizeOf(typeof(ShipEntry)) * entries.Count + 12, 0x11, 0x3D, 0x4, 0x0));
            w.WriteMagic((uint)entries.Count, 0xE418, 81); 
            foreach(ShipEntry entry in entries)
                w.WriteStruct(entry);

            w.Write((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            w.Write((Int32)1);

            s.Send(w.ToArray());
            s.Close();

        }

        private void doBlockBalance(Socket s) 
        {
            PacketWriter w = new PacketWriter();
            w.WriteStruct(new PacketHeader(0x90, 0x11, 0x2C, 0x0, 0x0));
            w.Write(new byte[0x64]);
            w.Write(IPAddress.Loopback.GetAddressBytes());
            w.Write((UInt16)12205);
            w.Write(new byte[0x90 - 0x6A]);

            s.Send(w.ToArray());
            s.Close();
        }
    }
}

