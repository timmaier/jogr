using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Specialized;
using System.Net;
using System.IO;
using System.Reflection;
using System.Timers;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.GoogleMaps;

using Plugin.Geolocator;
using Newtonsoft.Json;
using Plugin.Connectivity;
using GeoCoordinatePortable;


namespace jogr
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        //Get a reference to the map component
        public Map map;
        //Public pin value
        public Pin myLocation;
        //Timer for updating pin
        public Timer locationTimer;

        //Constructor
        public MapPage(double distance)
        {
            double setDistance = distance;
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            //Reference the map component
            map = (Map)MyMap;

            //Customize Map
            map.MapType = MapType.Street;
            map.IsTrafficEnabled = false;
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MapStyleClass)).Assembly;
            Stream stream = assembly.GetManifestResourceStream("jogr.CustomMap.json");
            if (stream != null)
            {
                string text = "";
                using (var reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                    map.MapStyle = MapStyle.FromJson(text);
                }
            }
            else
            {
                Console.Out.WriteLine("Style stream was empty");
            }

            //Create Location Pin
            myLocation = new Pin
            {
                Type = PinType.Place,
                Label = "myLocation",
                Icon = BitmapDescriptorFactory.DefaultMarker(Color.Red)
            };
            
            displayRoute();

            //Generate a route based on current location
            async Task displayRoute()
            {
                //Get the users current location
                Plugin.Geolocator.Abstractions.Position myGeoPos = null;
                try
                {
                    var locator = CrossGeolocator.Current;
                    locator.DesiredAccuracy = 100;
                    myGeoPos = await locator.GetPositionAsync(TimeSpan.FromSeconds(1));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("There was an error: " + ex);
                }

                Position myPos = new Position(myGeoPos.Latitude, myGeoPos.Longitude);

                //Display a pin at users location
                myLocation.Position = myPos;
                map.Pins.Add(myLocation);

                //Create Timer for updating users location
                locationTimer = new System.Timers.Timer(500);
                locationTimer.Elapsed += UpdateUserLocation;
                locationTimer.Enabled = true;
                locationTimer.AutoReset = true;
                
                //Zoom the map into the users location
                MapSpan mySpan = new MapSpan(myPos, 5, 5);
                map.MoveToRegion(mySpan);

                // Set distance between waypoints based on passed distance option
                double distanceMetres = setDistance * 1000;
                double quarterDistance = distanceMetres / 4;

                //Randomize route direction
                Random random = new Random(DateTime.UtcNow.Millisecond);
                int directionRandomizer = random.Next(0, 4);

                Position waypoint1 = ConvertDistToLatLng(myPos, directionRandomizer % 4, quarterDistance);
                Position waypoint2 = ConvertDistToLatLng(waypoint1, (directionRandomizer + 1) % 4, quarterDistance);
                Position waypoint3 = ConvertDistToLatLng(waypoint2, (directionRandomizer + 2) % 4, quarterDistance);

                //Request directions using waypoints
                requestRoute(myPos, myPos, waypoint1, waypoint2, waypoint3);
  
            }
        }

        // Timer method for updating users location on map
        async private void UpdateUserLocation(Object source, ElapsedEventArgs e)
        {
            //Request users location
            Plugin.Geolocator.Abstractions.Position myGeoPos = null;
            try
            {
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = 100;
                myGeoPos = await locator.GetPositionAsync(TimeSpan.FromSeconds(1));
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was an error: " + ex);
            }

            Position myPos = new Position(myGeoPos.Latitude, myGeoPos.Longitude);
            //Update pin location
            myLocation.Position = myPos;
        }

        // Method to try and link distance to lat/lng changes for a given direction (Hard Coded)
        // Using values for direction: 0 = North, 1 = East, 2 = South, 3 = West
        Position ConvertDistToLatLng(Position previousPos, int direction, double quarterDistance)
        {
            double preLat = previousPos.Latitude;
            double preLng = previousPos.Longitude;

            GeoCoordinate startLoc = new GeoCoordinate(preLat, preLng);

            double increment = 0.0001;
            double latLngChange = increment;
            bool distanceFound = false;
            double distCheck;

            switch (direction)
            {
                case 0:
                    while (!distanceFound)
                    {
                        distCheck = startLoc.GetDistanceTo(new GeoCoordinate(preLat + latLngChange, preLng));
                        latLngChange += increment;
                        if (distCheck > quarterDistance)
                        {
                            distanceFound = true;
                        }
                    }
                    return new Position(preLat + latLngChange, preLng);
                case 1:
                    while (!distanceFound)
                    {
                        distCheck = startLoc.GetDistanceTo(new GeoCoordinate(preLat, preLng + latLngChange));
                        latLngChange += increment;
                        if (distCheck > quarterDistance)
                        {
                            distanceFound = true;
                        }
                    }
                    return new Position(preLat, preLng + latLngChange);
                case 2:
                    while (!distanceFound)
                    {
                        distCheck = startLoc.GetDistanceTo(new GeoCoordinate(preLat - latLngChange, preLng));
                        latLngChange += increment;
                        if (distCheck > quarterDistance)
                        {
                            distanceFound = true;
                        }
                    }
                    return new Position(preLat - latLngChange, preLng);
                case 3:
                    while (!distanceFound)
                    {
                        distCheck = startLoc.GetDistanceTo(new GeoCoordinate(preLat, preLng - latLngChange));
                        latLngChange += increment;
                        if (distCheck > quarterDistance)
                        {
                            distanceFound = true;
                        }
                    }
                    return new Position(preLat, preLng - latLngChange);
                default:
                    return new Position(preLat, preLng);                    
            }
        }

        //Pressed Back Button
        async void GoToOptionsPage(object sender, EventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("Pressed");

            await Navigation.PopAsync();
        }

        //Get Hard Coded Location (Test Function)
        private Position GetLocation()
        {
            Position myPosition = new Position(-27.4698, 153.0251);

            return myPosition;
        }

        //Request a Route using Google Directions API
        async private void requestRoute(Position startLocation, Position endLocation, Position waypoint1, Position waypoint2, Position waypoint3)
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                Console.Out.WriteLine("Connection Valid");
            }
            else
            {
                Console.Out.WriteLine("Connection Invalid");
            }
            Console.Out.WriteLine("Try Requesting");
            string apiKey = "AIzaSyBj3FmgND9IRoLFfh25eiE2x6Hg37uzDg4";
            string requesturl = "https://maps.googleapis.com/maps/api/directions/json?" + "mode=walking" + "&units=metric"  + "&avoid=ferries"
                + "&origin=" + startLocation.Latitude.ToString() + "," + startLocation.Longitude.ToString() 
                + "&destination=" + endLocation.Latitude.ToString() + "," + endLocation.Longitude.ToString()
                + "&waypoints=optimize:true|" + waypoint1.Latitude.ToString() + "," + waypoint1.Longitude.ToString() + "|" + waypoint2.Latitude.ToString() + "," + waypoint2.Longitude.ToString() + "|" + waypoint3.Latitude.ToString() + "," + waypoint3.Longitude.ToString() 
                + "&key=" + apiKey;

            //Test for exceptions
            string strException = "-1";

            string strJSONDirectionResponse = await FnHttpRequest(requesturl);

            if (strJSONDirectionResponse == strException)
            {
                Console.Out.WriteLine("Error Returned");
                return;
            }

            //Convert Json to a class
            var objRoutes = JsonConvert.DeserializeObject<googledirectionclass>(strJSONDirectionResponse);

            // Check for error return case - Return to options screen if error returned
            if (objRoutes.status != "OK")
            {
                await DisplayAlert("Alert", "Problem with Directions Request: "+objRoutes.status, "OK");
                //Go back to options screen
                await Navigation.PopAsync();
                return;
            }
            else
            {
                //Decode The Returned points
                string encodedPoints = objRoutes.routes[0].overview_polyline.points;
                var lstDecodedPoints = FnDecodePolylinePoints(encodedPoints);

                Xamarin.Forms.GoogleMaps.Polyline polylineoption = new Xamarin.Forms.GoogleMaps.Polyline();

                polylineoption.StrokeWidth = 6f;
                polylineoption.StrokeColor = Color.White;

                double[] latList = new double[lstDecodedPoints.Count];
                double[] lngList = new double[lstDecodedPoints.Count];

                for (int i = 0; i < lstDecodedPoints.Count; i++)
                {
                    polylineoption.Positions.Add(lstDecodedPoints[i]);
                    latList[i] = lstDecodedPoints[i].Latitude;
                    lngList[i] = lstDecodedPoints[i].Longitude;
                }

                double latCentre, lngCentre;
                latCentre = (latList.Max() + latList.Min()) / 2;
                lngCentre = (lngList.Max() + lngList.Min()) / 2;
                Position routeCentrePos = new Position(latCentre, lngCentre);

                //Add polyline to map
                map.Polylines.Add(polylineoption);
                MapSpan routeSpan = new MapSpan(routeCentrePos, 0.02, 0.02);
                map.MoveToRegion(routeSpan);
            }


            //--TASKS--

            //Request String
            WebClient webclient;
            async Task<string> FnHttpRequest(string strUri)
            {
                webclient = new WebClient();
                string strResultData = "";
                try
                {
                    strResultData = await webclient.DownloadStringTaskAsync(new Uri(strUri));
                }
                catch (Exception ex)
                {
                    strResultData = strException;
                    Console.Out.WriteLine("Exception: " + ex);
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
        }
    }

    //Map Type classes

    public class MapStyleClass
    {
        public MapStyleFeature[] Features { get; set; }
    }

    public class MapStyleFeature
    {
        public string featureType { get; set; }
        public string elementType { get; set; }
        public MapStylers[] stylers { get; set; }
    }

    public class MapStylers
    {
        public string visibility { get; set; }
        public string color { get; set; }
        public string saturation { get; set; }
        public string hue { get; set; }
        public string lightness { get; set; }
    }


    //Google Directions Classes
    public class googledirectionclass
    {
        public Geocoded_Waypoints[] geocoded_waypoints { get; set; }
        public Route[] routes { get; set; }
        public string status { get; set; }
    }

    public class Geocoded_Waypoints
    {
        public string geocoder_status { get; set; }
        public string place_id { get; set; }
        public string[] types { get; set; }
    }

    public class Route
    {
        public Bounds bounds { get; set; }
        public string copyrights { get; set; }
        public Leg[] legs { get; set; }
        public Overview_Polyline overview_polyline { get; set; }
        public string summary { get; set; }
        public object[] warnings { get; set; }
        public object[] waypoint_order { get; set; }
    }

    public class Bounds
    {
        public Northeast northeast { get; set; }
        public Southwest southwest { get; set; }
    }

    public class Northeast
    {
        public float lat { get; set; }
        public float lng { get; set; }
    }

    public class Southwest
    {
        public float lat { get; set; }
        public float lng { get; set; }
    }

    public class Overview_Polyline
    {
        public string points { get; set; }
    }

    public class Leg
    {
        public Distance distance { get; set; }
        public Duration duration { get; set; }
        public string end_address { get; set; }
        public End_Location end_location { get; set; }
        public string start_address { get; set; }
        public Start_Location start_location { get; set; }
        public Step[] steps { get; set; }
        public object[] traffic_speed_entry { get; set; }
        public object[] via_waypoint { get; set; }
    }

    public class Distance
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    public class Duration
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    public class End_Location
    {
        public float lat { get; set; }
        public float lng { get; set; }
    }

    public class Start_Location
    {
        public float lat { get; set; }
        public float lng { get; set; }
    }

    public class Step
    {
        public Distance1 distance { get; set; }
        public Duration1 duration { get; set; }
        public End_Location1 end_location { get; set; }
        public string html_instructions { get; set; }
        public Polyline polyline { get; set; }
        public Start_Location1 start_location { get; set; }
        public string travel_mode { get; set; }
        public string maneuver { get; set; }
    }

    public class Distance1
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    public class Duration1
    {
        public string text { get; set; }
        public int value { get; set; }
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

    public class Start_Location1
    {
        public float lat { get; set; }
        public float lng { get; set; }
    }
}

