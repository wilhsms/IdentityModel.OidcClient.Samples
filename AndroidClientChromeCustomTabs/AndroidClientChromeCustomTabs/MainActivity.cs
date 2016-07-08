using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Net.Http;
using IdentityModel.OidcClient;
using Android.Support.CustomTabs;

namespace AndroidClientChromeCustomTabs
{

    [Activity(Label = "AndroidClientChromeCustomTabs", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private OidcClient _oidcClient;
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);


             // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);
            button.Click += Button_Click;            
        }

        private async void Button_Click(object sender, EventArgs e)
        {
            var authority = "https://demo.identityserver.io";

            var options = new OidcClientOptions(
                authority,
                "native",
                "secret",
                "openid profile api offline_access",
                "io.identitymodel.native://callback", 
                new ChromeCustomTabsWebView(this));

            _oidcClient = new OidcClient(options);
            var result = await _oidcClient.LoginAsync();
        }
    }
}

