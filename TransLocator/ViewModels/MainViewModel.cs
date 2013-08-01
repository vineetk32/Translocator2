﻿using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Windows;
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
    //public string text_color { get; set; }
    public string long_name { get; set; }
    //public string url { get; set; }
    public bool is_active { get; set; }
    //public string type { get; set; }
    public List<long> stops { get; set; }
    //segments is actually a list of a tuple of segment_id and direction in 1.2
    public List<long> segments { get; set; }
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

public class SegmentRoot
{
    public SegmentRoot() { data = new Dictionary<long, string>(); }
    public Dictionary<long, string> data;
}

public static class Util
{
    //Credits - StackOverflow (http://goo.gl/gZGIV)

    public static Collection<Double> decodePolyline(string polyline)
    {
        if (polyline == null || polyline == "") return null;

        char[] polylinechars = polyline.ToCharArray();
        int index = 0;
        Collection<Double> points = new Collection<Double>();
        int currentLat = 0;
        int currentLng = 0;
        int next5bits;
        int sum;
        int shifter;

        while (index < polylinechars.Length)
        {
            // calculate next latitude
            sum = 0;
            shifter = 0;
            do
            {
                next5bits = (int)polylinechars[index++] - 63;
                sum |= (next5bits & 31) << shifter;
                shifter += 5;
            } while (next5bits >= 32 && index < polylinechars.Length);

            if (index >= polylinechars.Length)
                break;

            currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

            //calculate next longitude
            sum = 0;
            shifter = 0;
            do
            {
                next5bits = (int)polylinechars[index++] - 63;
                sum |= (next5bits & 31) << shifter;
                shifter += 5;
            } while (next5bits >= 32 && index < polylinechars.Length);

            if (index >= polylinechars.Length && next5bits >= 32)
                break;

            currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

            points.Add(Convert.ToDouble(currentLat) / 100000.0);
            points.Add(Convert.ToDouble(currentLng) / 100000.0);
        }

        return points;
    }
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
            this.selectedRoutesNames = new ObservableCollection<string>();

            this.routeCache = new Dictionary<long, Route>();
            this.stopCache = new Dictionary<long, Stop>();
            this.arrivalCache = new Dictionary<long, Dictionary<long, ArrivalInfo>>();
            this.segmentCache = new Dictionary<long,string>();

            this.selectedRoutes = new List<long>();
            this.selectedAgencies = new List<long>();

