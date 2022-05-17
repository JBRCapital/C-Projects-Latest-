using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunEveryHour
{
    class Program
    {
        static void Main(string[] args)
        {
            do
            {
                System.Diagnostics.Process.Start(@"C:\JBR Code\AntiFraudLetterGenerator\bin\Debug\AnitFraudLetterGenerator.exe");
                System.Threading.Thread.Sleep(30 * 60 * 1000);
            } while (true);
        }
    }
}
