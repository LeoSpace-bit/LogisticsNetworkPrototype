using Models.Graph;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulations
{
    class NewOrdersSimulation
    {
        private Random _random;
        private List<Tuple<string, int, List<Type>>> _cargoСategories;
        private List<Way> _ways;
        private List<City> _cities;
        private Graph _graph;

        internal NewOrdersSimulation()
        {
            _random = new Random();
            _graph = new Graph();
            _cargoСategories = LoadCategories();

            LoadWays(out _ways);
            GetCities(_ways, out _cities);

            //Init graph
            _cities.ForEach(city => _graph.AddVertex(city));
            _graph.SetCountVertices(_cities.Max(city => city.ID) + 1);
            _ways.ForEach(way => _graph.AddEdge(way.InitialCity, way.DestinationCity, way.DeliveryType, way.Cost));

        }

        private void LoadWays(out List<Way> ways)
        {
            ways = new List<Way>();

            try
            {
                new Connection().ExecuteQuery("SELECT * FROM Way ORDER BY initialcityid, destinationcityid ASC;", out NpgsqlDataReader reader);
                while (reader.Read())
                {
                    ways.Add(new Way(LoadCity((int)reader[1]), LoadCity((int)reader[2]), (int)reader[3], reader[4].ToString()));
                }

            }
            catch (Exception e)
            {
                ways = null;
                Console.WriteLine($"[ERROR] <LoadWays>\n{e.Message}");
            }

        }

        private City LoadCity(int id)
        {
            City city = new City();
            city.Departments = new List<Department>();

            try
            {
                new Connection().ExecuteQuery($"SELECT id, name, departments::TEXT FROM City WHERE id = {id};", out NpgsqlDataReader reader);
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
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] <LoadCity>\n{e.Message}");
            }

            return city;
        }

        private void GetCities(List<Way> ways, out List<City> cities)
        {
            List<City> values = new List<City>();
            ways.ForEach(way =>
            {
                values.Add(way.InitialCity);
                values.Add(way.DestinationCity);
            });

            cities = values.GroupBy(o => o.ID).Select(o => o.FirstOrDefault()).ToList();
        }

        private void GetRandomCategories(out List<Tuple<string, int, List<Type>>> randomCategories)
        {
            randomCategories = new List<Tuple<string, int, List<Type>>>();

            int count = _random.Next(0, _cargoСategories.Count / 2 + 1);

            if (count == 0)
            {
                return;
            }

            for (int i = 0; i < count;)
            {
                var item = _cargoСategories[_random.Next(0, _cargoСategories.Count - 1)];

                if(!randomCategories.Contains(item))
                {
                    randomCategories.Add(item);
                    i++;
                }
            }
        }

        private List<Tuple<string, int, List<Type>>> LoadCategories()
        {
            var cargoСategories = new List<Tuple<string, int, List<Type>>>();
            try
            {
                new Connection().ExecuteQuery("SELECT * FROM CargoСategory", out NpgsqlDataReader reader);

                while (reader.Read())
                {
                    Tuple<string, int, List<Type>> cargo;

                    if (reader[3].ToString() == "System.String[]")
                    {
                        List<Type> types = new List<Type>();
                        (reader[3] as string[]).ToList().ForEach(item =>
                        {
                            Type value;
                            switch (item)
                            {
                                case "Land":
                                    value = Type.Land;
                                    break;

                                case "Water":
                                    value = Type.Water;
                                    break;
                                default:
                                    value = Type.Air;
                                    break;
                            }

                            types.Add(value);
                        });

                        cargo = new Tuple<string, int, List<Type>>(reader[1].ToString(), int.Parse(reader[2].ToString()), types);
                    }
                    else
                    {
                        cargo = new Tuple<string, int, List<Type>>(reader[1].ToString(), int.Parse(reader[2].ToString()), null);
                    }

                    cargoСategories.Add(cargo);
                }

                return cargoСategories;

            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] <LoadCategories>\n{e.Message}");
            }

            return null;
        }

        internal void CreateNew()
        {
            REPEAT:

            GetRandomCategories(out List<Tuple<string, int, List<Type>>> randomCategories);

            List<Type> blockedTypeCategories = new List<Type>();
            foreach (var categories in from categories in randomCategories where categories.Item3 != null select categories)
            {
                categories.Item3.ForEach(item =>
                {
                    blockedTypeCategories.Add(item);
                });
            }

            List<Type> rightTypes = (new[] { Type.Air, Type.Land, Type.Water }).Except(blockedTypeCategories).ToList();
            if (rightTypes is null)
            {
                rightTypes = new List<Type>() {Type.Air, Type.Land, Type.Water };
            }

            City selectedSenderCity = null;
            Department senderDepart = null;
            foreach (var item in _cities)
            {
                if (item.ID == 0)
                {
                    selectedSenderCity = item;
                    senderDepart = item.Departments.First();
                }
            }

            City selectedRecipientCity = null;
            Department recipientDepart = null;

            int localID = _random.Next(2, _cities.Count - 1);
            foreach (var item in _cities)
            {
                if (item.ID == localID)
                {
                    selectedRecipientCity = item;
                    recipientDepart = item.Departments.First();
                }
            }

            _graph.FindAllPaths(selectedSenderCity.ID, selectedRecipientCity.ID, rightTypes.ToArray());

            if (_graph.Ways.Count == 0)
            {
                goto REPEAT;
            }

            var cheapestRoad = _graph.GetCheapestWay();
            var temporaryNodes = (from node in cheapestRoad.Path select new Simulations.Node() { No = node.No, Cost = node.Cost, Type = node.Type.ToString() }).ToList();
            var road = new Road() { Cost = cheapestRoad.Cost, Path = temporaryNodes };

            var order = new Order()
            {
                Customer = new Customer()
                {
                    FirstName = "FN_Customer_Test_user",
                    LastName = "LS_Customer_Test_user",
                    Patronymic = "P_Customer_Test_user",
                    Passport = new Passport()
                    {
                        Series =  "0000",
                        Number = "000001"
                    },
                    PhoneNumber = "89000000001"
                },
                Recipient = new Customer()
                {
                    FirstName = "FN_Recipient_Test_user",
                    LastName = "LS_Recipient_Test_user",
                    Patronymic = "P_Recipient_Test_user",
                    Passport = new Passport()
                    {
                        Series = "0000",
                        Number = "000002"
                    },
                    PhoneNumber = "89000000002"
                },
                Path = road,
                Cost = road.Cost + randomCategories.Sum(item => item.Item2),
                ReceivingPoint = senderDepart,      //FROM
                DeliveryPoint = recipientDepart,    //TO
                CurrentState = "Accepted by the carrier"
            };
            order.TrackingCode = $"LV{MathTrackingCode(order)}l";

            AddFullOrder(order);

        }

        private void AddOrder(Order order, out int id)
        {
            try
            {
                new Connection().ExecuteQueryScalar($"INSERT INTO passport (series, number) VALUES ({order.Customer.Passport.Series}, {order.Customer.Passport.Number}) RETURNING id", out var customerPassID);
                new Connection().ExecuteQueryScalar($"INSERT INTO passport (series, number) VALUES ({order.Recipient.Passport.Series}, {order.Recipient.Passport.Number}) RETURNING id", out var recipientPassID);

                new Connection().ExecuteQueryScalar($"INSERT INTO customer (firstName, lastName, patronymic, passportID, phoneNumber) VALUES('{order.Customer.FirstName}', '{order.Customer.LastName}', '{order.Customer.Patronymic}', {(int)customerPassID}, '{order.Customer.PhoneNumber}') RETURNING id", out var customerID);
                new Connection().ExecuteQueryScalar($"INSERT INTO customer (firstName, lastName, patronymic, passportID, phoneNumber) VALUES('{order.Recipient.FirstName}', '{order.Recipient.LastName}', '{order.Recipient.Patronymic}', {(int)recipientPassID}, '{order.Recipient.PhoneNumber}') RETURNING id", out var recipientID);

                new Connection().ExecuteQueryScalar($"INSERT INTO \"order\" (customerID, recipientID, cost, receivingPoint, deliveryPoint, trackingCode) VALUES ({(int)customerID}, {(int)recipientID}, {order.Cost}, ROW('{order.ReceivingPoint.Name}', '{order.ReceivingPoint.Address}'), ROW('{order.DeliveryPoint.Name}', '{order.DeliveryPoint.Address}'), '{order.TrackingCode}' ) RETURNING id", out var resultID);

                id = (int)resultID;

            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] <AddOrder>\n{e.Message}");
                id = -1;
            }
        }

        private void AddFullOrder(Order order)
        {
            AddOrder(order, out int orderID);

            try
            {
                for (int i = 0; i < order.Path.Path.Count - 1; i++)
                {
                    Node node = order.Path.Path[i];
                    Node nextNode = order.Path.Path[i + 1];

                    new Connection().ExecuteQueryScalar($"SELECT id FROM way WHERE initialCityID = {node.No + 1} AND destinationCityID = {nextNode.No + 1} AND cost = {nextNode.Cost} AND possibleDeliveryType = '{nextNode.Type}';", out var wayID);
                    Console.WriteLine($"<{wayID}>  from [{node.No + 1}] to [{nextNode.No + 1}] where cost = {nextNode.Cost} AND possibleDeliveryType = '{nextNode.Type}' ");

                    if (i + 1 == 1)
                    {
                        new Connection().ExecuteQuery($"INSERT INTO road (orderID, wayID, serialNumber, visitStatus, datatime, datatimend) VALUES ({orderID}, {wayID}, {i + 1}, false, '{DateTime.UtcNow}', '{DateTime.MinValue}')", out var _); //{DateTime.Now} ExecuteCommand(conn, $"INSERT INTO road (orderID, wayID, visitStatus, datatime) VALUES ({orderID}, {wayID}, false, null)", out object obj);
                    }
                    else
                    {
                        new Connection().ExecuteQuery($"INSERT INTO road (orderID, wayID, serialNumber, visitStatus, datatime, datatimend) VALUES ({orderID}, {wayID}, {i + 1}, false, '{DateTime.MinValue}', '{DateTime.MinValue}')", out var _); //{DateTime.Now} ExecuteCommand(conn, $"INSERT INTO road (orderID, wayID, visitStatus, datatime) VALUES ({orderID}, {wayID}, false, null)", out object obj);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] <AddFullOrder>\n{e.Message}");
            }

        }

        private int MathTrackingCode(Order order)
        {
            int hashCode = 289314720;
            hashCode = hashCode * -1521134295 + EqualityComparer<Customer>.Default.GetHashCode(order.Customer);
            hashCode = hashCode * -1521134295 + EqualityComparer<Customer>.Default.GetHashCode(order.Recipient);
            hashCode = hashCode * -1521134295 + EqualityComparer<Road>.Default.GetHashCode(order.Path);
            hashCode = hashCode * -1521134295 + order.Cost.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Department>.Default.GetHashCode(order.DeliveryPoint);
            hashCode = hashCode * -1521134295 + EqualityComparer<Department>.Default.GetHashCode(order.ReceivingPoint);
            return Math.Abs(hashCode);
        }
    }
}
