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
using static SoftWing.SwSystem.SwSettings;

namespace SoftWing
{
    [Activity(Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTask)]
    public class ControllerSettingsActivity : AppCompatActivity, MessageSubscriber
    {
        private const String TAG = "ControllerSettingsActivity";
        private const int REQUEST_IMAGE_FILE_CALLBACK = 302;
        private int ignore_spinner_count = 0;
        private MessageDispatcher dispatcher;
        private SwSettings.ControlId selected_control = SwSettings.ControlId.A_Button;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "OnCreate()");
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.controller_settings);

            dispatcher = MessageDispatcher.GetInstance();
            dispatcher.Subscribe(MessageType.ControlUpdate, this);
            var inputKeyView = FindViewById<ViewGroup>(Resource.Id.imeKeyView);
            SetInputListeners(inputKeyView);
            ConfigureHelpButton();
            ConfigureBackgroundButton();
            ConfigureControlLabel(selected_control);
            MotionConfigurationActivity.control = selected_control;
            ConfigureControlButton();
            ConfigureVibrationSpinner();
            ConfigureProfileSpinner();
            RefreshInputHighlighting();
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

        private void SelectImageFile()
        {
            Intent intent = new Intent(Intent.ActionOpenDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("image/*");
            StartActivityForResult(intent, REQUEST_IMAGE_FILE_CALLBACK);
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

        private void ConfigureBackgroundButton()
        {
            var button = FindViewById<Button>(Resource.Id.setBackgroundButton);
            button.Click += delegate
            {
                PromptUserForBackgroundImage();
            };
        }

        private void SetInputBackground(View vin, ControlId id)
        {
            Log.Debug(TAG, "SetInputBackground");
            if (id == MotionConfigurationActivity.control)
            {
                vin.SetBackgroundColor(Android.Graphics.Color.SkyBlue);
            }
            else if (SwSettings.GetControlMotion(id).type == MotionType.Invalid)
            {
                vin.SetBackgroundColor(Android.Graphics.Color.Red);
            }
            else
            {
                vin.SetBackgroundColor(Android.Graphics.Color.Transparent);
            }
        }

        private void RefreshInputHighlighting()
        {
            Log.Debug(TAG, "RefreshInputHighlighting");
            foreach (var key in RESOURCE_TO_CONTROL_MAP.Keys)
            {
                View control = FindViewById<View>(key);
                var control_id = RESOURCE_TO_CONTROL_MAP[key];
                SetInputBackground(control, control_id);
            }
        }

        private void SetInputListener(View vin, ControlId id)
        {
            vin.SetOnTouchListener(new SwButtonListener(vin, id, true));
        }

        private void SetJoystickListener(JoyStickView joystick, ControlId id)
        {
            var listener = new SwJoystickListener(id);
            joystick.SetOnMoveListener(listener);
        }

        private void SetInputListeners(ViewGroup keyboard_view_group)
        {
            Log.Debug(TAG, "SetInputListeners");

            foreach (var key in RESOURCE_TO_CONTROL_MAP.Keys)
            {
                View control = FindViewById<View>(key);
                var control_id = RESOURCE_TO_CONTROL_MAP[key];
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

        private void ConfigureControlLabel(SwSettings.ControlId control)
        {
            Log.Debug(TAG, "ConfigureControlLabel");
            var label = FindViewById<TextView>(Resource.Id.inputName);
            label.Text = SwSettings.CONTROL_TO_STRING_MAP[control];
        }

        private void ConfigureControlButton()
        {
            Log.Debug(TAG, "ConfigureControlButton");
            var button = FindViewById<Button>(Resource.Id.inputKeycode);

            button.Click += delegate
            {
                Log.Debug(TAG, "MotionConfigurationActivity.control = " + MotionConfigurationActivity.control.ToString());
                StartActivity(typeof(MotionSelectionActivity));
            };
        }

        private void VibrationSpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Log.Debug(TAG, "VibrationSpinnerItemSelected");
            // Ignore the initial "Item Selected" calls during UI setup
            if (ignore_spinner_count != 0)
            {
                ignore_spinner_count--;
                return;
            }
            Spinner spinner = (Spinner)sender;
            var vibration_string = string.Format("{0}", spinner.GetItemAtPosition(e.Position));

            var enable = SwSettings.VIBRATION_TO_STRING_MAP[vibration_string];

            SwSettings.SetVibrationEnable(enable);
        }

        private void ProfileSpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Log.Debug(TAG, "ProfileSpinnerItemSelected");
            // Ignore the initial "Item Selected" calls during UI setup
            if (ignore_spinner_count != 0)
            {
                ignore_spinner_count--;
                return;
            }
            Spinner spinner = (Spinner)sender;
            var profile_string = string.Format("{0}", spinner.GetItemAtPosition(e.Position));
            SwSettings.SetSelectedKeymap(profile_string);
            RefreshInputHighlighting();
        }

        private void ConfigureProfileSpinner()
        {
            Log.Debug(TAG, "ConfigureProfileSpinner");
            var spinner = FindViewById<Spinner>(Resource.Id.controllerProfile);
            spinner.Prompt = "Select Controller Profile";

            var set_keymap = SwSettings.GetSelectedKeymap();
            var adapter = new ArrayAdapter<string>(this, Resource.Layout.spinner_item, SwSettings.KEYMAP_FILENAMES);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;

            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(ProfileSpinnerItemSelected);
            ignore_spinner_count++;

            int spinner_position = adapter.GetPosition(set_keymap);
            spinner.SetSelection(spinner_position);

            spinner.Invalidate();
        }

        private void ConfigureVibrationSpinner()
        {
            Log.Debug(TAG, "ConfigureVibrationSpinner");
            var spinner = FindViewById<Spinner>(Resource.Id.vibrationEnable);
            spinner.Prompt = "Enable/Disable vibration on button press";

            var set_vibration = SwSettings.GetVibrationEnable();
            var set_vibration_string = "";
            List<string> inputNames = new List<string>();
            foreach (var vib_str in SwSettings.VIBRATION_TO_STRING_MAP.Keys)
            {
                inputNames.Add(vib_str);
                if (set_vibration == SwSettings.VIBRATION_TO_STRING_MAP[vib_str])
                {
                    set_vibration_string = vib_str;
                }
            }
            var adapter = new ArrayAdapter<string>(this, Resource.Layout.spinner_item, inputNames);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;

            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(VibrationSpinnerItemSelected);
            ignore_spinner_count++;

            int spinner_position = adapter.GetPosition(set_vibration_string);
            spinner.SetSelection(spinner_position);

            spinner.Invalidate();
        }

        private bool IsAnalogControl(SwSettings.ControlId id)
        {
            switch (id)
            {
                case SwSettings.ControlId.L_Analog:
                case SwSettings.ControlId.R_Analog:
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
            var selected_control = control_message.Id;
            switch (control_message.Update)
            {
                case ControlUpdateMessage.UpdateType.Pressed:
                    Log.Debug(TAG, "Accept(UpdateType.Pressed)");
                    {
                        if (IsAnalogControl(selected_control))
                        {
                            ConfigureControlLabel(selected_control);
                            MotionConfigurationActivity.control = selected_control;
                        }
                    }
                    break;
                case ControlUpdateMessage.UpdateType.Released:
                    Log.Debug(TAG, "Accept(UpdateType.Released)");
                    {
                        if (!IsAnalogControl(selected_control))
                        {
                            ConfigureControlLabel(selected_control);
                            MotionConfigurationActivity.control = selected_control;
                        }
                    }
                    RefreshInputHighlighting();
                    break;
                default:
                    break;
            }
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
                    var background_text = FindViewById<TextView>(Resource.Id.backgroundName);
                    background_text.Text = MotionConfigurationActivity.BackgroundImageUri.LastPathSegment;
                    break;
                default:
                    Log.Debug(TAG, "Ignoring Activity Result");
                    break;
            }
        }
    }
}
