using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using IdentityModel.OidcClient.WebView;
using System.Threading.Tasks;

namespace AndroidClient
{
    class AuthWebView : IWebView
    {
        private readonly Context _context;

        public event EventHandler<HiddenModeFailedEventArgs> HiddenModeFailed;

        public AuthWebView(Context context)
        {
            _context = context;
        }

        public Task<InvokeResult> InvokeAsync(InvokeOptions options)
        {
            var tcs = new TaskCompletionSource<InvokeResult>();

            var intent = new Intent(_context, typeof(AuthWebViewActivity));
            intent.PutExtra("ClearCookies", true);
            var state = new AuthWebViewActivity.State
            {
                Options = options
            };
            state.OnSuccess += (onj, res) =>
            {
                
                tcs.SetResult(new InvokeResult
                {
                    Response = res,
                    ResultType = InvokeResultType.Success
                });
            };
            intent.PutExtra("StateKey", AuthWebViewActivity.StateRepo.Add(state));

            _context.StartActivity(intent);

            return tcs.Task;
        }
    }
}