using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LogisticClient.Views
{
    public partial class TrackingUserControl : UserControl
    {
        public string Header { get => lHeader.Content.ToString(); set => lHeader.Content = value; }
        public string AdditionalInfo { get => lAdditionalInfo.Content.ToString(); set => lAdditionalInfo.Content = value; }
        public string Date { get => lDate.Content.ToString(); set => lDate.Content = value; }
        public string Time { get => lTime.Content.ToString(); set => lTime.Content = value; }

        public TrackingUserControl(string header, string additionalInfo, DateTime dateTime)
        {
            InitializeComponent();

            Header = header;
            AdditionalInfo = additionalInfo;

            if(dateTime != DateTime.MinValue)
            {
                Date = dateTime.Date.ToString("dd/MM/yyyy");
                Time = dateTime.TimeOfDay.ToString();
            }
            else
            {
                Date = string.Empty;
                Time = string.Empty;
            }

        }

        public TrackingUserControl(string header, DateTime dateTime)
        {
            InitializeComponent();

            Header = header; Date = dateTime.Date.ToString();

            if (dateTime != DateTime.MinValue)
            {
                Date = dateTime.Date.ToString("dd/MM/yyyy");
                Time = dateTime.TimeOfDay.ToString();
            }
            else
            {
                Date = string.Empty;
                Time = string.Empty;
            }
        }

    }
}
