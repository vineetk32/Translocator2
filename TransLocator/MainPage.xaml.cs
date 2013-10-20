using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using System.Device.Location;
using Microsoft.Phone.Shell;

namespace Translocator
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        bool isMapUptoDate;

        public bool isMapUpDated
        {
            get
            {
                return isMapUptoDate;
            }
            set
            {
                if (isMapUptoDate != value)
                {
                    isMapUptoDate = value;
                }
            }
        }

        public MainPage()
        {
            InitializeComponent();

            ParentPivot.Items.Remove(StopsPivot);

            isMapUptoDate = false;

            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }
        
        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
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
            isMapUptoDate = false;
        }

        private void chkAgency_Unchecked(object sender, RoutedEventArgs e)
        {
            if (App.ViewModel.selectedAgencies.Count == 1)
            {
                HideRoutes();
            }
            isMapUptoDate = false;
        }

        private void chkRoute_Checked(object sender, RoutedEventArgs e)
        {
            if (App.ViewModel.selectedRoutes.Count == 0)
            {
                ShowStops();
                ShowMaps();
            }
            isMapUptoDate = false;
        }

        private void chkRoute_Unchecked(object sender, RoutedEventArgs e)
        {
            if (App.ViewModel.selectedRoutes.Count == 1)
            {
                HideStops();
                HideMaps();
            }
            isMapUptoDate = false;
        }

        private void ParentPivot_LoadedPivotItem(object sender, PivotItemEventArgs e)
        {
            if (ParentPivot.SelectedItem == StopsPivot)
            {
                App.ViewModel.cacheAllArrivals();
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
   }
}