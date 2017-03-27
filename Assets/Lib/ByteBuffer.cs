using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YggioUnity
{
    public class ByteBuffer
    {
        private static byte[] a16 = new byte[2];
        private static byte[] a32 = new byte[4];
        private static byte[] a64 = new byte[8];
        private static byte[] _readBuffer = new byte[8];
        private readonly List<byte> prebuffer;
        private readonly List<byte> postbuffer;

        private int packetId { get; set; }

        private PacketType packetType { get; set; }

        public static byte[] ReadBuffer
        {
            get { return _readBuffer; }
            set { _readBuffer = value; }
        }

        public ByteBuffer(PacketType packetType, int packetId)
        {
            this.packetType = packetType;
            this.packetId = packetId;
            prebuffer = new List<byte>();
            postbuffer = new List<byte>();
        }

        public static int Read(byte[] b)
        {
            return GameClient.Singleton.NetworkIn.Read(b, 0, b.Length);
        }

        public static int Read(byte[] b, int off, int len)
        {
            return GameClient.Singleton.NetworkIn.Read(b, off, len);
        }

        public static void ReadFully(byte[] b)
        {
            ReadFully(b, 0, b.Length);
        }

        public static void ReadFully(byte[] b, int off, int len)
        {
            if (len < 0)
                throw new IndexOutOfRangeException();
            var num1 = 0;
            while (num1 < len)
            {
                var num2 = GameClient.Singleton.NetworkIn.Read(b, off + num1, len - num1);
                if (num2 < 0)
                    throw new EndOfStreamException();
                num1 += num2;
            }
        }

        public void WriteByte(int value)
        {
            prebuffer.Add((byte) value);
        }

        public static byte ReadByte()
        {
            return GameClient.Singleton.NetworkIn.ReadByte();
        }

        public void WriteChar(char value)
        {
            prebuffer.Add((byte) ((uint) value >> 8 & byte.MaxValue));
            prebuffer.Add((byte) (value & (uint) byte.MaxValue));
        }

        public static char ReadChar()
        {
            return (char) (((uint) GameClient.Singleton.NetworkIn.ReadByte() << 8) + GameClient.Singleton.NetworkIn.ReadByte());
        }

        public void WriteDouble(double value)
        {
            WriteLong(BitConverter.DoubleToInt64Bits(value));
        }

        public static double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble(ReadLong());
        }

        public void WriteFloat(float value)
        {
            WriteInt(floatToIntBits(value));
        }

        public static float ReadFloat()
        {
            Array.Clear(a32, 0, 4);
            a32 = GameClient.Singleton.NetworkIn.ReadBytes(4);
            Array.Reverse(a32);
            return BitConverter.ToSingle(a32, 0);
        }

        public void WriteShort(int value)
        {
            prebuffer.Add((byte) ((ushort) value >> 8 & byte.MaxValue));
            prebuffer.Add((byte) ((ushort) value & (uint) byte.MaxValue));
        }

        public static short ReadShort()
        {
            Array.Clear(a16, 0, 2);
            a16 = GameClient.Singleton.NetworkIn.ReadBytes(2);
            Array.Reverse(a16);
            return BitConverter.ToInt16(a16, 0);
        }

        public void WriteInt(int value)
        {
            prebuffer.Add((byte) ((uint) value >> 24 & byte.MaxValue));
            prebuffer.Add((byte) ((uint) value >> 16 & byte.MaxValue));
            prebuffer.Add((byte) ((uint) value >> 8 & byte.MaxValue));
            prebuffer.Add((byte) (value & byte.MaxValue));
        }

        public static int ReadInt()
        {
            Array.Clear(a32, 0, 4);
            a32 = GameClient.Singleton.NetworkIn.ReadBytes(4);
            Array.Reverse(a32);
            return BitConverter.ToInt32(a32, 0);
        }

        public void WriteBoolean(bool value)
        {
            prebuffer.Add(value ? (byte) 1 : (byte) 0);
        }

        public static bool ReadBoolean()
        {
            return GameClient.Singleton.NetworkIn.ReadByte() != 0;
        }

        public void WriteLong(long value)
        {
            prebuffer.Add((byte) ((uint) value >> 24 & byte.MaxValue));
            prebuffer.Add((byte) ((uint) value >> 16 & byte.MaxValue));
            prebuffer.Add((byte) ((uint) value >> 8 & byte.MaxValue));
            prebuffer.Add((byte) ((uint) value & byte.MaxValue));
            prebuffer.Add((byte) ((uint) value >> 24 & byte.MaxValue));
            prebuffer.Add((byte) ((uint) value >> 16 & byte.MaxValue));
            prebuffer.Add((byte) ((uint) value >> 8 & byte.MaxValue));
            prebuffer.Add((byte) ((uint) value & byte.MaxValue));
        }

        public static long ReadLong()
        {
            Array.Clear(a64, 0, 8);
            a64 = GameClient.Singleton.NetworkIn.ReadBytes(8);
            Array.Reverse(a64);
            return BitConverter.ToInt64(a64, 0);
        }

        public void WriteString(string value)
        {
            WriteInt(value.Length);
            foreach (var ch in value.ToCharArray())
                WriteChar(ch);
        }

        public static string ReadString()
        {
            var num = ReadInt();
            var str = "";
            for (var index = 0; index < num; ++index)
                str += (string) (object) ReadChar();
            return str;
        }

        public void WriteUdpKey()
        {
            WriteLong(GameClient.Singleton.UdpKey.MostSigBits);
            WriteLong(GameClient.Singleton.UdpKey.LeastSigBits);
        }

        public static UUID ReadUUID()
        {
            return new UUID(ReadLong(), ReadLong());
        }

        public void WriteVector2(Vector2 value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
        }

        public static Vector2 ReadVector2()
        {
            return new Vector2(ReadFloat(), ReadFloat());
        }

        public void WriteVector3(Vector3 value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
            WriteFloat(value.z);
        }

        public static Vector3 ReadVector3()
        {
            return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
        }

        private int floatToIntBits(float value)
        {
            var num = BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
            if ((num & 2139095040) == 2139095040 && (num & 2147483648L) != 0L)
                num = 2143289344;
            return num;
        }

        public void Send()
        {
            postbuffer.Add((byte) ((uint) prebuffer.Count >> 24 & byte.MaxValue));
            postbuffer.Add((byte) ((uint) prebuffer.Count >> 16 & byte.MaxValue));
            postbuffer.Add((byte) ((uint) prebuffer.Count >> 8 & byte.MaxValue));
            postbuffer.Add((byte) (prebuffer.Count & byte.MaxValue));
            postbuffer.Add((byte) ((uint) packetId >> 24 & byte.MaxValue));
            postbuffer.Add((byte) ((uint) packetId >> 16 & byte.MaxValue));
            postbuffer.Add((byte) ((uint) packetId >> 8 & byte.MaxValue));
            postbuffer.Add((byte) (packetId & byte.MaxValue));
            foreach (var num in prebuffer)
                postbuffer.Add(num);
            GameClient.Singleton.NetworkOut.Write(postbuffer.ToArray());
        }
    }
}
