using System;
using System.IO;
using System.Security.Cryptography;

using PolarisServer.Database;
using PolarisServer.Models;
using PolarisServer.Network;
using PolarisServer.Packets;
using PolarisServer.Zone;

namespace PolarisServer
{
    public class Client
    {
        internal static RSACryptoServiceProvider rsaCsp = null;

        private bool isClosed = false;
        public bool IsClosed { get { return isClosed; } }

        private Server server;
        private SocketClient socket;

        public SocketClient Socket { get { return socket; } }

        // Game properties, TODO Consider moving these somewhere else
        public Player User { get; set; }
        public Character Character { get; set; }
        public Zone.Zone CurrentZone { get; set; }

        private byte[] readBuffer;
        private uint readBufferSize;

        internal ICryptoTransform inputARC4, outputARC4;

        private int packetID = 0;

        public Client(Server server, Network.SocketClient socket)
        {
            this.server = server;
            this.socket = socket;

            socket.DataReceived += HandleDataReceived;
            socket.ConnectionLost += HandleConnectionLost;

            readBuffer = new byte[1024 * 64];
            readBufferSize = 0;

            inputARC4 = null;
            outputARC4 = null;

            PacketWriter welcome = new PacketWriter();
            welcome.Write((ushort)3);
            welcome.Write((ushort)201);
            welcome.Write((ushort)0);
            welcome.Write((ushort)0);
            SendPacket(0x03, 0x08, 0, welcome.ToArray());
        }

        void HandleDataReceived(byte[] data, int size)
        {
            Logger.Write("[<--] Recieved {0} bytes", size);
            if ((readBufferSize + size) > readBuffer.Length)
            {
                // Buffer overrun
                // TODO: Drop the connection when this occurs?
                return;
            }

            Array.Copy(data, 0, readBuffer, readBufferSize, size);

            if (inputARC4 != null)
            {
                inputARC4.TransformBlock(readBuffer, (int)readBufferSize, (int)size, readBuffer, (int)readBufferSize);
            }

            readBufferSize += (uint)size;

            // Process ALL the packets
            uint position = 0;

            while ((position + 8) <= readBufferSize)
            {
                uint packetSize =
                    (uint)readBuffer[position] |
                    ((uint)readBuffer[position + 1] << 8) |
                    ((uint)readBuffer[position + 2] << 16) |
                    ((uint)readBuffer[position + 3] << 24);

                // Minimum size, just to avoid possible infinite loops etc
                if (packetSize < 8)
                    packetSize = 8;

                // If we don't have enough data for this one...
                if (packetSize > 0x1000000 || (packetSize + position) > readBufferSize)
                    break;

                // Now handle this one
                HandlePacket(
                    readBuffer[position + 4], readBuffer[position + 5],
                    readBuffer[position + 6], readBuffer[position + 7],
                    readBuffer, position + 8, packetSize - 8);

                // If the connection was closed, we have no more business here
                if (isClosed)
                    break;

                position += packetSize;
            }

            // Wherever 'position' is up to, is what was successfully processed
            if (position > 0)
            {
                if (position >= readBufferSize)
                    readBufferSize = 0;
                else
                {
                    Array.Copy(readBuffer, position, readBuffer, 0, readBufferSize - position);
                    readBufferSize -= position;
                }
            }
        }

        void HandleConnectionLost()
        {
            // :(
            Logger.Write("[BYE] Connection lost. :(");
            isClosed = true;
        }

        public void SendPacket(byte[] blob)
        {
            byte typeA = blob[4];
            byte typeB = blob[5];
            byte flags1 = blob[6];
            byte flags2 = blob[7];

            Logger.Write("[<--] Packet {0:X}-{1:X} (flags {2}, {3}) ({4} bytes)", typeA, typeB, flags1, flags2, blob.Length);
            LogPacket(false, typeA, typeB, flags1, flags2, blob);

            if (Logger.VerbosePackets)
            {
                string info = string.Format("[<--] {0:X}-{1:X} Data:", typeA, typeB);
                Logger.WriteHex(info, blob);
            }

            if (outputARC4 != null)
                outputARC4.TransformBlock(blob, 0, blob.Length, blob, 0);

            try
            {
                socket.Socket.Client.Send(blob);
            }
            catch (Exception ex)
            {
                Logger.WriteException("Error sending packet", ex);
            }
        }

        public void SendPacket(byte typeA, byte typeB, byte flags, byte[] data)
        {
            byte[] packet = new byte[8 + data.Length];

            // TODO: Use BinaryWriter here maybe?
            uint dataLen = (uint)data.Length + 8;
            packet[0] = (byte)(dataLen & 0xFF);
            packet[1] = (byte)((dataLen >> 8) & 0xFF);
            packet[2] = (byte)((dataLen >> 16) & 0xFF);
            packet[3] = (byte)((dataLen >> 24) & 0xFF);
            packet[4] = typeA;
            packet[5] = typeB;
            packet[6] = flags;
            packet[7] = 0;

            Array.Copy(data, 0, packet, 8, data.Length);

            SendPacket(packet);
        }

        public void SendPacket(Packet packet)
        {
            PacketHeader h = packet.GetHeader();
            SendPacket(h.type, h.subtype, h.flags1, packet.Build());
        }


        private void HandlePacket(byte typeA, byte typeB, byte flags1, byte flags2, byte[] data, uint position, uint size)
        {
            Logger.Write("[-->] Packet {0:X}-{1:X} (flags {2}, {3}) ({4} bytes)", typeA, typeB, flags1, flags2, size);
            if (Logger.VerbosePackets && size > 0) // TODO: This is trimming too far?
            {
                byte[] dataTrimmed = new byte[size];
                for (int i = 0; i < size; i++)
                    dataTrimmed[i] = data[i];

                string info = string.Format("[-->] {0:X}-{1:X} Data:", typeA, typeB);
                Logger.WriteHex(info, dataTrimmed);
            }

            byte[] packet = new byte[size];
            Array.Copy(data, position, packet, 0, size);
            LogPacket(true, typeA, typeB, flags1, flags2, packet);

            Packets.Handlers.PacketHandler handler = Packets.Handlers.PacketHandlers.GetHandlerFor(typeA, typeB);
            if (handler != null)
                handler.HandlePacket(this, packet, 0, size);
            else
            {
                Logger.WriteWarning("[!!!] UNIMPLEMENTED PACKET {0:X}-{1:X} - (Flags {2}, {3}) ({4} bytes)", typeA, typeB, flags1, flags2, size);
            }

            // throw new NotImplementedException();
        }


        private void LogPacket(bool fromClient, byte typeA, byte typeB, byte flags1, byte flags2, byte[] packet)
        {
            // Check for and create packets directory if it doesn't exist
            string packetPath = "packets/" + server.StartTime.ToShortDateString().Replace('/', '-') + "-" + server.StartTime.ToShortTimeString().Replace('/', '-').Replace(':', '-');
            if (!Directory.Exists(packetPath))
                Directory.CreateDirectory(packetPath);

            string filename = string.Format("{0}/{1}.{2:X}.{3:X}.{4}.bin", packetPath, packetID++, typeA, typeB, fromClient ? "C" : "S");

            using (var stream = File.OpenWrite(filename))
            {
                if (fromClient)
                {
                    stream.WriteByte(typeA);
                    stream.WriteByte(typeB);
                    stream.WriteByte(flags1);
                    stream.WriteByte(flags2);
                }
                stream.Write(packet, 0, packet.Length);
            }
        }
    }
}
