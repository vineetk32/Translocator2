using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace TestApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            ParentPivot.Items.Remove(RoutesPivot);
            ParentPivot.Items.Remove(StopsPivot);

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

        private void ListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ParentPivot.SelectedItem == StopsPivot)
            {
                string routeName = (sender as ListPicker).SelectedItem as string;
                if (routeName != null)
                    App.ViewModel.addArrivalsForRoute(routeName);
            }
        }

        private void ParentPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ParentPivot.SelectedItem == StopsPivot)
            {
                App.ViewModel.cacheAllArrivals();
            }
        }

        public void ShowRoutes()
        {
            //TODO - use a bool instead
            if (ParentPivot.Items.Contains(RoutesPivot) == false)
            {
                ParentPivot.Items.Add(RoutesPivot);
            }
        }

        public void HideRoutes()
        {
            //TODO - use a bool instead
            if (ParentPivot.Items.Contains(RoutesPivot) == true)
            {
                ParentPivot.Items.Remove(RoutesPivot);
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
            }
        }

        private void chkRoute_Unchecked(object sender, RoutedEventArgs e)
        {
            if (App.ViewModel.selectedRoutes.Count == 1)
            {
                HideStops();
            }
        }



    }
}