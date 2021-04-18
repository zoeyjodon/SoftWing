using Android.App;
using Android.Content;
using Android.InputMethodServices;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.Core.App;
using Java.Interop;
using System;
using System.Threading.Tasks;

namespace SoftWing
{
    [Service(Label = "SoftWingInput", Permission = "android.permission.BIND_INPUT_METHOD")]
    [IntentFilter(new[] { "android.view.InputMethod" })]
    [MetaData("android.view.im", Resource = "@xml/method")]
    [MetaData("com.lge.special_display", Value = "true")]
    [MetaData("android.allow_multiple_resumed_activities", Value = "true")]
    [MetaData("com.android.internal.R.bool.config_perDisplayFocusEnabled", Value = "true")]
    public class SoftWingInput : InputMethodService
    {
        private const String TAG = "SoftWingInput";
        public static IInputConnection Connection = null;
        public static SoftWingInput Instance = null;
        private static NotificationReceiver mNotificationReceiver = null;
        private static String NOTIFICATION_CHANNEL_ID = "SWKeyboard";
        private static int NOTIFICATION_ONGOING_ID = 1001;
        private static IWindowManager mWindowManager = null;

        public override void OnCreate()
        {
            Log.Debug(TAG, "onCreate()");
            base.OnCreate();

            if (mNotificationReceiver == null)
            {
                SetNotification();
            }
        }

        public override View OnCreateInputView()
        {
            Log.Debug(TAG, "onCreateInputView()");

            var keyboardView = LayoutInflater.Inflate(SoftWing.Resource.Layout.input, null);
            Instance = this;

            return keyboardView;
        }

        public override void OnStartInputView(EditorInfo info, bool restarting)
        {
            Log.Debug(TAG, "OnStartInputView()");
            base.OnStartInputView(info, restarting);

            // If we aren't running the swapper yet, we should be
            if (ServiceScreenSwapper.RunningSwapperActivity == null)
            {
                var intent = new Intent(this, typeof(ServiceScreenSwapper));
                var flags = ActivityFlags.NewTask | ActivityFlags.MultipleTask | ActivityFlags.ClearTop;
                intent.AddFlags(flags);
                StartActivity(intent);
            }
            SendDownUpKeyEvents(Android.Views.Keycode.DpadCenter);
            SendDownUpKeyEvents(Android.Views.Keycode.DpadDown);
        }

        public override AbstractInputMethodImpl OnCreateInputMethodInterface()
        {
            Log.Debug(TAG, "OnCreateInputMethodInterface()");
            return new MyInputMethodImpl(this);
        }

        public static IBinder mToken;
        public class MyInputMethodImpl : InputMethodImpl
        {
            public MyInputMethodImpl(InputMethodService owner)
                : base(owner)
            {
            }

            public override void AttachToken(IBinder token)
            {
                Log.Info(TAG, "attachToken " + token);
                base.AttachToken(token);
                if (mToken == null)
                {
                    Log.Info(TAG, "Saving new token");
                    mToken = token;
                }
            }
        }

        public static void ClickTestButton(Context calling_context)
        {
            if (Instance != null)
            {
                Instance.testButtonClicked(new View(calling_context));
            }
        }

        [Export("testButtonClicked")]
        public void testButtonClicked(View v)
        {
            Log.Debug(TAG, "testButtonClicked()");
            SendDownUpKeyEvents(Android.Views.Keycode.DpadCenter);
            SendDownUpKeyEvents(Android.Views.Keycode.DpadDown);
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
            String ns = Context.NotificationService;
            NotificationManager mNotificationManager = (NotificationManager)GetSystemService(ns);

            CreateNotificationChannel();
            int icon = Resource.Drawable.notification_icon_background;
            var text = "Keyboard notification enabled.";
            long when = Java.Lang.JavaSystem.CurrentTimeMillis();

            // TODO: clean this up?
            mNotificationReceiver = new NotificationReceiver(this);
            var pFilter = new IntentFilter(NotificationReceiver.ACTION_SHOW);
            RegisterReceiver(mNotificationReceiver, pFilter);

            Intent notificationIntent = new Intent(NotificationReceiver.ACTION_SHOW);
            PendingIntent contentIntent = PendingIntent.GetBroadcast(Application.Context, 1, notificationIntent, 0);
            //PendingIntent contentIntent = PendingIntent.getActivity(this, 0, notificationIntent, 0);

            String title = "Show SoftWing Keyboard";
            String body = "Select this to open the keyboard. Disable in settings.";

            NotificationCompat.Builder mBuilder = new NotificationCompat.Builder(this, NOTIFICATION_CHANNEL_ID)
                    .SetSmallIcon(Resource.Mipmap.ic_notification)
                    .SetColor(Resource.Color.accent_material_dark)
                    .SetAutoCancel(false) //Make this notification automatically dismissed when the user touches it -> false.
                    .SetTicker(text)
                    .SetContentTitle(title)
                    .SetContentText(body)
                    .SetContentIntent(contentIntent)
                    .SetOngoing(true)
                    .SetVisibility((int)NotificationVisibility.Public)
                    .SetPriority(NotificationCompat.PriorityDefault);

            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(this);

            // notificationId is a unique int for each notification that you must define
            notificationManager.Notify(NOTIFICATION_ONGOING_ID, mBuilder.Build());
        }
    }
}