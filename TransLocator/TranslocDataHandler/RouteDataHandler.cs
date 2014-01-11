using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using Translocator;

namespace Translocator
{
    public class RouteRoot
    {
        public RouteRoot() { data = new Dictionary<int, List<Route>>(); }
        public Dictionary<int, List<Route>> data { get; set; }
    }

    public class Route : INotifyPropertyChanged
    {
        public Route() { /*segments = new List<int>();*/ stops = new List<long>(); }
        //public string description { get; set; }
        public string short_name { get; set; }
        public long route_id { get; set; }
        public string color { get; set; }
        //public List<int> segments;
        public int agency_id { get; set; }
        //public string text_color { get; set; }
        public string long_name { get; set; }
        //public string url { get; set; }
        public bool is_active { get; set; }
        //public string type { get; set; }
        public List<long> stops { get; set; }
        //segments is actually a list of a tuple of segment_id and direction in 1.2
        public List<long> segments { get; set; }
        private bool _isSelected;

        public List<long> Stops
        {
            get
            {
                return stops;
            }
            set
            {
                if (value != stops)
                {
                    stops = value;
                }
            }
        }

        public string RouteName
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
                    NotifyPropertyChanged("RouteName");
                }
            }
        }

        public string RouteShortName
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
                    NotifyPropertyChanged("RouteShortName");
                }
            }
        }

        public string TextColor
        {
            get
            {
                return '#' + color;
            }
            set
            {
                if (value != color)
                {
                    color = value;
                    NotifyPropertyChanged("TextColor");
                }
            }
        }

        public void selectRoute()
        {
            _isSelected = true;
            NotifyPropertyChanged("IsSelected");
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
                        App.ViewModel.addRouteData(agency_id,route_id);
                    }
                    else
                    {
                        App.ViewModel.removeRouteData(agency_id,route_id);
                    }
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }

        public int AgencyID
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

    public class RouteDataHandler
    {

        private void ReadRoutesCallback(IAsyncResult asynchronousResult)
        {
            string resultString = Util.ProcessCallBack(asynchronousResult);
            if (resultString != null)
            {
                var routeroot = JsonConvert.DeserializeObject<RouteRoot>(resultString);

                List<Route> retrievedRoutes = new List<Route>();

                foreach (var agency in routeroot.data)
                {
                    foreach (var route in agency.Value)
                    {
                        route.is_active = true;
                        if (route.is_active == true)
                        {
                            retrievedRoutes.Add(route);
                        }
                    }
                }
                App.ViewModel.addRoutes(retrievedRoutes);
            }
        }

        public void addRoutes(long agencyID)
        {
            //String uri = "http://api.transloc.com/1.2/routes.json?agencies=" + agencyID;
            //1.1 has easier segment processing.
            String uri = Util.TRANSLOC_URL_BASE_11 + "routes.json?agencies=" + agencyID;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(new AsyncCallback(ReadRoutesCallback), request);
        }

    }
}