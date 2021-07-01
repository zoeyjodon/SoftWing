using Android.App;
using Android.Content;
using Android.Util;
using AndroidX.Core.App;
using Android.Views.InputMethods;
using Com.Lge.Display;
using Android.Provider;
using System;
using Android.Media;
using SoftWing.System.Messages;
using System.IO;

namespace SoftWing
{
    [Service(Exported = true, Enabled = true, Name = "com.jodonlucas.softwing.SoftWing.SwDisplayManager")]
    public class SwDisplayManager : Service, System.MessageSubscriber
    {
        private const String TAG = "SwDisplayManager";
        private DisplayManagerHelper lg_display_manager;
        private LgSwivelStateCallback swivel_state_cb;
        private System.MessageDispatcher dispatcher;
        private static SwDisplayManager instance;

        private const String NOTIFICATION_CHANNEL_ID = "SWKeyboard";
        private const int NOTIFICATION_ONGOING_ID = 1001;
        private static NotificationReceiver notification_receiver = null;

        private const int LG_KEYBOARD_TIMEOUT_MS = 500;
        private const int SHOW_IME_DELAY_MS = 500;

        private const int PLAY_SOUND_MAX_DELAY_MS = 500;
        private static String STORAGE_DIR = Android.OS.Environment.ExternalStorageDirectory.Path;
        private static String MUSIC_DIR = STORAGE_DIR + "/Music/";
        private static String OPEN_SOUND_PATH = MUSIC_DIR + "SwivelOpen.mp3";
        private static String CLOSE_SOUND_PATH = MUSIC_DIR + "SwivelClose.mp3";
        private int media_volume = 0;
        private MediaPlayer media_player;
        private AudioFocusRequestClass focus_request = new AudioFocusRequestClass.Builder(AudioFocus.GainTransient).Build();

        public static void StartSwDisplayManager()
        {
            StartSwDisplayManager(Application.Context);
        }

        public static void StartSwDisplayManager(Context calling_context)
        {
            Log.Debug(TAG, "StartSwDisplayManager");
            if (instance != null)
            {
                Log.Debug(TAG, "Display manager exists, skipping");
                return;
            }
            var intent = new Intent(calling_context, typeof(SwDisplayManager));
            calling_context.StartForegroundService(intent);
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Debug(TAG, "OnStartCommand");
            SetNotification();
            return StartCommandResult.Sticky;
        }

        public SwDisplayManager()
        {
            Log.Debug(TAG, "SwDisplayManager");

            lg_display_manager = new DisplayManagerHelper(this);
            instance = this;

            dispatcher = System.MessageDispatcher.GetInstance();
            dispatcher.Subscribe(System.MessageType.DisplayUpdate, this);
            dispatcher.Subscribe(System.MessageType.ShowIme, this);

            swivel_state_cb = new LgSwivelStateCallback();
            lg_display_manager.RegisterSwivelStateCallback(swivel_state_cb);
        }

        ~SwDisplayManager()
        {
            Log.Debug(TAG, "~SwDisplayManager");
        }

