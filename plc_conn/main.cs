using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HMX_PLC;

namespace all_console
{
    class Program
    {
        static void Main(string[] args)
        {
            var a = PLCConvert.ArrayByte(50).Reverse().ToArray();
            var b = BitConverter.ToInt32(a, 0);

            PLCFins fins = new PLCFins();

            var c = fins.Connect();
            var g = fins.ReadBools("D100.01", 16);
            var f = fins.ReadString("D100", 3);
            var d = fins.ReadInt16("D100");

            Console.ReadKey();
        }
    }
}
