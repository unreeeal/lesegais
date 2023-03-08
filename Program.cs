using Lesegais.Domain;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lesegais
{
    internal class Program
    {
        private const int DELAY = 10 * 60 * 1000;
        public static bool FullUniqMode=false;

        static void Main(string[] args)
        {
            while (true)
            {
                Crawler.Start();
                Thread.Sleep(DELAY);
            }
        }
    }
}
