using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows;
using System.Collections.ObjectModel;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Device.Location;


namespace Translocator
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            this.agencies = new ObservableCollection<Agency>();
            this.routes = new ObservableCollection<Route>();
            this.stops = new ObservableCollection<Stop>();
            this.selectedRoutesNames = new ObservableCollection<string>();
            this.currVehicles = new ObservableCollection<Vehicle>();

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

            dataHandler = new TranslocDataHandler();
        }

        public ObservableCollection<Agency> agencies { get; private set; }
        public ObservableCollection<Route> routes { get; private set; }
        public ObservableCollection<Stop> stops { get; private set; }
        public ObservableCollection<String> selectedRoutesNames { get; private set; }
        public ObservableCollection<Vehicle> currVehicles { get; private set; }

        public Dictionary<long, Route> routeCache;
        public Dictionary<long, Stop> stopCache;
        public Dictionary<long, Dictionary<long, ArrivalInfo>> arrivalCache;
        public Dictionary<long, string> segmentCache;

        public List<long> selectedRoutes;
        public List<long> selectedAgencies;

        private TranslocDataHandler dataHandler;

        public System.Windows.Threading.DispatcherTimer dt;

        public void dt_Tick(object sender, EventArgs r)
        {
            if (this.selectedRoutes.Count > 0)
            {
                cacheAllArrivals();
                dataHandler.cacheAllVehicles();
            }
        }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        internal void LoadData()
        {
            dataHandler.addAgencies();
            this.IsDataLoaded = true;
        }

        public void addAgencies(List<Agency> retrievedAgencies)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                foreach (Agency agency in retrievedAgencies)
                {
                    this.agencies.Add(agency);
                }
            });

        }

        public void addAgencyData(long agencyID)
        {
            selectedAgencies.Add(agencyID);
            dataHandler.addRoutes(agencyID);
            dataHandler.addStops(agencyID);
        }

        public void removeAgencyData(long agencyID)
        {
            selectedAgencies.Remove(agencyID);
            removeRoutes(agencyID);
        }

        public void addRoutes(List<Route> retrievedRoutes)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                foreach (Route route in retrievedRoutes)
                {
                    this.routes.Add(route);
                    if (routeCache.ContainsKey(route.route_id) == false)
                    {
                        routeCache.Add(route.route_id, route);
                    }
                }
            });
        }

        public void addRouteData(long agencyID, long routeID)
        {
            selectedRoutes.Add(routeID);

            string routeName = routeCache[routeID].short_name + " - " + routeCache[routeID].long_name;
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                this.selectedRoutesNames.Add(routeName);
            });

            //App.ViewModel.cacheArrivals(this.RouteID);
            dataHandler.addSegments(agencyID, routeID);
            dataHandler.addVehicles(agencyID, routeID);
        }

        public void removeRouteData(long agencyID, long routeID)
        {
            selectedRoutes.Remove(routeID);
            string routeName = routeCache[routeID].short_name + " - " + routeCache[routeID].long_name;
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                this.selectedRoutesNames.Remove(routeName);
            });

        }

        public void removeRoutes(long agencyID)
        {
            for (int i = this.routes.Count - 1; i >= 0; i--)
            {
                Route item = this.routes[i];
                if (this.routes[i].AgencyID == agencyID)
                {
                    removeRouteData(item.agency_id,item.route_id);
                    routeCache.Remove(routes[i].route_id);

                    Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        this.routes.Remove(item);
                    });
                }
            }
        }



        public void addStops(List<Stop> retrievedStops)
        {
            foreach (Stop stop in retrievedStops)
            {
                if (stopCache.ContainsKey(stop.stop_id) == false)
                {
                    stopCache.Add(stop.stop_id, stop);
                }
            }
        }

        public void addVehicles(List<Vehicle> retrievedVehicles)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                this.currVehicles.Clear();
            });

            foreach (Vehicle vehicle in retrievedVehicles)
            {

                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    this.currVehicles.Add(vehicle);
                });

            }
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
                if (arrivalCache.ContainsKey(stopID))
                {
                    currStop.ArrivalEstimates = arrivalCache[stopID];
                    Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        this.stops.Add(currStop);
                    });
                }

            }
        }

        public void cacheAllArrivals()
        {
            initArrivalCache();
            dataHandler.cacheAllArrivals();
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

    }
}