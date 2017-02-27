using System;
using System.IO;

namespace RoRLauncher
{
    public class PacketIn : System.IO.MemoryStream
    {
        public ulong Opcode;

        public ulong Size;

        public PacketIn(int size) : base(size)
        {
        }

        public PacketIn(byte[] buf, int start, int size) : base(buf, start, size)
        {
        }

        public virtual byte GetUint8()
        {
            return (byte)this.ReadByte();
        }

        public virtual ushort GetUint16()
        {
            byte v = (byte)this.ReadByte();
            byte v2 = (byte)this.ReadByte();
            return Marshal.ConvertToUInt16(v, v2);
        }

        public virtual ushort GetUint16Reversed()
        {
            byte v = (byte)this.ReadByte();
            byte v2 = (byte)this.ReadByte();
            return Marshal.ConvertToUInt16(v2, v);
        }

        public virtual short GetInt16()
        {
            byte[] value = new byte[]
            {
                this.GetUint8(),
                this.GetUint8()
            };
            return System.BitConverter.ToInt16(value, 0);
        }

        public virtual uint GetUint32()
        {
            byte v = (byte)this.ReadByte();
            byte v2 = (byte)this.ReadByte();
            byte v3 = (byte)this.ReadByte();
            byte v4 = (byte)this.ReadByte();
            return Marshal.ConvertToUInt32(v, v2, v3, v4);
        }

        public virtual int GetInt32()
        {
            byte[] value = new byte[]
            {
                this.GetUint8(),
                this.GetUint8(),
                this.GetUint8(),
                this.GetUint8()
            };
            return System.BitConverter.ToInt32(value, 0);
        }

        public void Skip(long num)
        {
            this.Seek(num, System.IO.SeekOrigin.Current);
        }

        public virtual string GetString()
        {
            int @uint = (int)this.GetUint32();
            byte[] array = new byte[@uint];
            this.Read(array, 0, @uint);
            return Marshal.ConvertToString(array);
        }

        public virtual string GetString(int maxlen)
        {
            byte[] array = new byte[maxlen];
            this.Read(array, 0, maxlen);
            return Marshal.ConvertToString(array);
        }

        public virtual string GetPascalString()
        {
            return this.GetString((int)this.GetUint8());
        }

        private char ReadPs()
        {
            if (this.Length >= this.Position + 2L)
            {
                return System.BitConverter.ToChar(new byte[]
                {
                    this.GetUint8(),
                    this.GetUint8()
                }, 0);
            }
            return '\0';
        }

        public virtual string GetParsedString()
        {
            string text = "";
            for (char c = this.ReadPs(); c != '\0'; c = this.ReadPs())
            {
                text += c;
            }
            return text;
        }

        public virtual uint GetUint32Reversed()
        {
            byte v = (byte)this.ReadByte();
            byte v2 = (byte)this.ReadByte();
            byte v3 = (byte)this.ReadByte();
            byte v4 = (byte)this.ReadByte();
            return Marshal.ConvertToUInt32(v4, v3, v2, v);
        }

        public override string ToString()
        {
            return base.GetType().Name;
        }

        public ulong GetUint64()
        {
            return (ulong)((this.GetUint32() << 24) + this.GetUint32());
        }

        public float GetFloat()
        {
            return System.BitConverter.ToSingle(new byte[]
            {
                (byte)this.ReadByte(),
                (byte)this.ReadByte(),
                (byte)this.ReadByte(),
                (byte)this.ReadByte()
            }, 0);
        }
    }
}
