using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using SoftWing.SwSystem;
using System;
using Android.Util;
using Android.Provider;
using Android.Widget;
using Android.Content.PM;
using Android.Content;
using System.IO;
using SoftWing.SwSystem.Messages;
using Xamarin.Essentials;
using Android.Database;

namespace SoftWing
{
    [Activity(Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTask)]
    public class SoundSettingsActivity : AppCompatActivity
    {
        private const String TAG = "SoundSettingsActivity";
        private const int REQUEST_OPEN_FILE_CALLBACK = 300;
        private const int REQUEST_CLOSE_FILE_CALLBACK = 301;
        private readonly string OPEN_LOCAL_PATH = Path.Combine(FileSystem.AppDataDirectory, "swivel_open");
        private readonly string CLOSE_LOCAL_PATH = Path.Combine(FileSystem.AppDataDirectory, "swivel_closed");
        private const int COPY_BUFFER_SIZE = 32768;
        private bool soundDisablePromptComplete = false;
        private MessageDispatcher dispatcher;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "OnCreate()");
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.sound_settings);

            dispatcher = MessageDispatcher.GetInstance();
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

            var reset_button = FindViewById<ImageButton>(Resource.Id.audioResetButton);
            reset_button.Click += delegate
            {
                File.Delete(OPEN_LOCAL_PATH);
                SwSettings.SetOpenSoundPath("");
                dispatcher.Post(new AudioUpdateMessage("", AudioUpdateMessage.AudioType.SwingOpen));

                File.Delete(CLOSE_LOCAL_PATH);
                SwSettings.SetCloseSoundPath("");
                dispatcher.Post(new AudioUpdateMessage("", AudioUpdateMessage.AudioType.SwingClose));

                var toast = Toast.MakeText(this, "Swivel Sounds have been reset!", ToastLength.Short);
                toast.Show();
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

        private void CopyURIToFile(Android.Net.Uri input, string output)
        {
            using (var inputStream = ContentResolver.OpenInputStream(input))
            {
                using (var outputStream = File.Create(output))
                {
                    var buffer = new byte[COPY_BUFFER_SIZE];
                    while (true)
                    {
                        var count = inputStream.Read(buffer, 0, COPY_BUFFER_SIZE);
                        if (count > 0)
                        {
                            outputStream.Write(buffer, 0, count);
                        }

                        if (count < COPY_BUFFER_SIZE) break;
                    }
                }
            }
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
                    {
                        // Copy the file to app data to ensure persistent access
                        CopyURIToFile(data.Data, OPEN_LOCAL_PATH);
                        SwSettings.SetOpenSoundPath(OPEN_LOCAL_PATH);
                        dispatcher.Post(new AudioUpdateMessage(OPEN_LOCAL_PATH, AudioUpdateMessage.AudioType.SwingOpen));
                    }
                    break;
                case REQUEST_CLOSE_FILE_CALLBACK:
                    {
                        // Copy the file to app data to ensure persistent access
                        CopyURIToFile(data.Data, CLOSE_LOCAL_PATH);
                        SwSettings.SetCloseSoundPath(CLOSE_LOCAL_PATH);
                        dispatcher.Post(new AudioUpdateMessage(CLOSE_LOCAL_PATH, AudioUpdateMessage.AudioType.SwingClose));
                    }
                    break;
                default:
                    Log.Debug(TAG, "Ignoring Activity Result");
                    break;
            }
        }
    }
}