using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;

namespace jogr.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            Xamarin.FormsGoogleMaps.Init("AIzaSyB4mMK5O9BvbM5c5__eIRJesoGVdWN16io");
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }
    }
}
