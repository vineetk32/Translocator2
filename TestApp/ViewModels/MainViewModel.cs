using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Net;
using System.IO;
using Newtonsoft.Json;

public class AgencyRoot
{
    public AgencyRoot() { data = new List<Agency>(); }
    public List<Agency> data { get; set; }
}

public class Agency
{
    public string long_name { get; set; }
    public string short_name { get; set; }
    //public string url { get; set; }
    public int agency_id { get; set; }
}


public class RouteRoot
{
    public RouteRoot() { data = new Dictionary<int, List<Route>>(); }
    public Dictionary<int, List<Route>> data { get; set; }
}

public class Route
{
    public Route() { /*segments = new List<int>();*/ stops = new List<long>(); }
    //public string description { get; set; }
    public string short_name { get; set; }
    public long route_id { get; set; }
    public string color { get; set; }
    //public List<int> segments;
    public int agency_id { get; set; }
    public string text_color { get; set; }
    public string long_name { get; set; }
    //public string url { get; set; }
    //public bool is_hidden { get; set; }
    //public string type { get; set; }
    public List<long> stops { get; set; }
}

public class StopRoot
{
    public StopRoot() { data = new List<Stop>(); }
    public List<Stop> data { get; set; }
}

public class Stop
{
    public Stop() { agency_ids = new List<int>(); routes = new List<long>(); }
    public string code;
    public List<int> agency_ids;
    //public string location_type;
    ///location is a tuple of float lat,float lng
    public long stop_id;
    public List<long> routes;
    public string name;
}

public class ArrivalRoot
{
    public ArrivalRoot() { data = new List<StopArrival>(); }
    public List<StopArrival> data { get; set; }
}

public class StopArrival
{
    public int agency_id;
    public long stop_id;
    public List<Arrival> arrivals;
}

public class Arrival
{
    public long route_id, vehicle_id;
    public string arrival_at;
}

