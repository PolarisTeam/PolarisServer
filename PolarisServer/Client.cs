using System;
using System.Security.Cryptography;

namespace PolarisServer
{
    public class Client
    {
        internal static RSACryptoServiceProvider _rsaCsp = null;

        private Server _server;
        private Network.SocketClient _socket;

        public Network.SocketClient Socket { get { return _socket; } }

        private byte[] _readBuffer;
        private uint _readBufferSize;

        internal ICryptoTransform _inputARC4, _outputARC4;
        //TODO Adjust visibility better.

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

            var welcome = new Packets.Writer();
            welcome.Write((ushort)3);
            welcome.Write((ushort)201);
            welcome.Write((ushort)0);
            welcome.Write((ushort)0);
            SendPacket(3, 8, 0, welcome.ToArray());
        }

        void HandleDataReceived(byte[] data, int size)
        {
            Console.WriteLine("[Received {0}]", size);
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
                    _readBuffer, position + 8, packetSize - 8);

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
            Console.WriteLine("[:( ] :(");
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

            var filename = string.Format("packets/{0}.{1:X}.{2:X}.S.bin", _packetID++, typeA, typeB);
            System.IO.File.WriteAllBytes(filename, packet);

            if (_outputARC4 != null)
                _outputARC4.TransformBlock(packet, 0, packet.Length, packet, 0);
            _socket.Socket.Client.Send(packet);
        }


        void HandlePacket(byte typeA, byte typeB, byte[] data, uint position, uint size)
        {
            Console.WriteLine("[-->] Packet {0:X}-{1:X} ({2} bytes)", typeA, typeB, size);



            Packets.Handlers.PacketHandler handler = Packets.Handlers.PacketHandlers.getHandlerFor(typeA, typeB);
            if (handler != null)
                handler.handlePacket(this, data, position, size);
            else
                Console.WriteLine("[!!!] UNIMPLEMENTED PACKET");
            //throw new NotImplementedException ();
        }
    }
}
