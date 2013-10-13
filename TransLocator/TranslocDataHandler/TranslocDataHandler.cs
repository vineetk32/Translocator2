using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Device.Location;

namespace Translocator
{
    public class TranslocDataHandler
    {
        private AgencyDataHandler agencyHandler;
        private RouteDataHandler routeHandler;
        private StopDatahandler stopHandler;
        private ArrivalDataHandler arrivalHandler;
        private SegmentDataHandler segmentHandler;
        private VehicleDataHandler vehicleHandler;

        public TranslocDataHandler()
        {
            agencyHandler = new AgencyDataHandler();
            routeHandler = new RouteDataHandler();
            stopHandler = new StopDatahandler();
            arrivalHandler = new ArrivalDataHandler();
            segmentHandler = new SegmentDataHandler();
            vehicleHandler = new VehicleDataHandler();
        }

        public void addAgencies()
        {
            agencyHandler.addAgencies();
        }

        public void addRoutes(long agencyID)
        {
            routeHandler.addRoutes(agencyID);
        }

        public void addStops(long agencyID)
        {
            stopHandler.addStops(agencyID);
        }

        public void addSegments(long agencyID, long routeID)
        {
            segmentHandler.addSegments(agencyID, routeID);
        }

        public void addVehicles(long agencyID, long routeID)
        {
            vehicleHandler.addVehicles(agencyID, routeID);
        }

        public void cacheAllArrivals()
        {
            arrivalHandler.cacheAllArrivals();
        }

        public void cacheAllVehicles()
        {
            vehicleHandler.cacheAllVehicles();
        }

    }


}
