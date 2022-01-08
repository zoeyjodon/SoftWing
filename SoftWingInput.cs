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

        public static void StartSoftWingInput(Context calling_context, int display_id)
        {
            Log.Debug(TAG, "StartSoftWingInput");
            if (instance != null)
            {
                Log.Debug(TAG, "Input instance exists, skipping");
                return;
            }
            Intent intent = new Intent(calling_context, typeof(SoftWingInput));
            ActivityOptions options = ActivityOptions.MakeBasic();
            options.SetLaunchDisplayId(display_id);
            intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.MultipleTask);
            calling_context.StartActivity(intent, options.ToBundle());
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
        }

        public override void Finish()
        {
            Log.Debug(TAG, "Finish()");
            base.Finish();
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
            for (int index = 0; index < keyboard_view_group.ChildCount; index++)
            {
                View nextChild = keyboard_view_group.GetChildAt(index);
                switch (nextChild.Id)
                {
                    case (Resource.Id.left_joyStick):
                        {
                            SetJoystickListener((JoyStickView)nextChild,
                                SwSettings.ControlId.L_Analog);
                        }
                        break;
                    case (Resource.Id.right_joyStick):
                        {
                            SetJoystickListener((JoyStickView)nextChild,
                                SwSettings.ControlId.R_Analog);
                        }
                        break;
                    case (Resource.Id.d_pad_up):
                        SetInputListener(nextChild, SwSettings.ControlId.D_Pad_Up);
                        break;
                    case (Resource.Id.d_pad_down):
                        SetInputListener(nextChild, SwSettings.ControlId.D_Pad_Down);
                        break;
                    case (Resource.Id.d_pad_left):
                        SetInputListener(nextChild, SwSettings.ControlId.D_Pad_Left);
                        break;
                    case (Resource.Id.d_pad_right):
                        SetInputListener(nextChild, SwSettings.ControlId.D_Pad_Right);
                        break;
                    case (Resource.Id.d_pad_center):
                        SetInputListener(nextChild, SwSettings.ControlId.D_Pad_Center);
                        break;
                    case (Resource.Id.a_button):
                        SetInputListener(nextChild, SwSettings.ControlId.A_Button);
                        break;
                    case (Resource.Id.b_button):
                        SetInputListener(nextChild, SwSettings.ControlId.B_Button);
                        break;
                    case (Resource.Id.y_button):
                        SetInputListener(nextChild, SwSettings.ControlId.Y_Button);
                        break;
                    case (Resource.Id.x_button):
                        SetInputListener(nextChild, SwSettings.ControlId.X_Button);
                        break;
                    case (Resource.Id.l_button):
                        SetInputListener(nextChild, SwSettings.ControlId.L_Button);
                        break;
                    case (Resource.Id.r_button):
                        SetInputListener(nextChild, SwSettings.ControlId.R_Button);
                        break;
                    case (Resource.Id.start_button):
                        SetInputListener(nextChild, SwSettings.ControlId.Start_Button);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
