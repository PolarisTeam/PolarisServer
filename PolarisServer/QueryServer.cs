using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

using PolarisServer.Models;
using PolarisServer.Packets;

namespace PolarisServer
{
    public enum QueryMode
    {
        ShipList,
        BlockBalance
    }

    public class QueryServer
    {
        private delegate void OnConnection(Socket server);

        public static List<Thread> RunningServers = new List<Thread>();

        private readonly QueryMode _mode;
        private readonly int _port;

        public QueryServer(QueryMode mode, int port)
        {
            _mode = mode;
            _port = port;
            var queryThread = new Thread(Run);
            queryThread.Start();
            RunningServers.Add(queryThread);
            Logger.WriteInternal("[---] Started a new QueryServer on port " + port);
        }

        private void Run()
        {
            OnConnection c;
            switch (_mode)
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

            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                Blocking = true
            };
            var ep = new IPEndPoint(IPAddress.Any, _port);
            serverSocket.Bind(ep); // TODO: Custom bind address.
            serverSocket.Listen(5);
            while (true)
            {
                var newConnection = serverSocket.Accept();
                c(newConnection);
            }
        }

        private void DoShipList(Socket socket)
        {
            var writer = new PacketWriter();
            var entries = new List<ShipEntry>();

            for (var i = 1; i < 11; i++)
            {
                var entry = new ShipEntry
                {
                    order = (ushort)i,
                    number = (uint)i,
                    status = ShipStatus.Online,
                    name = String.Format("Ship{0:0#}", i),
                    ip = PolarisApp.BindAddress.GetAddressBytes()
                };
                entries.Add(entry);
            }
            writer.WriteStruct(new PacketHeader(Marshal.SizeOf(typeof(ShipEntry)) * entries.Count + 12, 0x11, 0x3D, 0x4,
                0x0));
            writer.WriteMagic((uint)entries.Count, 0xE418, 81);
            foreach (var entry in entries)
                writer.WriteStruct(entry);

            writer.Write((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            writer.Write(1);

            socket.Send(writer.ToArray());
            socket.Close();

        }

        private void DoBlockBalance(Socket socket)
        {
            var writer = new PacketWriter();
            writer.WriteStruct(new PacketHeader(0x90, 0x11, 0x2C, 0x0, 0x0));
            writer.Write(new byte[0x68 - 8]);
            writer.Write(PolarisApp.BindAddress.GetAddressBytes());
            writer.Write((UInt16)12205);
            writer.Write(new byte[0x90 - 0x6A]);

            socket.Send(writer.ToArray());
            socket.Close();
        }
    }
}
