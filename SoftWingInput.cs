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
using Android.Widget;

namespace SoftWing
{
    [Service(Label = "SoftWingInput", Permission = "android.permission.BIND_INPUT_METHOD", Exported = true)]
    [IntentFilter(new[] { "android.view.InputMethod" })]
    [MetaData("android.view.im", Resource = "@xml/input_method_config")]
    public class SoftWingInput : InputMethodService, MessageSubscriber
    {
        private const String TAG = "SoftWingInput";
        private const int MULTI_DISPLAY_HEIGHT_PX = 1240;
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

            var keyboardView = LayoutInflater.Inflate(SwSettings.GetSelectedLayout(), null);

            // Force the controller to hide navigation buttons
            var uiOptions = SystemUiFlags.HideNavigation |
                 SystemUiFlags.LayoutHideNavigation |
                 SystemUiFlags.LayoutFullscreen |
                 SystemUiFlags.Fullscreen |
                 SystemUiFlags.LayoutStable |
                 SystemUiFlags.ImmersiveSticky;
            keyboardView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
            keyboardView.SetMinimumHeight(MULTI_DISPLAY_HEIGHT_PX);

            return keyboardView;
        }

        public override bool OnEvaluateFullscreenMode()
        {
            return false;
        }

        private void SetJoystickListener(View joystick, ControlId cid)
        {
            Log.Debug(TAG, "SetJoystickListener()");
            var joystick_frame = (FrameLayout)joystick;
            joystick_frame.RemoveAllViews();
            SurfaceView joystickSurface = new SurfaceView(this.BaseContext);
            joystick_frame.AddView(joystickSurface);
            var listener = new SwJoystickListener(joystickSurface, cid);
            joystickSurface.SetOnTouchListener(listener);
        }

        private void SetInputListener(View vin, ControlId cid)
        {
            Log.Debug(TAG, "SetInputListener()");
            var listener = new SwButtonListener(vin, cid);
            vin.SetOnTouchListener(listener);
        }

        private void SetInputListeners(ViewGroup keyboard_view_group)
        {
            Log.Debug(TAG, "SetInputListeners");
            foreach (var key in SwSettings.RESOURCE_TO_CONTROL_MAP.Keys)
            {
                View control = keyboard_view_group.FindViewById<View>(key);
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
            SetInputListeners((ViewGroup)this.Window.Window.DecorView);
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
