using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Simulations
{
    // TODO сделать проверку того, что элемент является последним и перевести состояние заказа в Готов для получения
    // +BUG: возможно при шаговом обновлении не присваивается значение даты (сбросить всё до false и пройтись)
    sealed class DataUpdateSimulation
    {
        /// <summary>
        /// Updates orders with status 'Accepted by the carrier' to 'In transit'
        /// </summary>
        internal void UpdateAcceptedOrders() 
        {
            new Connection().ExecuteQuery("UPDATE public.\"order\" SET currentstate = 'In transit' WHERE currentstate = 'Accepted by the carrier';", out NpgsqlDataReader answer);
        }

        /// <summary>
        /// Updates the list of past "cities"
        /// </summary>
        internal void UpdateRoadOrders()
        {
            new Connection().ExecuteQuery("SELECT road.orderid, MIN(road.serialnumber) FROM road JOIN \"order\" ON road.orderid = \"order\".id WHERE visitstatus = false AND \"order\".currentstate = 'In transit' GROUP BY road.orderid, road.visitstatus", out NpgsqlDataReader answer);

            while(answer.Read())
            {
                string datetime = string.Empty;

                if ((int)answer[1] == 1)
                {
                    datetime = $", datatimend = '{DateTime.UtcNow}' ";
                }
                else
                {
                    datetime = $", datatime = '{DateTime.UtcNow.AddSeconds(-1)}' , datatimend = '{DateTime.UtcNow}' ";
                }

                new Connection().ExecuteQuery($"UPDATE public.road SET visitstatus = true {datetime} WHERE orderid = {(int)answer[0]} AND serialnumber = {(int)answer[1]};");
                
            }

        }

        /// <summary>
        /// Moves the order from 'In transit' to 'Arrived at the pick-up point'
        /// </summary>
        internal void UpdateOrderReceive()
        {
            List<Tuple<int, int>> currentPosition = new List<Tuple<int, int>>();

            Task<List<Tuple<int, int>>> taskGetFinishPosition = new Task<List<Tuple<int, int>>>(() =>
            {
                List<Tuple<int, int>> values = new List<Tuple<int, int>>();

                new Connection().ExecuteQuery($"SELECT orderid,  MAX(serialnumber) FROM public.road JOIN \"order\" ON \"order\".id = road.orderid WHERE \"order\".currentstate = 'In transit' GROUP BY orderid ORDER BY orderid;", out NpgsqlDataReader finishData);

                while (finishData.Read())
                {
                    values.Add(new Tuple<int, int>((int)finishData[0], (int)finishData[1]));
                }

                return values;
            });
            taskGetFinishPosition.Start();

            new Connection().ExecuteQuery($"SELECT orderid,  MAX(serialnumber) FROM public.road JOIN \"order\" ON \"order\".id = road.orderid WHERE visitstatus = true AND \"order\".currentstate = 'In transit' GROUP BY orderid ORDER BY orderid;", out NpgsqlDataReader currentData);

            while (currentData.Read())
            {
                currentPosition.Add(new Tuple<int, int>((int)currentData[0], (int)currentData[1]));
            }

            List<Tuple<int, int>> finishPosition = taskGetFinishPosition.Result;  // ожидаем получение результата

            if (finishPosition.Count != currentPosition.Count)
            {
                Console.WriteLine("[ERROR] Расхождение количества объектов");
                return;
            }

            bool isExecuteUpdate = false;
            string ids = "";
            for (int i = 0; i < currentPosition.Count; i++)
            {
                if (currentPosition[i].Item1 == finishPosition[i].Item1 && currentPosition[i].Item2 == finishPosition[i].Item2)
                {
                    ids += $" id = {currentPosition[i].Item1} OR";
                    isExecuteUpdate = true;
                }
            }

            if(isExecuteUpdate)
            {
                new Connection().ExecuteQuery($"UPDATE public.\"order\" SET currentstate = 'Arrived at the pick-up point' WHERE {ids.Remove(ids.Length - 2)};");
            }

        }

    }

}
