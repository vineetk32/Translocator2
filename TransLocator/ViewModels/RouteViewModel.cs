using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace Translocator
{
    public class RouteViewModel : INotifyPropertyChanged
    {
        private string _routeName,_routeShortName;
        private bool _isSelected;
        private int _agencyID;
        private long _routeID;
        private string _text_color;
        private List<long> _stops;

        public RouteViewModel()
        {
            _stops = new List<long>();
        }

        public List<long> Stops
        {
            get
            {
                return _stops;
            }
            set
            {
                if (value != _stops)
                {
                    _stops = value;
                }
            }
        }

        public string RouteName
        {
            get
            {
                return _routeName;
            }
            set
            {
                if (value != _routeName)
                {
                    _routeName = value;
                    NotifyPropertyChanged("RouteName");
                }
            }
        }

        public long RouteID
        {
            get
            {
                return _routeID;
            }
            set
            {
                if (value != _routeID)
                {
                    _routeID = value;
                }
            }
        }



        public string RouteShortName
        {
            get
            {
                return _routeShortName;
            }
            set
            {
                if (value != _routeShortName)
                {
                    _routeShortName = value;
                    NotifyPropertyChanged("RouteShortName");
                }
            }
        }

        public string TextColor
        {
            get
            {
                return _text_color;
            }
            set
            {
                if (value != _text_color)
                {
                    _text_color = value;
                    NotifyPropertyChanged("TextColor");
                }
            }
        }


        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    if (value == true)
                    {
                        App.ViewModel.addRoute(this.RouteID,this.Stops);
                        //App.ViewModel.cacheArrivals(this.RouteID);
                        App.ViewModel.addSegments(this.AgencyID, this.RouteID);
                        App.ViewModel.addVehicles(this.AgencyID, this.RouteID);

                    }
                    else
                    {
                        App.ViewModel.removeRoute(this.RouteID, this.Stops);
                    }
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }

        public int AgencyID
        {
            get
            {
                return _agencyID;
            }
            set
            {
                if (value != _agencyID)
                {
                    _agencyID = value;
                    NotifyPropertyChanged("AgencyID");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}