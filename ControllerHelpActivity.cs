using Android.App;
using Android.OS;
using Android.Support.V7.App;
using System;
using Android.Util;
using Android.Content.PM;
using Android.Widget;
using Bumptech.Glide.Request.Target;
using Bumptech.Glide.Request;
using Bumptech.Glide;

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

            ImageView howToImage = (ImageView)FindViewById(Resource.Id.how_to_image);
            Glide.With(this)
           .Load(Resource.Drawable.how_to_use)
           .Apply(new RequestOptions())
           .Into(new DrawableImageViewTarget(howToImage));
        }

        protected override void OnStart()
        {
            base.OnStart();
        }
    }
}
