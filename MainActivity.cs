using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using System;
using Android.Util;
using Android.Widget;
using Android.Views;
using Android.Provider;
using Android.Content.PM;
using AndroidX.Core.App;
using Android;
using Android.Support.Design.Widget;
using Android.Content;
using SoftWing.SwSystem;

namespace SoftWing
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity, MessageSubscriber
    {
        private const String TAG = "MainActivity";
        private static String[] PERMISSIONS_LIST = {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.BindInputMethod,
            Manifest.Permission.Vibrate,
            Manifest.Permission.ForegroundService,
            Manifest.Permission.WriteSettings,
            Manifest.Permission.ReceiveBootCompleted
        };
        private DonationHandler donation;
        private MessageDispatcher dispatcher;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "OnCreate()");
            base.OnCreate(savedInstanceState);
            if (!IsTaskRoot)
            {
                // Only allow one instance per session
                Finish();
                return;
            }
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            donation = new DonationHandler(this);
            dispatcher = MessageDispatcher.GetInstance(this);
            dispatcher.Subscribe(MessageType.DonationUpdate, this);
            ConfigureNavigationButtons();
        }

        protected override void OnStart()
        {
            base.OnStart();
            SwDisplayManager.StartSwDisplayManager();
            RequestAllPermissions();
            RegisterReceiver(new SwBootReceiver(), new IntentFilter(Intent.ActionScreenOn));
            RegisterReceiver(new SwBootReceiver(), new IntentFilter(Intent.ActionUserUnlocked));
            RegisterReceiver(new SwBootReceiver(), new IntentFilter(Intent.ActionBootCompleted));
            donation.Start();
            EnsureControllerEnabled();
        }

        private void OfferControllerHelp()
        {
            Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
            var alert = dialog.Create();
            alert.SetTitle("Would You Like Some Help?");
            var message = "First time using the controller? How about some instructions?";
            alert.SetMessage(message);
            alert.SetButton("OK", (c, ev) =>
            {
                StartActivity(typeof(ControllerHelpActivity));
                alert.Cancel();
            });
            alert.SetButton2("SKIP", (c, ev) => { });
            alert.Show();
        }

        private void EnsureControllerEnabled()
        {
            Log.Debug(TAG, "EnsureControllerEnabled()");

            var enabled_imes = Settings.Secure.GetString(Application.Context.ContentResolver, Settings.Secure.EnabledInputMethods);
            Log.Debug(TAG, enabled_imes);
            if (enabled_imes.Contains("SoftWingInput") && enabled_imes.Contains("LgeImeImpl"))
            {
                return;
            }

            Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
            var alert = dialog.Create();
            alert.SetTitle("Enable Required Keyboards");
            var message = "In order for the controller to work, the following must be enabled in your settings:\n";
            message += "\t- LG Keyboard\n";
            message += "\t- SoftWingInput\n";
            message += "\t- Show icon for switching keyboards";
            alert.SetMessage(message);
            alert.SetButton("OK", (c, ev) =>
            {
                var enableIntent = new Intent(Settings.ActionInputMethodSettings);
                enableIntent.SetFlags(ActivityFlags.NewTask);
                StartActivity(enableIntent);
                alert.Cancel();
                OfferControllerHelp();
            });
            alert.SetButton2("SKIP", (c, ev) =>
            {
                OfferControllerHelp();
            });
            alert.Show();
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

        private void ConfigureNavigationButtons()
        {
            Log.Debug(TAG, "ConfigureNavigationButtons");
            var controller_button = FindViewById<ImageButton>(Resource.Id.controllerSettingsButton);
            controller_button.Click += delegate
            {
                StartActivity(typeof(ControllerSettingsActivity));
            };
            var sound_button = FindViewById<ImageButton>(Resource.Id.soundSettingsButton);
            sound_button.Click += delegate
            {
                StartActivity(typeof(SoundSettingsActivity));
            };
        }

        private void ConfigureDonationButton()
        {
            Log.Debug(TAG, "ConfigureDonationButton");
            var main_view = FindViewById<ViewGroup>(Resource.Id.mainLayout);
            var spinner = donation.CreateDonationSpinner();
            main_view.AddView(spinner);

            var donate_button = FindViewById<ImageButton>(Resource.Id.donateButton);
            donate_button.Click += delegate
            {
                spinner.PerformClick();
            };
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Log.Debug(TAG, "OnRequestPermissionsResult");
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void Accept(SystemMessage message)
        {
            Log.Debug(TAG, "Accept");
            if (message.getMessageType() != MessageType.DonationUpdate)
            {
                return;
            }
            var donation_message = (SwSystem.Messages.DonationUpdateMessage)message;
            if (donation_message.DonationType == SwSystem.Messages.DonationUpdateMessage.UpdateType.SetupComplete)
            {
                ConfigureDonationButton();
            }
        }
    }
}