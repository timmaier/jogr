using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Specialized;
using System.Net;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.GoogleMaps;

using Plugin.Geolocator;
using Newtonsoft.Json;

namespace jogr
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        //Get a reference to the map component
        public Map map;

        //Constructor
        public MapPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            //Reference the map component
            map = (Map)MyMap;

            Position myPos = GetLocation();
            Pin myLocation = new Pin
            {
                Type = PinType.Generic,
                Position = myPos,
                Label = "me",
                Address = "Brisbane"
            };
            map.Pins.Add(myLocation);

            MapSpan mySpan = new MapSpan(myPos, 5, 5);
            map.MoveToRegion(mySpan);
            //Testing receiving a default route
            Position endpos = new Position(myPos.Latitude + 0.05, myPos.Longitude + 0.05);
            //requestRoute(myPos, endpos);

            //This Function is not currently working
            //GoToLocationOnMap();            
        }

        async void GoToLocationOnMap()
        {
            Plugin.Geolocator.Abstractions.Position position = null;

            try
            {
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = 100;
                position = await locator.GetPositionAsync(TimeSpan.FromSeconds(3));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("There was an error: " + ex);
            }
            //--THERE IS AN ERROR WITH THIS LINE, 'position' IS NOT REFERENCING AN INSTANCE OF AN OBJECT--
            string output = string.Format("Lat: {0}, Lng: {1}", position.Latitude, position.Longitude);

            System.Diagnostics.Debug.WriteLine("My Lat and Long: " + output);

            double myLat = position.Latitude;
            double myLng = position.Longitude;

            Position myPos = new Position(myLat, myLng);

            Pin myLocation = new Pin
            {
                Type = PinType.Place,
                Position = myPos,
                Label = "me",
                Address = "Brisbane"
            };
            map.Pins.Add(myLocation);

            MapSpan mySpan = new MapSpan(myPos, 5, 5);
            map.MoveToRegion(mySpan);



            /*
            Position anotherPos = new Position(-30, 160);

            Pin secondPin = new Pin
            {
                Type = PinType.Place,
                Position = anotherPos,
                Label = "me",
                Address = "Not Brisbane"
            };
            map.Pins.Add(secondPin);

            Position[] posList = new Position[] { myPos, anotherPos };

            int numPositions = 2;

            double[] latList, lngList;
            latList = new double[numPositions];
            lngList = new double[numPositions];

            for (int i = 0; i < numPositions; i++)
            {
                latList[i] = (double)posList[i].Latitude;
                lngList[i] = (double)posList[i].Longitude;
            }

            Array.Sort(latList);
            Array.Sort(lngList);

            Position southWest = new Position(latList[0], lngList[0]);
            Position northEast = new Position(latList[latList.Length - 1], lngList[lngList.Length - 1]);

            Bounds myBounds = new Bounds(southWest, northEast);
            */
            // *************** Can't get camera update to set map bounds around positions ***************

            //CameraUpdate cu = new CameraUpdate();  //CameraUpdateFactory.NewBounds(myBounds, 5);

            //map.MoveCamera(CameraUpdateFactory.NewBounds(myBounds, 0));
        }

        //Pressed Back Button
        async void GoToOptionsPage(object sender, EventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("Pressed");

            await Navigation.PopAsync();
        }

        //Get Hard Coded Location
        private Position GetLocation()
        {
            Position myPosition = new Position(-27.4698, 153.0251);

            return myPosition;
        }

        //Request a test Route
        async private void requestRoute(Position startLocation, Position endLocation)
        {
            Console.Out.WriteLine("Try Requesting");
            string apiKey = "AlzaSyB4mMK5O9BvbM5c5__elRJesoGVdWN16io";
            string requesturl = "https://maps.googleapis.com/maps/api/directions/json?origin=" + startLocation.Latitude.ToString() + "," + startLocation.Longitude.ToString() + "&destination=" + endLocation.Latitude.ToString() + "," + endLocation.Longitude.ToString() + "&key =" + apiKey;

            //Test for exceptions
            string strException = "-1";

            string strJSONDirectionResponse = await FnHttpRequest(requesturl);

            if (strJSONDirectionResponse != strException)
            {
                //Mark source and destination on map
                Pin startloc = new Pin
                {
                    Type = PinType.Generic,
                    Position = startLocation,
                    Label = "start route"
                };
                map.Pins.Add(startloc);
                Pin endloc = new Pin
                {
                    Type = PinType.Generic,
                    Position = endLocation,
                    Label = "end route"
                };
                map.Pins.Add(endloc);
            }
            //Convert Json to a class
            var objRoutes = JsonConvert.DeserializeObject<googledirectionclass>(strJSONDirectionResponse);

            //Decode The Returned points
            string encodedPoints = objRoutes.routes[0].overview_polyline.points;
            var lstDecodedPoints = FnDecodePolylinePoints(encodedPoints);

            //Create polylines
            Xamarin.Forms.GoogleMaps.Polyline polylineoption = new Xamarin.Forms.GoogleMaps.Polyline();
            for (int i = 0; i < lstDecodedPoints.Count; i++)
                polylineoption.Positions.Add(lstDecodedPoints[i]);
            //Add polyline to map
            map.Polylines.Add(polylineoption);

            //Convert list of encoded points to array of ltlng type
            /*var ltLngPoints = new LatLng[lstDecodedPoints.Count];
            int index = 0;
            foreach(MyLocationButtonClickedEventArgs loc in lstDecodedPoints)
            {
                latLngPoints[index++] = new Latlng(loc.lat, loc.lng);
            }*/

            //--TASKS--

            //Request String
            WebClient webclient;
            async Task<string> FnHttpRequest(string strUri)
            {
                webclient = new WebClient();
                string strResultData;
                try
                {
                    strResultData = await webclient.DownloadStringTaskAsync(new Uri(strUri));
                    Console.WriteLine(strResultData);
                }
                catch
                {
                    strResultData = strException;
                }
                finally
                {
                    //Clear resources used
                    if (webclient != null)
                    {
                        webclient.Dispose();
                        webclient = null;
                    }
                }

                return strResultData;
            }

            //Function to decode encoded points
            List<Position> FnDecodePolylinePoints(string Points)
            {
                if (string.IsNullOrEmpty(Points))
                    return null;
                var poly = new List<Position>();
                char[] polylinechars = Points.ToCharArray();
                int ind = 0;

                int currentLat = 0;
                int currentLng = 0;
                int next5bits;
                int sum;
                int shifter;

                while (ind < polylinechars.Length)
                {
                    //Calculate next latitude
                    sum = 0;
                    shifter = 0;
                    do
                    {
                        next5bits = (int)polylinechars[ind++] - 63;
                        sum |= (next5bits & 31) << shifter;
                        shifter += 5;
                    }
                    while (next5bits >= 32 && ind < polylinechars.Length);

                    if (ind >= polylinechars.Length)
                        break;

                    currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                    //calculate next longitude
                    sum = 0;
                    shifter = 0;
                    do
                    {
                        next5bits = (int)polylinechars[ind++] - 63;
                        sum |= (next5bits & 31) << shifter;
                        shifter += 5;
                    }
                    while (next5bits >= 32 && ind < polylinechars.Length);

                    if (ind >= polylinechars.Length && next5bits >= 32)
                        break;

                    currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                    Position p = new Position(Convert.ToDouble(currentLat) / 100000.0, Convert.ToDouble(currentLng) / 100000.0);
                    poly.Add(p);
                }
                return poly;
            }


            /*
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("https://maps.googleapis.com/maps/api/directions/");
            WebRequest request = HttpWebRequest.Create(requesturl);

            request.ContentType = "application/json";
            request.Method = "GET";

            //WebResponse response = request.GetResponse();

/*
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    Console.Out.WriteLine("Error Fetching Data. Server returned status code: {0}", response.StatusCode);
                else
                {
                    Console.WriteLine("Response Body: {0}", response.GetResponseStream());
                }
            }*/



            // https://maps.googleapis.com/maps/api/directions/ 
            //mode=walking&origin=-27.4698,153.0251&destination=-27.4798,153.0251&key=AIzaSyB4mMK5O9BvbM5c5__eIRJesoGVdWN16io;
            //waypoints=optimize:true|-27.4798,153.0251|via:-27.4768,153.0251|via:-27.4798,153.0251|
            //units=metric
            //string JSONStringResponse = await FnHttpRequest(requesturl);
        }
    }

    //Google Directions Classes

    public class googledirectionclass
    {
        public string status { get; set; }
        public Geocoded_Waypoints[] geocoded_waypoints { get; set; }
        public Route[] routes { get; set; }
    }

    public class Geocoded_Waypoints
    {
        public string geocoder_status { get; set; }
        public string place_id { get; set; }
        public string[] types { get; set; }
    }

    public class Route
    {
        public string summary { get; set; }
        public Leg[] legs { get; set; }
        public string copyrights { get; set; }
        public Overview_Polyline overview_polyline { get; set; }
        public object[] warnings { get; set; }
        public int[] waypoint_order { get; set; }
        public Bounds bounds { get; set; }
    }

    public class Overview_Polyline
    {
        public string points { get; set; }
    }

    public class Bounds
    {
        public Southwest southwest { get; set; }
        public Northeast northeast { get; set; }
    }

    public class Southwest
    {
        public float lat { get; set; }
        public float lng { get; set; }
    }

    public class Northeast
    {
        public float lat { get; set; }
        public float lng { get; set; }
    }

    public class Leg
    {
        public Step[] steps { get; set; }
        public Duration duration { get; set; }
        public Distance distance { get; set; }
        public Start_Location start_location { get; set; }
        public End_Location end_location { get; set; }
        public string start_address { get; set; }
        public string end_address { get; set; }
    }

    public class Duration
    {
        public int value { get; set; }
        public string text { get; set; }
    }

    public class Distance
    {
        public int value { get; set; }
        public string text { get; set; }
    }

    public class Start_Location
    {
        public float lat { get; set; }
        public float lng { get; set; }
    }

    public class End_Location
    {
        public float lat { get; set; }
        public float lng { get; set; }
    }

    public class Step
    {
        public string travel_mode { get; set; }
        public Start_Location1 start_location { get; set; }
        public End_Location1 end_location { get; set; }
        public Polyline polyline { get; set; }
        public Duration1 duration { get; set; }
        public string html_instructions { get; set; }
        public Distance1 distance { get; set; }
    }

    public class Start_Location1
    {
        public float lat { get; set; }
        public float lng { get; set; }
    }

    public class End_Location1
    {
        public float lat { get; set; }
        public float lng { get; set; }
    }

    public class Polyline
    {
        public string points { get; set; }
    }

    public class Duration1
    {
        public int value { get; set; }
        public string text { get; set; }
    }

    public class Distance1
    {
        public int value { get; set; }
        public string text { get; set; }
    }

}

