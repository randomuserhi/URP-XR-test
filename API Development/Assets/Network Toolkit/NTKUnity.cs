using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NetworkToolkit
{
    public static partial class NTK
    {
        public partial struct Packet
        {
            public void Write(Vector3 value)
            {
                const int size = sizeof(float) * 3;
                if (index + size > data.Length) throw new Exception("Not enough space in buffer for Vector3.");
                Write(value.x);
                Write(value.y);
                Write(value.z);
            }

            public void Write(Quaternion value)
            {
                const int size = sizeof(float) * 3 + sizeof(byte);
                if (index + size > data.Length) throw new Exception("Not enough space in buffer for Quaternion.");

                float largest = value.x;
                byte i = 0;
                if (value.y > largest)
                {
                    largest = value.y;
                    i = 1;
                }
                if (value.z > largest)
                {
                    largest = value.z;
                    i = 2;
                }
                if (value.w > largest)
                {
                    largest = value.w;
                    i = 3;
                }

                Write(i);
                switch (i)
                {
                    case 0:
                        if (value.x >= 0)
                        {
                            Write(value.y);
                            Write(value.z);
                            Write(value.w);
                        }
                        else
                        {
                            Write(-value.y);
                            Write(-value.z);
                            Write(-value.w);
                        }
                        break;
                    case 1:
                        if (value.y >= 0)
                        {
                            Write(value.x);
                            Write(value.z);
                            Write(value.w);
                        }
                        else
                        {
                            Write(-value.x);
                            Write(-value.z);
                            Write(-value.w);
                        }
                        break;
                    case 2:
                        if (value.z >= 0)
                        {
                            Write(value.x);
                            Write(value.y);
                            Write(value.w);
                        }
                        else
                        {
                            Write(-value.x);
                            Write(-value.y);
                            Write(-value.w);
                        }
                        break;
                    case 3:
                        if (value.w >= 0)
                        {
                            Write(value.x);
                            Write(value.y);
                            Write(value.z);
                        }
                        else
                        {
                            Write(-value.x);
                            Write(-value.y);
                            Write(-value.z);
                        }
                        break;
                }
            }

            public Vector3 ReadVector3()
            {
                return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
            }

            public Quaternion ReadQuaternion()
            {
                byte i = ReadByte();
                float x = 0, y = 0, z = 0, w = 0;
                switch (i)
                {
                    case 0:
                        y = ReadFloat();
                        z = ReadFloat();
                        w = ReadFloat();
                        x = Mathf.Sqrt(1f - y * y - z * z - w * w);
                        break;
                    case 1:
                        x = ReadFloat();
                        z = ReadFloat();
                        w = ReadFloat();
                        y = Mathf.Sqrt(1f - x * x - z * z - w * w);
                        break;
                    case 2:
                        x = ReadFloat();
                        y = ReadFloat();
                        w = ReadFloat();
                        z = Mathf.Sqrt(1f - x * x - y * y - w * w);
                        break;
                    case 3:
                        x = ReadFloat();
                        y = ReadFloat();
                        z = ReadFloat();
                        w = Mathf.Sqrt(1f - x * x - y * y - z * z);
                        break;
                }
                return new Quaternion(x, y, z, w);
            }

            public void WriteHalf(Vector3 value)
            {
                const int size = sizeof(float) * 3;
                if (index + size > data.Length) throw new Exception("Not enough space in buffer for Vector3.");
                WriteHalf(value.x);
                WriteHalf(value.y);
                WriteHalf(value.z);
            }

            // TODO:: This is using a byte + 3 16-Float, but I should use 3 bits + 3 15-Float
            public void WriteHalf(Quaternion value)
            {
                const int size = sizeof(float) * 3 + sizeof(byte);
                if (index + size > data.Length) throw new Exception("Not enough space in buffer for Quaternion.");

                float largest = value.x;
                byte i = 0;
                if (value.y > largest)
                {
                    largest = value.y;
                    i = 1;
                }
                if (value.z > largest)
                {
                    largest = value.z;
                    i = 2;
                }
                if (value.w > largest)
                {
                    largest = value.w;
                    i = 3;
                }

                Write(i);
                switch (i)
                {
                    case 0:
                        if (value.x >= 0)
                        {
                            WriteHalf(value.y);
                            WriteHalf(value.z);
                            WriteHalf(value.w);
                        }
                        else
                        {
                            WriteHalf(-value.y);
                            WriteHalf(-value.z);
                            WriteHalf(-value.w);
                        }
                        break;
                    case 1:
                        if (value.y >= 0)
                        {
                            WriteHalf(value.x);
                            WriteHalf(value.z);
                            WriteHalf(value.w);
                        }
                        else
                        {
                            WriteHalf(-value.x);
                            WriteHalf(-value.z);
                            WriteHalf(-value.w);
                        }
                        break;
                    case 2:
                        if (value.z >= 0)
                        {
                            WriteHalf(value.x);
                            WriteHalf(value.y);
                            WriteHalf(value.w);
                        }
                        else
                        {
                            WriteHalf(-value.x);
                            WriteHalf(-value.y);
                            WriteHalf(-value.w);
                        }
                        break;
                    case 3:
                        if (value.w >= 0)
                        {
                            WriteHalf(value.x);
                            WriteHalf(value.y);
                            WriteHalf(value.z);
                        }
                        else
                        {
                            WriteHalf(-value.x);
                            WriteHalf(-value.y);
                            WriteHalf(-value.z);
                        }
                        break;
                }
            }

            public Vector3 ReadHalfVector3()
            {
                return new Vector3(ReadHalf(), ReadHalf(), ReadHalf());
            }

            // TODO:: This is using a byte + 3 16-Float, but I should use 3 bits + 3 15-Float
            public Quaternion ReadHalfQuaternion()
            {
                byte i = ReadByte();
                float x = 0, y = 0, z = 0, w = 0;
                switch (i)
                {
                    case 0:
                        y = ReadHalf();
                        z = ReadHalf();
                        w = ReadHalf();
                        x = Mathf.Sqrt(1f - y * y - z * z - w * w);
                        break;
                    case 1:
                        x = ReadHalf();
                        z = ReadHalf();
                        w = ReadHalf();
                        y = Mathf.Sqrt(1f - x * x - z * z - w * w);
                        break;
                    case 2:
                        x = ReadHalf();
                        y = ReadHalf();
                        w = ReadHalf();
                        z = Mathf.Sqrt(1f - x * x - y * y - w * w);
                        break;
                    case 3:
                        x = ReadHalf();
                        y = ReadHalf();
                        z = ReadHalf();
                        w = Mathf.Sqrt(1f - x * x - y * y - z * z);
                        break;
                }
                return new Quaternion(x, y, z, w);
            }
        }
    }
}
