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
        private const int REQUEST_IMAGE_FILE_CALLBACK = 302;

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

        private void PromptUserForBackgroundImage()
        {
            Log.Debug(TAG, "PromptUserForBackgroundImage()");

            Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
            var alert = dialog.Create();
            alert.SetTitle("Select a Background Image");
            var message = "In order to properly map touch controls to a game, please select a screenshot from your game to be used as a background during the setup process\n";

            alert.SetMessage(message);
            alert.SetButton("Continue", (c, ev) =>
            {
                SelectImageFile();
            });
            alert.Show();
        }

        private void SelectImageFile()
        {
            Intent intent = new Intent(Intent.ActionOpenDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("image/*");
            StartActivityForResult(intent, REQUEST_IMAGE_FILE_CALLBACK);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (data == null)
            {
                Log.Debug(TAG, "OnActivityResult received null");
                return;
            }
            Log.Debug(TAG, "OnActivityResult " + data.Data.ToString());
            base.OnActivityResult(requestCode, resultCode, data);
            switch (requestCode)
            {
                case REQUEST_IMAGE_FILE_CALLBACK:
                    MotionConfigurationActivity.BackgroundImageUri = data.Data;
                    StartActivity(typeof(MotionConfigurationActivity));
                    Finish();
                    break;
                default:
                    Log.Debug(TAG, "Ignoring Activity Result");
                    break;
            }
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
            PromptUserForBackgroundImage();
        }
    }
}
