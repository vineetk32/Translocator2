using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using Translocator;

namespace Translocator
{
    public class ArrivalEstimate
    {
        public long route_id;
        public string arrival_at;
        public long stop_id;
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

    public class ArrivalDataHandler
    {

        private void ReadArrivalCallback(IAsyncResult asynchronousResult)
        {
            string resultString = Util.ProcessCallBack(asynchronousResult);
            var arrivalsroot = JsonConvert.DeserializeObject<ArrivalRoot>(resultString);
            DateTime currTime = DateTime.Now;

            Dictionary<long,Route> routeCacheRef = App.ViewModel.routeCache;
            Dictionary<long,Dictionary<long,ArrivalInfo>> arrivalCacheRef = App.ViewModel.arrivalCache;

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
                        arrivalTime = "< 1 min";
                    }
                    else
                    {
                        arrivalTime = timeDiff.Minutes.ToString() + " mins";
                    }
                    //string routeName = routeCacheRef[arrival.route_id].short_name + " - " + routeCacheRef[arrival.route_id].long_name;
                    long routeID = arrival.route_id;

                    if (arrivalCacheRef[stopID][routeID].ArrivalTimes != "--")
                    {
                        (arrivalCacheRef[stopID])[routeID].ArrivalTimes += ", " + arrivalTime;
                    }
                    else
                    {
                        ArrivalInfo arrivalInfo = new ArrivalInfo();
                        arrivalInfo.RouteName = routeCacheRef[routeID].long_name;
                        arrivalInfo.RouteShortName = routeCacheRef[routeID].short_name;
                        arrivalInfo.RouteColor = '#' + routeCacheRef[routeID].color;
                        arrivalInfo.ArrivalTimes = arrivalTime;
                        (arrivalCacheRef[stopID])[routeID] = arrivalInfo;
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
            cleanUpStops(); 
            //TODO - remove this bullshit way of doing this, and use await() instead.
            if (App.ViewModel.selectedRoutesNames.Count > 0 && App.ViewModel.stops.Count == 0)
            {
                string route = App.ViewModel.selectedRoutesNames[0];
                App.ViewModel.addArrivalsForRoute(route);
            }*/
            App.ViewModel.updateStops();
        }

        public void cacheArrivals(long agencyID,long routeID)
        {
            String uri = Util.TRANSLOC_URL_BASE_12 +  "arrival-estimates.json?agencies=" + agencyID + "&routes=" + routeID;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(new AsyncCallback(ReadArrivalCallback), request);
        }

        public void cacheAllArrivals()
        {
            String uri = Util.TRANSLOC_URL_BASE_12 +  "arrival-estimates.json?agencies=" + string.Join(",", App.ViewModel.selectedAgencies) + "&routes=" + string.Join(",", App.ViewModel.selectedRoutes);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(new AsyncCallback(ReadArrivalCallback), request);
        }



    }
}