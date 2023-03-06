using System;
using System.Collections.Generic;
using System.Linq;

namespace Fohhn.FohhnNet
{
    class Response
    {
        public bool IsValid { get; private set; }
        public byte DeviceId { get; private set; }
        public byte[] Data { get; private set; }
        public bool IsAck { get { return Data.Length == 0; } }
        public byte[] Raw { get; private set; }

        /// <summary>
        /// Parses a response
        /// </summary>
        /// <param name="responseBytes">Must include the delimiter (0xF0)</param>
        public Response(byte[] responseBytes)
        {
            Raw = responseBytes;
            if (responseBytes.Length < 2 || responseBytes[responseBytes.Length - 1] != 0xF0)
                return;

            IsValid = true;
            DeviceId = responseBytes[responseBytes.Length - 2];
            var dataLength = responseBytes.Length - 2;
            if (dataLength == 0)
            {
                Data = new byte[0];
                return;
            }
            Data = ReplaceControlBytes(responseBytes);
        }
        private byte[] ReplaceControlBytes(byte[] responseBytes)
        {
            var newBytes = new List<byte>();
            for (int i = 0; i < responseBytes.Length - 2; i++)
            {
                if (responseBytes[i] == 0xFF)
                {
                    if (responseBytes[i + 1] == 0x00)
                        newBytes.Add(0xF0);
                    else if (responseBytes[i + 1] == 0x01)
                        newBytes.Add(0xFF);
                    i++;
                    continue;
                }
                newBytes.Add(responseBytes[i]);
            }
            return newBytes.ToArray();
        }
    }
}