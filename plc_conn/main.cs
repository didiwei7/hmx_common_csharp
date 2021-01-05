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
            var a = PLCFins.FinsConvert.ArrayByte(50).Reverse().ToArray();
            var b = BitConverter.ToInt32(a, 0);

            Console.ReadKey();
        }
    }
}
