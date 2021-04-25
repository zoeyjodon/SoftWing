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
using Com.Lge.Display;
using Java.Interop;
using System;
using SoftWing.System.Messages;
using System.Threading.Tasks;
using static Android.Views.View;
using SoftWing.System;

namespace SoftWing
{
    public class TestTouchListener : Java.Lang.Object, IOnTouchListener
    {
        private const String TAG = "TestTouchListener";

        public TestTouchListener()
        {
            Log.Info(TAG, "TestTouchListener");
        }

        ~TestTouchListener()
        {
            Log.Info(TAG, "~TestTouchListener");
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    Log.Info(TAG, "OnTouch - Down");
                    SwDisplayManager.Dispatcher.Post(new ControlUpdateMessage(ControlUpdateMessage.UpdateType.DownPressed));
                    break;
                case MotionEventActions.Up:
                    Log.Info(TAG, "OnTouch - Up");
                    SwDisplayManager.Dispatcher.Post(new ControlUpdateMessage(ControlUpdateMessage.UpdateType.DownReleased));
                    break;
                default:
                    Log.Info(TAG, "OnTouch - Other");
                    SwDisplayManager.Dispatcher.Post(new ControlUpdateMessage(ControlUpdateMessage.UpdateType.DownHeld));
                    break;
            }
            return true;
        }
    }

    [Service(Label = "SoftWingInput", Permission = "android.permission.BIND_INPUT_METHOD")]
    [IntentFilter(new[] { "android.view.InputMethod" })]
    [MetaData("android.view.im", Resource = "@xml/method")]
    [MetaData("com.lge.special_display", Value = "true")]
    [MetaData("android.allow_multiple_resumed_activities", Value = "true")]
    [MetaData("com.android.internal.R.bool.config_perDisplayFocusEnabled", Value = "true")]
    public class SoftWingInput : InputMethodService, System.MessageSubscriber
    {
        private const String TAG = "SoftWingInput";
        private static NotificationReceiver mNotificationReceiver = null;
        private const String NOTIFICATION_CHANNEL_ID = "SWKeyboard";
        private const int NOTIFICATION_ONGOING_ID = 1001;

        public static IBinder mToken;

        public class SwInputMethodImpl : InputMethodImpl
        {
            private SoftWingInput owner;
            public SwInputMethodImpl(SoftWingInput _owner)
                : base(_owner)
            {
                owner = _owner;
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

        public override void OnCreate()
        {
            Log.Debug(TAG, "onCreate()");
            base.OnCreate();

            if (mNotificationReceiver == null)
            {
                SetNotification();
            }
            // If we aren't running the swapper yet, we should be
            if (SwDisplayManager.Instance == null)
            {
                var intent = new Intent(this, typeof(SwDisplayManager));
                var flags = ActivityFlags.NewTask | ActivityFlags.MultipleTask | ActivityFlags.ClearTop;
                intent.AddFlags(flags);
                StartActivity(intent);
            }
        }

        public override View OnCreateInputView()
        {
            Log.Debug(TAG, "onCreateInputView()");

            var keyboardView = LayoutInflater.Inflate(Resource.Layout.input, null);
            SetTestButtonListener((ViewGroup)keyboardView);

            return keyboardView;
        }

        private void SetTestButtonListener(ViewGroup keyboard_view_group)
        {
            for (int index = 0; index < keyboard_view_group.ChildCount; index++)
            {
                View nextChild = keyboard_view_group.GetChildAt(index);
                if (nextChild.Id == Resource.Id.testButton)
                {
                    nextChild.SetOnTouchListener(new TestTouchListener());
                    break;
                }
            }
        }

        public override void OnStartInputView(EditorInfo info, bool restarting)
        {
            Log.Debug(TAG, "OnStartInputView()");
            base.OnStartInputView(info, restarting);
            SwDisplayManager.Dispatcher.Subscribe(System.MessageType.ControlUpdate, this);
        }

        public override AbstractInputMethodImpl OnCreateInputMethodInterface()
        {
            Log.Debug(TAG, "OnCreateInputMethodInterface()");
            return new SwInputMethodImpl(this);
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

        public void Accept(SystemMessage message)
        {
            if (message.getMessageType() != MessageType.ControlUpdate)
            {
                return;
            }
            var control_message = (ControlUpdateMessage)message;
            switch (control_message.getUpdateType())
            {
                case ControlUpdateMessage.UpdateType.DownPressed:
                    SendDownUpKeyEvents(Android.Views.Keycode.DpadCenter);
                    SendDownUpKeyEvents(Android.Views.Keycode.DpadDown);
                    break;
                case ControlUpdateMessage.UpdateType.DownReleased:
                    break;
                case ControlUpdateMessage.UpdateType.DownHeld:
                    SendDownUpKeyEvents(Android.Views.Keycode.DpadDown);
                    break;
                default:
                    break;
            }
        }
    }
}
