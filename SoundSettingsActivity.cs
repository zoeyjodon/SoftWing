using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using SoftWing.System;
using System;
using Android.Util;
using Android.Widget;
using Android.Content.PM;
using Android.Content;
using System.IO;
using SoftWing.System.Messages;

namespace SoftWing
{
    [Activity(Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait)]
    public class SoundSettingsActivity : AppCompatActivity
    {
        private const String TAG = "SoundSettingsActivity";
        private const int REQUEST_OPEN_FILE_CALLBACK = 300;
        private const int REQUEST_CLOSE_FILE_CALLBACK = 301;
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

        private void SelectAudioFile(int requestCode)
        {
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