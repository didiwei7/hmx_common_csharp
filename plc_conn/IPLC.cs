using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HMX_PLC;
using HslCommunication;
using HslCommunication.Profinet.Omron;

namespace HSL
{
    class Program
    {
        List<object> ls = new List<object>();
        static void Main(string[] args)
        {
            Program p = new Program();
            var a = PLCConvert.ArrayByte(50).Reverse().ToArray();
            var b = BitConverter.ToInt32(a, 0);
            //  p.HSLTEST();

            PLCFins fins = new PLCFins();

            var c = fins.Connect();
            var g = fins.ReadBools("D100.01", 16);
            var d = fins.ReadInt16("D100");
            var h = fins.ReadBool("D100");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            #region ceshi
            p.ls.Add(fins.ReadBools("D100", 8));
            p.ls.Add(fins.ReadBool("D100.00"));
            p.ls.Add(fins.ReadBool("D100.01"));
            p.ls.Add(fins.ReadBool("D100.02"));
            p.ls.Add(fins.ReadBool("D100.03"));
            p.ls.Add(fins.ReadBool("D100.04"));
            p.ls.Add(fins.ReadBool("D100.05"));
            p.ls.Add(fins.ReadBool("D100.06"));
            p.ls.Add(fins.ReadBool("D100.07"));

            p.ls.Add(fins.ReadInt16("D100"));

            p.ls.Add(fins.ReadString("D200", 10));

            p.ls.Add(fins.ReadDoubles("D300", 10));
            p.ls.Add(fins.ReadInt32s("D500", 10));
            p.ls.Add(fins.ReadFloats("D700", 10));

            UInt16 fff = Convert.ToUInt16(0x0102);
            int f = UInt16.MaxValue;
            fins.WriteInt16("H100", 123);

            fins.WriteDouble("H200", 123.454566);

            fins.WriteFloat("W200", 132.12f);


            p.ls.Add(fins.ReadDoubles("H200", 8));
            p.ls.Add(fins.ReadFloats("W200", 8));


            fins.WriteString("W500", "Chen123456");
            p.ls.Add(fins.ReadString("W500", 10));

            #endregion

            fins.WriteBools("W200.01", new List<bool> { true, false, true, false, false, true, false, false, false, true, true,false });
            p.ls.Add(fins.ReadBools("W200.01",20));


            stopwatch.Stop();
            Console.WriteLine($"耗时{stopwatch.ElapsedMilliseconds}");
            p.Print();
            Console.ReadKey();

        }
        void Print()
        {
            foreach (var item in ls)
            {
                if (item.GetType() == typeof(List<float>))
                {
                    foreach (var a in (List<float>)item)
                    {
                        Console.Write(a + "\t");
                    }
                    Console.WriteLine();
                    continue;
                }

                if (item.GetType() == typeof(List<double>))
                {
                    foreach (var a in (List<double>)item)
                    {
                        Console.Write(a + "\t");
                    }
                    Console.WriteLine();
                    continue;
                }

                if (item.GetType() == typeof(List<Int32>))
                {
                    foreach (var a in (List<Int32>)item)
                    {
                        Console.Write(a + "\t");
                    }
                    Console.WriteLine();
                    continue;
                }


                if (item.GetType() == typeof(List<bool>))
                {
                    foreach (var a in (List<bool>)item)
                    {
                        Console.Write(a + "\t");
                    }
                    Console.WriteLine();
                }
                else { Console.WriteLine(item); };
            }
        }
        void HSLTEST()
        {
            _omronFinsNet = new OmronFinsNet() { IpAddress = IP, Port = Port, SA1 = SA1, DA1 = DA1, DA2 = DA2, ConnectTimeOut = 1100 };

            _omronFinsNet.SetPersistentConnection();//长连接模式
                                                    // _omronFinsNet.Write()
                                                    //_omronFinsNet.Read("D100", 1);
                                                    //_omronFinsNet.Write("D100", true);
                                                    //_omronFinsNet.Read("D100", 1);




            _omronFinsNet.Write("W200.01", true);
            Stopwatch stopwatch = new Stopwatch();

            //_omronFinsNet.ReadBool("H5");

        }
        public static OmronFinsNet _omronFinsNet;
        private static string IP = "127.0.0.1";// PLC的IP地址
        private static int Port { get; set; } = 9600;// PLC的端口
        public static byte SA1 { get; set; } = 0x00; // PC网络号， PC的IP地址的最后一个数
        public static byte DA1 { get; set; } = 0x00;  // PLC网络号，PLC的IP地址的最后一个数
        public static byte DA2 { get; set; } = 0x00;
    }
}
