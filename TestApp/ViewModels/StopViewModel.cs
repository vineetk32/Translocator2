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

namespace TestApp
{
    public class StopViewModel : INotifyPropertyChanged
    {
        private string _stopName, _stopCode;
        private int _stopID;
        private List<int> _agencies,_routes;
        private Dictionary<string, string> _arrival_estimates;

        public StopViewModel()
        {
            _agencies = new List<int>();
            _routes = new List<int>();
            _arrival_estimates = new Dictionary<string, string>();
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

        public List<int> Routes
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

        public int StopID
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