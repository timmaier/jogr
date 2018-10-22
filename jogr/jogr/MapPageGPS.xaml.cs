using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Specialized;
using System.Net;
using System.IO;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.GoogleMaps;

using Plugin.Geolocator;
using Newtonsoft.Json;
using Plugin.Connectivity;

namespace jogr
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPageGPS : ContentPage
    {
        //Get a reference to the map component
        public Map map;

        //Constructor
        public MapPageGPS()
        {            

            async Task DisplayRoutes()
            {
                InitializeComponent();
                NavigationPage.SetHasNavigationBar(this, false);

                //Reference the map component
                map = (Map)MyMap;

                //Customize Map
                map.MapType = MapType.Street;
                map.IsTrafficEnabled = false;

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
                Position waypoint1 = new Position(myPos.Latitude + 0.005, myPos.Longitude);
                Position waypoint2 = new Position(myPos.Latitude + 0.005, myPos.Longitude + 0.005);
                Position waypoint3 = new Position(myPos.Latitude, myPos.Longitude + 0.005);
                //Request Route still being worked on
                requestRoute(myPos, myPos, waypoint1, waypoint2, waypoint3);
            }

            DisplayRoutes();
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
            string requesturl = "https://maps.googleapis.com/maps/api/directions/json?" + "mode=walking" + "&units=metric" + "&origin=" + startLocation.Latitude.ToString() + "," + startLocation.Longitude.ToString() + "&destination=" + endLocation.Latitude.ToString() + "," + endLocation.Longitude.ToString()
                + "&waypoints=optimize:true|" + waypoint1.Latitude.ToString() + "," + waypoint1.Longitude.ToString() + "|" + waypoint2.Latitude.ToString() + "," + waypoint2.Longitude.ToString() + "|" + waypoint3.Latitude.ToString() + "," + waypoint3.Longitude.ToString() + "&key=" + apiKey;

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
            else
            {
                Console.Out.WriteLine("Error Returned");
                return;
            }


            /*
             * NEED TO CHECK IF THE RETURNED JSON IS AN ERROR ONE
             * IF IT IS AN ERROR, RESEND THE REQUEST, AND DON'T
             * PROCEED WITH THE FUNCTION
             */

            //Convert Json to a class
            var objRoutes = JsonConvert.DeserializeObject<googledirectionclass>(strJSONDirectionResponse);

            //Decode The Returned points
            string encodedPoints = objRoutes.routes[0].overview_polyline.points;
            var lstDecodedPoints = FnDecodePolylinePoints(encodedPoints);

            Console.Out.WriteLine("Latitude: " + lstDecodedPoints[0].Latitude + ", Longitude:" + lstDecodedPoints[0].Longitude);


            Xamarin.Forms.GoogleMaps.Polyline polylineoption = new Xamarin.Forms.GoogleMaps.Polyline();

            polylineoption.StrokeWidth = 6f;
            polylineoption.StrokeColor = Color.FromHex("#315C6A");
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
                    Console.Out.WriteLine("Results: " + strResultData);
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
}

