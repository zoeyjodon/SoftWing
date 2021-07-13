using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using SoftWing.System;
using System;
using Android.Util;
using Android.Provider;
using Android.Widget;
using Android.Content.PM;
using Android.Content;
using System.IO;
using SoftWing.System.Messages;

namespace SoftWing
{
    [Activity(Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTask)]
    public class SoundSettingsActivity : AppCompatActivity
    {
        private const String TAG = "SoundSettingsActivity";
        private const int REQUEST_OPEN_FILE_CALLBACK = 300;
        private const int REQUEST_CLOSE_FILE_CALLBACK = 301;
        private bool soundDisablePromptComplete = false;
        private MessageDispatcher dispatcher;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "OnCreate()");
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.sound_settings);

            dispatcher = MessageDispatcher.GetInstance(this);
            ConfigureAudioSelectButtons();
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        private bool DisablePromptRequired()
        {
            return (SwSettings.GetOpenSoundPath() == "") && (SwSettings.GetCloseSoundPath() == "");
        }

        private void EnsureSoundDisabled()
        {
            Log.Debug(TAG, "EnsureSoundDisabled()");
            // Only need to prompt if open and close sounds are not set yet
            if (!DisablePromptRequired())
            {
                return;
            }
            Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
            var alert = dialog.Create();
            alert.SetTitle("Disable Swivel Up/Down Sounds");
            alert.SetMessage("In order for custom swivel sounds to work, you must navigate to System sounds and disable Swivel Up/Down sounds in your settings.");
            alert.SetButton("OK", (c, ev) =>
            {
                var enableIntent = new Intent(Settings.ActionSoundSettings);
                enableIntent.SetFlags(ActivityFlags.NewTask);
                StartActivity(enableIntent);
                alert.Cancel();
            });
            alert.SetButton2("Cancel", (c, ev) => { });
            alert.Show();
        }

        private void ConfigureAudioSelectButtons()
        {
            var open_button = FindViewById<ImageButton>(Resource.Id.selectOpenAudioButton);
            open_button.Click += delegate
            {
                SelectAudioFile(REQUEST_OPEN_FILE_CALLBACK);
            };

            var close_button = FindViewById<ImageButton>(Resource.Id.selectCloseAudioButton);
            close_button.Click += delegate
            {
                SelectAudioFile(REQUEST_CLOSE_FILE_CALLBACK);
            };
        }

        private void SelectAudioFile(int requestCode)
        {
            if (!soundDisablePromptComplete)
            {
                soundDisablePromptComplete = true;
                EnsureSoundDisabled();
                return;
            }
            Intent intent = new Intent(Intent.ActionOpenDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("audio/*");
            StartActivityForResult(intent, requestCode);
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
    }
}