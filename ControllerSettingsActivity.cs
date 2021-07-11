using Android.App;
using Android.OS;
using Android.Support.V7.App;
using SoftWing.System;
using System;
using Android.Util;
using Android.Widget;
using Android.Views;
using System.Collections.Generic;
using Android.Content.PM;

namespace SoftWing
{
    [Activity(Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ControllerSettingsActivity : AppCompatActivity
    {
        private const String TAG = "ControllerSettingsActivity";
        private Dictionary<int, SwSettings.ControlId> spinnerToControlMap = new Dictionary<int, SwSettings.ControlId>();
        private int ignore_keyset_count = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "OnCreate()");
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.controller_settings);

            CreateControlConfiguration();
            ConfigureResetButton();
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        private void ConfigureResetButton()
        {
            Button reset_button = FindViewById<Button>(Resource.Id.resetToDefaultButton);
            reset_button.Click += delegate
            {
                SwSettings.SetDefaultKeycodes();
                RefreshUISpinners();
            };
        }

        private void SpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            // Ignore the initial "Item Selected" calls during UI setup
            if (ignore_keyset_count != 0)
            {
                ignore_keyset_count--;
                return;
            }
            Log.Debug(TAG, "SpinnerItemSelected");
            Spinner spinner = (Spinner)sender;
            var key_string = string.Format("{0}", spinner.GetItemAtPosition(e.Position));

            var key = SwSettings.STRING_TO_KEYCODE_MAP[key_string];
            var control = spinnerToControlMap[spinner.Id];

            SwSettings.SetControlKeycode(control, key);
        }

        private void CreateControlConfiguration()
        {
            Log.Debug(TAG, "CreateControlConfiguration");
            LinearLayout mainLayout = FindViewById<LinearLayout>(Resource.Id.controllerSettings);

            foreach (var control in SwSettings.CONTROL_TO_STRING_MAP.Keys)
                mainLayout.AddView(CreateControlView(control));
        }

        private View CreateControlView(SwSettings.ControlId control)
        {
            var result = new LinearLayout(this);
            result.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
            result.Orientation = Orientation.Horizontal;

            result.AddView(CreateControlLabel(control));

            var control_spinner = CreateControlSpinner(control);
            result.AddView(control_spinner);
            spinnerToControlMap.Add(control_spinner.Id, control);

            return result;
        }

        private View CreateControlLabel(SwSettings.ControlId control)
        {
            var label = new TextView(this);
            label.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
            label.Text = SwSettings.CONTROL_TO_STRING_MAP[control];
            return label;
        }

        private View CreateControlSpinner(SwSettings.ControlId control)
        {
            var spinner = new Spinner(this);
            spinner.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent)
            {
                Gravity = GravityFlags.Right
            };
            spinner.Prompt = "Set " + SwSettings.CONTROL_TO_STRING_MAP[control] + " Keycode";
            spinner.Id = (int)control;

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
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, inputNames);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;

            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(SpinnerItemSelected);
            ignore_keyset_count++;

            int spinner_position = adapter.GetPosition(set_key_string);
            spinner.SetSelection(spinner_position);

            return spinner;
        }

        void RefreshUISpinners()
        {
            foreach (var spinner_id in spinnerToControlMap.Keys)
            {
                Spinner spinner = FindViewById<Spinner>(spinner_id);
                RefreshUISpinner(spinner);
            }
        }

        void RefreshUISpinner(Spinner spinner)
        {
            var control = spinnerToControlMap[spinner.Id];
            var set_key_code = SwSettings.GetControlKeycode(control);
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
    }
}