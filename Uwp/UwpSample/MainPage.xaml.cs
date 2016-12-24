using IdentityModel.Client;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.WebView.Uwp;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using Windows.Security.Authentication.Web;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UwpSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        HttpClient _client;

        string _authority = "https://demo.identityserver.io";
        string _api = "https://api.identityserver.io/";
        
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            
            var webView = new UwpWebView(enableWindowsAuthentication: false);

            var options = new OidcClientOptions(
                authority:    _authority,
                clientId:     "native.hybrid",
                clientSecret: "",
                scope:        "openid profile api offline_access",
                redirectUri:  WebAuthenticationBroker.GetCurrentApplicationCallbackUri().AbsoluteUri,
                webView:      webView);

            var client = new OidcClient(options);
            var result = await client.LoginAsync();

            if (!string.IsNullOrEmpty(result.Error))
            {
                ResultTextBox.Text = result.Error;
                return;
            }

            var sb = new StringBuilder(128);

            foreach (var claim in result.Claims)
            {
                sb.AppendLine($"{claim.Type}: {claim.Value}");
            }

            sb.AppendLine($"refresh token: {result?.RefreshToken ?? "none"}");
            sb.AppendLine($"access token: {result?.AccessToken ?? "none"}");
            
            ResultTextBox.Text = sb.ToString();

            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                _client = new HttpClient(result.Handler);
            }
            else
            {
                _client = new HttpClient();
            }
            _client.BaseAddress = new Uri(_api);
        }

        private async void CallApiButton_Click(object sender, RoutedEventArgs e)
        {
            if (_client == null)
            {
                return;
            }

            var result = await _client.GetAsync("identity");
            if (result.IsSuccessStatusCode)
            {
                ResultTextBox.Text = JArray.Parse(await result.Content.ReadAsStringAsync()).ToString();
            }
            else
            {
                ResultTextBox.Text = result.ReasonPhrase;
            }
        }
    }
}