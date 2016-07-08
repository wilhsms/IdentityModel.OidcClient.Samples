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

namespace AndroidClientChromeCustomTabs
{
    [Application]
    public class AndroidClientChromeCustomTabsApplication : Application
    {
        public static ActivityMediator Mediator { get; } = new ActivityMediator();
        public AndroidClientChromeCustomTabsApplication(IntPtr handle, JniHandleOwnership transfer)
            : base(handle,transfer)
        {
            // constructor must be available for Application instance to work
        }
    }
}