        private void CreateNotificationChannel()
        {
            var name = "SoftWing";
            var description = "SoftWing";
            var importance = NotificationImportance.Low;
            NotificationChannel channel = new NotificationChannel(NOTIFICATION_CHANNEL_ID, name, importance);
            channel.Description = description;
            // Register the channel with the system; you can't change the importance
            // or other notification behaviors after this
            NotificationManager notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        private void SetNotification()
        {
            Log.Debug(TAG, "SetNotification()");

            CreateNotificationChannel();
            var text = "Controller notification enabled.";

            notification_receiver = new NotificationReceiver();
            var pFilter = new IntentFilter(NotificationReceiver.ACTION_SHOW);
            RegisterReceiver(notification_receiver, pFilter);

            Intent notificationIntent = new Intent(NotificationReceiver.ACTION_SHOW);
            PendingIntent contentIntent = PendingIntent.GetBroadcast(Application.Context, 1, notificationIntent, 0);

            String title = "Show SoftWing Controller";
            String body = "Select this to open the controller.";

            NotificationCompat.Builder mBuilder = new NotificationCompat.Builder(this, NOTIFICATION_CHANNEL_ID)
                    .SetSmallIcon(Resource.Mipmap.ic_notification)
                    .SetColor(Resource.Color.accent_material_dark)
                    .SetAutoCancel(false)
                    .SetTicker(text)
                    .SetContentTitle(title)
                    .SetContentText(body)
                    .SetContentIntent(contentIntent)
                    .SetOngoing(true)
                    .SetVisibility((int)NotificationVisibility.Public)
                    .SetPriority(NotificationCompat.PriorityDefault);

            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(this);
            StartForeground(NOTIFICATION_ONGOING_ID, mBuilder.Build());
        }

        private static void LockOrientation()
        {
            Log.Debug(TAG, "Locking phone orientation");
            try
            {
                Settings.System.PutInt(Application.Context.ContentResolver, Settings.System.AccelerometerRotation, 0);
            }
            catch (Exception ex)
            {
                Log.Debug(TAG, "Failed to lock phone orientation");
            }
        }

        private static bool IsUsingSwKeyboard()
        {
            var current_ime = Settings.Secure.GetString(Application.Context.ContentResolver, Settings.Secure.DefaultInputMethod);
            return current_ime.Contains("SoftWingInput");
        }

        private static void SetInputMethod(string input_method_id)
        {
            Log.Debug(TAG, "Setting Input Method");
            try
            {
                // Note, to be able to perform this action you need to grant this app secure settings permissions
                // With debugging enabled on your device and adb installed on your PC, connect your phone and run the following command:
                // adb shell pm grant com.jodonlucas.softwing android.permission.WRITE_SECURE_SETTINGS
                Settings.Secure.PutString(Application.Context.ContentResolver, Settings.Secure.DefaultInputMethod, input_method_id);
            }
            catch (Exception ex)
            {
                // If we can't write the setting directly, try the old fashioned way.
                // Note: This will only work if SoftWing is the current input method.
                Log.Debug(TAG, "Failed to write secure setting, using IMM");
                InputMethodManager imm = (InputMethodManager)
                    Application.Context.GetSystemService(InputMethodService);
                imm.SetInputMethod(SoftWingInput.InputSessionToken, input_method_id);
            }
        }

        private void ShowSwKeyboard()
        {
            Log.Debug(TAG, "ShowSwKeyboard");
            InputMethodManager input_manager = (InputMethodManager)
                Application.Context.GetSystemService(InputMethodService);
            if (input_manager != null)
            {
                input_manager.ShowSoftInputFromInputMethod(SoftWingInput.InputSessionToken, ShowFlags.Forced);
            }
            // If we start from the open position, we want to execute the swivel transition immediately
            instance.dispatcher.Post(new DisplayUpdateMessage(lg_display_manager.SwivelState));
        }

        public static void UseLgKeyboard()
        {
            InputMethodManager imm = (InputMethodManager)
                Application.Context.GetSystemService(InputMethodService);

            foreach (var InputMethod in imm.EnabledInputMethodList)
            {
                Log.Debug(TAG, "InputMethod: " + InputMethod.Id.ToString());
                if (InputMethod.Id.Contains("LgeImeImpl"))
                {
                    SetInputMethod(InputMethod.Id);
                    return;
                }
            }
        }

        public static void UseSwKeyboard()
        {
            if (IsUsingSwKeyboard())
            {
                Log.Debug(TAG, "Already using SW IME, skipping");
                return;
            }
            InputMethodManager imm = (InputMethodManager)
                Application.Context.GetSystemService(InputMethodService);

            foreach (var InputMethod in imm.EnabledInputMethodList)
            {
                Log.Debug(TAG, "InputMethod: " + InputMethod.Id.ToString());
                if (InputMethod.Id.Contains("SoftWingInput"))
                {
                    SetInputMethod(InputMethod.Id);
                    return;
                }
            }
        }

        private void StartSound(String audio_path)
        {
            var audio_manager = (AudioManager)GetSystemService(AudioService);
            if ((media_player != null) && (media_player.IsPlaying))
            {
                media_player.Stop();
                media_player.Release();
            }
            else
            {
                media_volume = audio_manager.GetStreamVolume(Android.Media.Stream.Music);
                audio_manager.RequestAudioFocus(focus_request);
            }
            int systemVolume = audio_manager.GetStreamVolume(Android.Media.Stream.System);
            // Make sure we return all settings to normal after we're done
            media_player = MediaPlayer.Create(ApplicationContext, Android.Net.Uri.Parse(audio_path));
            media_player.Completion += delegate
            {
                audio_manager.SetStreamVolume(Android.Media.Stream.Music, media_volume, 0);
                audio_manager.AbandonAudioFocusRequest(focus_request);
            };
            // Give the system time to pause any running audio
            var start_time = Java.Lang.JavaSystem.CurrentTimeMillis();
            var end_time = start_time + PLAY_SOUND_MAX_DELAY_MS;
            while (audio_manager.IsMusicActive)
            {
                if (Java.Lang.JavaSystem.CurrentTimeMillis() > end_time)
                {
                    break;
                }
            }
            audio_manager.SetStreamVolume(Android.Media.Stream.Music, systemVolume, 0);
            media_player.Start();
        }

        public void PlayWingSound(String audio_path)
        {
            Log.Debug(TAG, "PlayWingSound");
            if (!File.Exists(audio_path))
            {
                Log.Debug(TAG, audio_path + " File Not Found!");
            }
            try
            {
                StartSound(audio_path);
            }
            catch (Exception ex)
            {
                Log.Debug(TAG, "Failed to play audio");
                var audio_manager = (AudioManager)GetSystemService(AudioService);
                audio_manager.SetStreamVolume(Android.Media.Stream.Music, media_volume, 0);
                audio_manager.AbandonAudioFocusRequest(focus_request);
            }
        }

        public void Accept(SoftWing.System.SystemMessage message)
        {
            Log.Debug(TAG, "Accept");

            if (message.getMessageType() == System.MessageType.ShowIme)
            {
                UseSwKeyboard();
                // Give the IME time to update
                new Android.OS.Handler().PostDelayed(delegate
                {
                    ShowSwKeyboard();
                }, SHOW_IME_DELAY_MS);
                return;
            }
            var display_message = (DisplayUpdateMessage)message;
            switch (display_message.SwivelState)
            {
                case DisplayManagerHelper.SwivelStart:
                    Log.Debug(TAG, "DisplayManagerHelper.SwivelStart");
                    PlayWingSound(OPEN_SOUND_PATH);
                    break;
                case DisplayManagerHelper.NonSwivelStart:
                    Log.Debug(TAG, "DisplayManagerHelper.NonSwivelStart");
                    PlayWingSound(CLOSE_SOUND_PATH);
                    break;
                default:
                    break;
            }
            if (lg_display_manager.SwivelState == DisplayManagerHelper.SwivelSwiveled)
            {
                Log.Debug(TAG, "DisplayManagerHelper.SwivelSwiveled");
                // We don't want to impose this behavior unless we are displaying the SoftWing IME
                if (!IsUsingSwKeyboard() || !SoftWingInput.ImeIsOpen)
                {
                    return;
                }
                // Lock the phone's orientation to prevent unexpected behavior.
                // Disabled for being more annoying than useful.
                //LockOrientation();
                UseLgKeyboard();
                // Give the LG keyboard time to perform the screen transition
                new Android.OS.Handler().PostDelayed(delegate
                {
                    UseSwKeyboard();
                }, LG_KEYBOARD_TIMEOUT_MS);
            }
        }

        public override Android.OS.IBinder OnBind(Intent intent)
        {
            Log.Debug(TAG, "OnBind");
            return null;
        }
    }

    /**
     * Updates the swivel state based on the callback actions.
     *
     * @param state The state of swivel, e.g. SWIVEL_START, SWIVEL_END, etc.
     */
    public class LgSwivelStateCallback : DisplayManagerHelper.SwivelStateCallback
    {
        private const String TAG = "LgSwivelStateCallback";
        private System.MessageDispatcher dispatcher;
        private bool ignore_transition = true;

        public LgSwivelStateCallback()
        {
            dispatcher = System.MessageDispatcher.GetInstance(new Activity());
        }

        public override void OnSwivelStateChanged(int state)
        {
            Log.Debug(TAG, "OnSwivelStateChanged");
            // The callback manager runs once on startup to report the initial state.
            // We only want updates if that state changes.
            if (ignore_transition)
            {
                Log.Debug(TAG, "Ignoring first swivel action");
                ignore_transition = false;
                return;
            }

            dispatcher.Post(new DisplayUpdateMessage(state));
        }
    }
}