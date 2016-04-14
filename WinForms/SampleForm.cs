using IdentityModel.OidcClient;
using IdentityModel.OidcClient.WebView.WinForms;
using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinForms
{
    public partial class SampleForm : Form
    {
        private OidcClient _client;

        public SampleForm()
        {
            InitializeComponent();

            var authority = "https://demo.identityserver.io";

            var options = new OidcClientOptions(
                authority, 
                "native", 
                "secret", 
                "openid email api offline_access",
                "http://localhost/winforms.client", 
                new WinFormsWebView());
            options.UseFormPost = true;

            _client = new OidcClient(options);
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            AccessTokenDisplay.Clear();
            OtherDataDisplay.Clear();
            
            var result = await _client.LoginAsync(Silent.Checked);

            if (result.Success)
            {
                AccessTokenDisplay.Text = result.AccessToken;

                var sb = new StringBuilder(128);
                foreach (var claim in result.Claims)
                {
                    sb.AppendLine($"{claim.Type}: {claim.Value}");
                }

                if (!string.IsNullOrWhiteSpace(result.RefreshToken))
                {
                    sb.AppendLine($"refresh token: {result.RefreshToken}");
                }

                OtherDataDisplay.Text = sb.ToString();
            }
            else
            {
                MessageBox.Show(this, result.Error, "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LogoutButton_Click(object sender, EventArgs e)
        {
            await _client.LogoutAsync(trySilent: Silent.Checked);
            AccessTokenDisplay.Clear();
            OtherDataDisplay.Clear();
        }
    }
}