using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using AndroidX.Core.App;
using Android.Views.InputMethods;
using Com.Lge.Display;
using SoftWing.System;
using Android.Provider;
using System;

namespace SoftWing
{
    [Service(Exported = true, Enabled = true, Name = "com.jodonlucas.softwing.SoftWing.SwDisplayManager")]
    public class SwDisplayManager : Service, System.MessageSubscriber
    {
        private const String TAG = "SwDisplayManager";
        private const String NOTIFICATION_CHANNEL_ID = "SWKeyboard";
        private const int NOTIFICATION_ONGOING_ID = 1001;
        private const int LG_KEYBOARD_TIMEOUT_MS = 500;
        private DisplayManagerHelper lg_display_manager;
        private LgSwivelStateCallback swivel_state_cb;
        private MessageDispatcher dispatcher;
        private static SwDisplayManager instance;
        private static NotificationReceiver notification_receiver = null;


        public static void StartSwDisplayManager(Context calling_context)
        {
            Log.Debug(TAG, "StartSwDisplayManager");
            if (instance != null)
            {
                Log.Debug(TAG, "Display manager exists, skipping");
                return;
            }
            var intent = new Intent(Application.Context, typeof(SwDisplayManager));
            Application.Context.StartForegroundService(intent);
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

            dispatcher = MessageDispatcher.GetInstance();
            dispatcher.Subscribe(System.MessageType.DisplayUpdate, this);

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
            NotificationManager notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
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

        public static void ShowSwKeyboard()
        {
            InputMethodManager input_manager = (InputMethodManager)
                Application.Context.GetSystemService(Context.InputMethodService);
            if (input_manager != null)
            {
                input_manager.ShowSoftInputFromInputMethod(SoftWingInput.InputSessionToken, ShowFlags.Forced);
            }
            // If we start from the open position, we want to execute the swivel transition immediately
            instance.dispatcher.Post(new System.Messages.DisplayUpdateMessage());
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

        public void Accept(SystemMessage message)
        {
            Log.Debug(TAG, "Accept");
            // We don't want to impose this behavior unless we are using the SoftWing IME
            if (!IsUsingSwKeyboard())
            {
                return;
            }
            if (lg_display_manager.SwivelState == DisplayManagerHelper.SwivelSwiveled)
            {
                // Lock the phone's orientation to prevent unexpected behavior
                LockOrientation();
                UseLgKeyboard();
                // Give the LG keyboard time to perform the screen transition
                new Handler().PostDelayed(delegate
                {
                    UseSwKeyboard();
                }, LG_KEYBOARD_TIMEOUT_MS);
            }
        }

        public override IBinder OnBind(Intent intent)
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
        private MessageDispatcher dispatcher;
        private bool ignore_transition = true;

        public LgSwivelStateCallback()
        {
            dispatcher = MessageDispatcher.GetInstance(new Activity());
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
            switch (state)
            {
                case DisplayManagerHelper.SwivelStart:
                    // Swivel start
                    Log.Debug(TAG, "Swivel Open start");
                    break;
                case DisplayManagerHelper.SwivelEnd:
                    // Swivel complete
                    Log.Debug(TAG, "Swivel Open end");
                    dispatcher.Post(new System.Messages.DisplayUpdateMessage());
                    break;
                case DisplayManagerHelper.NonSwivelStart:
                    // Non Swivel start
                    Log.Debug(TAG, "Swivel Closed start");
                    break;
                case DisplayManagerHelper.NonSwivelEnd:
                    // Non Swivel complete
                    Log.Debug(TAG, "Swivel Closed end");
                    dispatcher.Post(new System.Messages.DisplayUpdateMessage());
                    break;
                default:
                    // default value
                    break;
            }
        }
    }
}