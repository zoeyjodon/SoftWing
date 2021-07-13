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

            ConfigureLinks();
            ConfigureHelpButtons();
        }

        private void ConfigureLinks()
        {
            var tutorial_link = FindViewById<TextView>(Resource.Id.tutorialLinkText);
            tutorial_link.MovementMethod = LinkMovementMethod.Instance;

            var warning_link = FindViewById<TextView>(Resource.Id.warningLinkText);
            warning_link.MovementMethod = LinkMovementMethod.Instance;
        }

        private void ConfigureHelpButtons()
        {
            var simple_button = FindViewById<ImageButton>(Resource.Id.simpleSetupButton);
            simple_button.Click += delegate
            {
                var standard_help = FindViewById<LinearLayout>(Resource.Id.standardHelpView);
                standard_help.Visibility = ViewStates.Visible;

                var advanced_help = FindViewById<LinearLayout>(Resource.Id.advancedHelpView);
                advanced_help.Visibility = ViewStates.Gone;
            };

            var advanced_button = FindViewById<ImageButton>(Resource.Id.advancedSetupButton);
            advanced_button.Click += delegate
            {
                var standard_help = FindViewById<LinearLayout>(Resource.Id.standardHelpView);
                standard_help.Visibility = ViewStates.Gone;

                var advanced_help = FindViewById<LinearLayout>(Resource.Id.advancedHelpView);
                advanced_help.Visibility = ViewStates.Visible;
            };
        }

        protected override void OnStart()
        {
            base.OnStart();
        }
    }
}
