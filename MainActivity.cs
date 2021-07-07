using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using SoftWing.System;
using System;
using Android.Util;
using Android.Widget;
using Android.Views;
using System.Collections.Generic;
using Android.Content.PM;
using AndroidX.Core.App;
using Android;
using Android.Support.Design.Widget;
using Android.Content;
using System.IO;
using SoftWing.System.Messages;

namespace SoftWing
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        private const String TAG = "MainActivity";
        private const int REQUEST_OPEN_FILE_CALLBACK = 300;
        private const int REQUEST_CLOSE_FILE_CALLBACK = 301;
        private static String[] PERMISSIONS_LIST = {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.BindInputMethod,
            Manifest.Permission.Vibrate,
            Manifest.Permission.ForegroundService,
            Manifest.Permission.WriteSettings,
            Manifest.Permission.ReceiveBootCompleted
        };
        private Dictionary<int, SwSettings.ControlId> spinnerToControlMap = new Dictionary<int, SwSettings.ControlId>();
        private int ignore_keyset_count = 0;
        private MessageDispatcher dispatcher;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "OnCreate()");
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            dispatcher = MessageDispatcher.GetInstance(this);
            CreateControlConfiguration();
            ConfigureResetButton();
            ConfigureAudioSelectButtons();
            ForceInputOpen();
        }

        protected override void OnStart()
        {
            base.OnStart();
            SwDisplayManager.StartSwDisplayManager();
            RequestAllPermissions();
            RegisterReceiver(new SwBootReceiver(), new IntentFilter(Intent.ActionScreenOn));
            RegisterReceiver(new SwBootReceiver(), new IntentFilter(Intent.ActionUserUnlocked));
            RegisterReceiver(new SwBootReceiver(), new IntentFilter(Intent.ActionBootCompleted));
        }

        private void RequestAllPermissions()
        {
            foreach (var permission in PERMISSIONS_LIST)
            {
                if (CheckSelfPermission(permission) != Permission.Granted)
                {
                    RequestAppPermission(permission);
                }
            }
        }

        private void RequestAppPermission(string permission)
        {
            Log.Debug(TAG, "RequestAppPermissions");

            string[] permissions = { permission };
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, permission))
            {
                Log.Info(TAG, "Displaying storage permission rationale to provide additional context.");

                var main_view = FindViewById<View>(Resource.Id.mainLayout);
                Snackbar.Make(main_view,
                               Resource.String.permissions_rationale,
                               Snackbar.LengthIndefinite)
                        .SetAction(Resource.String.ok,
                                   new Action<View>(delegate (View obj)
                                   {
                                       ActivityCompat.RequestPermissions(this, permissions, 1);
                                   }
                        )
                ).Show();
            }
            else
            {
                ActivityCompat.RequestPermissions(this, permissions, 1);
            }
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

        private void ConfigureAudioSelectButtons()
        {
            Button open_button = FindViewById<Button>(Resource.Id.selectOpenAudioButton);
            open_button.Click += delegate
            {
                SelectAudioFile(REQUEST_OPEN_FILE_CALLBACK);
            };

            Button close_button = FindViewById<Button>(Resource.Id.selectCloseAudioButton);
            close_button.Click += delegate
            {
                SelectAudioFile(REQUEST_CLOSE_FILE_CALLBACK);
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
            LinearLayout mainLayout = FindViewById<LinearLayout>(Resource.Id.mainLayout);

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

        private void SelectAudioFile(int requestCode)
        {
            Intent intent = new Intent(Intent.ActionOpenDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("audio/*");
            StartActivityForResult(intent, requestCode);
        }

        private void ForceInputOpen()
        {
            var test_input = FindViewById<EditText>(Resource.Id.testInput);
            test_input.RequestFocus();
            Window.SetSoftInputMode(SoftInput.StateAlwaysVisible);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            Log.Debug(TAG, "OnActivityResult " + data.Data.ToString());
            base.OnActivityResult(requestCode, resultCode, data);
            if (!File.Exists(data.Data.ToString()))
            {
                Log.Debug(TAG, data.Data.ToString() + " File Not Found!");
            }
            switch (requestCode)
            {
                case REQUEST_OPEN_FILE_CALLBACK:
                    SwSettings.SetOpenSoundPath(data.Data);
                    dispatcher.Post(new AudioUpdateMessage(data.Data, AudioUpdateMessage.AudioType.SwingOpen));
                    {
                        // Make sure we can continue using the file after a reset
                        var takeFlags = ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission;
                        ContentResolver.TakePersistableUriPermission(data.Data, takeFlags);
                    }
                    break;
                case REQUEST_CLOSE_FILE_CALLBACK:
                    SwSettings.SetCloseSoundPath(data.Data);
                    dispatcher.Post(new AudioUpdateMessage(data.Data, AudioUpdateMessage.AudioType.SwingClose));
                    {
                        // Make sure we can continue using the file after a reset
                        var takeFlags = ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission;
                        ContentResolver.TakePersistableUriPermission(data.Data, takeFlags);
                    }
                    break;
                default:
                    Log.Debug(TAG, "Ignoring Activity Result");
                    break;
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Log.Debug(TAG, "OnRequestPermissionsResult");
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}