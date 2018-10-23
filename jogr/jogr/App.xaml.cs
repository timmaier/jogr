using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Permissions;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace jogr
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            //Reduce the DNS refresh time
            System.Net.ServicePointManager.DnsRefreshTimeout = 0;

            //Create a navigation parent to be able to switch between pages
            var navPage = new NavigationPage(new OptionsPage());
            NavigationPage.SetHasNavigationBar(this, false);

            //Start on the first page
            Application.Current.MainPage = navPage;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            //ActivityCompat.RequestPermissions(this, REQUEST_LOCATION);
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
