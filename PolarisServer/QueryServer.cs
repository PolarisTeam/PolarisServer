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
            ShipEntry entry = new ShipEntry();
            entry.number = 1;
            entry.order = 1;
            entry.status = (ushort)ShipStatus.SHIP_ONLINE;

            IPAddress addr = IPAddress.Parse("127.0.0.1");
            Marshal.Copy(addr.GetAddressBytes(), 0, (IntPtr)entry.ip, 4);
            Marshal.Copy("Ship01".ToCharArray(), 0, (IntPtr)entry.name, 6);

            w.Write((uint)Marshal.SizeOf(entry) + 8);
            w.Write((byte)0x11);
            w.Write((byte)0x3D);
            w.Write((byte)4);
            w.Write((byte)0);
            w.WriteStruct(entry);

            s.Send(w.ToArray());
            s.Close();

        }

        private void doBlockBalance(Socket s) 
        {

        }
    }
}

