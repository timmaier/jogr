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

namespace jogr
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPage : ContentPage
	{
        //Constructor
		public MapPage ()
		{
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            //Get a reference to the map component
            Map map = (Map)MyMap;
            //Add pin to map
            Pin myLocation = new Pin
            {
                Type = PinType.Place,
                Position = GetLocation(),
                Label = "me",
                Address = "Brisbane"
            };
            map.Pins.Add(myLocation);

            double lat = GetLocation().Latitude;
            double lon = GetLocation().Longitude;

            /*Xamarin.Forms.GoogleMaps.Bounds Australia = new Bounds(GetLocation(),GetLocation());

            map.MoveCamera(CameraUpdateFactory.NewBounds(Australia, 30));
            */
            map.MoveToRegion(new MapSpan(GetLocation(), 0.1, 0.1), true);

            Polyline testRoute = new Polyline();

            requestRoute(GetLocation(),GetLocation());
            
        }
        
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