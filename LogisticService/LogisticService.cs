using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using LogisticService.model;
using LogisticService;
using Npgsql;
using System.Xml.Linq;
using System.Diagnostics;

namespace LogisticService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class LogisticService : ILogisticService
    {
        private List<ServerUser> _users = new List<ServerUser>();
        private int _nextID = 1;

        public int Connect()
        {
            ServerUser user = new ServerUser()
            {
                ID = _nextID,
                OperationContext = OperationContext.Current
            };

            _nextID++;
            _users.Add(user);
            Console.WriteLine($"Client {user.ID} connected");
            return user.ID;
        }

        public void Disconnect(int id)
        {
            var user = _users.FirstOrDefault(item => item.ID == id);

            if (user != null)
            {
                _users.Remove(user);
                Console.WriteLine($"Client {user.ID} disconnected");
            }
        }

        public void Query(int id, string query)
        {
            var user = _users.FirstOrDefault(item => item.ID == id);

            if (user != null)
            {
                string answer = "";

                try
                {
                    var conn = new NpgsqlConnection(@"server=localhost;Port=5433;User Id=postgres;Password=123;Database=LogisticsNetworkPrototype");
                    conn.Open();
                    NpgsqlCommand command = new NpgsqlCommand();
                    command.Connection = conn;
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = query;

                    NpgsqlDataReader dr = command.ExecuteReader();

                    while(dr.Read())
                    {
                        foreach (var item in dr)
                        {
                            answer += $"{item}\t";
                        }
                        answer += "\n";
                    }

                    command.Dispose();
                    conn.Close();

                }
                catch (Exception e)
                {
                    Console.WriteLine($"[ERROR] <Query> {user.ID}\n{e.Message}");
                }

                user.OperationContext.GetCallbackChannel<ILogisticServiceCallback>().QueryCallback(answer);
            }

        }

        public List<CargoСategory> GetCargoСategories()
        {
            List<CargoСategory> cargoСategories = new List<CargoСategory>();
            try
            {
                var conn = new NpgsqlConnection(@"server=localhost;Port=5433;User Id=postgres;Password=123;Database=LogisticsNetworkPrototype");
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = "SELECT * FROM CargoСategory";

                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    CargoСategory cargo = new CargoСategory();

                    if (reader[3].ToString() == "System.String[]")
                    {
                        var v = (string[])reader[3];
                        cargo.Name = reader[1].ToString();
                        cargo.AddedСost = int.Parse(reader[2].ToString());
                        cargo.BlockedTypes = v.ToList<string>();
                    }
                    else
                    {
                        cargo.Name = reader[1].ToString();
                        cargo.AddedСost = int.Parse(reader[2].ToString());
                        cargo.BlockedTypes = null;
                    }

                    cargoСategories.Add(cargo);
                }

                command.Dispose();
                conn.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] <GetCargoСategories>\n{e.Message}");
            }

            return cargoСategories;
        }

        public City GetCity()
        {
            City city = new City();
            city.Departments = new List<Department>();

            try
            {
                var conn = new NpgsqlConnection(@"server=localhost;Port=5433;User Id=postgres;Password=123;Database=LogisticsNetworkPrototype");
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = "SELECT id, name, departments::TEXT FROM City WHERE id = 1;";

                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    city.ID = (int)reader[0] - 1;
                    city.Name = reader[1].ToString();

                    // { "(A1,\"Address 1\")","(A2,\"Address 2\")"}     -->     A1λAddress 1ΘA2λAddress 2
                    var departsStringType = reader[2].ToString().Trim('{', '}').Replace("\"(", "").Replace(")\"", "").Replace(@"\" + "\",", "Θ").Replace(@"\" + "\"", "").Replace(',', 'λ').Split('Θ');

                    foreach (var item in departsStringType)
                    {
                        var values = item.Split('λ');

                        city.Departments.Add(new Department() {Name = values[0], Address = values[1] });
                    }

                }

                command.Dispose();
                conn.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] <GetCity>\n{e.Message}");
            }

            return city;

        }

        public List<Way> GetWays()
        {
            List<Way> ways = new List<Way>();

            try
            {
                var conn = new NpgsqlConnection(@"server=localhost;Port=5433;User Id=postgres;Password=123;Database=LogisticsNetworkPrototype");
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = "SELECT * FROM Way ORDER BY initialcityid, destinationcityid ASC;";

                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Way way = new Way();

                    way.InitialCity = GetCity((int)reader[1]);
                    way.DestinationCity = GetCity((int)reader[2]);
                    way.Cost = (int)reader[3];
                    way.DeliveryType = reader[4].ToString();

                    ways.Add(way);
                }

                command.Dispose();
                conn.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] <GetWays>\n{e.Message}");
            }

            return ways;
        }

        private City GetCity(int id)
        {
            City city = new City();
            city.Departments = new List<Department>();

            try
            {
                var conn = new NpgsqlConnection(@"server=localhost;Port=5433;User Id=postgres;Password=123;Database=LogisticsNetworkPrototype");
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = $"SELECT id, name, departments::TEXT FROM City WHERE id = {id};";

                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    city.ID = (int)reader[0] - 1;
                    city.Name = reader[1].ToString();

                    // { "(A1,\"Address 1\")","(A2,\"Address 2\")"}     -->     A1λAddress 1ΘA2λAddress 2
                    var departsStringType = reader[2].ToString().Trim('{', '}').Replace("\"(", "").Replace(")\"", "").Replace(@"\" + "\",", "Θ").Replace(@"\" + "\"", "").Replace(',', 'λ').Split('Θ');

                    foreach (var item in departsStringType)
                    {
                        var values = item.Split('λ');

                        city.Departments.Add(new Department() { Name = values[0], Address = values[1] });
                    }

                }

                command.Dispose();
                conn.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] <GetCity>\n{e.Message}");
            }

            return city;

        }
        
        public void AddOrder(Order order, out int id)
        {
            try
            {
                var conn = new NpgsqlConnection(@"server=localhost;Port=5433;User Id=postgres;Password=123;Database=LogisticsNetworkPrototype");
                conn.Open();

                ExecuteCommand(conn, $"INSERT INTO passport (series, number) VALUES ({order.Customer.Passport.Series}, {order.Customer.Passport.Number}) RETURNING id", out object customerPassID);
                ExecuteCommand(conn, $"INSERT INTO passport (series, number) VALUES ({order.Recipient.Passport.Series}, {order.Recipient.Passport.Number}) RETURNING id", out object recipientPassID);

                ExecuteCommand(conn, $"INSERT INTO customer (firstName, lastName, patronymic, passportID, phoneNumber) VALUES('{order.Customer.FirstName}', '{order.Customer.LastName}', '{order.Customer.Patronymic}', {(int)customerPassID}, '{order.Customer.PhoneNumber}') RETURNING id", out object customerID);
                ExecuteCommand(conn, $"INSERT INTO customer (firstName, lastName, patronymic, passportID, phoneNumber) VALUES('{order.Recipient.FirstName}', '{order.Recipient.LastName}', '{order.Recipient.Patronymic}', {(int)recipientPassID}, '{order.Recipient.PhoneNumber}') RETURNING id", out object recipientID);

                ExecuteCommand(conn, $"INSERT INTO \"order\" (customerID, recipientID, cost, receivingPoint, deliveryPoint, trackingCode) VALUES ({(int)customerID}, {(int)recipientID}, {order.Cost}, ROW('{order.ReceivingPoint.Name}', '{order.ReceivingPoint.Address}'), ROW('{order.DeliveryPoint.Name}', '{order.DeliveryPoint.Address}'), '{order.TrackingCode}' ) RETURNING id", out object resultID);

                id = (int)resultID;

                conn.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] <AddOrder>\n{e.Message}");
                id = -1;
            }
        }

        public void AddFullOrder(Order order)
        {
            AddOrder(order, out int orderID);

            try
            {
                var conn = new NpgsqlConnection(@"server=localhost;Port=5433;User Id=postgres;Password=123;Database=LogisticsNetworkPrototype");
                conn.Open();

                for (int i = 0; i < order.Path.Path.Count - 1; i++)
                {
                    Node node = order.Path.Path[i];
                    Node nextNode = order.Path.Path[i + 1];

                    //(initialCityID, destinationCityID, cost, possibleDeliveryType

                    ExecuteCommand(conn, $"SELECT id FROM way WHERE initialCityID = {node.No + 1} AND destinationCityID = {nextNode.No + 1} AND cost = {nextNode.Cost} AND possibleDeliveryType = '{nextNode.Type}';", out object wayID);
                    //Console.WriteLine($"<{wayID}>  from [{node.No + 1}] to [{nextNode.No + 1}] where cost = {nextNode.Cost} AND possibleDeliveryType = '{nextNode.Type}' ");

                    if (i + 1 == 1)
                    {
                        ExecuteCommand(conn, $"INSERT INTO road (orderID, wayID, serialNumber, visitStatus, datatime, datatimend) VALUES ({orderID}, {wayID}, {i + 1}, false, '{DateTime.UtcNow}', '{DateTime.MinValue}')", out object obj); //{DateTime.Now} ExecuteCommand(conn, $"INSERT INTO road (orderID, wayID, visitStatus, datatime) VALUES ({orderID}, {wayID}, false, null)", out object obj);
                    }
                    else
                    {
                        ExecuteCommand(conn, $"INSERT INTO road (orderID, wayID, serialNumber, visitStatus, datatime, datatimend) VALUES ({orderID}, {wayID}, {i + 1}, false, '{DateTime.MinValue}', '{DateTime.MinValue}')", out object obj); //{DateTime.Now} ExecuteCommand(conn, $"INSERT INTO road (orderID, wayID, visitStatus, datatime) VALUES ({orderID}, {wayID}, false, null)", out object obj);
                    }


                }

                conn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] <AddFullOrder>\n{e.Message}");
            }

        }

        private void ExecuteCommand(NpgsqlConnection connection, string command, out object answer)
        {
            NpgsqlCommand npgsqlCommand = new NpgsqlCommand(command, connection);
            npgsqlCommand.Prepare();
            npgsqlCommand.CommandType = CommandType.Text;
            answer = npgsqlCommand.ExecuteScalar();
            npgsqlCommand.Dispose();
        }

        public List<RoadStatus> GetRoadStatuses(string trakingCode)
        {
            var roadStatuses = new List<RoadStatus>();

            try
            {
                var conn = new NpgsqlConnection(@"server=localhost;Port=5433;User Id=postgres;Password=123;Database=LogisticsNetworkPrototype");
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = $"SELECT road.serialnumber, (SELECT city.name FROM city WHERE city.id = way.initialcityid), (SELECT city.departments FROM city WHERE city.id = way.initialcityid)::TEXT, (SELECT city.name FROM city WHERE city.id = way.destinationcityid), (SELECT city.departments FROM city WHERE city.id = way.destinationcityid)::TEXT, road.visitstatus, road.datatime, road.datatimend, (\"order\".receivingpoint)::TEXT, (\"order\".deliverypoint)::TEXT, \"order\".currentstate FROM road JOIN way ON way.id = road.wayid JOIN \"order\" ON \"order\".id = orderid WHERE \"order\".trackingcode = '{trakingCode}' ORDER BY road.serialnumber;";

                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    roadStatuses.Add(new RoadStatus()
                    {
                        SerialNumber = (int)reader[0],
                        InitialCityName = reader[1].ToString(),
                        InitialCityDepartments = GetDepartments(reader[2].ToString()),
                        DestinationCity = reader[3].ToString(),
                        DestinationCityDepartments = GetDepartments(reader[4].ToString()),
                        VisitStatus = (bool)reader[5],
                        DateTimeBegin = (DateTime)reader[6],
                        DateTimeEnd = (DateTime)reader[7],
                        OrderBeginDepartments = GetDepartments(reader[8].ToString())[0],
                        OrderFinishDepartments = GetDepartments(reader[9].ToString())[0],
                        CurrentState = reader[10].ToString()
                    });
                }

                command.Dispose();
                conn.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] <GetRoadStatuses>\n{e.Message}");
            }

            return roadStatuses;

        }

        private List<Department> GetDepartments(string data)
        {
            var departments = new List<Department>();

            // { "(A1,\"Address 1\")","(A2,\"Address 2\")"}     -->     A1λAddress 1ΘA2λAddress 2
            var departsStringType = data.ToString().Trim('{', '}').Replace("\"(", "").Replace(")\"", "").Replace(@"\" + "\",", "Θ").Replace(@"\" + "\"", "").Replace(',', 'λ').Split('Θ');

            foreach (var item in departsStringType)
            {
                var values = item.Split('λ');
                departments.Add(new Department() { Name = values[0], Address = values[1] });
            }

            return departments;
        }

        public bool IsRightAnswer(string trakingCode, string phoneNumber)
        {
            bool value = false;
            try
            {
                var conn = new NpgsqlConnection(@"server=localhost;Port=5433;User Id=postgres;Password=123;Database=LogisticsNetworkPrototype");
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = $"SELECT CASE WHEN EXISTS(SELECT \"order\".id, customer.phonenumber FROM \"order\" JOIN customer ON \"order\".recipientid = customer.id WHERE trackingcode = '{trakingCode}' AND customer.phonenumber = '{phoneNumber}' AND \"order\".currentstate = 'Arrived at the pick-up point') THEN CAST(1 as boolean) ELSE CAST(0 AS boolean) END;";

                value = (bool) command.ExecuteScalar();

                command.Dispose();
                conn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] <IsRightAnswer>\n{e.Message}");
            }

            return value;
        }

        public void UpdateOrderStatus(string trakingCode, string status)
        {
            try
            {
                var conn = new NpgsqlConnection(@"server=localhost;Port=5433;User Id=postgres;Password=123;Database=LogisticsNetworkPrototype");
                conn.Open();

                ExecuteCommand(conn, $"UPDATE public.\"order\" SET currentstate = '{status}' WHERE trackingcode = '{trakingCode}';", out object obj);

                conn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] <UpdateOrderStatus>\n{e.Message}");
            }

        }
    }
}
