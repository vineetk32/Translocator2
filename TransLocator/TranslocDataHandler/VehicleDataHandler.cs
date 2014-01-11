using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Device.Location;
using System.Net;
using Translocator;

namespace Translocator
{


    public class Vehicle : INotifyPropertyChanged
    {
        public Vehicle() { arrival_estimates = new List<ArrivalEstimate>(); }
        public long vehicle_id;
        public long route_id;

        public List<ArrivalEstimate> arrival_estimates;
        public Location location;
        public string _color, _routeShortName;

        public GeoCoordinate VehicleLocation
        {
            get
            {
                return new GeoCoordinate(location.lat, location.lng);
            }
            set
            {
                NotifyPropertyChanged("VehicleLocation");
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

        public String RouteShortName
        {
            get
            {
                return _routeShortName;
            }
            set
            {
                if (_routeShortName != value)
                {
                    _routeShortName = value;
                    NotifyPropertyChanged("RouteShortName");
                }
            }

        }

        public String Color
        {
            get
            {
                return _color;
            }
            set
            {
                if (_color != value)
                {
                    _color = value;
                    NotifyPropertyChanged("Color");
                }
            }
        }

    }

    public class VehicleRoot
    {
        public VehicleRoot() { data = new Dictionary<long, List<Vehicle>>(); }
        public Dictionary<long, List<Vehicle>> data;
    }

    public class VehicleDataHandler
    {

        public void cacheAllVehicles()
        {
            String uri = Util.TRANSLOC_URL_BASE_12 + "vehicles.json?agencies=" + string.Join(",", App.ViewModel.selectedAgencies) + "&routes=" + string.Join(",", App.ViewModel.selectedRoutes);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(new AsyncCallback(ReadVehiclesCallback), request);
        }

        public void addVehicles(long AgencyID, long RouteID)
        {
            String uri = Util.TRANSLOC_URL_BASE_12 +  "vehicles.json?agencies=" + AgencyID.ToString() + "&routes=" + RouteID.ToString();
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(new AsyncCallback(ReadVehiclesCallback), request);
        }


        private void ReadVehiclesCallback(IAsyncResult asynchronousResult)
        {
            string resultString = Util.ProcessCallBack(asynchronousResult);
            if (resultString != null)
            {
                var vehiclesroot = JsonConvert.DeserializeObject<VehicleRoot>(resultString);
                Dictionary<long, Route> routeCacheRef = App.ViewModel.routeCache;

                List<Vehicle> retrievedVehicles = new List<Vehicle>();

                foreach (var agencyID in vehiclesroot.data.Keys)
                {
                    foreach (var vehicle in vehiclesroot.data[agencyID])
                    {
                        vehicle.Color = '#' + routeCacheRef[vehicle.route_id].color;
                        vehicle.RouteShortName = routeCacheRef[vehicle.route_id].short_name;

                        /*if (selectedRoutes.Contains(vehicle.route_id))
                        {}*/
                        retrievedVehicles.Add(vehicle);
                    }
                }
                App.ViewModel.addVehicles(retrievedVehicles);
            }
        }
    }
}