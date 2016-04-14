using IdentityModel.OidcClient;
using IdentityModel.OidcClient.IdentityTokenValidation;
using IdentityModel.OidcClient.WebView.Uwp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UwpSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var authority = "https://demo.identityserver.io";
            var webView = new UwpWebView(enableWindowsAuthentication: false);

            var options = new OidcClientOptions(
                authority,
                "native",
                "secret",
                "openid profile api",
                WebAuthenticationBroker.GetCurrentApplicationCallbackUri().AbsoluteUri,
                webView);

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

            sb.AppendLine($"access token: {result.AccessToken}");

            ResultTextBox.Text = sb.ToString();

        }
    }
}