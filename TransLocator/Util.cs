using System;
using System.IO;
using System.Net;
using System.Windows;
using Microsoft.Phone.Controls.Maps;
using System.Windows.Media;
using System.Device.Location;
using System.Windows.Data;
using System.Globalization;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Tasks;
using System.Text;
using Microsoft.Phone.Info;

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
        const string errFilename = "ErrLocator.txt";

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
                Deployment.Current.Dispatcher.BeginInvoke(() =>  MessageBox.Show("Error reaching transloc! Error: " + wb.Message));
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

 
        internal static void ReportException(Exception ex)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    SafeDeleteFile(store);
                    using (TextWriter output = new StreamWriter(store.CreateFile(errFilename)))
                    {
                        output.WriteLine(ex.Message);
                        output.WriteLine(ex.StackTrace);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
 
        internal static void CheckForPreviousException()
        {
            try
            {
                string contents = null;
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(errFilename))
                    {
                        using (TextReader reader = new StreamReader(store.OpenFile(errFilename, FileMode.Open, FileAccess.Read, FileShare.None)))
                        {
                            contents = reader.ReadToEnd();
                        }
                        SafeDeleteFile(store);
                    }
                }
                if (contents != null)
                {
                    if (MessageBox.Show("A problem occurred the last time you ran this application. Would you like to send an email to report it?", "Problem Report", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        EmailComposeTask email = new EmailComposeTask();
                        email.To = "vineetkrishnan@hotmail..com";
                        email.Subject = "Translocator auto-generated problem report";

                        StringBuilder deviceInfo = new StringBuilder();

                        long ApplicationMemoryUsage = DeviceStatus.ApplicationCurrentMemoryUsage;
                        string FirmwareVersion = DeviceStatus.DeviceFirmwareVersion;
                        string HardwareVersion = DeviceStatus.DeviceHardwareVersion;
                        string Manufacturer = DeviceStatus.DeviceManufacturer;
                        string DeviceName = DeviceStatus.DeviceName;
                        long TotalMemory = DeviceStatus.DeviceTotalMemory;
                        string OSVersion = Environment.OSVersion.Version.ToString();

                        deviceInfo.AppendLine("\n\n\n");
                        deviceInfo.AppendLine("Memory Usage :" + ApplicationMemoryUsage);
                        deviceInfo.AppendLine("Firmware Version :" + FirmwareVersion);
                        deviceInfo.AppendLine("Hardware Version :" + HardwareVersion);
                        deviceInfo.AppendLine("Manufacturer :" + Manufacturer);
                        deviceInfo.AppendLine("Device Name :" + DeviceName);
                        deviceInfo.AppendLine("Total Memory :" + TotalMemory);
                        deviceInfo.AppendLine("Operating System: Windows Phone " + OSVersion.ToString());

                        email.Body = contents;
                        email.Body += deviceInfo.ToString();

                        SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication());
                        email.Show();
                    }
                }
            }
            catch (Exception)
            { }
            finally
            {
                SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication());
            }
        }
 
        private static void SafeDeleteFile(IsolatedStorageFile store)
        {
            try
            {
                store.DeleteFile(errFilename);
            }
            catch (Exception ex)
            {}
        }
    }
}