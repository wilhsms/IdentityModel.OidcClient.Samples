using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using IdentityModel.OidcClient.WebView;
using Android.Support.CustomTabs;
using Android.Graphics;
using Android.Content.PM;

namespace AndroidClientChromeCustomTabs
{

    class ChromeCustomTabsWebView : IWebView
    {
        public event EventHandler<HiddenModeFailedEventArgs> HiddenModeFailed;

        private readonly Activity _context;
        private CustomTabsActivityManager _customTabs;
        private TaskCompletionSource<InvokeResult> _tcs;

        public ChromeCustomTabsWebView(Activity context)
        {
            _context = context;
        }

        public Task<InvokeResult> InvokeAsync(InvokeOptions options)
        {
            // must be able to wait for the intent to be finished to continue
            // with setting the task result
            _tcs = new TaskCompletionSource<InvokeResult>();

            // create & open chrome custom tab
            _customTabs = new CustomTabsActivityManager(_context);

            // build custom tab
            var builder = new CustomTabsIntent.Builder(_customTabs.Session)
               .SetToolbarColor(Color.Argb(255, 52, 152, 219))
               .SetShowTitle(true)
               .EnableUrlBarHiding();
            
            var customTabsIntent = builder.Build();

            customTabsIntent.Intent.AddFlags(ActivityFlags.NoHistory);
          //  customTabsIntent.Intent.AddFlags(ActivityFlags.ClearTop);

            AndroidClientChromeCustomTabsApplication.Mediator.ActivityMessageReceived
                += (response) =>
                {
                    _tcs.SetResult(new InvokeResult
                    {
                        Response = response,
                        ResultType = InvokeResultType.Success
                    });
                };             

            // launch
            customTabsIntent.LaunchUrl(_context, Android.Net.Uri.Parse(options.StartUrl));

             // need an intent to be triggered when browsing to the "io.identitymodel.native://callback"
            // scheme/URI => CallbackInterceptorActivity

            return _tcs.Task;
        } 
    }
}