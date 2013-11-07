using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Translocator
{
    public partial class AgencyPage : PhoneApplicationPage
    {
        public AgencyPage()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
        }

        private void RoutesButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }
    }
}