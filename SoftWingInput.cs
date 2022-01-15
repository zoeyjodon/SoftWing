using Android.App;
using Android.Content;
using Android.InputMethodServices;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using System;
using SoftWing.SwSystem.Messages;
using Com.Jackandphantom.Joystickview;
using SoftWing.SwSystem;
using Android.Content.PM;

namespace SoftWing
{
    [Activity(Label = "SoftWingInput", ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTask)]
    public class SoftWingInput : Activity
    {
        private const String TAG = "SoftWingInput";
        private const int MULTI_DISPLAY_HEIGHT_PX = 1240;
        private View? keyboardView = null;
        private static SoftWingInput instance;

        public static void StartSoftWingInput(int display_id)
        {
            Log.Debug(TAG, "StartSoftWingInput");
            Intent intent = new Intent(Application.Context, typeof(SoftWingInput));
            ActivityOptions options = ActivityOptions.MakeBasic();
            options.SetLaunchDisplayId(display_id);
            intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.MultipleTask);
            if (instance != null)
            {
                Log.Debug(TAG, "Input instance exists, restarting");
                instance.Finish();
            }
            Application.Context.StartActivity(intent, options.ToBundle());
        }

        public static void StopSoftWingInput()
        {
            Log.Debug(TAG, "StopSoftWingInput");
            if (instance == null)
            {
                Log.Debug(TAG, "Input instance does not exist, ignoring");
                return;
            }
            instance.Finish();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "onCreate()");
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // Force the controller into a full screen view
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            RequestWindowFeature(WindowFeatures.NoTitle);
            var uiOptions = SystemUiFlags.HideNavigation |
                 SystemUiFlags.LayoutHideNavigation |
                 SystemUiFlags.LayoutFullscreen |
                 SystemUiFlags.Fullscreen |
                 SystemUiFlags.LayoutStable |
                 SystemUiFlags.ImmersiveSticky;
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;

            keyboardView = LayoutInflater.Inflate(Resource.Layout.input, null);
            keyboardView.SetMinimumHeight(MULTI_DISPLAY_HEIGHT_PX);
            SetContentView(keyboardView);

            SwDisplayManager.StartSwDisplayManager();
            SetInputListeners((ViewGroup)keyboardView);
            instance = this;
        }

        public override void Finish()
        {
            Log.Debug(TAG, "Finish()");
            base.Finish();
            instance = null;
        }

        private void SetJoystickListener(JoyStickView joystick, SwSettings.ControlId cid)
        {
            var motion = SwSettings.GetControlMotion(cid);
            joystick.SetOnMoveListener(new SwJoystickListener(motion));
        }

        private void SetInputListener(View vin, SwSettings.ControlId cid)
        {
            var motion = SwSettings.GetControlMotion(cid);
            var vibrate = SwSettings.GetVibrationEnable();
            vin.SetOnTouchListener(new SwButtonListener(vin, motion, vibrate));
        }

        private void SetInputListeners(ViewGroup keyboard_view_group)
        {
            Log.Debug(TAG, "SetInputListeners");
            foreach (var key in SwSettings.RESOURCE_TO_CONTROL_MAP.Keys)
            {
                View control = FindViewById<View>(key);
                var control_id = SwSettings.RESOURCE_TO_CONTROL_MAP[key];
                switch (key)
                {
                    case (Resource.Id.left_joyStick):
                    case (Resource.Id.right_joyStick):
                        SetJoystickListener((JoyStickView)control, control_id);
                        break;
                    default:
                        SetInputListener(control, control_id);
                        break;
                }
            }
        }
    }
}
