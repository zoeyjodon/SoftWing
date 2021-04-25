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

namespace SoftWing
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private const String TAG = "MainActivity";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "OnCreate()");
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            ForceInputOpen();
        }

        private void ForceInputOpen()
        {
            var test_input = FindViewById<EditText>(Resource.Id.testInput);
            test_input.RequestFocus();
            Window.SetSoftInputMode(SoftInput.StateAlwaysVisible);

            //// Not working yet, maybe I need to force switch to SWing first?
            //InputMethodManager imm = (InputMethodManager)
            //    GetSystemService(Context.InputMethodService);

            //foreach (var InputMethod in imm.EnabledInputMethodList)
            //{
            //    Log.Debug(TAG, "InputMethod: " + InputMethod.Id.ToString());
            //    if (InputMethod.Id.Contains("SoftWingInput"))
            //    {
            //        Log.Debug(TAG, "Setting Input Method");
            //        imm.SetInputMethod(test_input.WindowToken, InputMethod.Id);
            //    }
            //}
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}