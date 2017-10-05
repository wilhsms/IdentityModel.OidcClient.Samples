using System;
using System.Text;
using IdentityModel.OidcClient;
using UIKit;

namespace iOS11Client
{
    public partial class ViewController : UIViewController
    {
        OidcClient _client;
        AuthorizeState _state;
        SafariServices.SFAuthenticationSession _session;

        protected ViewController(IntPtr handle) : base(handle)
        {
			var options = new OidcClientOptions
			{
				Authority = "https://demo.identityserver.io",
				ClientId = "native.hybrid",
				Scope = "openid profile email api",
                RedirectUri = "SFAuthenticationSessionExample://callback",

				ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect
			};

			_client = new OidcClient(options);   
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            LoginButton.TouchUpInside += LoginButton_TouchUpInside;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
        }

        async void LoginButton_TouchUpInside(object sender, EventArgs e)
        {
			_state = await _client.PrepareLoginAsync();

			_session = new SafariServices.SFAuthenticationSession(
				new Foundation.NSUrl(_state.StartUrl),
				"SFAuthenticationSessionExample://",
				async (callbackUrl, error) =>
				{
                    if (error != null) 
                    {
                        OutputText.Text = error.ToString();
                        return;
                    }

					var result = await _client.ProcessResponseAsync(callbackUrl.AbsoluteString, _state);
                    if (result.IsError)
                    {
                        OutputText.Text = result.Error;
                    }

					var sb = new StringBuilder(128);
					foreach (var claim in result.User.Claims)
					{
						sb.AppendFormat("{0}: {1}\n", claim.Type, claim.Value);
					}

					sb.AppendFormat("\n{0}: {1}\n", "refresh token", result?.RefreshToken ?? "none");
					sb.AppendFormat("\n{0}: {1}\n", "access token", result.AccessToken);

					OutputText.Text = sb.ToString();
				});

            _session.Start();
		}
    }
}
