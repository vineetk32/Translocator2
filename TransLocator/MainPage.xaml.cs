using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using System.Device.Location;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using Microsoft.Phone.Tasks;
using System.Text;
using Microsoft.Phone.Info;

namespace Translocator
{
    public partial class MainPage : PhoneApplicationPage
    {

        public MainPage()
        {
            InitializeComponent();

            ParentPivot.Items.Remove(StopsPivot);

            DataContext = App.ViewModel;

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }
        
        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            if (settings.Contains("SelectedAgencies"))
            {
                App.ViewModel.restoredAgencies = (List<long>)settings["SelectedAgencies"];
            }
            else
            {
                NavigationService.Navigate(new Uri("/AgencyPage.xaml", UriKind.Relative));
            }
            if (settings.Contains("SelectedRoutes"))
            {
                App.ViewModel.restoredRoutes = (List<long>)settings["SelectedRoutes"];
            }

            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
            this.Loaded -= new RoutedEventHandler(MainPage_Loaded);
        }

        private void RouteList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ParentPivot.SelectedItem == StopsPivot) {

                string routeName = (sender as ListPicker).SelectedItem as string;
                if (routeName != null)
                    App.ViewModel.addArrivalsForRoute(routeName);
            }
        }

        public void ShowMaps() {

            ApplicationBarIconButton MapsBtn = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            MapsBtn.IsEnabled = true;
        }

        public void HideMaps() {

            ApplicationBarIconButton MapsBtn = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            MapsBtn.IsEnabled = false;
        }


        public void ShowRoutes() {

            //TODO - use a bool instead
            if (ParentPivot.Items.Contains(RoutesPivot) == false) {
                ParentPivot.Items.Add(RoutesPivot);
            }
        }

        public void HideRoutes()
        {
            //TODO - use a bool instead
            if (ParentPivot.Items.Contains(RoutesPivot) == true)
            {
                ParentPivot.Items.Remove(RoutesPivot);
                HideStops();
                HideMaps();
            }
        }

        public void ShowStops()
        {
            //TODO - use a bool instead
            if (ParentPivot.Items.Contains(StopsPivot) == false)
            {
                ParentPivot.Items.Add(StopsPivot);
            }
        }

        public void HideStops()
        {
            //TODO - use a bool instead
            if (ParentPivot.Items.Contains(StopsPivot) == true)
            {
                ParentPivot.Items.Remove(StopsPivot);
            }
        }

        private void chkAgency_Checked(object sender, RoutedEventArgs e)
        {
            if (App.ViewModel.selectedAgencies.Count == 0)
            {
                ShowRoutes();
            }
        }

        private void chkAgency_Unchecked(object sender, RoutedEventArgs e)
        {
            if (App.ViewModel.selectedAgencies.Count == 1)
            {
                HideRoutes();
            }
        }

        private void chkRoute_Checked(object sender, RoutedEventArgs e)
        {
            if (App.ViewModel.selectedRoutes.Count == 0)
            {
                ShowStops();
                ShowMaps();
            }
        }

        private void chkRoute_Unchecked(object sender, RoutedEventArgs e)
        {
            if (App.ViewModel.selectedRoutes.Count == 1)
            {
                HideStops();
                HideMaps();
            }
        }

        private void ParentPivot_LoadedPivotItem(object sender, PivotItemEventArgs e)
        {
            if (ParentPivot.SelectedItem == StopsPivot)
            {
                App.ViewModel.cacheAllArrivals();
            }
            else if (ParentPivot.SelectedItem == RoutesPivot)
            {
                if (App.ViewModel.selectedRoutes.Count == 0 || App.ViewModel.selectedAgencies.Count == 0)
                {
                    HideStops();
                }
            }
        }

        private void MapsButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MapPage.xaml", UriKind.Relative));
        }

        private void AgenciesButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AgencyPage.xaml", UriKind.Relative));
        }

        private void ClickEvent(object sender, EventArgs e)
        {
            StringBuilder deviceInfo = new StringBuilder();

            long ApplicationMemoryUsage = DeviceStatus.ApplicationCurrentMemoryUsage;
            string FirmwareVersion = DeviceStatus.DeviceFirmwareVersion;
            string HardwareVersion = DeviceStatus.DeviceHardwareVersion;
            string Manufacturer = DeviceStatus.DeviceManufacturer;
            string DeviceName = DeviceStatus.DeviceName;
            long TotalMemory = DeviceStatus.DeviceTotalMemory;
            string OSVersion = Environment.OSVersion.Version.ToString();

            deviceInfo.AppendLine("\n\n\n\n\n\n\n\n\n");
            deviceInfo.AppendLine("Memory Usage :" + ApplicationMemoryUsage);
            deviceInfo.AppendLine("Firmware Version :" + FirmwareVersion);
            deviceInfo.AppendLine("Hardware Version :" + HardwareVersion);
            deviceInfo.AppendLine("Manufacturer :" + Manufacturer);
            deviceInfo.AppendLine("Device Name :" + DeviceName);
            deviceInfo.AppendLine("Total Memory :" + TotalMemory);
            deviceInfo.AppendLine("Operating System: Windows Phone " + OSVersion.ToString());

            EmailComposeTask emailcomposer = new EmailComposeTask();
            emailcomposer.To = "vineetkrishnan@hotmail.com";
            emailcomposer.Subject = "Translocator Feedback";
            emailcomposer.Body = deviceInfo.ToString();
            emailcomposer.Show();
        }
   }
}