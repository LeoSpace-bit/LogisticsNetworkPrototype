using LogisticClient.Models;
using LogisticClient.Views;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace LogisticClient
{
    public partial class MainWindow : Window, LogisticService.ILogisticServiceCallback
    {
        public int CLIENT_ID { get; private set; }
        private bool isConnected = false;
        private LogisticService.LogisticServiceClient _client;
        private MainManager _manager;

        public MainWindow() => InitializeComponent();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (isConnected)
            {
                Disconnect();
            }
            else
            {
                Connect();

                _manager = new MainManager();
                _manager.LoadCargoCategories(CargoCategories, _client);
                _manager.LoadWays(_client);
                _manager.PrepareObjects(SenderCity, RecipientCity);
            }
        }

        void Connect()
        {
            if (!isConnected)
            {
                try
                {
                    _client = new LogisticService.LogisticServiceClient(new System.ServiceModel.InstanceContext(this));
                    CLIENT_ID = _client.Connect();
                    isConnected = true;
                }
                catch (System.Exception e)
                {
                    MessageBox.Show(e.ToString(), "[ ERROR ] CONNECTING");
                    Close();
                }
            }
        }

        void Disconnect()
        {
            if (isConnected)
            {
                _client.Disconnect(CLIENT_ID);
                _client = null;
                isConnected = false;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e) => Disconnect();

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var order = _manager.GetOrder(
                CargoCategories, SenderCity, SenderDepartment, RecipientCity, RecipientDepartment,
                FirstNameSender, LastNameSender, PatronymicSender, PassportSeriesSender, PassportNumberSender, PhoneNumberSender,
                FirstNameRecipient, LastNameRecipient, PatronymicRecipient, PassportSeriesRecipient, PassportNumberRecipient, PhoneNumberRecipient);

            if(order != null)
            {
                TrakingCode.Content = order.TrackingCode;
                _client.AddFullOrder(order);
                ClearData();
            }
        }

        private void ClearData()
        {
            FirstNameSender.Text = string.Empty;
            LastNameSender.Text = string.Empty;
            PatronymicSender.Text = string.Empty;
            PassportSeriesSender.Text = string.Empty;
            PassportNumberSender.Text = string.Empty;
            PhoneNumberSender.Text = string.Empty;
            FirstNameRecipient.Text = string.Empty;
            LastNameRecipient.Text = string.Empty;
            PatronymicRecipient.Text = string.Empty;
            PassportSeriesRecipient.Text = string.Empty;
            PassportNumberRecipient.Text = string.Empty;
            PhoneNumberRecipient.Text = string.Empty;
        }

        private void SenderCity_SelectionChanged(object sender, SelectionChangedEventArgs e) => _manager.ShowDepartments(SenderCity, SenderDepartment);

        private void SenderDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e) => RecipientCity.IsEnabled = true;

        private void RecipientCity_SelectionChanged(object sender, SelectionChangedEventArgs e) => _manager.ShowDepartments(RecipientCity, RecipientDepartment);

        public void QueryCallback(string answer) => Debug.WriteLine($"<CALLBACK> {answer}");

        private void TrakingCode_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => Clipboard.SetText(TrakingCode.Content.ToString());

        private void SearchLine_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            PathHistory.Items.Clear();

            if (e.Key == System.Windows.Input.Key.Enter && SearchLine.Text.StartsWith("LV") && SearchLine.Text.EndsWith("l"))
            {
                var roadStatuses = _client.GetRoadStatuses(SearchLine.Text);

                if (roadStatuses.Length == 0)
                {
                    MessageBox.Show("No records");
                    return;
                }

                PathHistory.Items.Add(new TrackingUserControl("Accepted by the carrier", $"Department {roadStatuses[0].OrderBeginDepartments.Name.Trim('(')}", roadStatuses[0].DateTimeBegin));

                if (roadStatuses.Length == 1)
                {
                    if (roadStatuses[0].VisitStatus == true)
                    {
                        PathHistory.Items.Add(new TrackingUserControl($"Ready for issue in {roadStatuses[0].DestinationCity}", $"Department {roadStatuses[0].OrderFinishDepartments.Name.Trim('(')}", roadStatuses[0].DateTimeEnd));
                    }

                    if (roadStatuses[0].CurrentState == "Received at the collection point")
                    {
                        PathHistory.Items.Add(new TrackingUserControl($"Received in {roadStatuses[0].DestinationCity}", $"Department {roadStatuses[0].OrderFinishDepartments.Name.Trim('(')}", roadStatuses[0].DateTimeEnd));
                    }

                }
                else
                {
                    for (int i = 1; i < roadStatuses.Length; i++)
                    {
                        LogisticService.RoadStatus roadCurrent = roadStatuses[i];
                        LogisticService.RoadStatus roadPrev = roadStatuses[i - 1];

                        if (i != 0)
                        {
                            if (roadPrev.VisitStatus == true && roadCurrent.VisitStatus == true)
                            {
                                if (!((PathHistory.Items[PathHistory.Items.Count - 1] as TrackingUserControl).Header.Contains($"Is in {roadCurrent.InitialCityName}")))
                                {
                                    PathHistory.Items.Add(new TrackingUserControl($"Is in {roadCurrent.DestinationCity}", roadCurrent.DateTimeEnd));
                                }
                            }
                        }

                        if (i == roadStatuses.Length - 1 && roadStatuses[i].VisitStatus == true)
                        {
                            PathHistory.Items.Add(new TrackingUserControl($"Ready for issue in {roadCurrent.DestinationCity}", $"Department {roadCurrent.OrderFinishDepartments.Name.Trim('(')}", roadCurrent.DateTimeEnd));

                            if (roadCurrent.CurrentState == "Received at the collection point")
                            {
                                PathHistory.Items.Add(new TrackingUserControl($"Received in {roadCurrent.DestinationCity}", $"Department {roadCurrent.OrderFinishDepartments.Name.Trim('(')}", roadCurrent.DateTimeEnd));
                            }

                        }

                    }
                }

            }
        }

        private void Button_CheckOrder(object sender, RoutedEventArgs e)
        {
            if(_client.IsRightAnswer(ATrackingCode.Text, APhoneNumber.Text))
            {
                MessageBox.Show("Confirmed, the order can be placed.");
                _client.UpdateOrderStatusAsync(ATrackingCode.Text, "Received at the collection point");
            }
            else
            {
                MessageBox.Show("Not confirmed, check if you entered it correctly");
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();

        private void CloseForm_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
            Close();
        }

        private void RecipientDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e) => AcceptButton.IsEnabled = true;

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if(StartDP.SelectedDate is null || EndDP.SelectedDate is null)
            {
                return;
            }

            if (StartDP.SelectedDate > EndDP.SelectedDate)
            {
                MessageBoxResult result = MessageBox.Show($"Did you mean from {EndDP.SelectedDate.Value.Date.ToShortDateString()} to {StartDP.SelectedDate.Value.Date.ToShortDateString()}? ", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    StartDP.SelectedDateChanged -= DatePicker_SelectedDateChanged;
                    EndDP.SelectedDateChanged -= DatePicker_SelectedDateChanged;

                    DateTime? start = StartDP.SelectedDate;
                    StartDP.SelectedDate = EndDP.SelectedDate;
                    EndDP.SelectedDate = start;


                    StartDP.SelectedDateChanged += DatePicker_SelectedDateChanged;
                    EndDP.SelectedDateChanged += DatePicker_SelectedDateChanged;
                }
            }

            if (StartDP.SelectedDate <= EndDP.SelectedDate)
            {
                var currentProfit = PrepareInt(_client.GetCurrentProfitAsync(StartDP.SelectedDate.Value.Date, EndDP.SelectedDate.Value.Date).Result);

                CurrentProfitLable.Content = currentProfit;
                ExpectedProfitLable.Content = currentProfit == "Invalid input" ? "Invalid input" : _client.GetExpectedProfitAsync(StartDP.SelectedDate.Value.Date, EndDP.SelectedDate.Value.Date).Result + currentProfit;

                AcceptedCarrierLable.Content = PrepareInt(_client.GetOrderNumberAsync(StartDP.SelectedDate.Value.Date, EndDP.SelectedDate.Value.Date, "Accepted by the carrier").Result);
                InTransitLable.Content = PrepareInt(_client.GetOrderNumberAsync(StartDP.SelectedDate.Value.Date, EndDP.SelectedDate.Value.Date, "In transit").Result);
                ArrivedPUPPointLable.Content = PrepareInt(_client.GetOrderNumberAsync(StartDP.SelectedDate.Value.Date, EndDP.SelectedDate.Value.Date, "Arrived at the pick-up point").Result);
                ReceivedRecipientLable.Content = PrepareInt(_client.GetOrderNumberAsync(StartDP.SelectedDate.Value.Date, EndDP.SelectedDate.Value.Date, "Received at the collection point").Result);
            }
            else
            {
                ReceivedRecipientLable.Content = ArrivedPUPPointLable.Content = InTransitLable.Content = AcceptedCarrierLable.Content = ExpectedProfitLable.Content =  CurrentProfitLable.Content = "Invalid input";
            }

        }

        private string PrepareInt(int value) => value == int.MinValue? "Invalid input" : value.ToString();

    }
}
