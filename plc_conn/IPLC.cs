using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace HMX_PLC
{
    public enum EValueType
    {
        _UNKNOW,
        _BOOL,
        _INT16,
        _INT32,
        _INT64,
        _UINT16,
        _UINT32,
        _UINT64,
        _FLOAT,
        _DOUBLE,
        _STRING
    }


    public static class PLCConvert
    {
        public static byte LowByte(Int16 value) { return Convert.ToByte(value & 0xFF); }
        public static byte LowByte(UInt16 value) { return Convert.ToByte(value & 0xFF); }
        public static byte HighByte(Int16 value) { return Convert.ToByte((value >> 8) & 0xFF); }
        public static byte HighByte(UInt16 value)
        {
            byte ff = Convert.ToByte((value >> 8) & 0xFF);
            return ff;
        }
        public static byte[] ArrayByte(Int16 value) { return new byte[] { Convert.ToByte((value >> 8) & 0xFF), Convert.ToByte(value & 0xFF) }; }
        public static byte[] ArrayByte(UInt16 value) { return new byte[] { Convert.ToByte((value >> 8) & 0xFF), Convert.ToByte(value & 0xFF) }; }
        public static byte[] ArrayByte(Int32 value) { return new byte[] { Convert.ToByte((value >> 24) & 0xFF), Convert.ToByte((value >> 16) & 0xFF), Convert.ToByte((value >> 8) & 0xFF), Convert.ToByte(value & 0xFF) }; }
        public static byte[] ArrayByte(UInt32 value) { return new byte[] { Convert.ToByte((value >> 24) & 0xFF), Convert.ToByte((value >> 16) & 0xFF), Convert.ToByte((value >> 8) & 0xFF), Convert.ToByte(value & 0xFF) }; }
        public static byte[] ArrayByte(Int64 value)
        {
            return new byte[] { Convert.ToByte((value >> 56) & 0xFF), Convert.ToByte((value >> 48) & 0xFF), Convert.ToByte((value >> 40) & 0xFF), Convert.ToByte((value >> 32) & 0xFF),
                                Convert.ToByte((value >> 24) & 0xFF), Convert.ToByte((value >> 16) & 0xFF), Convert.ToByte((value >> 8) & 0xFF), Convert.ToByte(value & 0xFF) };
        }
        public static byte[] ArrayByte(UInt64 value)
        {
            return new byte[] { Convert.ToByte((value >> 56) & 0xFF), Convert.ToByte((value >> 48) & 0xFF), Convert.ToByte((value >> 40) & 0xFF), Convert.ToByte((value >> 32) & 0xFF),
                                Convert.ToByte((value >> 24) & 0xFF), Convert.ToByte((value >> 16) & 0xFF), Convert.ToByte((value >> 8) & 0xFF), Convert.ToByte(value & 0xFF) };
        }


        public static bool AddrValueType(string type, ref EValueType valueType)
        {
            if (typeof(bool).Name == type)
                valueType = EValueType._BOOL;
            else if (typeof(Int16).Name == type)
                valueType = EValueType._INT16;
            else if (typeof(Int32).Name == type)
                valueType = EValueType._INT32;
            else if (typeof(Int64).Name == type)
                valueType = EValueType._INT64;
            else if (typeof(UInt16).Name == type)
                valueType = EValueType._UINT16;
            else if (typeof(UInt32).Name == type)
                valueType = EValueType._UINT32;
            else if (typeof(Single).Name == type)
                valueType = EValueType._FLOAT;
            else if (typeof(Double).Name == type)
                valueType = EValueType._DOUBLE;
            else if (typeof(Byte).Name == type)
                valueType = EValueType._STRING;
            else if (typeof(String).Name == type)
                valueType = EValueType._STRING;
            else return false;

            return true;
        }

        public enum EFinsMode { Single, Double }
        public static bool FinsAddrArea(string value, EFinsMode mode, ref byte area)
        {
            if (value == "c" || value == "C" || value == "CIO")
                if (mode == EFinsMode.Double)
                    area = 0xB0;
                else
                    area = 0x30;
            else if (value == "w" || value == "W")
                if (mode == EFinsMode.Double)
                    area = 0xB1;
                else
                    area = 0x31;
            else if (value == "h" || value == "H")
                if (mode == EFinsMode.Double)
                    area = 0xB2;
                else
                    area = 0x32;
            else if (value == "d" || value == "D" || value == "DM")
                if (mode == EFinsMode.Double)
                    area = 0x82;
                else
                    area = 0x02;
            else
                return false;

            return true;
        }
        public static bool FinsAddrNum(string value, ref byte high, ref byte low, ref byte decima)
        {
            var result = value.Split('.');

            if (result.Count() == 1)
            {
                var num = Convert.ToUInt16(result[0]);
                high = Convert.ToByte((num >> 8) & 0xFF);
                low = Convert.ToByte(num & 0xFF);
                decima = 0x00;
            }
            else if (result.Count() == 2)
            {
                var num = Convert.ToUInt16(result[0]);
                high = Convert.ToByte((num >> 8) & 0xFF);
                low = Convert.ToByte(num & 0xFF);
                decima = Convert.ToByte(result[1]);
            }
            else
                return false;

            return true;
        }

    }

    public class PLCConn
    {
    }

    public class PLCHc : PLCConn
    {
        public enum EHcOperate { Read, Write }
        public class HcAddrInfo { }
        public static class HcConvert { }
    }

    public class PLCFins : PLCConn
    {
        private Mutex mutex = new Mutex();
        public string IP { get; set; } = "127.0.0.1";// "192.168.1.10";
        public UInt16 Port { get; set; } = 9600;
        public UInt16 ConnTimeout { get; set; } = 300;
        public bool IsConnect { get; set; } = false;
        public enum EFinsOperate { Read = 0x0101, Write = 0x0102 }

        public class FinsHeaderTcp
        {
            public byte[] header = new byte[] { 0x46, 0x49, 0x4E, 0x53 };
            public byte[] length = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            public byte[] command = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            public byte[] errorCode = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            public byte[] clientNode = new byte[] { 0x00, 0x00, 0x00, 0x00 };
        };
        public class FinsHeaderFrame
        {
            public byte ICF { get; set; } = 0b10000000;
            public byte RSV { get; set; } = 0x00;
            public byte GCT { get; set; } = 0x02;
            public byte DNA { get; set; } = 0x00;
            // 握手后设置
            public byte DA1 { get; set; } = 0x00;
            public byte DA2 { get; set; } = 0x00;
            public byte SNA { get; set; } = 0x00;
            // 握手后设置
            public byte SA1 { get; set; } = 0x00;
            public byte SA2 { get; set; } = 0x00;
            public byte SID { get; set; } = 0x00;
        };
        public class FinsAddrInfo
        {
            public string Address = "";
            public EValueType ValueType = EValueType._BOOL;

            public EFinsOperate Operator = EFinsOperate.Read;
            public byte Area = Convert.ToByte(0);
            public byte HighByte = Convert.ToByte(0);
            public byte LowByte = Convert.ToByte(0);
            public byte Decimal = Convert.ToByte(0);
            public UInt16 Count = 1;

            public UInt16 FinsCount = 1;
        }


        private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { SendTimeout = 300, ReceiveTimeout = 1000 };
        private List<byte> header = new List<byte>();
        public PLCFins()
        {

        }

        public bool Connect()
        {
            if (!Connect(IP, Port))
                return false;

            if (!ShakeHand())
            {
                DisConnect();
                return false;
            }

            return true;
        }
        public bool Connect(string ip, UInt16 port)
        {
            try { IPAddress.Parse(ip); }
            catch { return false; }

            socket.Connect(IPAddress.Parse(ip), port);
            // task.Wait(ConnTimeout);
            IsConnect = socket.Connected;

            return IsConnect;
        }

        public void DisConnect()
        {
            IsConnect = false;
            socket.Disconnect(true);
        }
        public bool ReConnect()
        {
            return true;
        }

        private bool SocketSend(byte[] array, ref int recvlen, ref byte[] recvmsg)
        {
            mutex.WaitOne();
            try
            {
                if (array.Length != socket.Send(array))
                    return false;

                recvlen = socket.Receive(recvmsg);
            }
            catch
            {
            }

            mutex.ReleaseMutex();
            return true;

        }
        private bool ShakeHand()
        {
            // lock

            if (!IsConnect)
                return false;

            FinsHeaderTcp headerTcp = new FinsHeaderTcp { length = new byte[] { 0x00, 0x00, 0x00, 0x0C } };

            List<byte> sendmsg = new List<byte>();
            sendmsg.AddRange(headerTcp.header);
            sendmsg.AddRange(headerTcp.length);
            sendmsg.AddRange(headerTcp.command);
            sendmsg.AddRange(headerTcp.errorCode);
            sendmsg.AddRange(headerTcp.clientNode);

            string msg = BitConverter.ToString(sendmsg.ToArray());
            byte[] recvmsg = new byte[2077];
            int len = 0;
            if (!SocketSend(sendmsg.ToArray(), ref len, ref recvmsg))
                return false;

            if (len != 24 ||
                recvmsg[12] != 0x00 ||
                recvmsg[13] != 0x00 ||
                recvmsg[14] != 0x00 ||
                recvmsg[15] != 0x00)
            {
                Console.WriteLine("返回信息错误");
                return false;
            }

            FinsHeaderFrame headerFrame = new FinsHeaderFrame { DA1 = recvmsg[23], SA1 = recvmsg[19] };

            headerTcp.command = new byte[] { 0x00, 0x00, 0x00, 0x02 };

            header.Clear();
            header.AddRange(headerTcp.header);
            header.AddRange(headerTcp.length);
            header.AddRange(headerTcp.command);
            header.AddRange(headerTcp.errorCode);
            header.Add(headerFrame.ICF);
            header.Add(headerFrame.RSV);
            header.Add(headerFrame.GCT);
            header.Add(headerFrame.DNA);
            header.Add(headerFrame.DA1);
            header.Add(headerFrame.DA2);
            header.Add(headerFrame.SNA);
            header.Add(headerFrame.SA1);
            header.Add(headerFrame.SA2);
            header.Add(headerFrame.SID);

            return true;
        }

        private bool ParseAddrInfo(string addr, UInt16 count, string valueType, EFinsOperate opt, ref FinsAddrInfo addrInfo)
        {
            addrInfo.Address = addr;
            addrInfo.Operator = opt;
            addrInfo.Count = count;

            if (!PLCConvert.AddrValueType(valueType, ref addrInfo.ValueType))
                return false;

            var matchArea = Regex.Matches(addr, @"[A-Za-z]+");
            if (matchArea.Count != 1)
                return false;
            var sarea = matchArea[0].Value;

            var matchNum = Regex.Matches(addr, @"([1-9]\d*\.?\d*)|(0\.\d*[1-9])");
            if (matchNum.Count != 1)
                return false;
            var snum = matchNum[0].Value;

            if (addrInfo.ValueType == EValueType._BOOL && addrInfo.Operator == EFinsOperate.Write)
            {
                if (!PLCConvert.FinsAddrArea(sarea, PLCConvert.EFinsMode.Single, ref addrInfo.Area))
                    return false;
            }
            else
            {
                if (!PLCConvert.FinsAddrArea(sarea, PLCConvert.EFinsMode.Double, ref addrInfo.Area))
                    return false;
            }


            if (!PLCConvert.FinsAddrNum(snum, ref addrInfo.HighByte, ref addrInfo.LowByte, ref addrInfo.Decimal))
                return false;

            switch (addrInfo.ValueType)
            {
                case EValueType._UNKNOW:
                    return false;

                case EValueType._BOOL:
                    addrInfo.FinsCount = (UInt16)((addrInfo.Decimal + addrInfo.Count) / 16 + 1);
                    break;

                case EValueType._STRING:

                case EValueType._INT16:
                    addrInfo.FinsCount = addrInfo.Count;
                    break;

                case EValueType._FLOAT:

                case EValueType._INT32:
                    addrInfo.FinsCount = (UInt16)(addrInfo.Count * 2);
                    break;

                case EValueType._DOUBLE:

                case EValueType._INT64:
                    addrInfo.FinsCount = (UInt16)(addrInfo.Count * 4);
                    break;

                default:
                    break;

            }

            return true;
        }
        private bool ParseReadSend(FinsAddrInfo addrInfo, ref List<byte> sendmsg)
        {
            byte[] command = new byte[8];

            // 指令 读
            command[0] = PLCConvert.HighByte(Convert.ToUInt16(addrInfo.Operator));
            command[1] = PLCConvert.LowByte(Convert.ToUInt16(addrInfo.Operator));
            // 地址
            command[2] = addrInfo.Area;                     // 区域
            command[3] = addrInfo.HighByte;                 // 地址-整数位
            command[4] = addrInfo.LowByte;
            command[5] = addrInfo.Decimal;                  // 地址-小数位

            // 长度
            command[6] = PLCConvert.HighByte(Convert.ToUInt16(addrInfo.FinsCount));
            command[7] = PLCConvert.LowByte(Convert.ToUInt16(addrInfo.FinsCount));

            var length = -8 + header.Count + 8;
            sendmsg.AddRange(header);
            sendmsg.RemoveRange(4, 4);                           // 数据段长度设置
            sendmsg.InsertRange(4, PLCConvert.ArrayByte((UInt32)length));
            sendmsg.AddRange(command);                           // 添加command

            return true;
        }
        private byte[] ByteArrayFormat(byte[] array)
        {
            for (int i = 0; 2 * i + 1 < array.Length; i++)
            {
                byte temp = array[2 * i];
                array[2 * i] = array[2 * i + 1];
                array[2 * i + 1] = temp;
            }
            return array;
        }
        private bool ParseReadRecv<T>(FinsAddrInfo addrInfo, byte[] data, ref List<T> result)
        {
            int length = data.Count();

            switch (addrInfo.ValueType)
            {
                case EValueType._UNKNOW:
                    return false;

                case EValueType._BOOL:
                    {
                        data = ByteArrayFormat(data);
                        for (int i = 0; i < length / 2; i++)
                        {
                            BitArray bits = new BitArray(data.Skip(i * 2).Take(2).ToArray());

                            if (i == 0)
                            {
                                for (int j = addrInfo.Decimal; j < Math.Min(16, addrInfo.Decimal + addrInfo.Count); ++j)
                                    result.Add((T)(Object)bits.Get(j));
                            }
                            else if (i == length / 2 - 1)
                            {
                                for (int j = 0; j < (addrInfo.Decimal + addrInfo.Count) % 16; ++j)
                                    result.Add((T)(Object)bits.Get(j));
                            }
                            else
                            {
                                for (int j = 0; j < 16; ++j)
                                    result.Add((T)(Object)bits.Get(j));
                            }

                        }

                        if (result.Count != addrInfo.Count)
                            return false;

                    }
                    break;

                case EValueType._INT16:
                    data = ByteArrayFormat(data);
                    for (int i = 0; i < length / 2; i++)
                        result.Add((T)(Object)BitConverter.ToInt16(data, 2 * i));
                    break;

                case EValueType._INT32:
                    data = ByteArrayFormat(data);
                    for (int i = 0; i < length / 4; i++)
                        result.Add((T)(Object)BitConverter.ToInt32(data, 4 * i));
                    break;

                case EValueType._INT64:
                    data = ByteArrayFormat(data);
                    for (int i = 0; i < length / 8; i++)
                        result.Add((T)(Object)BitConverter.ToInt64(data, 8 * i));
                    break;

                case EValueType._UINT16:
                    data = ByteArrayFormat(data);
                    for (int i = 0; i < length / 2; i++)
                        result.Add((T)(Object)BitConverter.ToInt16(data, 2 * i));
                    break;

                case EValueType._UINT32:
                    data = ByteArrayFormat(data);
                    for (int i = 0; i < length / 4; i++)
                        result.Add((T)(Object)BitConverter.ToUInt32(data, 4 * i));
                    break;

                case EValueType._UINT64:
                    data = ByteArrayFormat(data);
                    for (int i = 0; i < length / 8; i++)
                        result.Add((T)(Object)BitConverter.ToUInt64(data, 8 * i));
                    break;

                case EValueType._FLOAT:
                    data = ByteArrayFormat(data);
                    for (int i = 0; i < length / 4; i++)
                        result.Add((T)(Object)BitConverter.ToSingle(data, 4 * i));
                    break;

                case EValueType._DOUBLE:
                    data = ByteArrayFormat(data);
                    for (int i = 0; i < length / 8; i++)
                        result.Add((T)(Object)BitConverter.ToDouble(data, 8 * i));
                    break;

                case EValueType._STRING:
                    for (int i = 0; i < length; i++)
                        result.Add((T)(Object)data[i]);
                    break;

                default:
                    break;
            }

            return true;
        }
        private bool Read<T>(string addr, UInt16 count, ref List<T> rst)
        {
            if (!IsConnect)
                return false;

            if (count <= 0 || count > 1000)
                return false;

            FinsAddrInfo addrInfo = new FinsAddrInfo();
            if (!ParseAddrInfo(addr, count, typeof(T).Name, EFinsOperate.Read, ref addrInfo))
                return false;

            List<byte> sendmsg = new List<byte>();
            if (!ParseReadSend(addrInfo, ref sendmsg))
                return false;

            byte[] recvmsg = new byte[2077];
            int recvlen = 0;
            if (!SocketSend(sendmsg.ToArray(), ref recvlen, ref recvmsg))
                return false;

            if (recvlen != 30 + addrInfo.FinsCount * 2)
                return false;

            if (!ParseReadRecv(addrInfo, recvmsg.Skip(30).Take(recvlen - 30).ToArray(), ref rst))
                return false;

            return true;
        }
        private bool ParseWriteSend<T>(FinsAddrInfo addrInfo, List<T> values, ref List<byte> sendmsg)
        {
            List<byte> command = new List<byte>();

            // 指令 读
            command.Add(PLCConvert.HighByte(Convert.ToUInt16(addrInfo.Operator)));
            command.Add(PLCConvert.LowByte(Convert.ToUInt16(addrInfo.Operator)));
            // 地址
            command.Add(addrInfo.Area);                     // 区域
            command.Add(addrInfo.HighByte);                 // 地址-整数位
            command.Add(addrInfo.LowByte);
            command.Add(addrInfo.Decimal);                  // 地址-小数位
            // 长度
            command.Add(PLCConvert.HighByte(Convert.ToUInt16(addrInfo.Count)));
            command.Add(PLCConvert.LowByte(Convert.ToUInt16(addrInfo.Count)));

            // 数据
            EValueType valueType = EValueType._BOOL;
            if (!PLCConvert.AddrValueType(typeof(T).Name, ref valueType))
                return false;

            List<byte> data = new List<byte>();
            switch (valueType)
            {
                case EValueType._UNKNOW:
                    return false;

                case EValueType._BOOL:
                    foreach (var i in values)
                        data.AddRange(ByteArrayFormat(BitConverter.GetBytes((bool)(Object)i)));
                    break;

                case EValueType._INT16:
                    foreach (var i in values)
                        data.AddRange(ByteArrayFormat(BitConverter.GetBytes((Int16)(Object)i)));
                    break;

                case EValueType._INT32:
                    foreach (var i in values)
                        data.AddRange(ByteArrayFormat(BitConverter.GetBytes((Int32)(Object)i)));
                    break;

                case EValueType._INT64:
                    foreach (var i in values)
                        data.AddRange(ByteArrayFormat(BitConverter.GetBytes((Int64)(Object)i)));
                    break;

                case EValueType._UINT16:
                    foreach (var i in values)
                        data.AddRange(ByteArrayFormat(BitConverter.GetBytes((UInt16)(Object)i)));
                    break;

                case EValueType._UINT32:
                    foreach (var i in values)
                        data.AddRange(ByteArrayFormat(BitConverter.GetBytes((UInt32)(Object)i)));
                    break;

                case EValueType._UINT64:
                    foreach (var i in values)
                        data.AddRange(ByteArrayFormat(BitConverter.GetBytes((UInt64)(Object)i)));
                    break;

                case EValueType._FLOAT:
                    foreach (var i in values)
                        data.AddRange(ByteArrayFormat(BitConverter.GetBytes((Single)(Object)i)));
                    break;

                case EValueType._DOUBLE:
                    foreach (var i in values)
                        data.AddRange(ByteArrayFormat(BitConverter.GetBytes((Double)(Object)i)));
                    break;

                case EValueType._STRING:
                    foreach (var i in values)
                        data.AddRange((System.Text.Encoding.UTF8.GetBytes(Convert.ToString(i) + '\0')));
                    break;
            }


            var length = -8 + header.Count + command.Count + data.Count;

            var length_read = data.Count;
            sendmsg.AddRange(header);
            sendmsg.RemoveRange(4, 4);                           // 数据段长度设置
            sendmsg.InsertRange(4, PLCConvert.ArrayByte((UInt32)length));
            sendmsg.AddRange(command);                           // 添加command
            sendmsg.AddRange(data);                              // 添加data

            return true;
        }
        private bool ParseWriteRecv(List<byte> recvmsg) { return recvmsg.Last() == 0; }
        private bool Write<T>(string addr, UInt16 count, List<T> values, EFinsOperate eFinsOperate = EFinsOperate.Read)
        {
            if (!IsConnect)
                return false;

            if (count <= 0 || count > 2000)
                return false;

            FinsAddrInfo addrInfo = new FinsAddrInfo();
            if (!ParseAddrInfo(addr, count, typeof(T).Name, eFinsOperate, ref addrInfo))
                return false;

            List<byte> sendmsg = new List<byte>();
            if (!ParseWriteSend(addrInfo, values, ref sendmsg))
                return false;

            byte[] recvmsg = new byte[100];
            int len = 0;
            if (!SocketSend(sendmsg.ToArray(), ref len, ref recvmsg))
                return false;

            if (len < 30)
                return false;

            return ParseWriteRecv(recvmsg.ToList());
        }
        public List<bool> ReadBools(string addr, UInt16 count)
        {
            List<Boolean> rst = new List<Boolean>();
            if (!Read<Boolean>(addr, count, ref rst))
                Console.WriteLine("ReadBools ERR");

            return rst;
        }
        public bool ReadBool(string addr) => ReadBools(addr, 1).First();

        public int ReadInt16(string addr) => ReadInt16s(addr, 1).First();

        public List<Int16> ReadInt16s(string addr, UInt16 count)
        {
            List<Int16> rst = new List<Int16>();
            if (!Read<Int16>(addr, count, ref rst))
                Console.WriteLine("ReadInt16s ERR");
            return rst;
        }

        public string ReadString(string addr, UInt16 count)
        {
            List<Byte> rst = new List<Byte>();
            if (!Read<Byte>(addr, count, ref rst))
                Console.WriteLine("ReadString ERR");
            return Encoding.ASCII.GetString(rst.ToArray());
        }

        public double ReadDouble(string addr) => ReadDoubles(addr, 1).First();

        public List<double> ReadDoubles(string addr, UInt16 count)
        {
            List<double> rst = new List<double>();
            if (!Read<double>(addr, count, ref rst))
                Console.WriteLine("ReadDoubles ERR");
            return rst;
        }

        public Int32 ReadInt32(string addr) => ReadInt32s(addr, 1).First();

        public List<Int32> ReadInt32s(string addr, UInt16 count)
        {
            List<Int32> rst = new List<Int32>();
            if (!Read<Int32>(addr, count, ref rst))
                Console.WriteLine("ReadInt32s ERR");
            return rst;
        }

        public UInt32 ReadUInt32(string addr) => ReadUInt32s(addr, 1).First();

        public List<UInt32> ReadUInt32s(string addr, UInt16 count)
        {
            List<UInt32> rst = new List<UInt32>();
            if (!Read<UInt32>(addr, count, ref rst))
                Console.WriteLine("ReadUInt32s ERR");
            return rst;
        }

        public UInt64 ReadUInt64(string addr) => ReadUInt64s(addr, 1).First();

        public List<UInt64> ReadUInt64s(string addr, UInt16 count)
        {
            List<UInt64> rst = new List<UInt64>();
            if (!Read<UInt64>(addr, count, ref rst))
                Console.WriteLine("ReadUInt64 ERR");
            return rst;
        }

        public Int64 ReadInt64(string addr) => ReadInt64s(addr, 1).First();

        public List<Int64> ReadInt64s(string addr, UInt16 count)
        {
            List<Int64> rst = new List<Int64>();
            if (!Read<Int64>(addr, count, ref rst))
                Console.WriteLine("ReadInt64s ERR");
            return rst;
        }


        public float ReadFloat(string addr) => ReadFloats(addr, 1).First();

        public List<float> ReadFloats(string addr, UInt16 count)
        {
            List<float> rst = new List<float>();
            if (!Read<float>(addr, count, ref rst))
                Console.WriteLine("ReadFloats ERR");
            return rst;

        }
        public bool WriteBool(string addr, bool value) => WriteBools(addr, new List<bool> { value });
        public bool WriteBools(string addr, List<bool> value) { return Write<bool>(addr, 1, value, EFinsOperate.Write); }
        public bool WriteInt16(string addr, Int16 value) => WriteInt16s(addr, new List<Int16> { value });
        public bool WriteInt16s(string addr, List<Int16> value) { return Write<Int16>(addr, 1, value, EFinsOperate.Write); }
        public bool WriteUInt16(string addr, UInt16 value) => WriteUInt16s(addr, new List<UInt16> { value });
        public bool WriteUInt16s(string addr, List<UInt16> value) { return Write<UInt16>(addr, 1, value, EFinsOperate.Write); }


        public bool WriteInt32(string addr, Int32 value) => WriteInt32s(addr, new List<Int32> { value });
        public bool WriteInt32s(string addr, List<Int32> value) { return Write<Int32>(addr, 1, value, EFinsOperate.Write); }
        public bool WriteUInt32(string addr, UInt32 value) => WriteUInt32s(addr, new List<UInt32> { value });
        public bool WriteUInt32s(string addr, List<UInt32> value) { return Write<UInt32>(addr, 1, value, EFinsOperate.Write); }



        public bool WriteInt64(string addr, Int64 value) => WriteInt64s(addr, new List<Int64> { value });
        public bool WriteInt64s(string addr, List<Int64> value) { return Write<Int64>(addr, 1, value, EFinsOperate.Write); }
        public bool WriteUInt64(string addr, UInt64 value) => WriteUInt64s(addr, new List<UInt64> { value });
        public bool WriteUInt64s(string addr, List<UInt64> value) { return Write<UInt64>(addr, 1, value, EFinsOperate.Write); }

        public bool WriteDouble(string addr, double value) => WriteDoubles(addr, new List<double> { value });
        public bool WriteDoubles(string addr, List<double> value) { return Write<double>(addr, 1, value, EFinsOperate.Write); }


        public bool WriteFloat(string addr, float value) => WriteFloats(addr, new List<float> { value });
        public bool WriteFloats(string addr, List<float> value) { return Write<float>(addr, 1, value, EFinsOperate.Write); }


        public bool WriteString(string addr, string value) { return Write<String>(addr, 1, new List<string> { value }, EFinsOperate.Write); }



    }
}
