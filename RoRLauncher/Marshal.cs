using System;
using System.Text;

namespace RoRLauncher
{
    public static class Marshal
    {
        public static string ConvertToString(byte[] cstyle)
        {
            if (cstyle == null)
            {
                return null;
            }
            for (int i = 0; i < cstyle.Length; i++)
            {
                if (cstyle[i] == 0)
                {
                    return System.Text.Encoding.UTF8.GetString(cstyle, 0, i);
                }
            }
            return System.Text.Encoding.UTF8.GetString(cstyle);
        }

        public static int ConvertToInt32(byte[] val)
        {
            return Marshal.ConvertToInt32(val, 0);
        }

        public static int ConvertToInt32(byte[] val, int startIndex)
        {
            return Marshal.ConvertToInt32(val[startIndex], val[startIndex + 1], val[startIndex + 2], val[startIndex + 3]);
        }

        public static int ConvertToInt32(byte v1, byte v2, byte v3, byte v4)
        {
            return (int)v1 << 24 | (int)v2 << 16 | (int)v3 << 8 | (int)v4;
        }

        public static uint ConvertToUInt32(byte[] val)
        {
            return Marshal.ConvertToUInt32(val, 0);
        }

        public static uint ConvertToUInt32(byte[] val, int startIndex)
        {
            return Marshal.ConvertToUInt32(val[startIndex], val[startIndex + 1], val[startIndex + 2], val[startIndex + 3]);
        }

        public static uint ConvertToUInt32(byte v1, byte v2, byte v3, byte v4)
        {
            return (uint)((int)v1 << 24 | (int)v2 << 16 | (int)v3 << 8 | (int)v4);
        }

        public static float ConvertToFloat(byte v1, byte v2, byte v3, byte v4)
        {
            return (float)((int)v1 << 24 | (int)v2 << 16 | (int)v3 << 8 | (int)v4);
        }

        public static short ConvertToInt16(byte[] val)
        {
            return Marshal.ConvertToInt16(val, 0);
        }

        public static short ConvertToInt16(byte[] val, int startIndex)
        {
            return Marshal.ConvertToInt16(val[startIndex], val[startIndex + 1]);
        }

        public static short ConvertToInt16(byte v1, byte v2)
        {
            return (short)((int)v1 << 8 | (int)v2);
        }

        public static ushort ConvertToUInt16(byte[] val)
        {
            return Marshal.ConvertToUInt16(val, 0);
        }

        public static ushort ConvertToUInt16(byte[] val, int startIndex)
        {
            return Marshal.ConvertToUInt16(val[startIndex], val[startIndex + 1]);
        }

        public static ushort ConvertToUInt16(byte v1, byte v2)
        {
            return (ushort)((int)v2 | (int)v1 << 8);
        }
    }
}