namespace TestApp
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            this.agencies = new ObservableCollection<AgencyViewModel>();
            this.routes = new ObservableCollection<RouteViewModel>();
            this.stops = new ObservableCollection<StopViewModel>();

            this.routeCache = new Dictionary<long, Route>();
            this.stopCache = new Dictionary<long, Stop>();
            this.arrivalCache = new Dictionary<long, Dictionary<string, string>>();

            this.selectedRoutes = new List<long>();
            this.availableStops = new Dictionary<long, int>();
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<AgencyViewModel> agencies { get; private set; }
        public ObservableCollection<RouteViewModel> routes { get; private set; }
        public ObservableCollection<StopViewModel> stops { get; private set; }

        public Dictionary<long,Route> routeCache;
        public Dictionary<long, Stop> stopCache;
        public Dictionary<long, Dictionary<string, string>> arrivalCache;

        public List<long> selectedRoutes;
        public Dictionary<long, int> availableStops;

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            // Sample data; replace with real data
            //this.Items.Add(new ItemViewModel() { LineOne = "runtime one", LineTwo = "Maecenas praesent accumsan bibendum" });

            String uri = "http://api.transloc.com/1.1/agencies.json";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(new AsyncCallback(ReadCallback), request);

        }

        public void addRoutes(int agencyID)
        {
            String uri = "http://api.transloc.com/1.1/routes.json?agencies=" + agencyID;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(new AsyncCallback(ReadRoutesCallback), request);
        }

        public void addStops(int agencyID)
        {
            String uri = "http://api.transloc.com/1.1/stops.json?agencies=" + agencyID;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(new AsyncCallback(ReadStopsCallback), request);
        }

        public void addArrivals()
        {
            StringBuilder agencies,routes;
            agencies = new StringBuilder();
            routes = new StringBuilder();

            foreach (int routeID in selectedRoutes)
            {
                agencies.Append(routeCache[routeID].agency_id.ToString() + ',');
            }
            String uri = "http://api.transloc.com/1.1/arrival-estimates.json?agencies=" + agencies.ToString() + "&routes=" + string.Join(",", selectedRoutes);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(new AsyncCallback(ReadArrivalsCallback), request);
        }


        public void removeRoutes(int agencyID)
        {
            for (int i = this.routes.Count - 1; i >= 0 ; i-- )
            {
                RouteViewModel item = this.routes[i];
                if (this.routes[i].AgencyID == agencyID)
                {
                    routeCache.Remove(routes[i].RouteID);
                    removeRoute(item.RouteID, item.Stops);

                    Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        this.routes.Remove(item);
                    });
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

        private void ReadCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
            using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
            {
                string resultString = streamReader1.ReadToEnd();
                var agencyroot = JsonConvert.DeserializeObject<AgencyRoot>(resultString);

                //List<AgencyViewModel> retrievedAgencies;
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    foreach (var agency in agencyroot.data)
                    {
                        //Console.WriteLine(agency.short_name + ":" + agency.long_name + '(' + agency.agency_id + ')');

                        //retrievedAgencies.Add(new AgencyViewModel() { AgencyName = agency.long_name });
                        this.agencies.Add(new AgencyViewModel()
                        { 
                            AgencyName = agency.long_name,
                            AgencyShortName = agency.short_name,
                            IsSelected = false,
                            AgencyID = agency.agency_id
                        });
                    }
                    this.IsDataLoaded = true;
                });
                //Deployment.Current.Dispatcher.BeginInvoke(() => this.agencies.);
            }
        }

        private void ReadRoutesCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
            using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
            {
                string resultString = streamReader1.ReadToEnd();
                var routeroot = JsonConvert.DeserializeObject<RouteRoot>(resultString);

                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    foreach (var agency in routeroot.data)
                    {
                        foreach (var route in agency.Value)
                        {
                            this.routes.Add(new RouteViewModel()
                            {
                                RouteName = route.long_name,
                                RouteShortName = route.short_name,
                                IsSelected = false,
                                AgencyID = route.agency_id,
                                RouteID = route.route_id,
                                TextColor = "#" + route.color.ToUpper(),
                                Stops = route.stops
                            });
                            routeCache.Add(route.route_id, route);
                        }
                    }
                    //cleanUpStops();
                    //this.IsDataLoaded = true;
                });
            }
        }

        private void ReadStopsCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
            using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
            {
                string resultString = streamReader1.ReadToEnd();
                var stopsroot = JsonConvert.DeserializeObject<StopRoot>(resultString);
                foreach (var stop in stopsroot.data)
                {
                    if (stopCache.ContainsKey(stop.stop_id) == false)
                    {
                        stopCache.Add(stop.stop_id, stop);
                    }
                }
            }
        }

        private void ReadArrivalsCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
            using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
            {
                string resultString = streamReader1.ReadToEnd();
                var arrivalsroot = JsonConvert.DeserializeObject<ArrivalRoot>(resultString);
                foreach (var stoparrival in arrivalsroot.data)
                {
                    //Dictionary<long, string> arrivalsAtStop = new Dictionary<long, string>();
                  
                    long stopID = stoparrival.stop_id;
                    foreach (var arrival in stoparrival.arrivals)
                    {
                        /* arrivalTime = DateTime.ParseExact(arrival.arrival_at, "o", 
                            System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);*/
                        //DateTime arrivalTime = new DateTime();
                        string arrivalTime = DateTime.Parse(arrival.arrival_at).ToShortTimeString();
                        string routeName = routeCache[arrival.route_id].short_name + " - " + routeCache[arrival.route_id].long_name;

                        if (arrivalCache.ContainsKey(stopID))
                        {
                            if ( (arrivalCache[stopID]).ContainsKey(routeName))
                            {
                                (arrivalCache[stopID])[routeName] += ", " + arrivalTime;
                            }
                            else
                            {
                                (arrivalCache[stopID]).Add(routeName, arrivalTime);
                            }
                        }
                        else
                        {
                            arrivalCache.Add(stopID,new Dictionary<string,string>());
                            (arrivalCache[stopID]).Add(routeName, arrivalTime);
                        }
                    }
                }
            }
            /*StringBuilder contents = new StringBuilder();
            foreach (long StopID in arrivalCache.Keys)
            {
                contents.Append("\n" + StopID.ToString());
                foreach (string RouteName in arrivalCache[StopID].Keys)
                {
                    contents.Append(" " + RouteName.ToString() + ":" + arrivalCache[StopID][RouteName]);
                }
            }*/
            cleanUpStops();
        }


        public void removeRoute(long RouteID, List<long> Stops)
        {
            selectedRoutes.Remove(RouteID);
            foreach (long stop in Stops)
            {
                if (availableStops.ContainsKey(stop))
                {
                    if (availableStops[stop] == 1)
                    {
                        availableStops.Remove(stop);
                        stopCache.Remove(stop);
                    }
                    else
                        availableStops[stop]--;
                }
            }
        }

        public void addRoute(long RouteID, List<long> Stops)
        {
            selectedRoutes.Add(RouteID);
            foreach (long stop in Stops)
            {
                if (availableStops.ContainsKey(stop))
                    availableStops[stop]++;
                else
                    availableStops.Add(stop, 1);
            }
        }

        public void cleanUpStops()
        {
            /*bool flag = false;
            for (int i = this.stops.Count - 1; i >= 0; i--)
            {
                flag = false;
                StopViewModel item = this.stops[i];
                foreach (int routeid in selectedRoutes)
                {
                    if (item.Routes.Contains(routeid))
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag == false)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        //this.stops.Remove(item);
                        availableStops.Remove(item.StopID);
                    });
                }
            }*/
            
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                this.stops.Clear();
            });

            foreach (long stopID in availableStops.Keys)
            {
                Stop currStop = stopCache[stopID];
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    StopViewModel newStop = new StopViewModel()
                    {
                        StopName = currStop.name,
                        StopID = currStop.stop_id,
                        Agencies = currStop.agency_ids,
                        Routes = currStop.routes,
                        StopCode = currStop.code,
                    };
                    if (arrivalCache.ContainsKey(stopID))
                    {
                        newStop.ArrivalEstimates = arrivalCache[stopID];
                    }
                    else
                    {
                        newStop.ArrivalEstimates = new Dictionary<string,string>();
                    }
                    this.stops.Add(newStop);
                });
            }
        }
    }
}