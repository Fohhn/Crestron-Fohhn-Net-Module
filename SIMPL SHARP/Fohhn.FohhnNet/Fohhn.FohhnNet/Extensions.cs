using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Fohhn.FohhnNet
{
    static class Extensions
    {
        public static ushort ToUshort(this byte[] array, int startIndex)
        {
            var bytes = new byte[2];
            Buffer.BlockCopy(array, startIndex, bytes, 0, 2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToUInt16(bytes, 0);
        }
        public static short ToShort(this byte[] array, int startIndex)
        {
            var bytes = new byte[2];
            Buffer.BlockCopy(array, startIndex, bytes, 0, 2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }
        public static byte[] GetBytes(this short val)
        {
            var bytes = BitConverter.GetBytes(val);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }
        public static bool[] ToBits(this byte b)
        {
            var bits = new bool[8];
            for (int i = 0; i < 8; i++)
                bits[i] = ((b >> i) & 1) == 1;
            return bits;
        }
        public static string GetString(this byte[] bytes)
        {
            return new string(bytes.Select(b => (char) b).ToArray());
        }
        public static string GetString(this byte[] bytes, int startIndex, int count)
        {
            var arr = new byte[count];
            Buffer.BlockCopy(bytes, startIndex, arr, 0, count);
            return GetString(arr);
        }
        public static string GetBytesAsString(this byte[] bytes)
        {
            return String.Join(" ", bytes.Select(x => x.ToString("X2")).ToArray());
        }
    }
}