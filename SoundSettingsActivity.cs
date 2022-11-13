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

        private string GetActualPathFromFile(Android.Net.Uri uri)
        {
            bool isKitKat = Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;

            if (isKitKat && DocumentsContract.IsDocumentUri(this, uri))
            {
                // ExternalStorageProvider
                if (isExternalStorageDocument(uri))
                {
                    string docId = DocumentsContract.GetDocumentId(uri);

                    char[] chars = { ':' };
                    string[] split = docId.Split(chars);
                    string type = split[0];

                    if ("primary".Equals(type, StringComparison.OrdinalIgnoreCase))
                    {
                        return Android.OS.Environment.ExternalStorageDirectory + "/" + split[1];
                    }
                }
                // DownloadsProvider
                else if (isDownloadsDocument(uri))
                {
                    string id = DocumentsContract.GetDocumentId(uri);

                    Android.Net.Uri contentUri = ContentUris.WithAppendedId(
                                    Android.Net.Uri.Parse("content://downloads/public_downloads"), long.Parse(id));

                    //System.Diagnostics.Debug.WriteLine(contentUri.ToString());

                    return getDataColumn(this, contentUri, null, null);
                }
                // MediaProvider
                else if (isMediaDocument(uri))
                {
                    String docId = DocumentsContract.GetDocumentId(uri);

                    char[] chars = { ':' };
                    String[] split = docId.Split(chars);

                    String type = split[0];

                    Android.Net.Uri contentUri = null;
                    if ("image".Equals(type))
                    {
                        contentUri = MediaStore.Images.Media.ExternalContentUri;
                    }
                    else if ("video".Equals(type))
                    {
                        contentUri = MediaStore.Video.Media.ExternalContentUri;
                    }
                    else if ("audio".Equals(type))
                    {
                        contentUri = MediaStore.Audio.Media.ExternalContentUri;
                    }

                    String selection = "_id=?";
                    String[] selectionArgs = new String[]
                    {
                split[1]
                    };

                    return getDataColumn(this, contentUri, selection, selectionArgs);
                }
            }
            // MediaStore (and general)
            else if ("content".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {

                // Return the remote address
                if (isGooglePhotosUri(uri))
                    return uri.LastPathSegment;

                return getDataColumn(this, uri, null, null);
            }
            // File
            else if ("file".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return uri.Path;
            }

            return null;
        }

        public static String getDataColumn(Context context, Android.Net.Uri uri, String selection, String[] selectionArgs)
        {
            ICursor cursor = null;
            String column = "_data";
            String[] projection =
            {
        column
        };

            try
            {
                cursor = context.ContentResolver.Query(uri, projection, selection, selectionArgs, null);
                if (cursor != null && cursor.MoveToFirst())
                {
                    int index = cursor.GetColumnIndexOrThrow(column);
                    return cursor.GetString(index);
                }
            }
            finally
            {
                if (cursor != null)
                    cursor.Close();
            }
            return null;
        }

        //Whether the Uri authority is ExternalStorageProvider.
        public static bool isExternalStorageDocument(Android.Net.Uri uri)
        {
            return "com.android.externalstorage.documents".Equals(uri.Authority);
        }

        //Whether the Uri authority is DownloadsProvider.
        public static bool isDownloadsDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.downloads.documents".Equals(uri.Authority);
        }

        //Whether the Uri authority is MediaProvider.
        public static bool isMediaDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.media.documents".Equals(uri.Authority);
        }

        //Whether the Uri authority is Google Photos.
        public static bool isGooglePhotosUri(Android.Net.Uri uri)
        {
            return "com.google.android.apps.photos.content".Equals(uri.Authority);
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
                        File.Copy(GetActualPathFromFile(data.Data), OPEN_LOCAL_PATH);

                        SwSettings.SetOpenSoundPath(OPEN_LOCAL_PATH);
                        dispatcher.Post(new AudioUpdateMessage(OPEN_LOCAL_PATH, AudioUpdateMessage.AudioType.SwingOpen));
                    }
                    break;
                case REQUEST_CLOSE_FILE_CALLBACK:
                    {
                        // Copy the file to app data to ensure persistent access
                        File.Copy(GetActualPathFromFile(data.Data), CLOSE_LOCAL_PATH);

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