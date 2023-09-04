using Android.App;
using Android.Content;
using Android.Util;
using AndroidX.Core.App;
using Android.Views.InputMethods;
using Com.Lge.Display;
using Android.Provider;
using System;
using Android.Media;
using SoftWing.SwSystem.Messages;
using SoftWing.SwSystem;
using System.Threading.Tasks;

namespace SoftWing
{
    [Service(Exported = true, Enabled = true, Name = "com.jodonlucas.softwing.SoftWing.SwDisplayManager")]
    public class SwDisplayManager : Service, SwSystem.MessageSubscriber
    {
        private const String TAG = "SwDisplayManager";
        private DisplayManagerHelper lg_display_manager;
        private LgSwivelStateCallback swivel_state_cb;
        private MessageDispatcher dispatcher;
        private static SwDisplayManager instance;

        private const String NOTIFICATION_CHANNEL_ID = "SWKeyboard";
        private const int NOTIFICATION_ONGOING_ID = 3532;
        private static NotificationReceiver notification_receiver = null;

        private const int IME_TRANSITION_DELAY_MS = 1000;
        private const int SHOW_IME_DELAY_MS = 2000;
        private const int IME_STARTUP_WAIT_MS = 5000;

        private static String OPEN_SOUND_PATH;
        private static String CLOSE_SOUND_PATH;
        private int media_volume = 0;
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
            SetNotificationInternal();
            return StartCommandResult.Sticky;
        }

