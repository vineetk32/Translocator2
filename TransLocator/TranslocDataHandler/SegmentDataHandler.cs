using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Device.Location;
using System.Net;
using Translocator;

namespace Translocator
{
    public class SegmentRoot
    {
        public SegmentRoot() { data = new Dictionary<long, string>(); }
        public Dictionary<long, string> data;
    }

    public class SegmentDataHandler
    {
        private void ReadSegmentsCallback(IAsyncResult asynchronousResult)
        {
            string resultString = Util.ProcessCallBack(asynchronousResult);
            if (resultString != null)
            {
                var segmentsroot = JsonConvert.DeserializeObject<SegmentRoot>(resultString);

                Dictionary<long, string> segmentCacheRef = App.ViewModel.segmentCache;
                foreach (var segment in segmentsroot.data.Keys)
                {
                    if (segmentCacheRef.ContainsKey(segment) == false)
                    {
                        segmentCacheRef.Add(segment, segmentsroot.data[segment]);
                    }
                }
            }
        }

        public void addSegments(long AgencyID, long RouteID)
        {
            String uri = Util.TRANSLOC_URL_BASE_12 + "segments.json?agencies=" + AgencyID.ToString() + "&routes=" + RouteID.ToString();
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(new AsyncCallback(ReadSegmentsCallback), request);
        }
    }
}