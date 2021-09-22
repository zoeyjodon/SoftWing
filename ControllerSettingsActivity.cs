using Android.App;
using Android.OS;
using Android.Support.V7.App;
using SoftWing.SwSystem;
using System;
using Android.Util;
using Android.Widget;
using Android.Views;
using System.Collections.Generic;
using Android.Content.PM;
using Com.Jackandphantom.Joystickview;
using SoftWing.SwSystem.Messages;
using Android.Content;
using Android.Runtime;

namespace SoftWing
{
    [Activity(Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTask)]
    public class ControllerSettingsActivity : AppCompatActivity, MessageSubscriber
    {
        private const String TAG = "ControllerSettingsActivity";
        private int ignore_keyset_count = 0;
        private int ignore_delayset_count = 0;
        private MessageDispatcher dispatcher;
        private SwSettings.ControlId selected_control = SwSettings.ControlId.A_Button;
        private const int REQUEST_IMAGE_FILE_CALLBACK = 302;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "OnCreate()");
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.controller_settings);

            dispatcher = MessageDispatcher.GetInstance(this);
            dispatcher.Subscribe(MessageType.ControlUpdate, this);
            var inputKeyView = FindViewById<ViewGroup>(Resource.Id.imeKeyView);
            SetInputListeners(inputKeyView);
            ConfigureHelpButton();
            ConfigureResetButton();
            ConfigureControlLabel(selected_control);
            ConfigureControlSpinner(selected_control);
            ConfigureControlSpinner(selected_control);
            ConfigureDelaySpinner();
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        private void ConfigureHelpButton()
        {
            var help_button = FindViewById<ImageButton>(Resource.Id.controllerHelpButton);
            help_button.Click += delegate
            {
                StartActivity(typeof(ControllerHelpActivity));
            };
        }

        private void ConfigureResetButton()
        {
            var reset_button = FindViewById<ImageButton>(Resource.Id.resetToDefaultButton);
            reset_button.Click += delegate
            {
                SwSettings.SetDefaultKeycodes();
                RefreshUISpinner();
                var toast = Toast.MakeText(this, "The controller has been reset to default", ToastLength.Short);
                toast.Show();
            };
        }

        private void SetJoystickListener(JoyStickView joystick, Android.Views.Keycode up, Android.Views.Keycode down, Android.Views.Keycode left, Android.Views.Keycode right)
        {
            var listener = new SwJoystickListener(up, down, left, right);
            joystick.SetOnMoveListener(listener);
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
                            var up = SwSettings.Default_L_Analog_Up;
                            var down = SwSettings.Default_L_Analog_Down;
                            var left = SwSettings.Default_L_Analog_Left;
                            var right = SwSettings.Default_L_Analog_Right;
                            SetJoystickListener((JoyStickView)nextChild, up, down, left, right);
                        }
                        break;
                    case (Resource.Id.right_joyStick):
                        {
                            var up = SwSettings.Default_R_Analog_Up;
                            var down = SwSettings.Default_R_Analog_Down;
                            var left = SwSettings.Default_R_Analog_Left;
                            var right = SwSettings.Default_R_Analog_Right;
                            SetJoystickListener((JoyStickView)nextChild, up, down, left, right);
                        }
                        break;
                    case (Resource.Id.d_pad_up):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, SwSettings.Default_D_Pad_Up));
                        break;
                    case (Resource.Id.d_pad_down):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, SwSettings.Default_D_Pad_Down));
                        break;
                    case (Resource.Id.d_pad_left):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, SwSettings.Default_D_Pad_Left));
                        break;
                    case (Resource.Id.d_pad_right):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, SwSettings.Default_D_Pad_Right));
                        break;
                    case (Resource.Id.d_pad_center):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, SwSettings.Default_D_Pad_Center));
                        break;
                    case (Resource.Id.a_button):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, SwSettings.Default_A_Button));
                        break;
                    case (Resource.Id.b_button):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, SwSettings.Default_B_Button));
                        break;
                    case (Resource.Id.y_button):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, SwSettings.Default_Y_Button));
                        break;
                    case (Resource.Id.x_button):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, SwSettings.Default_X_Button));
                        break;
                    case (Resource.Id.l_button):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, SwSettings.Default_L_Button));
                        break;
                    case (Resource.Id.r_button):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, SwSettings.Default_R_Button));
                        break;
                    case (Resource.Id.start_button):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, SwSettings.Default_Start_Button));
                        break;
                    default:
                        break;
                }
            }
        }

        private void ControlSpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
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
            // Special case: Touch input
            if (key == Keycode.Unknown)
            {
                SelectImageFile();
            }
            else
            {
                SwSettings.SetControlKeycode(selected_control, key);
            }
        }

        private void ConfigureControlLabel(SwSettings.ControlId control)
        {
            Log.Debug(TAG, "ConfigureControlLabel");
            var label = FindViewById<TextView>(Resource.Id.inputName);
            label.Text = SwSettings.CONTROL_TO_STRING_MAP[control];
        }

        private void ConfigureControlSpinner(SwSettings.ControlId control)
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

            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(ControlSpinnerItemSelected);
            ignore_keyset_count++;

            int spinner_position = adapter.GetPosition(set_key_string);
            spinner.SetSelection(spinner_position);

            spinner.Invalidate();
        }

        private void DelaySpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Log.Debug(TAG, "DelaySpinnerItemSelected");
            // Ignore the initial "Item Selected" calls during UI setup
            if (ignore_delayset_count != 0)
            {
                ignore_delayset_count--;
                return;
            }
            Spinner spinner = (Spinner)sender;
            var delay_string = string.Format("{0}", spinner.GetItemAtPosition(e.Position));

            var delay = SwSettings.DELAY_TO_STRING_MAP[delay_string];

            SwSettings.SetTransitionDelayMs(delay);
        }

        private void ConfigureDelaySpinner()
        {
            Log.Debug(TAG, "ConfigureDelaySpinner");
            var spinner = FindViewById<Spinner>(Resource.Id.transitionDelay);
            spinner.Prompt = "Select transition delay (seconds)";

            var set_delay = SwSettings.GetTransitionDelayMs();
            var set_delay_string = "";
            List<string> inputNames = new List<string>();
            foreach (var delay_str in SwSettings.DELAY_TO_STRING_MAP.Keys)
            {
                inputNames.Add(delay_str);
                if (set_delay == SwSettings.DELAY_TO_STRING_MAP[delay_str])
                {
                    set_delay_string = delay_str;
                }
            }
            var adapter = new ArrayAdapter<string>(this, Resource.Layout.spinner_item, inputNames);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;

            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(DelaySpinnerItemSelected);
            ignore_delayset_count++;

            int spinner_position = adapter.GetPosition(set_delay_string);
            spinner.SetSelection(spinner_position);

            spinner.Invalidate();
        }

        void RefreshUISpinner()
        {
            var spinner = FindViewById<Spinner>(Resource.Id.inputKeycode);
            var set_key_code = SwSettings.GetControlKeycode(selected_control);
            var set_key_string = "";
            foreach (var key_string in SwSettings.STRING_TO_KEYCODE_MAP.Keys)
            {
                if (set_key_code == SwSettings.STRING_TO_KEYCODE_MAP[key_string])
                {
                    set_key_string = key_string;
                }
            }

            var adapter = (ArrayAdapter<string>)spinner.Adapter;
            int spinner_position = adapter.GetPosition(set_key_string);
            spinner.SetSelection(spinner_position);
        }

        private bool IsAnalogControl(SwSettings.ControlId id)
        {
            switch (id)
            {
                case SwSettings.ControlId.L_Analog_Up:
                case SwSettings.ControlId.L_Analog_Down:
                case SwSettings.ControlId.L_Analog_Left:
                case SwSettings.ControlId.L_Analog_Right:
                case SwSettings.ControlId.R_Analog_Up:
                case SwSettings.ControlId.R_Analog_Down:
                case SwSettings.ControlId.R_Analog_Left:
                case SwSettings.ControlId.R_Analog_Right:
                    return true;
                default:
                    return false;
            }
        }

        public void Accept(SystemMessage message)
        {
            if (message.getMessageType() != MessageType.ControlUpdate)
            {
                return;
            }
            var control_message = (ControlUpdateMessage)message;
            var key_code = control_message.Key;
            switch (control_message.Update)
            {
                case ControlUpdateMessage.UpdateType.Pressed:
                    Log.Debug(TAG, "Accept(UpdateType.Pressed)");
                    {
                        selected_control = SwSettings.DefaultKeycodeToControlId(key_code);
                        if (IsAnalogControl(selected_control))
                        {
                            ConfigureControlLabel(selected_control);
                            ConfigureControlSpinner(selected_control);
                        }
                    }
                    break;
                case ControlUpdateMessage.UpdateType.Released:
                    Log.Debug(TAG, "Accept(UpdateType.Released)");
                    {
                        selected_control = SwSettings.DefaultKeycodeToControlId(key_code);
                        if (!IsAnalogControl(selected_control))
                        {
                            ConfigureControlLabel(selected_control);
                            ConfigureControlSpinner(selected_control);
                        }
                    }
                    break;
                default:
                    break;
            }

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
                    break;
                default:
                    Log.Debug(TAG, "Ignoring Activity Result");
                    break;
            }
        }
    }
}
