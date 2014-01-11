using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using Translocator;

namespace Translocator
{

    public class AgencyRoot
    {
        public AgencyRoot() { data = new List<Agency>(); }
        public List<Agency> data { get; set; }
    }

    public class Agency : INotifyPropertyChanged
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
        //public string url { get; set; }
        public long agency_id { get; set; }

        private bool _isSelected;

        public string AgencyName
        {
            get
            {
                return long_name;
            }
            set
            {
                if (value != long_name)
                {
                    long_name = value;
                    NotifyPropertyChanged("AgencyName");
                }
            }
        }

        public string AgencyShortName
        {
            get
            {
                return short_name;
            }
            set
            {
                if (value != short_name)
                {
                    short_name = value;
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
                    NotifyPropertyChanged("IsSelected");
                    if (value == true)
                    {
                        App.ViewModel.addAgencyData(agency_id);
                    }
                    else if (value == false)
                    {
                        App.ViewModel.removeAgencyData(agency_id);
                    }
                }
            }
        }

        public void selectAgency()
        {
                _isSelected = true;
                NotifyPropertyChanged("IsSelected");
        }

        public long AgencyID
        {
            get
            {
                return agency_id;
            }
            set
            {
                if (value != agency_id)
                {
                    agency_id = value;
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


    public class AgencyDataHandler
    {
        private void ReadAgencyCallback(IAsyncResult asynchronousResult)
        {
            string resultString = Util.ProcessCallBack(asynchronousResult);
            if (resultString != null)
            {
                var agencyroot = JsonConvert.DeserializeObject<AgencyRoot>(resultString);

                //List<Agency> retrievedAgencies;

                foreach (Agency agency in agencyroot.data)
                {
                    if (App.ViewModel.restoredAgencies.Contains(agency.agency_id))
                    {
                        agency.IsSelected = true;
                    }
                }
                App.ViewModel.restoredAgencies.Clear();
                App.ViewModel.addAgencies(agencyroot.data);
            }
        }

        public void addAgencies()
        {
            String uri = Util.TRANSLOC_URL_BASE_12 + "agencies.json";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(new AsyncCallback(ReadAgencyCallback), request);
        }
    }
}

