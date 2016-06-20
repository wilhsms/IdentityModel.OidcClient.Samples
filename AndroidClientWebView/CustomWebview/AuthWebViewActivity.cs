using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Webkit;
using IdentityModel.OidcClient.WebView;
using System;

namespace AndroidClient
{
    [Activity(Label = "AuthWebViewActivity")]
    public class AuthWebViewActivity : Activity
    {
        private WebView _webView;

        internal class State : Java.Lang.Object
        {
            public InvokeOptions Options;
            public EventHandler<string> OnSuccess;
        }

        internal State state;

        internal static readonly ActivityStateRepository<State> StateRepo = new ActivityStateRepository<State>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Intent.GetBooleanExtra("ClearCookies", true))
            {
                CookieManager.Instance.RemoveAllCookie();
            }

            if (state == null && Intent.HasExtra("StateKey"))
            {
                var stateKey = Intent.GetStringExtra("StateKey");
                state = StateRepo.Remove(stateKey);
            }

            _webView = new WebView(this)
            {
                Id = 42,
            };
            _webView.Settings.JavaScriptEnabled = true;
            _webView.SetWebViewClient(new WebViewClientTeste(this));
            SetContentView(_webView);

            _webView.LoadUrl(state.Options.StartUrl);
        }

        class WebViewClientTeste : WebViewClient
        {
            private readonly AuthWebViewActivity activity;

            protected WebViewClientTeste(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
            {
            }

            public WebViewClientTeste(AuthWebViewActivity activity)
            {
                this.activity = activity;
            }

            public override void OnPageFinished(WebView view, string url)
            {
                var uri = new Uri(url);
                var uriBase = new Uri(activity.state.Options.EndUrl);
                if (uri.Host == uriBase.Host)
                {
                    activity.state.OnSuccess(this, url);
                    activity.Finish();
                }
            }
        }
    }
}