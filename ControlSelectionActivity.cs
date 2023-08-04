using Android.App;
using Android.OS;
using Android.Support.V7.App;
using System;
using Android.Util;
using Android.Views;
using Android.Content.PM;
using SoftWing.SwSystem.Messages;
using static Android.Views.View;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request.Target;
using Bumptech.Glide.Request;
using Android.Content;
using Android.Runtime;
using SoftWing.SwSystem;
using System.Collections.Generic;
using static SoftWing.SwSystem.SwSettings;

namespace SoftWing
{
    [Activity(Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTask)]
    public class ControlSelectionActivity : AppCompatActivity, IOnClickListener
    {
        private const String TAG = "MotionSelectionActivity";
        private int ignore_keyset_count = 0;
        private SwSettings.ControlId control;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "OnCreate()");
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.control_selection);

            ImageView actionImage = (ImageView)FindViewById(Resource.Id.touch_action_image);
            Glide.With(this)
           .Load(Resource.Drawable.touch_actions)
           .Apply(new RequestOptions())
           .Into(new DrawableImageViewTarget(actionImage));

            control = MotionConfigurationActivity.control;

            Button tapButton = (Button)FindViewById(Resource.Id.touch_action_tap);
            tapButton.SetOnClickListener(this);
            Button swipeButton = (Button)FindViewById(Resource.Id.touch_action_swipe);
            swipeButton.SetOnClickListener(this);
            Button continuousButton = (Button)FindViewById(Resource.Id.touch_action_continuous);
            continuousButton.SetOnClickListener(this);
            ConfigureKeycodeSpinner();
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.touch_action_tap:
                    MotionConfigurationActivity.motionType = MotionType.Tap;
                    break;
                case Resource.Id.touch_action_swipe:
                    MotionConfigurationActivity.motionType = MotionType.Swipe;
                    break;
                case Resource.Id.touch_action_continuous:
                    MotionConfigurationActivity.motionType = MotionType.Continuous;
                    break;
                default:
                    return;
            }
            StartActivity(typeof(MotionConfigurationActivity));
            Finish();
        }

        private void ConfigureKeycodeSpinner()
        {
            Log.Debug(TAG, "ConfigureControlSpinner");
            var spinner = FindViewById<Spinner>(Resource.Id.inputKeycode);
            spinner.Prompt = "Set " + SwSettings.CONTROL_TO_STRING_MAP[control] + " Keycode";

            var set_key_code = SwSettings.GetControlKeycode(control);
            var set_key_string = "";
            List<string> inputNames = new List<string>();
            foreach (var key_string in SwSettings.STRING_TO_KEYCODE_MAP.Keys)
            {
                inputNames.Add(key_string);
                if (set_key_code == SwSettings.STRING_TO_KEYCODE_MAP[key_string])
                {
                    set_key_string = key_string;
                }
            }
            var adapter = new ArrayAdapter<string>(this, Resource.Layout.spinner_item, inputNames);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;

            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(KeycodeSpinnerItemSelected);
            ignore_keyset_count++;

            int spinner_position = adapter.GetPosition(set_key_string);
            spinner.SetSelection(spinner_position);

            spinner.Invalidate();
        }

        private void KeycodeSpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Log.Debug(TAG, "ControlSpinnerItemSelected");
            // Ignore the initial "Item Selected" calls during UI setup
            if (ignore_keyset_count != 0)
            {
                ignore_keyset_count--;
                return;
            }
            Spinner spinner = (Spinner)sender;
            var key_string = string.Format("{0}", spinner.GetItemAtPosition(e.Position));

            var key = SwSettings.STRING_TO_KEYCODE_MAP[key_string];
            if (SwSettings.GetControlKeycode(control) == key)
            {
                Log.Debug(TAG, "Item already selected, ignoring");
                return;
            }
            SwSettings.SetControlKeycode(control, key);
            Finish();
        }
    }
}
