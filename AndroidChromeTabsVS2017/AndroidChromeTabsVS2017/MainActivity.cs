using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Net.Http;
using IdentityModel.OidcClient;

namespace AndroidChromeTabsVS2017
{
    [Activity(Label = "Android Client", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private TextView _loginOutput;
        private LoginResult _result;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

            FindViewById<Button>(Resource.Id.LoginButton).Click += _loginButton_Click;
            FindViewById<Button>(Resource.Id.ApiButton).Click += _apiButton_Click;

            _loginOutput = FindViewById<TextView>(Resource.Id.LoginOutput);
        }

        private async void _loginButton_Click(object sender, System.EventArgs e)
        {
            var authority = "https://demo.identityserver.io";

            try
            {
                var options = new OidcClientOptions
                {
                    Authority = authority,
                    ClientId = "native.hybrid",
                    Scope = "openid profile api",
                    RedirectUri = "io.identitymodel.native://callback",
                    ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect,
                    Browser = new ChromeCustomTabsBrowser(this)
                };

                var oidcClient = new OidcClient(options);
                _result = await oidcClient.LoginAsync();

                if (_result.IsError)
                {
                    Log("Error:" + _result.Error, true);
                }
                else
                {
                    Log("Claims:", true);
                    foreach (var claim in _result.User.Claims)
                    {
                        Log($"   {claim.Type}:{claim.Value}");
                    }
                    Log("Access Token: " + _result.AccessToken);
                }
            }
            catch (Exception ex)    
            {
                Log("Exception: " + ex.Message, true);
            }
        }

        private async void _apiButton_Click(object sender, EventArgs e)
        {
            if (_result?.IsError == false)
            {
                var apiUrl = "https://api.identityserver.io/identity";

                var client = new HttpClient();
                client.SetBearerToken(_result.AccessToken);

                try
                {
                    var result = await client.GetAsync(apiUrl);
                    if (result.IsSuccessStatusCode)
                    {
                        Log("API Results:", true);

                        var json = await result.Content.ReadAsStringAsync();
                        Log(json);
                    }
                    else
                    {
                        Log("API Error: " + (int)result.StatusCode, true);
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception: " + ex.Message, true);
                }
            }
            else
            {
                Log("Login to call API");
            }
        }

        public void Log(string msg, bool clear = false)
        {
            if (clear)
            {
                _loginOutput.Text = "";
            }
            else
            {
                _loginOutput.Text += "\r\n";
            }

            _loginOutput.Text += msg;
        }
    }
}

