﻿using System;
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

namespace TestApp
{
    public class ArrivalInfo
    {
        private string routeName, arrivalTimes, routeColor;
        //private List<DateTime> rawArrivalTimes;

        public string RouteName
        {
            get
            {
                return routeName;
            }
            set
            {
                if (value != routeName)
                    routeName = value;
            }
        }

        public string ArrivalTimes
        {
            get
            {
                return arrivalTimes;
            }
            set
            {
                if (value != arrivalTimes)
                    arrivalTimes = value;
            }
        }

        public string RouteColor
        {
            get
            {
                return routeColor;
            }
            set
            {
                if (value != routeColor)
                    routeColor = value;
            }
        }

    }

    public class StopViewModel : INotifyPropertyChanged
    {
        private string _stopName, _stopCode;
        private long _stopID;
        private List<int> _agencies;
        private List<long> _routes;
        private Dictionary<long,ArrivalInfo> _arrival_estimates;

        public StopViewModel()
        {
            _agencies = new List<int>();
            _routes = new List<long>();
            _arrival_estimates = new Dictionary<long, ArrivalInfo>();
        }

        public List<int> Agencies
        {
            get
            {
                return _agencies;
            }
            set
            {
                if (value != _agencies)
                {
                    _agencies = value;
                }
            }
        }

        public List<long> Routes
        {
            get
            {
                return _routes;
            }
            set
            {
                if (value != _routes)
                {
                    _routes = value;
                }
            }
        }

        public string StopName
        {
            get
            {
                return _stopName;
            }
            set
            {
                if (value != _stopName)
                {
                    _stopName = value;
                    NotifyPropertyChanged("StopName");
                }
            }
        }

        public long StopID
        {
            get
            {
                return _stopID;
            }
            set
            {
                if (value != _stopID)
                {
                    _stopID = value;
                }
            }
        }

        public string StopCode
        {
            get
            {
                return _stopCode;
            }
            set
            {
                if (value != _stopCode)
                {
                    _stopCode = value;
                }
            }
        }

        public Dictionary<long, ArrivalInfo> ArrivalEstimates
        {
            get
            {
                return _arrival_estimates;
            }
            set
            {
                if (value != _arrival_estimates)
                {
                    _arrival_estimates = value;
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