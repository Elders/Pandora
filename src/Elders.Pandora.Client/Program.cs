using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elders.Pandora.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Client.Connect("127.0.0.1", 3000);
        }
    }
}
