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
        public MapPage()
        {
            InitializeComponent();

            /*locationWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            locationWatcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(locationWatcher_getPosition);
            locationWatcher.Start();*/

            DataContext = App.ViewModel;


            myMap.ZoomBarVisibility = Visibility.Visible;
            myMap.Center = new GeoCoordinate(35.76733, -78.69568);
            myMap.ZoomLevel = 17.0;

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

        private void MapPage_GotFocus(object sender, RoutedEventArgs e)
        {
            foreach (long routeID in App.ViewModel.selectedRoutes)
            {
                foreach (long segmentID in App.ViewModel.routeCache[routeID].segments)
                {
                    addSegmentToMap(App.ViewModel.segmentCache[segmentID], App.ViewModel.routeCache[routeID].color);
                }
            }
        }
    }
}