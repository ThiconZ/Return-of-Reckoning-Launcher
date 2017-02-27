using System;
using System.IO;
using System.Text;

namespace RoRLauncher
{
    public class PacketOut : System.IO.MemoryStream
    {
        public static int SizeLen = 4;

        public static bool OpcodeInLen = false;

        public int OpcodeLen = 2;

        public static bool reversed = false;

        public ulong Opcode;

        protected PacketOut()
        {
        }

        public PacketOut(byte opcode) : base(1 + PacketOut.SizeLen)
        {
            this.WriteSize();
            this.Opcode = (ulong)opcode;
            this.WriteByte(opcode);
            this.OpcodeLen = 1;
        }

        public PacketOut(ushort opcode) : base(2 + PacketOut.SizeLen)
        {
            this.WriteSize();
            this.Opcode = (ulong)opcode;
            this.OpcodeLen = 2;
            if (!PacketOut.reversed)
            {
                this.WriteUInt16(opcode);
                return;
            }
            this.WriteUInt16Reverse(opcode);
        }

        public PacketOut(uint opcode) : base(4 + PacketOut.SizeLen)
        {
            this.WriteSize();
            this.Opcode = (ulong)opcode;
            this.OpcodeLen = 4;
            if (!PacketOut.reversed)
            {
                this.WriteUInt32(opcode);
                return;
            }
            this.WriteUInt32Reverse(opcode);
        }

        public PacketOut(ulong opcode) : base(8 + PacketOut.SizeLen)
        {
            this.WriteSize();
            this.Opcode = opcode;
            this.OpcodeLen = 8;
            if (!PacketOut.reversed)
            {
                this.WriteUInt64(opcode);
                return;
            }
            this.WriteUInt64Reverse(opcode);
        }

        public void WriteSize()
        {
            for (int i = 0; i < PacketOut.SizeLen; i++)
            {
                this.WriteByte(0);
            }
        }

        public virtual void WriteUInt16(ushort val)
        {
            this.WriteByte((byte)(val >> 8));
            this.WriteByte((byte)(val & 255));
        }

        public virtual void WriteUInt16Reverse(ushort val)
        {
            this.WriteByte((byte)(val & 255));
            this.WriteByte((byte)(val >> 8));
        }

        public virtual void WriteUInt32(uint val)
        {
            this.WriteByte((byte)(val >> 24));
            this.WriteByte((byte)(val >> 16 & 255u));
            this.WriteByte((byte)((val & 65535u) >> 8));
            this.WriteByte((byte)(val & 65535u & 255u));
        }

        public virtual void WriteUInt32Reverse(uint val)
        {
            this.WriteByte((byte)(val & 65535u & 255u));
            this.WriteByte((byte)((val & 65535u) >> 8));
            this.WriteByte((byte)(val >> 16 & 255u));
            this.WriteByte((byte)(val >> 24));
        }

        public virtual void WriteUInt64(ulong val)
        {
            this.WriteByte((byte)(val >> 56));
            this.WriteByte((byte)(val >> 48 & 255uL));
            this.WriteByte((byte)(val >> 40 & 255uL));
            this.WriteByte((byte)(val >> 32 & 255uL));
            this.WriteByte((byte)(val >> 24 & 255uL));
            this.WriteByte((byte)(val >> 16 & 255uL));
            this.WriteByte((byte)(val >> 8 & 255uL));
            this.WriteByte((byte)(val & 255uL));
        }

        public virtual void WriteInt16(short val)
        {
            byte[] bytes = System.BitConverter.GetBytes(val);
            for (int i = bytes.Length; i > 0; i--)
            {
                this.WriteByte(bytes[i - 1]);
            }
        }

        public virtual void WriteInt32(int val)
        {
            byte[] bytes = System.BitConverter.GetBytes(val);
            for (int i = bytes.Length; i > 0; i--)
            {
                this.WriteByte(bytes[i - 1]);
            }
        }

        public virtual void WriteInt32Reverse(int val)
        {
            byte[] bytes = System.BitConverter.GetBytes(val);
            for (int i = 0; i < bytes.Length; i++)
            {
                this.WriteByte(bytes[i]);
            }
        }

        public virtual void WriteInt64(long val)
        {
            byte[] bytes = System.BitConverter.GetBytes(val);
            for (int i = bytes.Length; i > 0; i--)
            {
                this.WriteByte(bytes[i - 1]);
            }
        }

        public virtual void WriteInt64Reverse(long val)
        {
            byte[] bytes = System.BitConverter.GetBytes(val);
            for (int i = 0; i < bytes.Length; i++)
            {
                this.WriteByte(bytes[i]);
            }
        }

        public virtual void WriteFloat(float val)
        {
            byte[] bytes = System.BitConverter.GetBytes(val);
            for (int i = 0; i < bytes.Length; i++)
            {
                byte value = bytes[i];
                this.WriteByte(value);
            }
        }

