using LogisticClient.Models.GraphModel;
using LogisticService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Way = LogisticService.Way;

namespace LogisticClient.Models
{
    class MainManager
    {
        private List<Way> _ways;

        internal List<Models.GraphModel.Type> RightTypes { get; private set; }
        internal List<CargoСategory> CargoСategories { get; set; }
        internal List<City> Cities { get; set; }
        internal Models.GraphModel.Graph Graph { get; set; }

        internal MainManager()
        {
            CargoСategories = new List<CargoСategory>();
            _ways = new List<Way>();
            Graph = new LogisticClient.Models.GraphModel.Graph();
        }

        private List<City>  GetCities()
        {
            List<City> values = new List<City>();
            foreach (var way in _ways)
            {
                values.Add(way.InitialCity);
                values.Add(way.DestinationCity);
            }

            return values.GroupBy(o => o.ID).Select(o => o.FirstOrDefault()).ToList();
        }

        internal void PrepareObjects(ComboBox senderCity, ComboBox recipientCity)
        {
            Cities = GetCities();
            senderCity.ItemsSource = (from city in Cities select new ComboBoxItem() { Content = city.Name });

            List<string> strings = (from way in _ways.GroupBy(x => x.DestinationCity).Select(y => y.First()) select way.DestinationCity.Name).ToList();
            strings.Sort();
            recipientCity.ItemsSource = (from line in strings.Distinct() select new ComboBoxItem() { Content = line });

            //Init graph
            Cities.ForEach(city => Graph.AddVertex(city));
            Graph.SetCountVertices(Cities.Max(city => city.ID) + 1);
            _ways.ForEach(way => Graph.AddEdge(way.InitialCity, way.DestinationCity, way.DeliveryType, way.Cost));
        }

        internal void LoadWays(LogisticService.LogisticServiceClient service)
        {
            foreach (var item in service.GetWaysAsync().Result)
            {
                _ways.Add(new Way()
                {
                    InitialCity = new City()
                    {
                        ID = item.InitialCity.ID,
                        Name = item.InitialCity.Name,
                        Departments = GetDepartments(item.InitialCity.Departments)
                    },
                    DestinationCity = new City()
                    {
                        ID = item.DestinationCity.ID,
                        Name = item.DestinationCity.Name,
                        Departments = GetDepartments(item.DestinationCity.Departments)
                    },
                    Cost = item.Cost,
                    DeliveryType = item.DeliveryType
                });
            }
        }

        private List<Department> GetDepartments(LogisticService.Department[] departments) =>
            (from initDepartment in departments select new Department() { Name = initDepartment.Name, Address = initDepartment.Address }).ToList();

        internal void LoadCargoCategories(ListBox cargoCategories, LogisticService.LogisticServiceClient service)
        {
            foreach (var item in service.GetCargoСategoriesAsync().Result)
            {
                if (item.BlockedTypes is null)
                {
                    CargoСategories.Add(new CargoСategory() { Name = item.Name, AddedСost = item.AddedСost, BlockedTypes = null });
                }
                else
                {
                    CargoСategories.Add(new CargoСategory() { Name = item.Name, AddedСost = item.AddedСost, BlockedTypes = (item.BlockedTypes).ToList() });
                }

                cargoCategories.Items.Add(new CheckBox() { Content = item.Name });
            }
        }

        private int MathTrackingCode(LogisticClient.LogisticService.Order order)
        {
            int hashCode = 289314720;
            hashCode = hashCode * -1521134295 + EqualityComparer<LogisticClient.LogisticService.Customer>.Default.GetHashCode(order.Customer);
            hashCode = hashCode * -1521134295 + EqualityComparer<LogisticClient.LogisticService.Customer>.Default.GetHashCode(order.Recipient);
            hashCode = hashCode * -1521134295 + EqualityComparer<LogisticClient.LogisticService.Road>.Default.GetHashCode(order.Path);
            hashCode = hashCode * -1521134295 + order.Cost.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<LogisticClient.LogisticService.Department>.Default.GetHashCode(order.DeliveryPoint);
            hashCode = hashCode * -1521134295 + EqualityComparer<LogisticClient.LogisticService.Department>.Default.GetHashCode(order.ReceivingPoint);
            return Math.Abs(hashCode);
        }

        internal void ShowDepartments(ComboBox city, ComboBox department)
        {
            department.Items.Clear();
            foreach (var item in from value in Cities where value.Name == (city.SelectedItem as ComboBoxItem).Content.ToString() from item in value.Departments select item)
            {
                department.Items.Add(item.Name);
            }

            department.IsEnabled = true;
        }
        
