using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Simulations
{
    class Program
    {
        private static DataUpdateSimulation _dataUpdateSimulation;
        private static NewOrdersSimulation _newOrdersSimulation;

        private static void Main(string[] args)
        {
            _dataUpdateSimulation = new DataUpdateSimulation();
            _newOrdersSimulation = new NewOrdersSimulation();

            Timer aTimer = new Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 10000;
            aTimer.Enabled = true;

            Console.WriteLine("Emulation is running.\nDo you really want to leave ?");
            Console.ReadLine();

        }

        private static void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            _dataUpdateSimulation.UpdateAcceptedOrders();
            _dataUpdateSimulation.UpdateRoadOrders();
            _dataUpdateSimulation.UpdateOrderReceive();

            _newOrdersSimulation.CreateNew();
        }
    }
}