        public virtual void WriteUInt64Reverse(ulong val)
        {
            this.WriteByte((byte)(val & 255uL));
            this.WriteByte((byte)(val >> 8 & 255uL));
            this.WriteByte((byte)(val >> 16 & 255uL));
            this.WriteByte((byte)(val >> 24 & 255uL));
            this.WriteByte((byte)(val >> 32 & 255uL));
            this.WriteByte((byte)(val >> 40 & 255uL));
            this.WriteByte((byte)(val >> 48 & 255uL));
            this.WriteByte((byte)(val >> 56));
        }

        public virtual byte GetChecksum()
        {
            byte b = 0;
            byte[] buffer = this.GetBuffer();
            int num = 0;
            while ((long)num < this.Position - 6L)
            {
                b += buffer[num + 8];
                num++;
            }
            return b;
        }

        public virtual void Fill(byte val, int num)
        {
            for (int i = 0; i < num; i++)
            {
                this.WriteByte(val);
            }
        }

        public virtual ulong WritePacketLength()
        {
            this.Position = 0L;
            long num = (!PacketOut.OpcodeInLen) ? (this.Length - (long)this.OpcodeLen) : this.Length;
            if (!PacketOut.reversed)
            {
                int sizeLen = PacketOut.SizeLen;
                switch (sizeLen)
                {
                    case 1:
                        this.WriteByte((byte)num);
                        break;
                    case 2:
                        this.WriteUInt16((ushort)num);
                        break;
                    case 3:
                        break;
                    case 4:
                        this.WriteUInt32((uint)num);
                        break;
                    default:
                        if (sizeLen == 8)
                        {
                            this.WriteUInt64((ulong)num);
                        }
                        break;
                }
            }
            else
            {
                int sizeLen2 = PacketOut.SizeLen;
                switch (sizeLen2)
                {
                    case 1:
                        this.WriteByte((byte)num);
                        break;
                    case 2:
                        this.WriteUInt16Reverse((ushort)num);
                        break;
                    case 3:
                        break;
                    case 4:
                        this.WriteUInt32Reverse((uint)num);
                        break;
                    default:
                        if (sizeLen2 == 8)
                        {
                            this.WriteUInt64Reverse((ulong)num);
                        }
                        break;
                }
            }
            this.Capacity = (int)this.Length;
            return (ulong)num;
        }

        public virtual void WritePascalString(string str)
        {
            if (str == null || str.Length <= 0)
            {
                this.WriteByte(0);
                return;
            }
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(str);
            this.WriteByte((byte)bytes.Length);
            this.Write(bytes, 0, bytes.Length);
        }

        public virtual void WriteStringToZero(string str)
        {
            if (str == null || str.Length <= 0)
            {
                this.WriteByte(1);
            }
            else
            {
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(str);
                this.WriteByte((byte)(bytes.Length + 1));
                this.Write(bytes, 0, bytes.Length);
            }
            this.WriteByte(0);
        }

        public virtual void WriteString(string str)
        {
            this.WriteUInt32((uint)str.Length);
            this.WriteStringBytes(str);
        }

        public virtual void WriteStringBytes(string str)
        {
            if (str.Length <= 0)
            {
                return;
            }
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            this.Write(bytes, 0, bytes.Length);
        }

        public virtual void WriteString(string str, int maxlen)
        {
            if (str.Length <= 0)
            {
                return;
            }
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            this.Write(bytes, 0, (bytes.Length < maxlen) ? bytes.Length : maxlen);
        }

        public virtual void WriteParsedString(string str)
        {
            byte[] bytes = System.Text.Encoding.Unicode.GetBytes(str);
            for (int i = 0; i < bytes.Length; i++)
            {
                this.WriteByte(bytes[i]);
            }
            this.WriteByte(0);
            this.WriteByte(0);
        }

        public virtual void WriteParsedString(string str, int maxlen)
        {
            byte[] bytes = System.Text.Encoding.Unicode.GetBytes(str);
            int i = 0;
            while (i < bytes.Length && i < maxlen)
            {
                this.WriteByte(bytes[i]);
                i++;
            }
            if (i < maxlen)
            {
                while (i < maxlen)
                {
                    this.WriteByte(0);
                    i++;
                }
            }
            this.WriteByte(0);
            this.WriteByte(0);
        }

        public virtual void FillString(string str, int len)
        {
            long position = this.Position;
            this.Fill(0, len);
            if (str == null)
            {
                return;
            }
            this.Position = position;
            if (str.Length <= 0)
            {
                this.Position = position + (long)len;
                return;
            }
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            this.Write(bytes, 0, (len > bytes.Length) ? bytes.Length : len);
            this.Position = position + (long)len;
        }

        public override string ToString()
        {
            return base.GetType().Name;
        }
    }
}
