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
        private const String NEW_PROFILE_ITEM = "New Profile";
        private int vibration_spinner_count = 0;
        private int profile_spinner_count = 0;
        private int layout_spinner_count = 0;
        private MessageDispatcher dispatcher;
        private ControlId selected_control = ControlId.A_Button;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "OnCreate()");
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.controller_settings);

            dispatcher = MessageDispatcher.GetInstance();
            dispatcher.Subscribe(MessageType.ControlUpdate, this);
            UpdateLayoutVisibility();
            ConfigureHelpButton();
            ConfigureBackgroundButton();
            ConfigureControlLabel(selected_control);
            MotionConfigurationActivity.control = selected_control;
            ControlSelectionActivity.control = selected_control;
            ConfigureControlButton();
            ConfigureVibrationSpinner();
            ConfigureAnalogSpinner();
            ConfigureButtonBehaviorSpinner();
            ConfigureProfileSpinner();
            ConfigureLayoutSpinner();
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

        private void SetInputListener(View vin, ControlId id)
        {
            vin.SetOnTouchListener(new SwButtonListener(vin, id, true));
        }

        private void SetJoystickListener(View joystick, ControlId id)
        {
            var joystick_frame = (FrameLayout)joystick;
            joystick_frame.RemoveAllViews();
            SurfaceView joystickSurface = new SurfaceView(this.BaseContext);
            joystick_frame.AddView(joystickSurface);
            var listener = new SwJoystickListener(joystickSurface, id, true);
            joystickSurface.SetOnTouchListener(listener);
        }

        private void SetInputListeners(ViewGroup keyboard_view_group)
        {
            Log.Debug(TAG, "SetInputListeners");

            
            foreach (var key in RESOURCE_TO_CONTROL_MAP.Keys)
            {
                View control = keyboard_view_group.FindViewById<View>(key);
                if (control == null) {
                    continue;
                }
                var control_id = RESOURCE_TO_CONTROL_MAP[control.Id];
                if (IsAnalogControl(control_id))
                {
                    SetJoystickListener(control, control_id);
                }
                else
                {
                    SetInputListener(control, control_id);
                }
            }
        }

        private void ConfigureControlLabel(ControlId control)
        {
            Log.Debug(TAG, "ConfigureControlLabel");
            var label = FindViewById<TextView>(Resource.Id.inputName);
            label.Text = CONTROL_TO_STRING_MAP[control];
        }

        private void ConfigureControlButton()
        {
            Log.Debug(TAG, "ConfigureControlButton");
            var button = FindViewById<Button>(Resource.Id.inputKeycode);

            button.Click += delegate
            {
                Log.Debug(TAG, "ControlSelectionActivity.control = " + ControlSelectionActivity.control.ToString());
                Log.Debug(TAG, "MotionConfigurationActivity.control = " + MotionConfigurationActivity.control.ToString());
                StartActivity(typeof(ControlSelectionActivity));
            };
        }

        private void VibrationSpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Log.Debug(TAG, "VibrationSpinnerItemSelected: " + vibration_spinner_count.ToString());
            // Ignore the initial "Item Selected" calls during UI setup
            if (vibration_spinner_count != 0)
            {
                vibration_spinner_count--;
                return;
            }
            Spinner spinner = (Spinner)sender;
            var vibration_string = string.Format("{0}", spinner.GetItemAtPosition(e.Position));

            var enable = VIBRATION_TO_STRING_MAP[vibration_string];

            SetVibrationEnable(enable);
        }

        private void AnalogSpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Log.Debug(TAG, "AnalogSpinnerItemSelected");
            // Ignore the initial "Item Selected" calls during UI setup
            if ((!IsAnalogControl(selected_control)) ||
                (GetControlMotion(selected_control).type == MotionType.Invalid))
            {
                Log.Debug(TAG, "Cannot set direction for control");
                return;
            }
            Spinner spinner = (Spinner)sender;
            var direction_string = string.Format("{0}", spinner.GetItemAtPosition(e.Position));

            var direction = DIRECTION_TO_STRING_MAP[direction_string];

            var motion = GetControlMotion(selected_control);
            motion.directionCount = direction;
            SetControlMotion(selected_control, motion);
        }

        private void UpdateProfileSpinner()
        {
            var spinner = FindViewById<Spinner>(Resource.Id.controllerProfile);

            var keymaps = new List<string> { NEW_PROFILE_ITEM };
            keymaps.AddRange(GetKeymapList());
            var adapter = new ArrayAdapter<string>(this, Resource.Layout.spinner_item, keymaps);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

            spinner.Adapter = adapter;
            int spinner_position = adapter.GetPosition(GetSelectedKeymap());
            profile_spinner_count++;
            spinner.SetSelection(spinner_position);
            spinner.Invalidate();
        }

        private void ProfileSpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Log.Debug(TAG, "ProfileSpinnerItemSelected: " + profile_spinner_count.ToString());
            // Ignore the initial "Item Selected" calls during UI setup
            if (profile_spinner_count != 0)
            {
                profile_spinner_count--;
                return;
            }
            Spinner spinner = (Spinner)sender;
            var profile_string = string.Format("{0}", spinner.GetItemAtPosition(e.Position));
            if (profile_string == NEW_PROFILE_ITEM)
            {
                EditText input = new EditText(this);
                Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
                dialog.SetTitle("Enter Profile Name");
                dialog.SetView(input);
                dialog.SetPositiveButton("OK", (c, ev) =>
                {
                    var keymap_name = input.Text.Trim();
                    keymap_name = keymap_name.Replace(' ', '_');
                    SetSelectedKeymap(keymap_name);
                    UpdateProfileSpinner();
                    // Add the new profile to the notification tray
                    SwDisplayManager.SetNotification();
                });
                dialog.Show();
            }
            else
            {
                UpdateProfileSpinner();
                ProfileSpinnerItemPrompt(profile_string);
            }
        }

        private void ProfileSpinnerItemPrompt(string profile_name)
        {
            Log.Debug(TAG, "ProfileSpinnerItemPrompt");

            Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
            var alert = dialog.Create();
            alert.SetTitle("Profile \"" + profile_name + "\"");
            alert.SetButton("Use", (c, ev) => {
                SetSelectedKeymap(profile_name);
                UpdateProfileSpinner();
                RefreshAnalogSpinner();
                RefreshButtonBehaviorSpinner();
            });
            alert.SetButton2("Cancel", (c, ev) => { });
            alert.SetButton3("Delete", (c, ev) =>
            {
                DeleteStoredKeymap(profile_name);
                SetSelectedKeymap(Default_Keymap_Filename);
                UpdateProfileSpinner();
                RefreshAnalogSpinner();
                RefreshButtonBehaviorSpinner();
                // Remove the profile from the notification tray
                SwDisplayManager.SetNotification();
            });
            alert.Show();
        }

        private void ConfigureProfileSpinner()
        {
            Log.Debug(TAG, "ConfigureProfileSpinner");
            var spinner = FindViewById<Spinner>(Resource.Id.controllerProfile);
            spinner.Prompt = "Select Controller Profile";
            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(ProfileSpinnerItemSelected);
            
            UpdateProfileSpinner();
        }

        private void UpdateLayoutVisibility()
        {
            var inputViewA = FindViewById<ViewGroup>(Resource.Id.imeKeyViewA);
            var inputViewB = FindViewById<ViewGroup>(Resource.Id.imeKeyViewB);
            var inputViewC = FindViewById<ViewGroup>(Resource.Id.imeKeyViewC);

            inputViewA.Visibility = ViewStates.Gone;
            inputViewB.Visibility = ViewStates.Gone;
            inputViewC.Visibility = ViewStates.Gone;

            switch (GetSelectedLayout())
            {
                case (Resource.Layout.input_b):
                    inputViewB.Visibility = ViewStates.Visible;
                    SetInputListeners(inputViewB);
                    break;
                case (Resource.Layout.input_c):
                    inputViewC.Visibility = ViewStates.Visible;
                    SetInputListeners(inputViewC);
                    break;
                default:
                    inputViewA.Visibility = ViewStates.Visible;
                    SetInputListeners(inputViewA);
                    break;
            }
        }

        private void LayoutSpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Log.Debug(TAG, "LayoutSpinnerItemSelected: " + layout_spinner_count.ToString());
            // Ignore the initial "Item Selected" calls during UI setup
            if (layout_spinner_count != 0)
            {
                layout_spinner_count--;
                return;
            }
            Spinner spinner = (Spinner)sender;
            var layout_string = string.Format("{0}", spinner.GetItemAtPosition(e.Position));
            var layout = LAYOUT_TO_STRING_MAP[layout_string];
            SetSelectedLayout(layout);
            UpdateLayoutVisibility();
        }

        private void ConfigureLayoutSpinner()
        {
            Log.Debug(TAG, "ConfigureLayoutSpinner");
            var spinner = FindViewById<Spinner>(Resource.Id.controllerLayout);
            spinner.Prompt = "Select Controller Layout";

            var set_layout = GetSelectedLayout();
            var set_layout_string = "";
            List<string> inputNames = new List<string>();
            foreach (var layout_str in LAYOUT_TO_STRING_MAP.Keys)
            {
                inputNames.Add(layout_str);
                if (set_layout == LAYOUT_TO_STRING_MAP[layout_str])
                {
                    set_layout_string = layout_str;
                }
            }
            var adapter = new ArrayAdapter<string>(this, Resource.Layout.spinner_item, inputNames);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;

            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(LayoutSpinnerItemSelected);

            int spinner_position = adapter.GetPosition(set_layout_string);
            layout_spinner_count++;
            spinner.SetSelection(spinner_position);

            spinner.Invalidate();
        }

        private void ConfigureVibrationSpinner()
        {
            Log.Debug(TAG, "ConfigureVibrationSpinner");
            var spinner = FindViewById<Spinner>(Resource.Id.vibrationEnable);
            spinner.Prompt = "Enable/Disable vibration on button press";

            var set_vibration = GetVibrationEnable();
            var set_vibration_string = "";
            List<string> inputNames = new List<string>();
            foreach (var vib_str in VIBRATION_TO_STRING_MAP.Keys)
            {
                inputNames.Add(vib_str);
                if (set_vibration == VIBRATION_TO_STRING_MAP[vib_str])
                {
                    set_vibration_string = vib_str;
                }
            }
            var adapter = new ArrayAdapter<string>(this, Resource.Layout.spinner_item, inputNames);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;

            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(VibrationSpinnerItemSelected);

            int spinner_position = adapter.GetPosition(set_vibration_string);
            vibration_spinner_count++;
            spinner.SetSelection(spinner_position);

            spinner.Invalidate();
        }

        private void ButtonBehaviorSpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Log.Debug(TAG, "ButtonBehaviorSpinnerItemSelected");
            // Ignore the initial "Item Selected" calls during UI setup
            if (IsAnalogControl(selected_control))
            {
                Log.Debug(TAG, "Cannot set button behavior for control");
                return;
            }
            Spinner spinner = (Spinner)sender;
            var behavior_string = string.Format("{0}", spinner.GetItemAtPosition(e.Position));

            var behavior = StringToButtonBehavior(behavior_string);
            SetControlBehavior(selected_control, behavior);
        }

        private void RefreshButtonBehaviorSpinner()
        {
            Log.Debug(TAG, "RefreshButtonBehaviorSpinner");
            var behaviorGrid = FindViewById<GridLayout>(Resource.Id.buttonBehaviorGrid);
            if (IsAnalogControl(selected_control))
            {
                behaviorGrid.Visibility = ViewStates.Gone;
                return;
            }
            
            behaviorGrid.Visibility = ViewStates.Visible;
            var spinner = FindViewById<Spinner>(Resource.Id.buttonBehavior);
            var set_behavior = GetControlBehavior(selected_control);
            var set_behavior_string = ButtonBehaviorToString(set_behavior);

            var adapter = (ArrayAdapter)spinner.Adapter;
            int spinner_position = adapter.GetPosition(set_behavior_string);
            spinner.SetSelection(spinner_position);

            spinner.Invalidate();
        }

        private void ConfigureButtonBehaviorSpinner()
        {
            Log.Debug(TAG, "ConfigureButtonBehaviorSpinner");

            var spinner = FindViewById<Spinner>(Resource.Id.buttonBehavior);
            spinner.Prompt = "Select the behavior of the button on press";

            List<string> inputNames = new List<string>();
            foreach (ButtonBehavior behavior in Enum.GetValues(typeof(ButtonBehavior)))
            {
                var behavior_string = ButtonBehaviorToString(behavior);
                inputNames.Add(behavior_string);
            }
            var adapter = new ArrayAdapter<string>(this, Resource.Layout.spinner_item, inputNames);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;

            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(ButtonBehaviorSpinnerItemSelected);
            RefreshButtonBehaviorSpinner();
        }

        private void RefreshAnalogSpinner()
        {
            Log.Debug(TAG, "RefreshAnalogSpinner");
            var analogDirectionGrid = FindViewById<GridLayout>(Resource.Id.analogDirectionGrid);
            var spinner = FindViewById<Spinner>(Resource.Id.analogDirections);

            var set_direction = GetControlMotion(selected_control).directionCount;

            Log.Debug(TAG, "Control = " + CONTROL_TO_STRING_MAP[selected_control] + " directions = " + set_direction.ToString());

            if (!IsAnalogControl(selected_control))
            {
                analogDirectionGrid.Visibility = ViewStates.Gone;
                return;
            }
            else if (GetControlMotion(selected_control).type == MotionType.Invalid)
            {
                analogDirectionGrid.Visibility = ViewStates.Visible;
                spinner.Visibility = ViewStates.Invisible;
                return;
            }

            analogDirectionGrid.Visibility = ViewStates.Visible;
            spinner.Visibility = ViewStates.Visible;
            var set_direction_string = "";
            foreach (var key in DIRECTION_TO_STRING_MAP.Keys)
            {
                if (DIRECTION_TO_STRING_MAP[key] == set_direction)
                {
                    set_direction_string = key;
                    break;
                }
            }

            var adapter = (ArrayAdapter)spinner.Adapter;
            int spinner_position = adapter.GetPosition(set_direction_string);
            spinner.SetSelection(spinner_position);

            spinner.Invalidate();
        }

        private void ConfigureAnalogSpinner()
        {
            Log.Debug(TAG, "ConfigureAnalogSpinner");

            var spinner = FindViewById<Spinner>(Resource.Id.analogDirections);
            spinner.Prompt = "Select the number of analog directions";

            List<string> inputNames = new List<string>();
            foreach (var dir_str in DIRECTION_TO_STRING_MAP.Keys)
            {
                inputNames.Add(dir_str);
            }
            var adapter = new ArrayAdapter<string>(this, Resource.Layout.spinner_item, inputNames);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;

            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(AnalogSpinnerItemSelected);
            RefreshAnalogSpinner();
        }

        public void Accept(SystemMessage message)
        {
            if (message.getMessageType() != MessageType.ControlUpdate)
            {
                return;
            }
            var control_message = (ControlUpdateMessage)message;
            selected_control = control_message.Id;
            switch (control_message.Update)
            {
                case ControlUpdateMessage.UpdateType.Pressed:
                    Log.Debug(TAG, "Accept(UpdateType.Pressed) " + CONTROL_TO_STRING_MAP[selected_control]);
                    {
                        if (IsAnalogControl(selected_control))
                        {
                            ConfigureControlLabel(selected_control);
                            MotionConfigurationActivity.control = (ControlId)GetAnalogFromDirection(selected_control);
                            ControlSelectionActivity.control = selected_control;
                            RefreshAnalogSpinner();
                            RefreshButtonBehaviorSpinner();
                        }
                    }
                    break;
                case ControlUpdateMessage.UpdateType.Released:
                    Log.Debug(TAG, "Accept(UpdateType.Released)" + CONTROL_TO_STRING_MAP[selected_control]);
                    {
                        if (!IsAnalogControl(selected_control))
                        {
                            ConfigureControlLabel(selected_control);
                            MotionConfigurationActivity.control = selected_control;
                            ControlSelectionActivity.control = selected_control;
                            RefreshAnalogSpinner();
                            RefreshButtonBehaviorSpinner();
                        }
                    }
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
