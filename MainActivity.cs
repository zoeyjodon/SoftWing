using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using System;
using Android.Util;
using Android.Widget;
using Android.Views.InputMethods;
using Android.Content;
using Android.Views;
using System.Collections.Generic;

namespace SoftWing
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private const String TAG = "MainActivity";
        private Dictionary<int, System.KeymapStorage.ControlId> spinnerToControlMap = new Dictionary<int, System.KeymapStorage.ControlId>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "OnCreate()");
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            CreateControlConfiguration();
            ForceInputOpen();
        }

        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            var key_string = string.Format("{0}", spinner.GetItemAtPosition(e.Position));
            Keycode key;
            System.KeymapStorage.ControlId control;

            System.KeymapStorage.STRING_TO_KEYCODE_MAP.TryGetValue(key_string.ToString(), out key);
            spinnerToControlMap.TryGetValue(spinner.Id, out control);

            System.KeymapStorage.SetControlKeycode(control, key);
        }

        private void CreateControlConfiguration()
        {
            LinearLayout mainLayout = FindViewById<LinearLayout>(Resource.Id.mainLayout);

            foreach (var control in System.KeymapStorage.CONTROL_TO_STRING_MAP.Keys)
                mainLayout.AddView(CreateControlView(control));
        }

        private View CreateControlView(System.KeymapStorage.ControlId control)
        {
            var result = new LinearLayout(this);
            result.Orientation = Orientation.Horizontal;

            result.AddView(CreateControlLabel(control));
            result.AddView(CreateControlSpinner(control));

            return result;
        }

        private View CreateControlLabel(System.KeymapStorage.ControlId control)
        {
            var label = new TextView(this);
            label.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
            label.Text = System.KeymapStorage.CONTROL_TO_STRING_MAP[control];
            return label;
        }

        private View CreateControlSpinner(System.KeymapStorage.ControlId control)
        {
            var spinner = new Spinner(this);
            spinner.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
            spinner.Prompt = "Set " + System.KeymapStorage.CONTROL_TO_STRING_MAP[control] + " Keycode";
            List<string> inputNames = new List<string>();
            foreach (var key in System.KeymapStorage.STRING_TO_KEYCODE_MAP.Keys)
                inputNames.Add(key);
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, inputNames);
            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;
            return spinner;
        }

        private void ForceInputOpen()
        {
            var test_input = FindViewById<EditText>(Resource.Id.testInput);
            test_input.RequestFocus();
            Window.SetSoftInputMode(SoftInput.StateAlwaysVisible);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}