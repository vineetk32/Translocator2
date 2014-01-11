using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Device.Location;
using System.Net;
using Translocator;

namespace Translocator
{
    public class ArrivalInfo
    {
        private string routeShortName,routeName, arrivalTimes, routeColor;
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

        public string RouteShortName
        {
            get
            {
                return routeShortName;
            }
            set
            {
                if (value != routeShortName)
                    routeShortName = value;
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

    public class StopRoot
    {
        public StopRoot() { data = new List<Stop>(); }
        public List<Stop> data { get; set; }
    }

    public class Stop : INotifyPropertyChanged
    {

        public string code;
        public List<long> agency_ids;
        //public string location_type;
        public Location location;
        public long stop_id;
        public List<long> routes;
        public string name;
        private Dictionary<long, ArrivalInfo> _arrival_estimates;
        private bool _isVisible;

        public Stop()
        {
            agency_ids = new List<long>();
            routes = new List<long>();
            _arrival_estimates = new Dictionary<long, ArrivalInfo>();
            location = new Location();
            _isVisible = true;
        }

        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                if (value != _isVisible)
                {
                    _isVisible = value;
                    NotifyPropertyChanged("IsVisible");
                }
            }
        }

        public List<long> AgencyIDs
        {
            get
            {
                return agency_ids;
            }
            set
            {
                if (value != agency_ids)
                {
                    agency_ids = value;
                }
            }
        }

        public GeoCoordinate StopLocation
        {
            get
            {
                return new GeoCoordinate(location.lat, location.lng);
            }
            set
            {
                NotifyPropertyChanged("StopLocation");
            }
        }


        public List<long> RouteIDs
        {
            get
            {
                return routes;
            }
            set
            {
                if (value != routes)
                {
                    routes = value;
                }
            }
        }

        public string StopName
        {
            get
            {
                return name;
            }
            set
            {
                if (value != name)
                {
                    name = value;
                    NotifyPropertyChanged("StopName");
                }
            }
        }

        public long StopID
        {
            get
            {
                return stop_id;
            }
            set
            {
                if (value != stop_id)
                {
                    stop_id = value;
                }
            }
        }

        public string StopCode
        {
            get
            {
                return code;
            }
            set
            {
                if (value != code)
                {
                    code = value;
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

    public class StopDatahandler
    {
        public void addStops(long agencyID)
        {
            String uri = Util.TRANSLOC_URL_BASE_12 + "/stops.json?agencies=" + agencyID;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(new AsyncCallback(ReadStopsCallback), request);
        }

        private void ReadStopsCallback(IAsyncResult asynchronousResult)
        {
            string resultString = Util.ProcessCallBack(asynchronousResult);
            if (resultString != null)
            {
                var stopsroot = JsonConvert.DeserializeObject<StopRoot>(resultString);
                App.ViewModel.addStops(stopsroot.data);
            }
        }
    }
}