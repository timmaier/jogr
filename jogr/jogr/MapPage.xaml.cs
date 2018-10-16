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

namespace jogr
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPage : ContentPage
	{
        //Constructor
        public MapPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            //Get a reference to the map component
            Map map = (Map)MyMap;

            //public double myLat, myLng;

            //Creating Position variable based on emulator GPS, exporting lat/lng to make Google Position

            async Task GetCurrentLatLng()
            {
                Plugin.Geolocator.Abstractions.Position position = null;
                try
                {
                    var locator = CrossGeolocator.Current;
                    locator.DesiredAccuracy = 100;
                    position = await locator.GetPositionAsync(TimeSpan.FromSeconds(1));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("There was an error: " + ex);
                }

                var output = string.Format("Lat: {0}, Lng: {1}", position.Latitude, position.Longitude);

                Console.WriteLine(output);

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

                /*************** Can't get camera update to set map bounds around positions ***************/

                //CameraUpdate cu = new CameraUpdate();  //CameraUpdateFactory.NewBounds(myBounds, 5);

                //map.MoveCamera(CameraUpdateFactory.NewBounds(myBounds, 0));

                MapSpan mySpan = new MapSpan(myPos, 5, 5);
                map.MoveToRegion(mySpan);
            }

            GetCurrentLatLng();
            
        }



            /*async Task<String> getLocation() {

                var locator = CrossGeolocator.Current;

                var position = await locator.GetPositionAsync(TimeSpan.FromSeconds(10));

                String latStr = position.Latitude.ToString();
                String lngStr = position.Longitude.ToString();

                String displayString = ("Latitude: " + latStr + ", Longitude: " + lngStr);

                return displayString;                
            }

            Task<String> locStr = getLocation();

            System.Diagnostics.Debug.WriteLine(locStr);
            
        }*/

                //Pressed Back Button
                async void GoToOptionsPage(object sender, EventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("Pressed");

            await Navigation.PopAsync();
        }

        private Position GetLocation()
        {
            Position myPosition = new Position(-27.4698, 153.0251);

            return myPosition;
        }

        private string requestRoute(Position startLocation, Position endLocation)
        {
            Console.Out.WriteLine("Try Requesting");
            string requesturl = "https://maps.googleapis.com/maps/api/directions/json?-27.4698,153.0251&destination=-27.4798,153.0251&key=keyGoesHere";

            var request = HttpWebRequest.Create(requesturl);

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
            return "null";
        }
    }
}

