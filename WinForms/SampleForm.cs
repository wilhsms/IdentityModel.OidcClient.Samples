using IdentityModel.OidcClient;
using IdentityModel.OidcClient.WebView.WinForms;
using System;
using System.Linq;
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

            var validator = new EndpointIdentityTokenValidator(authority, "native");
            var options = new OidcClientOptions(
                authority, 
                "native", 
                "secret", 
                "openid email api",
                "http://localhost/winforms.client", 
                validator,
                new WinFormsWebView());
            options.UseFormPost = true;

            _client = new OidcClient(options);
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            AccessTokenDisplay.Clear();
            IdentityTokenDisplay.Clear();

            var result = await _client.LoginAsync(Silent.Checked);

            if (result.Success)
            {
                AccessTokenDisplay.Text = result.AccessToken;
                IdentityTokenDisplay.Text = string.Join(Environment.NewLine,
                    result.Claims.Select(c => string.Format("{0}: {1}", c.Type, c.Value)));
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
            IdentityTokenDisplay.Clear();
        }
    }
}