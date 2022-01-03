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

namespace SoftWing
{
    [Activity(Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTask)]
    public class MotionSelectionActivity : AppCompatActivity, IOnClickListener
    {
        private const String TAG = "MotionSelectionActivity";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "OnCreate()");
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.motion_selection);

            ImageView actionImage = (ImageView)FindViewById(Resource.Id.touch_action_image);
            Glide.With(this)
           .Load(Resource.Drawable.touch_actions)
           .Apply(new RequestOptions())
           .Into(new DrawableImageViewTarget(actionImage));

            Button tapButton = (Button)FindViewById(Resource.Id.touch_action_tap);
            tapButton.SetOnClickListener(this);
            Button swipeButton = (Button)FindViewById(Resource.Id.touch_action_swipe);
            swipeButton.SetOnClickListener(this);
            Button continuousButton = (Button)FindViewById(Resource.Id.touch_action_continuous);
            continuousButton.SetOnClickListener(this);
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
    }
}