        internal LogisticClient.LogisticService.Order GetOrder(
            ListBox cargoCategories, ComboBox senderCity, ComboBox senderDepartment, ComboBox recipientCity, ComboBox recipientDepartment,
            TextBox firstNameSender, TextBox lastNameSender, TextBox patronymicSender, TextBox passportSeriesSender, TextBox passportNumberSender,
            TextBox phoneNumberSender, TextBox firstNameRecipient, TextBox lastNameRecipient, TextBox patronymicRecipient, TextBox passportSeriesRecipient,
            TextBox passportNumberRecipient, TextBox phoneNumberRecipient)
        {
            var selectedCategories = new List<CargoСategory>();
            var blockedTypeCategories = new List<Models.GraphModel.Type>();

            foreach (var categories in from checkedItems in from CheckBox cb in cargoCategories.Items where cb.IsChecked == true select cb
                                       from categories in CargoСategories
                                       where checkedItems.Content.ToString() == categories.Name
                                       select categories)
            {
                if (categories.BlockedTypes != null)
                {
                    categories.BlockedTypes.ForEach(item =>
                    {
                        Models.GraphModel.Type value;
                        switch (item)
                        {
                            case "Land":
                                value = Models.GraphModel.Type.Land;
                                break;

                            case "Water":
                                value = Models.GraphModel.Type.Water;
                                break;
                            default:
                                value = Models.GraphModel.Type.Air;
                                break;
                        }
                        blockedTypeCategories.Add(value);
                    });
                }

                selectedCategories.Add(categories);
            }

            RightTypes = (new[] { Models.GraphModel.Type.Air, Models.GraphModel.Type.Land, Models.GraphModel.Type.Water }).Except(blockedTypeCategories).ToList();

            City selectedSenderCity = null;
            City selectedRecipientCity = null;
            LogisticClient.LogisticService.Department senderDepart = null;
            LogisticClient.LogisticService.Department recipientDepart = null;

            Cities.ForEach(city =>
            {
                if (city.Name == (senderCity.SelectedItem as ComboBoxItem).Content.ToString())
                {
                    selectedSenderCity = city;
                    foreach (var item in from item in city.Departments where item.Name == senderDepartment.Text.ToString() select item)
                    {
                        senderDepart = new LogisticClient.LogisticService.Department() { Name = item.Name, Address = item.Address };
                    }
                }

                if (city.Name == (recipientCity.SelectedItem as ComboBoxItem).Content.ToString())
                {
                    selectedRecipientCity = city;
                    foreach (var item in from item in city.Departments where item.Name == recipientDepartment.Text.ToString() select item)
                    {
                        recipientDepart = new LogisticClient.LogisticService.Department() { Name = item.Name, Address = item.Address };
                    }
                }
            });

            if (selectedSenderCity.Name == selectedRecipientCity.Name)
            {
                MessageBox.Show("We do not deliver packages within the same city");
                return null;
            }

            if (RightTypes is null)
            {
                RightTypes = new List<Models.GraphModel.Type>() { Models.GraphModel.Type.Air, Models.GraphModel.Type.Land, Models.GraphModel.Type.Water };
            }

            Graph.FindAllPaths(selectedSenderCity.ID, selectedRecipientCity.ID, RightTypes.ToArray());

            if (Graph.Ways.Count == 0)
            {
                MessageBox.Show("Мы не сможем доставить Ваш заказ в этот город");
                return null;
            }

            Models.GraphModel.Way cheapestRoad = Graph.GetCheapestWay();
            var temporaryNodes = (from node in cheapestRoad.Path select new LogisticClient.LogisticService.Node() { No = node.No, Cost = node.Cost, Type = node.Type.ToString() }).ToList();
            var road = new LogisticClient.LogisticService.Road() { Cost = cheapestRoad.Cost, Path = temporaryNodes.ToArray() };

            var order = new LogisticClient.LogisticService.Order()
            {
                Customer = new LogisticClient.LogisticService.Customer()
                {
                    FirstName = firstNameSender.Text,
                    LastName = lastNameSender.Text,
                    Patronymic = patronymicSender.Text,
                    Passport = new LogisticClient.LogisticService.Passport()
                    {
                        Series = passportSeriesSender.Text,
                        Number = passportNumberSender.Text
                    },
                    PhoneNumber = phoneNumberSender.Text
                },
                Recipient = new LogisticClient.LogisticService.Customer()
                {
                    FirstName = firstNameRecipient.Text,
                    LastName = lastNameRecipient.Text,
                    Patronymic = patronymicRecipient.Text,
                    Passport = new LogisticClient.LogisticService.Passport()
                    {
                        Series = passportSeriesRecipient.Text,
                        Number = passportNumberRecipient.Text
                    },
                    PhoneNumber = phoneNumberRecipient.Text
                },
                Path = road,
                Cost = road.Cost + selectedCategories.Sum(item => item.AddedСost),
                ReceivingPoint = senderDepart,      //FROM
                DeliveryPoint = recipientDepart,    //TO
                CurrentState = "Accepted by the carrier"
            };
            order.TrackingCode = $"LV{MathTrackingCode(order)}l";

            return order;
        }

    }
}
