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

            await Navigation.PushAsync(new MapPage());
        }
	}
}