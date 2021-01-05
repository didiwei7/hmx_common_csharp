using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

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
            public string Address { get; set; } = "";
            public EValueType ValueType = EValueType._BOOL;
            public EFinsOperate Operator = EFinsOperate.Read;
            public byte Area = Convert.ToByte(0);
            public byte HighByte = Convert.ToByte(0);
            public byte LowByte = Convert.ToByte(0);
            public byte Decimal = Convert.ToByte(0);
            public UInt16 Count { get; set; } = 1;
        }
        public static class FinsConvert
        {
            public static byte LowByte(Int16 value) { return Convert.ToByte(value & 0xFF); }
            public static byte LowByte(UInt16 value) { return Convert.ToByte(value & 0xFF); }
            public static byte HighByte(Int16 value) { return Convert.ToByte((value >> 8) & 0xFF); }
            public static byte HighByte(UInt16 value) { return Convert.ToByte((value >> 8) & 0xFF); }
            public static bool AddrArea(string value, ref byte area)
            {
                if (value == "c" || value == "C" || value == "CIO")
                    area = 0xB0;
                else if (value == "w" || value == "W")
                    area = 0xB1;
                else if (value == "h" || value == "H")
                    area = 0xB2;
                else if (value == "d" || value == "D" || value == "DM")
                    area = 0x82;
                else return false;

                return true;
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
                else if (typeof(float).Name == type)
                    valueType = EValueType._FLOAT;
                else if (typeof(double).Name == type)
                    valueType = EValueType._DOUBLE;
                else if (typeof(string).Name == type)
                    valueType = EValueType._STRING;
                else return false;

                return true;
            }
            public static bool AddrNum(string value, ref byte high, ref byte low, ref byte decima)
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
            public static byte[] ArrayByte(Int16 value) { return new byte[] { Convert.ToByte((value >> 8) & 0xFF), Convert.ToByte(value & 0xFF) }; }
            public static byte[] ArrayByte(UInt16 value) { return new byte[] { Convert.ToByte((value >> 8) & 0xFF), Convert.ToByte(value & 0xFF) }; }
            public static byte[] ArrayByte(Int32 value) { return new byte[] { Convert.ToByte((value >> 24) & 0xFF), Convert.ToByte((value >> 16) & 0xFF), Convert.ToByte((value >> 8) & 0xFF), Convert.ToByte(value & 0xFF) }; }
            public static byte[] ArrayByte(UInt32 value) { return new byte[] { Convert.ToByte((value >> 24) & 0xFF), Convert.ToByte((value >> 16) & 0xFF), Convert.ToByte((value >> 8) & 0xFF), Convert.ToByte(value & 0xFF) }; }
            public static byte[] ArrayByte(Int64 value) { return new byte[] 
            { 
                Convert.ToByte((value >> 56) & 0xFF), Convert.ToByte((value >> 48) & 0xFF), Convert.ToByte((value >> 40) & 0xFF), Convert.ToByte((value >> 32) & 0xFF),
                Convert.ToByte((value >> 24) & 0xFF), Convert.ToByte((value >> 16) & 0xFF), Convert.ToByte((value >> 8) & 0xFF), Convert.ToByte(value & 0xFF) }; 
            }
            public static byte[] ArrayByte(UInt64 value)
            {
                return new byte[]
{
                Convert.ToByte((value >> 56) & 0xFF), Convert.ToByte((value >> 48) & 0xFF), Convert.ToByte((value >> 40) & 0xFF), Convert.ToByte((value >> 32) & 0xFF),
                Convert.ToByte((value >> 24) & 0xFF), Convert.ToByte((value >> 16) & 0xFF), Convert.ToByte((value >> 8) & 0xFF), Convert.ToByte(value & 0xFF) };
            }
        }

        public string IP { get; set; } = "127.0.0.1";
        public UInt16 Port { get; set; } = 9600;
        public UInt16 ConnTimeout { get; set; } = 300;
        public bool IsConnect { get; set; } = false;
        
        private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { SendTimeout = 300, ReceiveTimeout = 300 };
        private List<Byte> header = new List<byte>();
        PLCFins()
        {

        }

        public bool Connect() { return Connect(IP, Port); }
        public bool Connect(string ip, UInt16 port)
        {
            try { IPAddress.Parse(ip); }
            catch { return false; }

            var task = socket.ConnectAsync(IPAddress.Parse(ip), port);
            IsConnect = task.Wait(ConnTimeout);

            return IsConnect;
        }

        public void DisConnect() { }
        public bool ReConnect()
        {
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

            if (0 >= socket.Send(sendmsg.ToArray()))
                return false;

            byte[] recvmsg = new byte[] { };
            if (socket.Receive(recvmsg) != 24 ||
                recvmsg[12] != 0x00 ||
                recvmsg[13] != 0x00 ||
                recvmsg[14] != 0x00 ||
                recvmsg[15] != 0x00)
            {
                Console.WriteLine("返回信息错误");
                return false;
            }

            FinsHeaderFrame headerFrame = new FinsHeaderFrame { DA1 = recvmsg[23], SA1 = recvmsg[19] };

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
            header.Add(headerFrame.SID);

            return true;
        }
        // 这个正则不熟，要测试
        private bool ParseAddrInfo(string addr, UInt16 count, string valueType, EFinsOperate opt, ref FinsAddrInfo addrInfo)
        {
            addrInfo.Address = addr;
            addrInfo.Operator = opt;
            addrInfo.Count = count;

            var matchs = Regex.Matches(addr, @"a-zA-Z");
            if (matchs.Count != 1)
                return false;

            var sarea = matchs[0].Value;
            var snum = ""; // 未match到的
            if (!FinsConvert.AddrArea(sarea, ref addrInfo.Area))
                return false;

            if (!FinsConvert.AddrValueType(valueType, ref addrInfo.ValueType))
                return false;

            if (!FinsConvert.AddrNum(snum, ref addrInfo.HighByte, ref addrInfo.LowByte, ref addrInfo.Decimal))
                return false;

            return true;
        }
        private bool ParseReadSend(FinsAddrInfo addrInfo, ref List<byte> sendmsg)
        {
            byte[] command = new byte[8];

            // 指令 读
            command[0] = FinsConvert.HighByte(Convert.ToUInt16(addrInfo.Operator));
            command[1] = FinsConvert.LowByte(Convert.ToUInt16(addrInfo.Operator));
            // 地址
            command[2] = addrInfo.Area;                     // 区域
            command[3] = addrInfo.HighByte;                 // 地址-整数位
            command[4] = addrInfo.LowByte;
            command[5] = addrInfo.Decimal;                  // 地址-小数位
            // 长度
            command[6] = FinsConvert.HighByte(Convert.ToUInt16(addrInfo.Count));
            command[7] = FinsConvert.LowByte(Convert.ToUInt16(addrInfo.Count));

            var length = -8 + header.Count + 8;
            sendmsg.AddRange(header);
            sendmsg.RemoveRange(4, 4);                           // 数据段长度设置
            sendmsg.InsertRange(4, FinsConvert.ArrayByte((UInt32)length));
            sendmsg.AddRange(command);                           // 添加command

            return true;
        }
        private bool ParseReadRecv<T>(FinsAddrInfo addrInfo, byte[] data, ref List<T> result)
        {
            int length = data.Count();

            switch (addrInfo.ValueType)
            {
                case EValueType._UNKNOW:
                    return false;

                case EValueType._BOOL:
                    if (length != addrInfo.Count)
                        return false;
                    for (int i = 0; i < length; ++i)
                        result.Add((T)(Object)BitConverter.ToBoolean(data.Skip(i * 1).Take(1).Reverse().ToArray(), 0));
                    break;

                case EValueType._INT16:
                    if (length != addrInfo.Count * 2)
                        return false;
                    for (int i = 0; i < length / 2; ++i)
                        result.Add((T)(Object)BitConverter.ToInt16(data.Skip(i * 2).Take(2).Reverse().ToArray(), 0));
                    break;

                case EValueType._INT32:
                    if (length != addrInfo.Count * 4)
                        return false;
                    for (int i = 0; i < length / 4; ++i)
                        result.Add((T)(Object)BitConverter.ToInt32(data.Skip(i * 4).Take(4).Reverse().ToArray(), 0));
                    break;

                case EValueType._INT64:
                    if (length != addrInfo.Count * 8)
                        return false;
                    for (int i = 0; i < length / 8; ++i)
                        result.Add((T)(Object)BitConverter.ToInt64(data.Skip(i * 8).Take(8).Reverse().ToArray(), 0));
                    break;

                case EValueType._UINT16:
                    if (length != addrInfo.Count * 2)
                        return false;
                    for (int i = 0; i < length / 2; ++i)
                        result.Add((T)(Object)BitConverter.ToUInt16(data.Skip(i * 2).Take(2).Reverse().ToArray(), 0));
                    break;

                case EValueType._UINT32:
                    if (length != addrInfo.Count * 4)
                        return false;
                    for (int i = 0; i < length / 4; ++i)
                        result.Add((T)(Object)BitConverter.ToUInt32(data.Skip(i * 4).Take(4).Reverse().ToArray(), 0));
                    break;

                case EValueType._UINT64:
                    if (length != addrInfo.Count * 8)
                        return false;
                    for (int i = 0; i < length / 8; ++i)
                        result.Add((T)(Object)BitConverter.ToUInt64(data.Skip(i * 8).Take(8).Reverse().ToArray(), 0));
                    break;

                case EValueType._FLOAT:
                    if (length != addrInfo.Count * 4)
                        return false;
                    for (int i = 0; i < length / 4; ++i)
                        result.Add((T)(Object)BitConverter.ToString(data.Skip(i * 4).Take(4).Reverse().ToArray(), 0));
                    break;

                case EValueType._DOUBLE:
                    if (length != addrInfo.Count * 8)
                        return false;
                    for (int i = 0; i < length / 8; ++i)
                        result.Add((T)(Object)BitConverter.ToDouble(data.Skip(i * 8).Take(8).Reverse().ToArray(), 0));
                    break;

                case EValueType._STRING:
                    if (length != addrInfo.Count)
                        return false;
                    for (int i = 0; i < length; ++i)
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

            if (count <= 0 || count > 2000)
                return false;

            FinsAddrInfo addrInfo = new FinsAddrInfo();
            if (!ParseAddrInfo(addr, count, typeof(T).Name, EFinsOperate.Read, ref addrInfo))
                return false;

            List<byte> sendmsg = new List<byte>();
            if (!ParseReadSend(addrInfo, ref sendmsg))
                return false;

            if (sendmsg.Count() != socket.Send(sendmsg.ToArray()))
                return false;

            // 这里应该用错了
            List<byte> recvmsg = new List<byte>();
            if (socket.Receive(recvmsg.ToArray()) <= 30)
                return false;

            if (!ParseReadRecv(addrInfo, recvmsg.Skip(30).ToArray(), ref rst))
                return false;

            return true;
        }
        private bool ParseWriteSend<T>(FinsAddrInfo addrInfo, List<T> values, ref List<byte> sendmsg)
        {
            List<byte> command = new List<byte>(8);

            // 指令 读
            command[0] = FinsConvert.HighByte(Convert.ToUInt16(addrInfo.Operator));
            command[1] = FinsConvert.LowByte(Convert.ToUInt16(addrInfo.Operator));
            // 地址
            command[2] = addrInfo.Area;                     // 区域
            command[3] = addrInfo.HighByte;                 // 地址-整数位
            command[4] = addrInfo.LowByte;
            command[5] = addrInfo.Decimal;                  // 地址-小数位
            // 长度
            command[6] = FinsConvert.HighByte(Convert.ToUInt16(addrInfo.Count));
            command[7] = FinsConvert.LowByte(Convert.ToUInt16(addrInfo.Count));

            // 数据
            EValueType valueType = EValueType._BOOL;
            if (!FinsConvert.AddrValueType(typeof(T).Name, ref valueType))
                return false;

            List<byte> data = new List<byte>();
            switch (valueType)
            {
                case EValueType._UNKNOW:
                    return false;

                case EValueType._BOOL:
                    foreach (var i in values)
                        data.AddRange(BitConverter.GetBytes((Byte)(Object)i).Reverse());
                    break;

                case EValueType._INT16:
                    foreach (var i in values)
                        data.AddRange(BitConverter.GetBytes((Int16)(Object)i).Reverse());
                    break;

                case EValueType._INT32:
                    foreach (var i in values)
                        data.AddRange(BitConverter.GetBytes((Int32)(Object)i).Reverse());
                    break;

                case EValueType._INT64:
                    foreach (var i in values)
                        data.AddRange(BitConverter.GetBytes((Int64)(Object)i).Reverse());
                    break;

                case EValueType._UINT16:
                    foreach (var i in values)
                        data.AddRange(BitConverter.GetBytes((UInt16)(Object)i).Reverse());
                    break;

                case EValueType._UINT32:
                    foreach (var i in values)
                        data.AddRange(BitConverter.GetBytes((UInt32)(Object)i).Reverse());
                    break;

                case EValueType._UINT64:
                    foreach (var i in values)
                        data.AddRange(BitConverter.GetBytes((UInt64)(Object)i).Reverse());
                    break;

                case EValueType._FLOAT:
                    foreach (var i in values)
                        data.AddRange(BitConverter.GetBytes((Single)(Object)i).Reverse());
                    break;

                case EValueType._DOUBLE:
                    foreach (var i in values)
                        data.AddRange(BitConverter.GetBytes((Double)(Object)i).Reverse());
                    break;
            }

            command.AddRange(data);

            var length = -8 + header.Count + command.Count + data.Count;

            var length_read = data.Count;
            sendmsg.AddRange(header);
            sendmsg.RemoveRange(4, 4);                           // 数据段长度设置
            sendmsg.InsertRange(4, FinsConvert.ArrayByte((UInt32)length));
            sendmsg.AddRange(command);                           // 添加command
            sendmsg.AddRange(data);                              // 添加data

            return true;
        }
        private bool ParseWriteRecv(List<byte> recvmsg) { return recvmsg.Last() == 0; }
        private bool Write<T>(string addr, UInt16 count, List<T> values)
        {
            if (!IsConnect)
                return false;

            if (count <= 0 || count > 2000)
                return false;

            FinsAddrInfo addrInfo = new FinsAddrInfo();
            if (!ParseAddrInfo(addr, count, typeof(T).Name, EFinsOperate.Read, ref addrInfo))
                return false;

            List<byte> sendmsg = new List<byte>();
            if (!ParseWriteSend(addrInfo, values, ref sendmsg))
                return false;

            if (sendmsg.Count() != socket.Send(sendmsg.ToArray()))
                return false;

            List<byte> recvmsg = new List<byte>();
            if (socket.Receive(recvmsg.ToArray()) <= 30)
                return false;

            return ParseWriteRecv(recvmsg);
        }


        public int ReadInt16(string addr)
        {
            List<Int16> rst = new List<Int16>();
            if (!Read<Int16>(addr, 1, ref rst))
            {
                Console.WriteLine("ReadInt16 ERR");
                return 0;
            }

            return rst.First();
        }
        // 不知道怎么List<short> -> List<int>
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
                Console.WriteLine("ReadInt16s ERR");
            return Encoding.ASCII.GetString(rst.ToArray());
        }
        public bool WriteInt16(string addr, Int16 value) { return Write<Int16>(addr, 1, new List<Int16>() { value }); }
        public bool WriteInt16s(string addr, List<Int16> value) { return Write<Int16>(addr, 1, value); }
    }
}
