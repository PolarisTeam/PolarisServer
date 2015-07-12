//
// ARC4Managed.cs: Alleged RC4(tm) compatible symmetric stream cipher
//  RC4 is a trademark of RSA Security
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR RotX PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Security.Cryptography;

namespace PolarisServer.Crypto
{
    // References:
    // a.   Usenet 1994 - RC4 Algorithm revealed
    //  http://www.qrst.de/html/dsds/rc4.htm

#if !INSIDE_CORLIB
    public
#endif
        class Arc4Managed : Rc4, ICryptoTransform
    {
        private byte[] _key;
        private bool _mDisposed;
        private byte[] _state;
        private byte _x;
        private byte _y;

        public Arc4Managed()
        {
            _state = new byte[256];
            _mDisposed = false;
        }

        public override byte[] Key
        {
            get
            {
                if (KeyValue == null)
                    GenerateKey();
                return (byte[])KeyValue.Clone();    
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Key");
                KeyValue = _key = (byte[]) value.Clone();
                KeySetup(_key);
            }
        }

        public bool CanReuseTransform
        {
            get { return false; }
        }

        public bool CanTransformMultipleBlocks
        {
            get { return true; }
        }

        public int InputBlockSize
        {
            get { return 1; }
        }

        public int OutputBlockSize
        {
            get { return 1; }
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer,
            int outputOffset)
        {
            CheckInput(inputBuffer, inputOffset, inputCount);
            // check output parameters
            if (outputBuffer == null)
                throw new ArgumentNullException("outputBuffer");
            if (outputOffset < 0)
                throw new ArgumentOutOfRangeException("outputOffset", "< 0");
            // ordered to avoid possible integer overflow
            if (outputOffset > outputBuffer.Length - inputCount)
                throw new ArgumentException("outputBuffer overflow");

            return InternalTransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            CheckInput(inputBuffer, inputOffset, inputCount);

            var output = new byte[inputCount];
            InternalTransformBlock(inputBuffer, inputOffset, inputCount, output, 0);
            return output;
        }

        ~Arc4Managed()
        {
            Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_mDisposed)
            {
                _x = 0;
                _y = 0;
                if (_key != null)
                {
                    Array.Clear(_key, 0, _key.Length);
                    _key = null;
                }
                Array.Clear(_state, 0, _state.Length);
                _state = null;
                GC.SuppressFinalize(this);
                _mDisposed = true;
            }
        }

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgvIv)
        {
            Key = rgbKey;
            return this;
        }

        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgvIv)
        {
            Key = rgbKey;
            return CreateEncryptor();
        }

        public override void GenerateIV()
        {
            // not used for a stream cipher
            IV = new byte[0];
        }

        public override void GenerateKey()
        {
            throw new NotImplementedException();
        }

        private void KeySetup(byte[] key)
        {
            byte index1 = 0;
            byte index2 = 0;

            for (var counter = 0; counter < 256; counter++)
                _state[counter] = (byte) counter;
            _x = 0;
            _y = 0;
            for (var counter = 0; counter < 256; counter++)
            {
                index2 = (byte) (key[index1] + _state[counter] + index2);
                // swap byte
                var tmp = _state[counter];
                _state[counter] = _state[index2];
                _state[index2] = tmp;
                index1 = (byte) ((index1 + 1)%key.Length);
            }
        }

        private void CheckInput(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            if (inputBuffer == null)
                throw new ArgumentNullException("inputBuffer");
            if (inputOffset < 0)
                throw new ArgumentOutOfRangeException("inputOffset", "< 0");
            if (inputCount < 0)
                throw new ArgumentOutOfRangeException("inputCount", "< 0");
            // ordered to avoid possible integer overflow
            if (inputOffset > inputBuffer.Length - inputCount)
                throw new ArgumentException("inputBuffer overflow");
        }

        private int InternalTransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer,
            int outputOffset)
        {
            for (var counter = 0; counter < inputCount; counter++)
            {
                _x = (byte) (_x + 1);
                _y = (byte) (_state[_x] + _y);
                // swap byte
                var tmp = _state[_x];
                _state[_x] = _state[_y];
                _state[_y] = tmp;

                var xorIndex = (byte) (_state[_x] + _state[_y]);
                outputBuffer[outputOffset + counter] = (byte) (inputBuffer[inputOffset + counter] ^ _state[xorIndex]);
            }
            return inputCount;
        }
    }
}