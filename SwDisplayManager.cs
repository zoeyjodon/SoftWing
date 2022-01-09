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
using System.IO;
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
        private SwSystem.MessageDispatcher dispatcher;
        private static SwDisplayManager instance;

        private const String NOTIFICATION_CHANNEL_ID = "SWKeyboard";
        private const int NOTIFICATION_ONGOING_ID = 1001;
        private static NotificationReceiver notification_receiver = null;

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
            SetNotification();
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

            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(this);
            StartForeground(NOTIFICATION_ONGOING_ID, mBuilder.Build());
        }

        private void ShowSwKeyboard()
        {
            Log.Debug(TAG, "ShowSwKeyboard");
            SoftWingInput.StartSoftWingInput(lg_display_manager.MultiDisplayId);
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
            ShowSwKeyboard();
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
                    break;
                case DisplayManagerHelper.NonSwivelStart:
                    Log.Debug(TAG, "DisplayManagerHelper.NonSwivelStart");
                    PlayWingSound(CLOSE_SOUND_PATH);
                    break;
                case DisplayManagerHelper.NonSwivelEnd:
                    Log.Debug(TAG, "DisplayManagerHelper.NonSwivelEnd");
                    SoftWingInput.StopSoftWingInput();
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