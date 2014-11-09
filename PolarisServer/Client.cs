using System;
using System.Security.Cryptography;

namespace PolarisServer
{
	public class Client
	{
		private static RSACryptoServiceProvider _rsaCsp = null;

		private Server _server;
		private Network.SocketClient _socket;
		public Network.SocketClient Socket { get { return _socket; } }

		private byte[] _readBuffer;
		private uint _readBufferSize;

		private ICryptoTransform _inputARC4, _outputARC4;

		private int _packetID = 0;

		public Client (Server server, Network.SocketClient socket)
		{
			_server = server;
			_socket = socket;

			socket.DataReceived += HandleDataReceived;
			socket.ConnectionLost += HandleConnectionLost;

			_readBuffer = new byte[1024 * 64];
			_readBufferSize = 0;

			_inputARC4 = null;
			_outputARC4 = null;

			var welcome = new Packets.Writer ();
			welcome.Write ((ushort)3);
			welcome.Write ((ushort)201);
			welcome.Write ((ushort)0);
			welcome.Write ((ushort)0);
			SendPacket (3, 8, 0, welcome.ToArray());
		}

		void HandleDataReceived (byte[] data, int size)
		{
			Console.WriteLine ("[Received {0}]", size);
			if ((_readBufferSize + size) > _readBuffer.Length) {
				// Buffer overrun
				// TODO: Drop the connection when this occurs?
				return;
			}

			Array.Copy (data, 0, _readBuffer, _readBufferSize, size);

			if (_inputARC4 != null) {
				_inputARC4.TransformBlock (_readBuffer, (int)_readBufferSize, (int)size, _readBuffer, (int)_readBufferSize);
			}

			_readBufferSize += (uint)size;

			// Process ALL the packets
			uint position = 0;

			while ((position + 8) <= _readBufferSize) {
				uint packetSize =
					(uint)_readBuffer [position] |
					((uint)_readBuffer [position + 1] << 8) |
					((uint)_readBuffer [position + 2] << 16) |
					((uint)_readBuffer [position + 3] << 24);

				// Minimum size, just to avoid possible infinite loops etc
				if (packetSize < 8)
					packetSize = 8;

				// If we don't have enough data for this one...
				if (packetSize > 0x1000000 || (packetSize + position) > _readBufferSize)
					break;

				// Now handle this one
				HandlePacket (
					_readBuffer [position + 4], _readBuffer [position + 5],
					_readBuffer, position + 8, packetSize - 8);

				position += packetSize;
			}

			// Wherever 'position' is up to, is what was successfully processed
			if (position > 0) {
				if (position >= _readBufferSize)
					_readBufferSize = 0;
				else {
					Array.Copy (_readBuffer, position, _readBuffer, 0, _readBufferSize - position);
					_readBufferSize -= position;
				}
			}
		}

		void HandleConnectionLost()
		{
			// :(
		}


		void SendPacket(byte typeA, byte typeB, byte flags, byte[] data)
		{
			var packet = new byte[8 + data.Length];

			// TODO: Use BinaryWriter here maybe?
			uint dataLen = (uint)data.Length + 8;
			packet [0] = (byte)(dataLen & 0xFF);
			packet [1] = (byte)((dataLen >> 8) & 0xFF);
			packet [2] = (byte)((dataLen >> 16) & 0xFF);
			packet [3] = (byte)((dataLen >> 24) & 0xFF);
			packet [4] = typeA;
			packet [5] = typeB;
			packet [6] = flags;
			packet [7] = 0;

			Array.Copy (data, 0, packet, 8, data.Length);

			var filename = string.Format ("packets/{0}.{1:X}.{2:X}.S.bin", _packetID++, typeA, typeB);
			System.IO.File.WriteAllBytes (filename, packet);

			if (_outputARC4 != null)
				_outputARC4.TransformBlock (packet, 0, packet.Length, packet, 0);
			_socket.Socket.Client.Send (packet);
		}


		void HandlePacket (byte typeA, byte typeB, byte[] data, uint position, uint size)
		{
			Console.WriteLine ("[-->] Packet {0:X}-{1:X} ({2} bytes)", typeA, typeB, size);

			if (typeA == 0x11 && typeB == 0xB) {
				if (_inputARC4 == null)
					HandleKeyExchange (data, position, size);
			} else if (typeA == 0x11 && typeB == 0) {
				HandleLogin (data, position, size);
			} else {
				Console.WriteLine ("[!!!] UNIMPLEMENTED PACKET");
				//throw new NotImplementedException ();
			}
		}


		// PSO2 encryption junk, just because
		void HandleKeyExchange (byte[] data, uint position, uint size)
		{
			if (size < 0x80)
				return;

			// Extract the first 0x80 bytes into a separate array
			var cryptedBlob = new byte[0x80];
			Array.Copy (data, position, cryptedBlob, 0, 0x80);
			Array.Reverse (cryptedBlob);

			// FIXME
			if (_rsaCsp == null) {
				_rsaCsp = new RSACryptoServiceProvider ();
				var rsaBlob = System.IO.File.ReadAllBytes ("privateKey.blob");
				_rsaCsp.ImportCspBlob (rsaBlob);
			}

			var pkcs = new RSAPKCS1KeyExchangeDeformatter (_rsaCsp);
			byte[] decryptedBlob = null;

			try {
				decryptedBlob = pkcs.DecryptKeyExchange (cryptedBlob);
			} catch (CryptographicException) {
				// Failed. Should probably drop the connection here.
				// TODO ?
				return;
			}

			// Also a failure.
			if (decryptedBlob.Length < 0x20)
				return;


			// Extract the RC4 key
			var arc4Key = new byte[16];
			Array.Copy (decryptedBlob, 0x10, arc4Key, 0, 0x10);

			// Create three RC4 mungers
			var arc4 = new Mono.Security.Cryptography.ARC4Managed ();
			arc4.Key = arc4Key;
			_inputARC4 = arc4.CreateDecryptor ();

			arc4 = new Mono.Security.Cryptography.ARC4Managed ();
			arc4.Key = arc4Key;
			_outputARC4 = arc4.CreateEncryptor ();

			arc4 = new Mono.Security.Cryptography.ARC4Managed ();
			arc4.Key = arc4Key;
			var tempDecryptor = arc4.CreateDecryptor ();

			// Also, grab the init token for the client
			var decryptedToken = new byte[16];
			tempDecryptor.TransformBlock (decryptedBlob, 0, 0x10, decryptedToken, 0);

			SendPacket (0x11, 0xC, 0, decryptedToken);
		}



		void HandleLogin(byte[] data, uint position, uint size)
		{
			// Mystery packet
			var mystery = new Packets.Writer ();
			mystery.Write ((uint)100);
			//SendPacket (0x11, 0x49, 0, mystery.ToArray ());

			// Login response packet
			var resp = new Packets.Writer ();
			resp.Write ((uint)0); // Status flag: 0=success, 1=error
			resp.WriteUTF16 ("This is an error", 0x8BA4, 0xB6);
			resp.Write ((uint)200); // Player ID
			resp.Write ((uint)0); // Unknown
			resp.Write ((uint)0); // Unknown
			resp.WriteFixedLengthUTF16 ("B001-DarkFox", 0x20);
			for (int i = 0; i < 0xBC; i++)
				resp.Write ((byte)0);
			SendPacket (0x11, 1, 4, resp.ToArray ());

			// Settings packet
			var settings = new Packets.Writer ();
			settings.WriteASCII (
				System.IO.File.ReadAllText ("settings.txt"),
				0x54AF, 0x100);
			SendPacket (0x2B, 2, 4, settings.ToArray ());
		}
	}
}

