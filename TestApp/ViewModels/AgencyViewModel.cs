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

namespace TestApp
{
    public class AgencyViewModel : INotifyPropertyChanged
    {
        private string _agencyName,_agencyShortName;
        private bool _isSelected;
        private int _agencyID;

        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string AgencyName
        {
            get
            {
                return _agencyName;
            }
            set
            {
                if (value != _agencyName)
                {
                    _agencyName = value;
                    NotifyPropertyChanged("AgencyName");
                }
            }
        }

        public string AgencyShortName
        {
            get
            {
                return _agencyShortName;
            }
            set
            {
                if (value != _agencyShortName)
                {
                    _agencyShortName = value;
                    NotifyPropertyChanged("AgencyShortName");
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
                        App.ViewModel.addRoutes(this.AgencyID);
                        App.ViewModel.addStops(this.AgencyID);
                    }
                    else if (value == false)
                    {
                        App.ViewModel.removeRoutes(this.AgencyID);
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