using System;
using System.IO;
using System.Security.Cryptography;

using PolarisServer.Database;
using PolarisServer.Models;
using PolarisServer.Packets;

namespace PolarisServer
{
    public class Client
    {
        internal static RSACryptoServiceProvider _rsaCsp = null;

        private bool _isClosed = false;

        private Server _server;
        private Network.SocketClient _socket;

        public Network.SocketClient Socket { get { return _socket; } }

        public Player User { get; set; }
        public Character Character { get; set; }

        private byte[] _readBuffer;
        private uint _readBufferSize;

        internal ICryptoTransform _inputARC4, _outputARC4;

        private int _packetID = 0;

        public Client(Server server, Network.SocketClient socket)
        {
            _server = server;
            _socket = socket;

            socket.DataReceived += HandleDataReceived;
            socket.ConnectionLost += HandleConnectionLost;

            _readBuffer = new byte[1024 * 64];
            _readBufferSize = 0;

            _inputARC4 = null;
            _outputARC4 = null;

            var welcome = new Packets.PacketWriter();
            welcome.Write((ushort)3);
            welcome.Write((ushort)201);
            welcome.Write((ushort)0);
            welcome.Write((ushort)0);
            SendPacket(3, 8, 0, welcome.ToArray());
        }

        void HandleDataReceived(byte[] data, int size)
        {
            Logger.Write("[<--] Recieved {0} bytes", size);
            if ((_readBufferSize + size) > _readBuffer.Length)
            {
                // Buffer overrun
                // TODO: Drop the connection when this occurs?
                return;
            }

            Array.Copy(data, 0, _readBuffer, _readBufferSize, size);

            if (_inputARC4 != null)
            {
                _inputARC4.TransformBlock(_readBuffer, (int)_readBufferSize, (int)size, _readBuffer, (int)_readBufferSize);
            }

            _readBufferSize += (uint)size;

            // Process ALL the packets
            uint position = 0;

            while ((position + 8) <= _readBufferSize)
            {
                uint packetSize =
                    (uint)_readBuffer[position] |
                    ((uint)_readBuffer[position + 1] << 8) |
                    ((uint)_readBuffer[position + 2] << 16) |
                    ((uint)_readBuffer[position + 3] << 24);

                // Minimum size, just to avoid possible infinite loops etc
                if (packetSize < 8)
                    packetSize = 8;

                // If we don't have enough data for this one...
                if (packetSize > 0x1000000 || (packetSize + position) > _readBufferSize)
                    break;

                // Now handle this one
                HandlePacket(
                    _readBuffer[position + 4], _readBuffer[position + 5],
                    _readBuffer[position + 6], _readBuffer[position + 7],
                    _readBuffer, position + 8, packetSize - 8);

                // If the connection was closed, we have no more business here
                if (_isClosed)
                    break;

                position += packetSize;
            }

            // Wherever 'position' is up to, is what was successfully processed
            if (position > 0)
            {
                if (position >= _readBufferSize)
                    _readBufferSize = 0;
                else
                {
                    Array.Copy(_readBuffer, position, _readBuffer, 0, _readBufferSize - position);
                    _readBufferSize -= position;
                }
            }
        }

        void HandleConnectionLost()
        {
            // :(
            Logger.Write("[BYE] Connection lost. :(");
            _isClosed = true;
        }

        public void SendPacket(byte[] blob)
        {
            byte typeA = blob[4];
            byte typeB = blob[5];
            byte flags1 = blob[6];
            byte flags2 = blob[7];

            Logger.Write("[<--] Packet {0:X}-{1:X} (flags {2} {3}, {4} bytes)", typeA, typeB, flags1, flags2, blob.Length);
            LogPacket(false, typeA, typeB, flags1, flags2, blob);

            if (Logger.VerbosePackets)
            {
                string info = string.Format("[<--] {0:X}-{1:X} Data:", typeA, typeB);
                Logger.WriteHex(info, blob);
            }
            
            if (_outputARC4 != null)
                _outputARC4.TransformBlock(blob, 0, blob.Length, blob, 0);
            
            try
            {
                _socket.Socket.Client.Send(blob);
            }
            catch (Exception ex)
            {
                Logger.WriteException("Error sending packet", ex);
            }
        }

        public void SendPacket(byte typeA, byte typeB, byte flags, byte[] data)
        {
            var packet = new byte[8 + data.Length];

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


        void HandlePacket(byte typeA, byte typeB, byte flags1, byte flags2, byte[] data, uint position, uint size)
        {
            Logger.Write("[-->] Packet {0:X}-{1:X} (flags {2}, {3}) ({4} bytes)", typeA, typeB, flags1, flags2, size);
            if (Logger.VerbosePackets && size > 0)
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
                /* Dump the contents of the packet
                string dataString = string.Empty;
                for (int i = 0; i < size; i++)
                    dataString += packet[i].ToString("X2") + " ";
                if (size > 0)
                    Logger.WriteWarning("[!!!] Unimplemented Packet Data: {0}", dataString);
                */ 
            }
            // throw new NotImplementedException();
        }


        void LogPacket(bool fromClient, byte typeA, byte typeB, byte flags1, byte flags2, byte[] packet)
        {
            // Check for and create packets directory if it doesn't exist
            string packetPath = "packets/" + _server.StartTime.ToShortDateString().Replace('/', '-') + "-" + _server.StartTime.ToShortTimeString().Replace('/', '-').Replace(':', '-');
            if (!Directory.Exists(packetPath))
                Directory.CreateDirectory(packetPath);

            var filename = string.Format("{0}/{1}.{2:X}.{3:X}.{4}.bin", packetPath, _packetID++, typeA, typeB, fromClient ? "C" : "S");

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
