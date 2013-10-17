using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using System.Device.Location;

namespace Translocator
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        GeoCoordinateWatcher locationWatcher;
        bool isMapUptoDate;

        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            ParentPivot.Items.Remove(RoutesPivot);
            ParentPivot.Items.Remove(StopsPivot);
            ParentPivot.Items.Remove(MapsPivot);

            isMapUptoDate = false;
            /*locationWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            locationWatcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(locationWatcher_getPosition);
            locationWatcher.Start();*/

            myMap.ZoomBarVisibility = Visibility.Visible;
            myMap.Center = new GeoCoordinate(35.76733, -78.69568);
            myMap.ZoomLevel = 17.0;

            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }
        
        public void locationWatcher_getPosition(object sender, GeoPositionChangedEventArgs<GeoCoordinate> newLoc)
        {
            Pushpin pin = new Pushpin();
            //pin.Location = new GeoCoordinate(newLoc.Position.Location.Latitude, newLoc.Position.Location.Longitude);
            //pin.Location = new GeoCoordinate(35.76733,-78.69568);
            pin.Template = (ControlTemplate)App.Current.Resources["MyLocationPin"];
            myMap.Children.Add(pin);
            myMap.SetView(new GeoCoordinate(newLoc.Position.Location.Latitude, newLoc.Position.Location.Longitude), 17.0);
            //myMap.SetView(new GeoCoordinate(35.76733, -78.69568), 16.0);
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
            if (ParentPivot.SelectedItem == StopsPivot)
            {
                string routeName = (sender as ListPicker).SelectedItem as string;
                if (routeName != null)
                    App.ViewModel.addArrivalsForRoute(routeName);
            }
        }

        public void ShowMaps()
        {
            //TODO - use a bool instead
            if (ParentPivot.Items.Contains(MapsPivot) == false)
            {
                ParentPivot.Items.Add(MapsPivot);
            }
        }

        public void HideMaps()
        {
            //TODO - use a bool instead
            if (ParentPivot.Items.Contains(MapsPivot) == true)
            {
                ParentPivot.Items.Remove(MapsPivot);
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



        private void addSegmentToMap(string segment,string routeColor)
        {
            LocationCollection coordinates = Util.DecodePolylinePoints(segment);
            MapPolyline myPoly = new MapPolyline();

            myPoly.Stroke = Util.stringtoBrush(routeColor);
            myPoly.StrokeThickness = 5;
            myPoly.Opacity = 1;

            myPoly.Locations = coordinates;
            myMap.Children.Add(myPoly);
        }

        private void ParentPivot_LoadedPivotItem(object sender, PivotItemEventArgs e)
        {
            if (ParentPivot.SelectedItem == StopsPivot)
            {
                App.ViewModel.cacheAllArrivals();
            }
            else if (ParentPivot.SelectedItem == MapsPivot)
            {
                if (isMapUptoDate == false)
                {
                    foreach (long routeID in App.ViewModel.selectedRoutes)
                    {
                        foreach (long segmentID in App.ViewModel.routeCache[routeID].segments)
                        {
                            addSegmentToMap(App.ViewModel.segmentCache[segmentID], App.ViewModel.routeCache[routeID].color);
                        }
                    }
                    isMapUptoDate = true;
                }
            }

        }
   }

}