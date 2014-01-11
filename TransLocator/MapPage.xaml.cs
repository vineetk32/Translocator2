//#define RELEASE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Device.Location;
using Microsoft.Phone.Controls.Maps;


namespace Translocator
{
    public partial class MapPage : PhoneApplicationPage
    {
        GeoCoordinateWatcher locationWatcher;
        bool centerInitialized;

        public MapPage()
        {
            InitializeComponent();

            locationWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            DataContext = App.ViewModel;


            //myMap.ZoomBarVisibility = Visibility.Visible;
            //myMap.Center = new GeoCoordinate(35.76733, -78.69568);
            //myMap.ZoomLevel = 17.0;
            centerInitialized = false;

        }

        public void locationWatcher_getPosition(object sender, GeoPositionChangedEventArgs<GeoCoordinate> newLoc)
        {
            Pushpin pin = new Pushpin();

            pin.Location = new GeoCoordinate(newLoc.Position.Location.Latitude, newLoc.Position.Location.Longitude);
            //pin.Location = new GeoCoordinate(35.76733,-78.69568);

            pin.Template = (ControlTemplate)App.Current.Resources["MyLocationPin"];
            myMap.Children.Add(pin);
            myMap.SetView(new GeoCoordinate(newLoc.Position.Location.Latitude, newLoc.Position.Location.Longitude), 17.0);
            //myMap.SetView(new GeoCoordinate(35.76733, -78.69568), 16.0);

            locationWatcher.PositionChanged -= new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(locationWatcher_getPosition);
            locationWatcher.Stop();
        }

        private void addSegmentToMap(string segment, string routeColor)
        {
            LocationCollection coordinates = Util.DecodePolylinePoints(segment);
            MapPolyline myPoly = new MapPolyline();

            myPoly.Stroke = Util.stringtoBrush(routeColor);
            myPoly.StrokeThickness = 5;
            myPoly.Opacity = 1;

            myPoly.Locations = coordinates;
            myMap.Children.Add(myPoly);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            foreach (long routeID in App.ViewModel.selectedRoutes)
            {
                foreach (long segmentID in App.ViewModel.routeCache[routeID].segments)
                {
                    addSegmentToMap(App.ViewModel.segmentCache[segmentID], App.ViewModel.routeCache[routeID].color);
                }
            }
            if (App.ViewModel.currVehicles.Count() > 0 && centerInitialized == false)
            {
                myMap.SetView(App.ViewModel.currVehicles[0].VehicleLocation, 15.0);
                centerInitialized = true;
            }
        }

        private void Pushpin_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var currStopPin = sender as Pushpin;
            ContextMenu contextMenu = ContextMenuService.GetContextMenu(currStopPin);
            if (contextMenu.Parent == null)
            {
                contextMenu.IsOpen = true;
            }
        }

        private void MyLocationButton_Click(object sender, EventArgs e)
        {
            locationWatcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(locationWatcher_getPosition);
            if (locationWatcher.Status == GeoPositionStatus.Disabled)
            {
                MessageBox.Show("Cannot access location. Please check settings.");
                return;
            }
            locationWatcher.Start();
        }
    }
}