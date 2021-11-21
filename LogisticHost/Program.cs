using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LogisticHost
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var host = new ServiceHost(typeof(LogisticService.LogisticService)))
            {
                host.Open();
                Console.WriteLine("Server is opened");

                Console.ReadLine();
            }
        }
    }
}
