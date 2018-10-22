using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace jogr
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class OptionsPage : ContentPage
	{
        public double DISTANCE;

        public OptionsPage ()
		{
			InitializeComponent ();
            //Hide navigation bar
            NavigationPage.SetHasNavigationBar(this, false);
        }

        //Pressed 'go.' button
        async void GoToMapsPage(object sender, EventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("Pressed");
            try
            {
                await Navigation.PushAsync(new MapPage());
            }
            catch (Exception ex)
            {
                Console.WriteLine("MyStackTrace: {0}", ex.ToString());
            }
        }

        // Minor estimation calculations for time and kilojoules burned using distance defined
        void OnSliderValueChanged(object sender, ValueChangedEventArgs args)
        {
            double value = args.NewValue;
            DISTANCE = value;
            distance.Text = String.Format("{0}km*", DISTANCE);
            double timestamp = DISTANCE * 6;
            double kilojoulesstamp = DISTANCE * 105;
            time.Text = String.Format("{0}min*", timestamp);
            kilojoules.Text = String.Format("{0}kJ*", kilojoulesstamp);
        }

    }
}