using System;
using System.IO;
using System.Net;
using System.Windows;

namespace Translocator
{
    public class Location
    {
        public double lat;
        public double lng;
    }

    class Util
    {
        public const string TRANSLOC_URL_BASE_12 = "http://api.transloc.com/1.2/";
        public const string TRANSLOC_URL_BASE_11 = "http://api.transloc.com/1.1/";

        public static string ProcessCallBack(IAsyncResult asynchronousResult)
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

    }
}