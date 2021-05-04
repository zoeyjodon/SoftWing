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
        }

        public override View OnCreateInputView()
        {
            Log.Debug(TAG, "onCreateInputView()");

            SwDisplayManager.StartSwDisplayManager(this);

            var keyboardView = LayoutInflater.Inflate(Resource.Layout.input, null);
            SetInputListeners((ViewGroup)keyboardView);

            keyboardView.SetMinimumHeight(MULTI_DISPLAY_HEIGHT_PX);

            return keyboardView;
        }

        public override bool OnEvaluateFullscreenMode()
        {
            return false;
        }

        private void SetJoystickListener(JoyStickView joystick, Android.Views.Keycode up, Android.Views.Keycode down, Android.Views.Keycode left, Android.Views.Keycode right)
        {
            var listener = new SwJoystickListener(up, down, left, right);
            joystick.SetOnMoveListener(listener);
        }

        private void SetInputListeners(ViewGroup keyboard_view_group)
        {
            Log.Debug(TAG, "SetInputListeners");
            for (int index = 0; index < keyboard_view_group.ChildCount; index++)
            {
                View nextChild = keyboard_view_group.GetChildAt(index);
                switch (nextChild.Id)
                {
                    case (Resource.Id.left_joyStick):
                        SetJoystickListener((JoyStickView)nextChild, Android.Views.Keycode.W, Android.Views.Keycode.S, Android.Views.Keycode.A, Android.Views.Keycode.D);
                        break;
                    case (Resource.Id.right_joyStick):
                        SetJoystickListener((JoyStickView)nextChild, Android.Views.Keycode.Button1, Android.Views.Keycode.Button2, Android.Views.Keycode.Button3, Android.Views.Keycode.Button4);
                        break;
                    case (Resource.Id.d_pad_up):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, Android.Views.Keycode.DpadUp));
                        break;
                    case (Resource.Id.d_pad_down):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, Android.Views.Keycode.DpadDown));
                        break;
                    case (Resource.Id.d_pad_left):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, Android.Views.Keycode.DpadLeft));
                        break;
                    case (Resource.Id.d_pad_right):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, Android.Views.Keycode.DpadRight));
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
                    case (Resource.Id.start_button):
                        nextChild.SetOnTouchListener(new SwButtonListener(nextChild, Android.Views.Keycode.ButtonStart));
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