        public SwDisplayManager()
        {
            Log.Debug(TAG, "SwDisplayManager");

            lg_display_manager = new DisplayManagerHelper(this);
            instance = this;

            dispatcher = SwSystem.MessageDispatcher.GetInstance();
            dispatcher.Subscribe(SwSystem.MessageType.DisplayUpdate, this);
            dispatcher.Subscribe(SwSystem.MessageType.ShowIme, this);
            dispatcher.Subscribe(SwSystem.MessageType.AudioUpdate, this);

            swivel_state_cb = new LgSwivelStateCallback();
            lg_display_manager.RegisterSwivelStateCallback(swivel_state_cb);
            OPEN_SOUND_PATH = SwSettings.GetOpenSoundPath();
            CLOSE_SOUND_PATH = SwSettings.GetCloseSoundPath();
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

        public static void SetNotification()
        {
            if (instance != null)
            {
                instance.SetNotificationInternal();
            }
            else
            {
                StartSwDisplayManager();
            }
        }

        public void SetNotificationInternal()
        {
            Log.Debug(TAG, "SetNotificationInternal()");

            CreateNotificationChannel();
            var text = "Controller notification enabled.";

            notification_receiver = new NotificationReceiver();
            var pFilter = new IntentFilter(NotificationReceiver.ACTION_SHOW);
            RegisterReceiver(notification_receiver, pFilter);

            Intent notificationIntent = new Intent(NotificationReceiver.ACTION_SHOW);
            PendingIntent contentIntent = PendingIntent.GetBroadcast(Application.Context, 1, notificationIntent, PendingIntentFlags.Mutable);

            String title = "Show SoftWing Controller";
            String body = "Select this to open the controller.";

            NotificationCompat.Builder mBuilder = new NotificationCompat.Builder(this, NOTIFICATION_CHANNEL_ID)
                    .SetSmallIcon(Resource.Mipmap.ic_launcher_foreground)
                    .SetColor(Resource.Color.accent_material_dark)
                    .SetAutoCancel(false)
                    .SetTicker(text)
                    .SetContentTitle(title)
                    .SetContentText(body)
                    .SetContentIntent(contentIntent)
                    .SetOngoing(true)
                    .SetVisibility((int)NotificationVisibility.Public)
                    .SetPriority(NotificationCompat.PriorityDefault);

            StartForeground(NOTIFICATION_ONGOING_ID, mBuilder.Build());
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
                Log.Debug(TAG, "Failed to write secure setting, using IMM: " + ex.Message);
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
            // Alert the system to the initial swivel state after the IME has opened
            DisplayUpdateMessage msg = new DisplayUpdateMessage(DisplayManagerHelper.NonSwivelEnd);
            if (lg_display_manager.SwivelState == DisplayManagerHelper.SwivelSwiveled)
            {
                msg = new DisplayUpdateMessage(DisplayManagerHelper.SwivelEnd);
            }
            Task.Factory.StartNew(() =>
            {
                Log.Debug(TAG, "Waiting for IME to open...");
                var start_time = Java.Lang.JavaSystem.CurrentTimeMillis();
                var end_time = start_time + IME_STARTUP_WAIT_MS;
                while (!SoftWingInput.ImeIsOpen)
                {
                    // Make sure we aren't waiting forever
                    if (Java.Lang.JavaSystem.CurrentTimeMillis() > end_time)
                    {
                        Log.Error(TAG, "IME NEVER OPENED!");
                        return;
                    }
                }
                Log.Debug(TAG, "Launching switching task");
                instance.dispatcher.Post(msg);
            });
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
            // Play as a system sound
            Ringtone swingRing = RingtoneManager.GetRingtone(ApplicationContext, Android.Net.Uri.Parse(audio_path));
            swingRing.AudioAttributes = new AudioAttributes.Builder().SetFlags(AudioFlags.None).SetLegacyStreamType(Android.Media.Stream.System).Build();
            swingRing.Play();
        }

        public void PlayWingSound(String audio_path)
        {
            Log.Debug(TAG, "PlayWingSound");
            if (String.IsNullOrEmpty(audio_path))
            {
                Log.Debug(TAG, "Audio File Invalid!");
                return;
            }
            try
            {
                StartSound(audio_path);
            }
            catch (Exception ex)
            {
                Log.Debug(TAG, "Failed to play audio: " + ex.Message);
                var audio_manager = (AudioManager)GetSystemService(AudioService);
                audio_manager.SetStreamVolume(Android.Media.Stream.Music, media_volume, 0);
                audio_manager.AbandonAudioFocusRequest(focus_request);
            }
        }

        private void HandleAudioUpdate(AudioUpdateMessage audio_message)
        {
            switch (audio_message.Type)
            {
                case AudioUpdateMessage.AudioType.SwingOpen:
                    OPEN_SOUND_PATH = audio_message.AudioPath.ToString();
                    break;
                case AudioUpdateMessage.AudioType.SwingClose:
                    CLOSE_SOUND_PATH = audio_message.AudioPath.ToString();
                    break;
                default:
                    break;
            }
        }

        private void HandleShowIme()
        {
            UseSwKeyboard();
            // Give the IME time to update
            new Android.OS.Handler(Android.OS.Looper.MainLooper).PostDelayed(delegate
            {
                ShowSwKeyboard();
            }, SHOW_IME_DELAY_MS);
        }

        private void HandleDisplayUpdate(DisplayUpdateMessage display_message)
        {
            switch (display_message.SwivelState)
            {
                case DisplayManagerHelper.SwivelStart:
                    Log.Debug(TAG, "DisplayManagerHelper.SwivelStart");
                    PlayWingSound(OPEN_SOUND_PATH);
                    break;
                case DisplayManagerHelper.SwivelEnd:
                    Log.Debug(TAG, "DisplayManagerHelper.SwivelEnd");
                    // We don't want to impose this behavior unless we are displaying the SoftWing IME
                    if (!IsUsingSwKeyboard() || !SoftWingInput.ImeIsOpen)
                    {
                        return;
                    }
                    UseLgKeyboard();
                    // Give the LG keyboard time to perform the screen transition
                    new Android.OS.Handler(Android.OS.Looper.MainLooper).PostDelayed(delegate
                    {
                        UseSwKeyboard();
                    }, IME_TRANSITION_DELAY_MS);
                    break;
                case DisplayManagerHelper.NonSwivelStart:
                    Log.Debug(TAG, "DisplayManagerHelper.NonSwivelStart");
                    PlayWingSound(CLOSE_SOUND_PATH);
                    break;
                default:
                    break;
            }
        }

        public void Accept(SoftWing.SwSystem.SystemMessage message)
        {
            Log.Debug(TAG, "Accept");

            switch (message.getMessageType())
            {
                case SwSystem.MessageType.ShowIme:
                    HandleShowIme();
                    break;
                case SwSystem.MessageType.DisplayUpdate:
                    HandleDisplayUpdate((DisplayUpdateMessage)message);
                    break;
                case SwSystem.MessageType.AudioUpdate:
                    HandleAudioUpdate((AudioUpdateMessage)message);
                    break;
                default:
                    Log.Debug(TAG, "Invalid message type");
                    break;
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
        private SwSystem.MessageDispatcher dispatcher;
        private bool ignore_transition = true;

        public LgSwivelStateCallback()
        {
            dispatcher = SwSystem.MessageDispatcher.GetInstance();
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