            dt = new System.Windows.Threading.DispatcherTimer();
            dt.Interval = new TimeSpan(0, 0, 0, 5);
            dt.Tick += new EventHandler(dt_Tick);
            dt.Start();
        }

        public ObservableCollection<AgencyViewModel> agencies { get; private set; }
        public ObservableCollection<RouteViewModel> routes { get; private set; }
        public ObservableCollection<StopViewModel> stops { get; private set; }
        public ObservableCollection<String> selectedRoutesNames { get; private set; }

        public Dictionary<long, Route> routeCache;
        public Dictionary<long, Stop> stopCache;
        public Dictionary<long, Dictionary<long, ArrivalInfo>> arrivalCache;
        public Dictionary<long, string> segmentCache;

        public List<long> selectedRoutes;
        public List<long> selectedAgencies;

        public System.Windows.Threading.DispatcherTimer dt;

        public void dt_Tick(object sender, EventArgs r)
        {
            if (this.selectedRoutes.Count > 0)
            {
                cacheAllArrivals();
            }
        }

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

            String uri = "http://api.transloc.com/1.2/agencies.json";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(new AsyncCallback(ReadCallback), request);
            this.IsDataLoaded = true;
        }

        public void addRoutes(int agencyID)
        {
            //String uri = "http://api.transloc.com/1.2/routes.json?agencies=" + agencyID;
            //1.1 has easier segment processing.
            String uri = "http://api.transloc.com/1.1/routes.json?agencies=" + agencyID;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(new AsyncCallback(ReadRoutesCallback), request);
        }

        public void addStops(int agencyID)
        {
            String uri = "http://api.transloc.com/1.2/stops.json?agencies=" + agencyID;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(new AsyncCallback(ReadStopsCallback), request);
        }

        public void cacheArrivals(long routeID)
        {
            String uri = "http://api.transloc.com/1.2/arrival-estimates.json?agencies=" + routeCache[routeID].agency_id + "&routes=" + routeID;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(new AsyncCallback(ReadArrivalsCallback), request);
        }

        public void cacheAllArrivals()
        {
            initArrivalCache();
            String uri = "http://api.transloc.com/1.2/arrival-estimates.json?agencies=" + string.Join(",", selectedAgencies) + "&routes=" + string.Join(",", selectedRoutes);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(new AsyncCallback(ReadArrivalsCallback), request);
        }

        public void initArrivalCache()
        {
            //Set the arrival times of all routes at all stops to be --.
            //TODO - be smarter about this.
            arrivalCache.Clear();
            foreach (long currRouteID in selectedRoutes)
            {
                foreach (long currStopID in routeCache[currRouteID].stops)
                {
                    ArrivalInfo arrivalInfo = new ArrivalInfo();
                    arrivalInfo.ArrivalTimes = "--";
                    arrivalInfo.RouteColor = '#' + routeCache[currRouteID].color;
                    arrivalInfo.RouteName = routeCache[currRouteID].short_name + " - " + routeCache[currRouteID].long_name;
                    if (arrivalCache.ContainsKey(currStopID))
                    {
                        arrivalCache[currStopID].Add(currRouteID, arrivalInfo);
                    }
                    else
                    {
                        arrivalCache.Add(currStopID, new Dictionary<long, ArrivalInfo>());
                        arrivalCache[currStopID].Add(currRouteID, arrivalInfo);
                    }
                }
            }
        }

        public void addArrivalsForRoute(string routeName)
        {
            Route selectedRoute;

            int routeIndex = selectedRoutesNames.IndexOf(routeName);
            selectedRoute = routeCache[selectedRoutes[routeIndex]];

            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                this.stops.Clear();
            });

            foreach (long stopID in selectedRoute.stops)
            {

                Stop currStop = stopCache[stopID];
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
                    Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        this.stops.Add(newStop);
                    });
                }

            }
        }


        public void removeRoutes(int agencyID)
        {
            for (int i = this.routes.Count - 1; i >= 0; i--)
            {
                RouteViewModel item = this.routes[i];
                if (this.routes[i].AgencyID == agencyID)
                {
                    removeRoute(item.RouteID, item.Stops);
                    routeCache.Remove(routes[i].RouteID);

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

        private string ProcessCallBack(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
            }
            catch (System.Net.WebException wb)
            {
                MessageBox.Show("Transloc API returned " + wb.Message);
                return null;
            }
            StreamReader streamReader = new StreamReader(response.GetResponseStream());
            return streamReader.ReadToEnd();
        }

        private void ReadCallback(IAsyncResult asynchronousResult)
        {
            string resultString = ProcessCallBack(asynchronousResult);
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
            });
        }

        private void ReadRoutesCallback(IAsyncResult asynchronousResult)
        {
            string resultString = ProcessCallBack(asynchronousResult);
            var routeroot = JsonConvert.DeserializeObject<RouteRoot>(resultString);

            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                foreach (var agency in routeroot.data)
                {
                    foreach (var route in agency.Value)
                    {
                        route.is_active = true;
                        if (route.is_active == true)
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
                            if (routeCache.ContainsKey(route.route_id) == false)
                            {
                                routeCache.Add(route.route_id, route);
                            }
                        }
                    }
                }
            });
        }

        private void ReadStopsCallback(IAsyncResult asynchronousResult)
        {
            string resultString = ProcessCallBack(asynchronousResult);
            var stopsroot = JsonConvert.DeserializeObject<StopRoot>(resultString);
            foreach (var stop in stopsroot.data)
            {
                if (stopCache.ContainsKey(stop.stop_id) == false)
                {
                    stopCache.Add(stop.stop_id, stop);
                }
            }
        }

        private void ReadArrivalsCallback(IAsyncResult asynchronousResult)
        {
            string resultString = ProcessCallBack(asynchronousResult);
            var arrivalsroot = JsonConvert.DeserializeObject<ArrivalRoot>(resultString);
            DateTime currTime = DateTime.Now;

            foreach (var stoparrival in arrivalsroot.data)
            {
                long stopID = stoparrival.stop_id;
                foreach (var arrival in stoparrival.arrivals)
                {
                    DateTime rawArrivalTime = DateTime.Parse(arrival.arrival_at);
                    TimeSpan timeDiff = rawArrivalTime - currTime;

                    //string arrivalTime = DateTime.Parse(arrival.arrival_at).ToShortTimeString();
                    string arrivalTime;
                    if (timeDiff.Minutes < 1)
                    {
                        arrivalTime = "<1 min";
                    }
                    else
                    {
                        arrivalTime = timeDiff.Minutes.ToString() + " mins";
                    }
                    string routeName = routeCache[arrival.route_id].short_name + " - " + routeCache[arrival.route_id].long_name;
                    long routeID = arrival.route_id;

                    if (arrivalCache[stopID][routeID].ArrivalTimes != "--")
                    {
                        (arrivalCache[stopID])[routeID].ArrivalTimes += ", " + arrivalTime;
                    }
                    else
                    {
                        ArrivalInfo arrivalInfo = new ArrivalInfo();
                        arrivalInfo.RouteName = routeName;
                        arrivalInfo.RouteColor = '#' + routeCache[routeID].color;
                        arrivalInfo.ArrivalTimes = arrivalTime;
                        (arrivalCache[stopID])[routeID] = arrivalInfo;
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
            }
            cleanUpStops(); */
            //TODO - remove this bullshit way of doing this, and use await() instead.
            if (selectedRoutesNames.Count > 0 && this.stops.Count == 0)
            {
                string route = selectedRoutesNames[0];
                addArrivalsForRoute(route);
            }
        }


        public void removeRoute(long RouteID, List<long> Stops)
        {
            selectedRoutes.Remove(RouteID);
            string routeName = routeCache[RouteID].short_name + " - " + routeCache[RouteID].long_name;
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                this.selectedRoutesNames.Remove(routeName);
            });
        }

        public void addRoute(long RouteID, List<long> Stops)
        {
            selectedRoutes.Add(RouteID);
            string routeName = routeCache[RouteID].short_name + " - " + routeCache[RouteID].long_name;
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                this.selectedRoutesNames.Add(routeName);
            });
        }

        /* public void cleanUpStops()
        {
        
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                this.stops.Clear();
            });

            foreach (long stopID in availableStops.Keys)
            {
                Stop currStop = stopCache[stopID];
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
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    this.stops.Add(newStop);
                });
            }
        }*/

        public bool drawMapInfo()
        {
            //TODO;
            return true;
        }

        public void addSegments()
        {
            String uri = "http://api.transloc.com/1.2/segments.json?agencies=" + string.Join(",", selectedAgencies) + "&routes=" + string.Join(",", selectedRoutes);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(new AsyncCallback(ReadSegmentsCallback), request);
        }


        private void ReadSegmentsCallback(IAsyncResult asynchronousResult)
        {
            string resultString = ProcessCallBack(asynchronousResult);
            var segmentsroot = JsonConvert.DeserializeObject<SegmentRoot>(resultString);
            foreach (var segment in segmentsroot.data.Keys)
            {
                if (segmentCache.ContainsKey(segment) == false)
                {
                    segmentCache.Add(segment, segmentsroot.data[segment]);
                }
            }
        }

    }
}