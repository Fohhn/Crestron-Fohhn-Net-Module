using System;
using System.Collections.Generic;
using System.Linq;

namespace Fohhn.FohhnNet
{
    class Request
    {
        /* Request command  
           1. Byte Startbyte        <SB>
           2. Byte Device ID        <ID>
           3. Byte Databyte Count   <COUNT>
           4. Byte Command Byte     <CMD>
           5. Byte Address MSB      <ADR_MSB>    We add this as part of the Data section
           6. Byte Address LSB      <ADR_LSB>    We add this as part of the Data section
           7. Byte Databyte 1       <DATA>       Minimum one databyte
           N. Bytes
        */

        public byte DeviceId { get; set; }
        public byte Command { get; set; }
        public byte[] Data { get; set; }
        private readonly byte[] _raw;

        private readonly byte _dataLength;

        public Action<Request, Response> ResponseCallback { get; private set; }

        public Request(byte[] raw, Action<Request, Response> callback)
        {
            if (raw.Length < 4)
                throw new ArgumentException("Command must be at least 4 bytes long");

            DeviceId = raw[1];
            Command = raw[3];
            _raw = raw;
            ResponseCallback = callback;
        }

        public Request(byte deviceId, byte command, byte[] data, Action<Request, Response> responseCallback)
        {
            ResponseCallback = responseCallback;
            if (data.Length < 3)
                throw new ArgumentException("Data must be at least 3 bytes long (ADR_MSB, ADR_LSB, Data)");

            DeviceId = deviceId;
            Command = command;
            Data = ReplaceIllegalBytes(data);
            _dataLength = (byte)(data.Length - 2);
        }

        private byte[] ReplaceIllegalBytes(byte[] bytes)
        {
            var newBytes = new List<byte>();
            newBytes.Add(bytes[0]);
            newBytes.Add(bytes[1]);
            for (int i = 2; i < bytes.Length; i++)
            {
                switch (bytes[i])
                {
                    case 0xF0:
                        newBytes.Add(0xFF);
                        newBytes.Add(0x00);
                        break;
                    case 0xFF:
                        newBytes.Add(0xFF);
                        newBytes.Add(0x01);
                        break;
                    default:
                        newBytes.Add(bytes[i]);
                        break;
                }
            }
            return newBytes.ToArray();
        } 

        public byte[] GetBytes()
        {
            if (_raw != null)
                return _raw;
            
            var bytes = new byte[4 + Data.Length];
            bytes[0] = 0xF0;
            bytes[1] = DeviceId;
            bytes[2] = _dataLength;
            bytes[3] = Command;
            for (int i = 0; i < Data.Length; i++)
            {
                bytes[4 + i] = Data[i];
            }
            return bytes;
        }
    }
}