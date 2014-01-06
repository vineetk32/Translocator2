using System;
using System.IO;
using System.Net;
using System.Windows;
using Microsoft.Phone.Controls.Maps;
using System.Windows.Media;
using System.Device.Location;
using System.Windows.Data;
using System.Globalization;

namespace Translocator
{
    public class Location
    {
        public double lat;
        public double lng;
    }

    //Taken from Nokia developer
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            var isVisible = (bool)value;

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visiblity = (Visibility)value;

            return visiblity == Visibility.Visible;
        }
    }

    public class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            var count = (int) value;

            if (count == 0)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visiblity = (Visibility)value;

            return visiblity == Visibility.Visible;
        }
    }
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            var count = (String)value;

            if (count.Length > 0 )
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visiblity = (Visibility)value;

            return visiblity == Visibility.Visible;
        }
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

        public static SolidColorBrush stringtoBrush(string colour)
        {
            return new SolidColorBrush(
            Color.FromArgb(
                Convert.ToByte("FF", 16),
                Convert.ToByte(colour.Substring(0, 2), 16),
                Convert.ToByte(colour.Substring(2, 2), 16),
                Convert.ToByte(colour.Substring(4, 2), 16)
            )
            );
        }


        public static LocationCollection DecodePolylinePoints(string encodedPoints)
        {
            if (encodedPoints == null || encodedPoints == "") return null;
            LocationCollection poly = new LocationCollection();
            char[] polylinechars = encodedPoints.ToCharArray();
            int index = 0;

            int currentLat = 0;
            int currentLng = 0;
            int next5bits;
            int sum;
            int shifter;

            try
            {
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
                    GeoCoordinate p = new GeoCoordinate();
                    p.Latitude = Convert.ToDouble(currentLat) / 100000.0;
                    p.Longitude = Convert.ToDouble(currentLng) / 100000.0;

                    poly.Add(p);
                }
            }
            catch (Exception ex)
            {
                // log it
            }
            return poly;
        }


    }
}