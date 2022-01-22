using Android.App;
using Android.OS;
using Android.Support.V7.App;
using System;
using Android.Util;
using Android.Widget;
using Android.Views;
using Android.Content.PM;
using Android.Text.Method;

namespace SoftWing
{
    [Activity(Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTask)]
    public class ControllerHelpActivity : AppCompatActivity
    {
        private const String TAG = "ControllerHelpActivity";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "OnCreate()");
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.controller_help);
        }

        protected override void OnStart()
        {
            base.OnStart();
        }
    }
}
