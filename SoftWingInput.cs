using Android.App;
using Android.Content;
using Android.InputMethodServices;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using AndroidX.Core.App;
using System;
using SoftWing.System.Messages;
using Com.Jackandphantom.Joystickview;
using SoftWing.System;

namespace SoftWing
{
    [Service(Label = "SoftWingInput", Permission = "android.permission.BIND_INPUT_METHOD")]
    [IntentFilter(new[] { "android.view.InputMethod" })]
    [MetaData("android.view.im", Resource = "@xml/method")]
    [MetaData("com.lge.special_display", Value = "true")]
    [MetaData("android.allow_multiple_resumed_activities", Value = "true")]
    [MetaData("com.android.internal.R.bool.config_perDisplayFocusEnabled", Value = "true")]
    public class SoftWingInput : InputMethodService, System.MessageSubscriber
    {
        private const String TAG = "SoftWingInput";
        private const String NOTIFICATION_CHANNEL_ID = "SWKeyboard";
        private const int NOTIFICATION_ONGOING_ID = 1001;
        private const int MULTI_DISPLAY_HEIGHT_PX = 1240;
        private MessageDispatcher dispatcher;
        private static NotificationReceiver notification_receiver = null;

        public static IBinder InputSessionToken;

        public class SwInputMethodImpl : InputMethodImpl
        {
            public SwInputMethodImpl(SoftWingInput _owner)
                : base(_owner)
            {
            }

            public override void AttachToken(IBinder token)
            {
                Log.Info(TAG, "attachToken " + token);
                base.AttachToken(token);
                InputSessionToken = token;
            }
        }

        public override void OnCreate()
        {
            Log.Debug(TAG, "onCreate()");
            base.OnCreate();

            SetNotification();
            SwDisplayManager.StartSwDisplayManager(this);
        }

        public override View OnCreateInputView()
        {
            Log.Debug(TAG, "onCreateInputView()");

            var keyboardView = LayoutInflater.Inflate(Resource.Layout.input, null);
            SetTestButtonListener((ViewGroup)keyboardView);

            keyboardView.SetMinimumHeight(MULTI_DISPLAY_HEIGHT_PX);

            return keyboardView;
        }

        public override bool OnEvaluateFullscreenMode()
        {
            return true;
        }

        private void SetJoystickListener(JoyStickView joystick)
        {
            var listener = new SwJoystickListener();
            joystick.SetOnMoveListener(listener);
        }

        private void SetTestButtonListener(ViewGroup keyboard_view_group)
        {
            Log.Debug(TAG, "SetTestButtonListener");
            for (int index = 0; index < keyboard_view_group.ChildCount; index++)
            {
                View nextChild = keyboard_view_group.GetChildAt(index);
                switch (nextChild.Id)
                {
                    case (Resource.Id.joyStick):
                        SetJoystickListener((JoyStickView)nextChild);
                        break;
                    case (Resource.Id.d_pad_up):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, Android.Views.Keycode.W));
                        break;
                    case (Resource.Id.d_pad_down):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, Android.Views.Keycode.S));
                        break;
                    case (Resource.Id.d_pad_left):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, Android.Views.Keycode.A));
                        break;
                    case (Resource.Id.d_pad_right):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, Android.Views.Keycode.D));
                        break;
                    case (Resource.Id.d_pad_center):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, Android.Views.Keycode.DpadCenter));
                        break;
                    case (Resource.Id.a_button):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, Android.Views.Keycode.ButtonA));
                        break;
                    case (Resource.Id.b_button):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, Android.Views.Keycode.ButtonB));
                        break;
                    case (Resource.Id.y_button):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, Android.Views.Keycode.ButtonY));
                        break;
                    case (Resource.Id.x_button):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, Android.Views.Keycode.ButtonX));
                        break;
                    case (Resource.Id.l_button):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, Android.Views.Keycode.ButtonL1));
                        break;
                    case (Resource.Id.r_button):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, Android.Views.Keycode.ButtonR1));
                        break;
                    default:
                        break;
                }
            }
        }

        public override void OnStartInputView(EditorInfo info, bool restarting)
        {
            Log.Debug(TAG, "OnStartInputView()");
            base.OnStartInputView(info, restarting);
            dispatcher = MessageDispatcher.GetInstance(new Activity());
            dispatcher.Subscribe(System.MessageType.ControlUpdate, this);
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

            notificationManager.Notify(NOTIFICATION_ONGOING_ID, mBuilder.Build());
        }

        public void Accept(SystemMessage message)
        {
            if (message.getMessageType() != MessageType.ControlUpdate)
            {
                return;
            }
            if (CurrentInputConnection == null)
            {
                Log.Debug(TAG, "Connection is null, ignoring key update");
                return;
            }
            var control_message = (ControlUpdateMessage)message;
            var key_code = control_message.Key;
            switch (control_message.Update)
            {
                case ControlUpdateMessage.UpdateType.Pressed:
                    Log.Debug(TAG, "Accept(UpdateType.Pressed)");
                    CurrentInputConnection.SendKeyEvent(new KeyEvent(KeyEventActions.Down, key_code));
                    break;
                case ControlUpdateMessage.UpdateType.Released:
                    Log.Debug(TAG, "Accept(UpdateType.Released)");
                    CurrentInputConnection.SendKeyEvent(new KeyEvent(KeyEventActions.Up, key_code));
                    break;
                default:
                    break;
            }
        }
    }
}
