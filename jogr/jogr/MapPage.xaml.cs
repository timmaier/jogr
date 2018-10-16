using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
        
        //Pressed Back Button
        async void GoToOptionsPage(object sender, EventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("Pressed");

            await Navigation.PopAsync();
        }

        private Xamarin.Forms.GoogleMaps.Position GetLocation()
        {
            Xamarin.Forms.GoogleMaps.Position myPosition = new Xamarin.Forms.GoogleMaps.Position(-27.4698, 153.0251);

            return myPosition;
        }
    }
}