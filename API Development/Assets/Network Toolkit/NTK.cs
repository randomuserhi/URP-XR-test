using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace NetworkToolkit
{
    public static partial class NTK
    {
        public const int bufferSize = 4069;
        public const int bufferSizeNoHeader = bufferSize - NTK.Packet.groupHeaderSize;

        public static float tickRate = 100f;
        public static int timeout = 1000;

        private static ushort _packetID = 0;
        public static ushort packetID { get => _packetID++; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool isSequenceGreater(ushort a, ushort b) // a > b
        {
            return ((a > b) && (a - b <= 32768)) ||
                    ((a < b) && (b - a > 32768));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RotateLeft(uint value, int offset)
            => (value << offset) | (value >> (32 - offset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RotateRight(uint value, int offset)
            => (value >> offset) | (value << (32 - offset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReverseEndianness(int value) => (int)ReverseEndianness((uint)value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReverseEndianness(short value) => (short)ReverseEndianness((ushort)value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort ReverseEndianness(ushort value)
        {
            // Don't need to AND with 0xFF00 or 0x00FF since the final
            // cast back to ushort will clear out all bits above [ 15 .. 00 ].
            // This is normally implemented via "movzx eax, ax" on the return.
            // Alternatively, the compiler could elide the movzx instruction
            // entirely if it knows the caller is only going to access "ax"
            // instead of "eax" / "rax" when the function returns.

            return (ushort)((value >> 8) + (value << 8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ReverseEndianness(uint value)
        {
            // This takes advantage of the fact that the JIT can detect
            // ROL32 / ROR32 patterns and output the correct intrinsic.
            //
            // Input: value = [ ww xx yy zz ]
            //
            // First line generates : [ ww xx yy zz ]
            //                      & [ 00 FF 00 FF ]
            //                      = [ 00 xx 00 zz ]
            //             ROR32(8) = [ zz 00 xx 00 ]
            //
            // Second line generates: [ ww xx yy zz ]
            //                      & [ FF 00 FF 00 ]
            //                      = [ ww 00 yy 00 ]
            //             ROL32(8) = [ 00 yy 00 ww ]
            //
            //                (sum) = [ zz yy xx ww ]
            //
            // Testing shows that throughput increases if the AND
            // is performed before the ROL / ROR.

            return RotateRight(value & 0x00FF00FFu, 8) // xx zz
                + RotateLeft(value & 0xFF00FF00u, 8); // ww yy
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void _WriteBytes(byte* source, int size, byte[] destination, int index = 0)
        {
            for (int i = 0; i < size; ++i)
            {
                destination[index + i] = source[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void WriteBytes(uint value, byte[] destination, int index = 0)
        {
            if (!BitConverter.IsLittleEndian) value = ReverseEndianness(value);
            _WriteBytes((byte*)&value, sizeof(uint), destination, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void WriteBytes(ushort value, byte[] destination, int index = 0)
        {
            if (!BitConverter.IsLittleEndian) value = ReverseEndianness(value);
            _WriteBytes((byte*)&value, sizeof(ushort), destination, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void WriteBytes(int value, byte[] destination, int index = 0)
        {
            if (!BitConverter.IsLittleEndian) value = ReverseEndianness(value);
            _WriteBytes((byte*)&value, sizeof(int), destination, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void WriteBytes(short value, byte[] destination, int index = 0)
        {
            if (!BitConverter.IsLittleEndian) value = ReverseEndianness(value);
            _WriteBytes((byte*)&value, sizeof(short), destination, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void WriteBytes(float value, byte[] destination, int index = 0)
        {
            int to32 = *((int*)&value);
            if (!BitConverter.IsLittleEndian) to32 = ReverseEndianness(to32);
            _WriteBytes((byte*)&to32, sizeof(int), destination, index);
        }

        // https://stackoverflow.com/questions/1659440/32-bit-to-16-bit-floating-point-conversion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint AsUInt(float x) {
            return *(uint*)&x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe float AsFloat(uint x)
        {
            return *(float*)&x;
        }

        // NOTE:: These Half <-> Float conversions do not account for Infinity or NaN!

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float HalfToFloat(ushort x)
        { 
            // IEEE-754 16-bit floating-point format (without infinity): 1-5-10, exp-15, +-131008.0, +-6.1035156E-5, +-5.9604645E-8, 3.311 digits
            int e = (x & 0x7C00) >> 10; // exponent
            int m = (x & 0x03FF) << 13; // mantissa
            int v = (int)(AsUInt((float)m) >> 23); // evil log2 bit hack to count leading zeros in denormalized format
            return AsFloat((uint)((x & 0x8000) << 16 | Convert.ToInt32(e != 0) * ((e + 112) << 23 | m) | (Convert.ToInt32(e == 0) & Convert.ToInt32(m != 0)) * ((v - 37) << 23 | ((m << (150 - v)) & 0x007FE000)))); // sign : normalized : denormalized
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort FloatToHalf(float x)
        { 
            // IEEE-754 16-bit floating-point format (without infinity): 1-5-10, exp-15, +-131008.0, +-6.1035156E-5, +-5.9604645E-8, 3.311 digits
            uint b = AsUInt(x) + 0x00001000; // round-to-nearest-even: add last bit after truncated mantissa
            uint e = (b & 0x7F800000) >> 23; // exponent
            uint m = b & 0x007FFFFF; // mantissa; in line below: 0x007FF000 = 0x00800000-0x00001000 = decimal indicator flag - initial rounding
            return (ushort)((b & 0x80000000) >> 16 | Convert.ToInt32(e > 112) * ((((e - 112) << 10) & 0x7C00) | m >> 13) | (Convert.ToInt32(e < 113) & Convert.ToInt32(e > 101)) * ((((0x007FF000 + m) >> (int)(125 - e)) + 1) >> 1) | Convert.ToUInt32(e > 143) * 0x7FFF); // sign : normalized : denormalized : saturate
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint ReadUInt(byte[] source, int index = 0)
        {
            fixed (byte* converted = source)
            {
                uint result = *(uint*)(converted + index);
                if (!BitConverter.IsLittleEndian)
                {
                    result = ReverseEndianness(result);
                }
                return result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe int ReadInt(byte[] source, int index = 0)
        {
            fixed (byte* converted = source)
            {
                int result = *(int*)(converted + index);
                if (!BitConverter.IsLittleEndian)
                {
                    result = ReverseEndianness(result);
                }
                return result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe ushort ReadUShort(byte[] source, int index = 0)
        {
            fixed (byte* converted = source)
            {
                ushort result = *(ushort*)(converted + index);
                if (!BitConverter.IsLittleEndian)
                {
                    result = ReverseEndianness(result);
                }
                return result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe short ReadShort(byte[] source, int index = 0)
        {
            fixed (byte* converted = source)
            {
                short result = *(short*)(converted + index);
                if (!BitConverter.IsLittleEndian)
                {
                    result = ReverseEndianness(result);
                }
                return result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe float ReadFloat(byte[] source, int index = 0)
        {
            fixed (byte* converted = source)
            {
                if (!BitConverter.IsLittleEndian)
                {
                    int value = ReverseEndianness(*(int*)(converted + index));
                    return *((float*)&value);
                }
                return *(float*)(converted + index);
            }
        }

        public struct PacketHeader
        {
            // sequence + ack + ackBitField
            public const ushort size = sizeof(ushort) + sizeof(ushort) + sizeof(int);

            public ushort sequence;
            public ushort ack;
            public int ackBitfield;

            public unsafe byte[] bytes 
            { 
                get
                {
                    byte[] buffer = new byte[size];
                    WriteBytes(sequence, buffer);
                    WriteBytes(ack, buffer, sizeof(ushort));
                    WriteBytes(ackBitfield, buffer, 2 * sizeof(ushort));

                    return buffer;
                }
            }
        }

        public partial struct Packet
        {
            // packetID + totalByteCount + packetIndex + groups
            // packetIndex is only a byte since its not expected to send more than 256 * bufferSize amount of data
            public const ushort groupHeaderSize = sizeof(ushort) + sizeof(ushort) + sizeof(byte) + sizeof(byte);

            private byte[] data;
            public int writtenSize { get; private set; }
            public int index;

            public static Packet Create(int size)
            {
                return new Packet()
                {
                    data = new byte[size],
                    index = 0,
                    writtenSize = 0
                };
            }

            public static Packet Create(byte[] data)
            {
                return new Packet()
                {
                    data = data,
                    index = 0,
                    writtenSize = 0
                };
            }

            public void Copy(byte[] source, int numBytes)
            {
                Array.Copy(source, data, numBytes);
                writtenSize = numBytes;
            }

            public void CopyTo(byte[] destination, int index, int size)
            {
                Array.Copy(data, this.index, destination, index, size);
            }

            public void Reset()
            {
                index = 0;
                writtenSize = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(string value)
            {
                byte[] temp = Encoding.ASCII.GetBytes(value);
                if (index + temp.Length > data.Length) throw new Exception("Not enough space in buffer for string.");
                Write((byte)temp.Length);
                Array.Copy(temp, 0, data, index, temp.Length);
                index += temp.Length;
                writtenSize += temp.Length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(byte value)
            {
                if (index + sizeof(byte) > data.Length) throw new Exception("Not enough space in buffer for byte.");
                data[index] = value;
                index += sizeof(byte);
                writtenSize += sizeof(byte);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(bool value)
            {
                if (index + sizeof(byte) > data.Length) throw new Exception("Not enough space in buffer for bool.");
                data[index] = value ? (byte)1 : (byte)0;
                index += sizeof(byte);
                writtenSize += sizeof(byte);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(uint value)
            {
                if (index + sizeof(uint) > data.Length) throw new Exception("Not enough space in buffer for uint.");
                WriteBytes(value, data, index);
                index += sizeof(uint);
                writtenSize += sizeof(uint);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(int value)
            {
                if (index + sizeof(int) > data.Length) throw new Exception("Not enough space in buffer for int.");
                WriteBytes(value, data, index);
                index += sizeof(int);
                writtenSize += sizeof(int);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(ushort value)
            {
                if (index + sizeof(ushort) > data.Length) throw new Exception("Not enough space in buffer for ushort.");
                WriteBytes(value, data, index);
                index += sizeof(ushort);
                writtenSize += sizeof(ushort);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(short value)
            {
                if (index + sizeof(short) > data.Length) throw new Exception("Not enough space in buffer for short.");
                WriteBytes(value, data, index);
                index += sizeof(short);
                writtenSize += sizeof(short);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(float value)
            {
                if (index + sizeof(float) > data.Length) throw new Exception("Not enough space in buffer for float.");
                WriteBytes(value, data, index);
                index += sizeof(float);
                writtenSize += sizeof(float);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void WriteHalf(float value)
            {
                if (index + sizeof(ushort) > data.Length) throw new Exception("Not enough space in buffer for half.");
                WriteBytes(FloatToHalf(value), data, index); // TODO implement this
                index += sizeof(ushort);
                writtenSize += sizeof(ushort);
            }

            // TODO:: add exception for end of data (index + sizeof(data) > writtenLength)

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public string ReadString()
            {
                int length = ReadByte();
                string temp = Encoding.ASCII.GetString(data, index, length);
                index += length;
                return temp;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public byte ReadByte()
            {
                byte temp = data[index];
                index += sizeof(byte);
                return temp;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ReadBool()
            {
                bool temp = data[index] != 0;
                index += sizeof(byte);
                return temp;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint ReadUInt()
            {
                uint temp = NTK.ReadUInt(data, index);
                index += sizeof(uint);
                return temp;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int ReadInt()
            {
                int temp = NTK.ReadInt(data, index);
                index += sizeof(int);
                return temp;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ushort ReadUShort()
            {
                ushort temp = NTK.ReadUShort(data, index);
                index += sizeof(ushort);
                return temp;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public short ReadShort()
            {
                short temp = NTK.ReadShort(data, index);
                index += sizeof(short);
                return temp;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public float ReadFloat()
            {
                float temp = NTK.ReadFloat(data, index);
                index += sizeof(float);
                return temp;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public float ReadHalf()
            {
                ushort temp = NTK.ReadUShort(data, index);
                index += sizeof(ushort);
                return HalfToFloat(temp);
            }

            // TODO:: add exception for sending no data
            public unsafe bool GetBytes(PacketHeader packetHead, out byte[][] bytes)
            {
                bytes = null;

                byte[] packetheader = packetHead.bytes;
                int packetSize = packetheader.Length + writtenSize;
                ushort total = (ushort)packetSize;
                int space = (bufferSize - groupHeaderSize);
                int groups = packetSize / space;
                if (packetSize % space != 0) { ++groups; }
                if (groups > 256) return false; // There are more packet groups than packet indices

                bytes = new byte[groups][];
                ushort id = packetID;
                int read = 0;
                int remainingBytes = groups * groupHeaderSize + packetSize;
                for (byte i = 0; i < groups; ++i)
                {
                    int delta = bufferSize;
                    if (remainingBytes < bufferSize) delta = remainingBytes;
                    remainingBytes -= delta;

                    bytes[i] = new byte[delta];
                    byte[] buffer = bytes[i];

                    WriteBytes(id, buffer);
                    WriteBytes(total, buffer, sizeof(ushort));
                    buffer[2 * sizeof(ushort)] = i;
                    buffer[2 * sizeof(ushort) + sizeof(byte)] = (byte)groups;
                    delta -= groupHeaderSize;

                    int offset = 0;
                    if (i == 0)
                    {
                        Array.Copy(packetheader, 0, buffer, groupHeaderSize, packetheader.Length);
                        offset = packetheader.Length;
                        delta -= packetheader.Length;
                    }
                    Array.Copy(data, read, buffer, offset + groupHeaderSize, delta);

                    read += delta;
                }

                return true;
            }
        }
    }
}
