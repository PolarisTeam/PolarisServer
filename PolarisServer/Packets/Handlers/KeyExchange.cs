using System;
using System.Security.Cryptography;

using PolarisServer.Crypto;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x11, 0xB)]
    public class KeyExchange : PacketHandler
    {
        public KeyExchange()
        {
        }

        public override void handlePacket(Client context, byte[] data, uint position, uint size)
        {
            if (context._inputARC4 != null)
                return;
            if (size < 0x80)
                return;

            // Extract the first 0x80 bytes into a separate array
            var cryptedBlob = new byte[0x80];
            Array.Copy(data, position, cryptedBlob, 0, 0x80);
            Array.Reverse(cryptedBlob);

            // FIXME
            if (Client._rsaCsp == null)
            {
                Client._rsaCsp = new RSACryptoServiceProvider();
                var rsaBlob = System.IO.File.ReadAllBytes("privateKey.blob");
                Client._rsaCsp.ImportCspBlob(rsaBlob);
            }

            var pkcs = new RSAPKCS1KeyExchangeDeformatter(Client._rsaCsp);
            byte[] decryptedBlob = null;

            try
            {
                decryptedBlob = pkcs.DecryptKeyExchange(cryptedBlob);
            }
            catch (CryptographicException)
            {
                // Failed. Should probably drop the connection here.
                // TODO ?
                return;
            }

            // Also a failure.
            if (decryptedBlob.Length < 0x20)
                return;


            // Extract the RC4 key
            var arc4Key = new byte[16];
            Array.Copy(decryptedBlob, 0x10, arc4Key, 0, 0x10);

            // Create three RC4 mungers
            var arc4 = new ARC4Managed();
            arc4.Key = arc4Key;
            context._inputARC4 = arc4.CreateDecryptor();

            arc4 = new ARC4Managed();
            arc4.Key = arc4Key;
            context._outputARC4 = arc4.CreateEncryptor();

            arc4 = new ARC4Managed();
            arc4.Key = arc4Key;
            var tempDecryptor = arc4.CreateDecryptor();

            // Also, grab the init token for the client
            var decryptedToken = new byte[16];
            tempDecryptor.TransformBlock(decryptedBlob, 0, 0x10, decryptedToken, 0);

            context.SendPacket(0x11, 0xC, 0, decryptedToken);
        }
    }
}

