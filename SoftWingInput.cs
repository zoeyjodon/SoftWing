using Android.App;
using Android.Content;
using Android.InputMethodServices;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using System;
using SoftWing.SwSystem.Messages;
using SoftWing.SwSystem;

namespace SoftWing
{
    [Service(Label = "SoftWingInput", Permission = "android.permission.BIND_INPUT_METHOD", Exported = true)]
    [IntentFilter(new[] { "android.view.InputMethod" })]
    [MetaData("android.view.im", Resource = "@xml/input_method_config")]
    public class SoftWingInput : InputMethodService, MessageSubscriber
    {
        private const String TAG = "SoftWingInput";
        private const int MULTI_DISPLAY_HEIGHT_PX = 1240;
        private View? keyboardView = null;
        private MessageDispatcher dispatcher;

        public static IBinder InputSessionToken;
        public static bool ImeIsOpen = false;

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
        }

        public override View OnCreateInputView()
        {
            Log.Debug(TAG, "onCreateInputView()");

            SwDisplayManager.StartSwDisplayManager();

            keyboardView = LayoutInflater.Inflate(SwSettings.GetSelectedLayout(), null);

            // Force the controller to hide navigation buttons
            var uiOptions = SystemUiFlags.HideNavigation |
                 SystemUiFlags.LayoutHideNavigation |
                 SystemUiFlags.LayoutFullscreen |
                 SystemUiFlags.Fullscreen |
                 SystemUiFlags.LayoutStable |
                 SystemUiFlags.ImmersiveSticky;
            keyboardView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
            SetInputListeners((ViewGroup)keyboardView);
            keyboardView.SetMinimumHeight(MULTI_DISPLAY_HEIGHT_PX);
            ImeIsOpen = true;

            return keyboardView;
        }

        public override bool OnEvaluateFullscreenMode()
        {
            return false;
        }

        private void SetJoystickListener(View joystick, SwSettings.ControlId cid)
        {
            Log.Debug(TAG, "SetJoystickListener()");
            var motion = SwSettings.GetControlMotion(cid);
            joystick.SetOnTouchListener(new SwJoystickListener(motion));
        }

        private void SetInputListener(View vin, SwSettings.ControlId cid)
        {
            Log.Debug(TAG, "SetInputListener()");
            var motion = SwSettings.GetControlMotion(cid);
            var vibrate = SwSettings.GetVibrationEnable();
            vin.SetOnTouchListener(new SwButtonListener(vin, motion, vibrate));
        }

        private void SetInputListeners(ViewGroup keyboard_view_group)
        {
            Log.Debug(TAG, "SetInputListeners");
            foreach (var key in SwSettings.RESOURCE_TO_CONTROL_MAP.Keys)
            {
                View control = keyboardView.FindViewById<View>(key);
                var control_id = SwSettings.RESOURCE_TO_CONTROL_MAP[key];
                switch (key)
                {
                    case (Resource.Id.left_joyStick):
                    case (Resource.Id.right_joyStick):
                        SetJoystickListener(control, control_id);
                        break;
                    default:
                        SetInputListener(control, control_id);
                        break;
                }
            }
        }

        public override void OnStartInputView(EditorInfo info, bool restarting)
        {
            Log.Debug(TAG, "OnStartInputView()");
            base.OnStartInputView(info, restarting);
            dispatcher = MessageDispatcher.GetInstance();
            dispatcher.Subscribe(SwSystem.MessageType.ControlUpdate, this);
            ImeIsOpen = true;
        }

        public override void OnFinishInputView(bool finishingInput)
        {
            Log.Debug(TAG, "OnFinishInputView()");
            base.OnFinishInputView(finishingInput);
            dispatcher.Unsubscribe(SwSystem.MessageType.ControlUpdate, this);
            ImeIsOpen = false;
        }

        public override AbstractInputMethodImpl OnCreateInputMethodInterface()
        {
            Log.Debug(TAG, "OnCreateInputMethodInterface()");
            return new SwInputMethodImpl(this);
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
            if (control_message.Key == null) { return; }
            var key_code = (Android.Views.Keycode) control_message.Key;
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
