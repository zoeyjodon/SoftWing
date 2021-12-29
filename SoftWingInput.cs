using Android.App;
using Android.Content;
using Android.InputMethodServices;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using System;
using SoftWing.SwSystem.Messages;
using Com.Jackandphantom.Joystickview;
using SoftWing.SwSystem;

namespace SoftWing
{
    [Service(Label = "SoftWingInput", Permission = "android.permission.BIND_INPUT_METHOD")]
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

            keyboardView = LayoutInflater.Inflate(Resource.Layout.input, null);
            keyboardView.SetMinimumHeight(MULTI_DISPLAY_HEIGHT_PX);
            ImeIsOpen = true;

            return keyboardView;
        }

        public override void OnFinishInputView(bool finishingInput)
        {
            base.OnFinishInputView(finishingInput);
            ImeIsOpen = false;
        }

        public override bool OnEvaluateFullscreenMode()
        {
            return false;
        }

        private void SetJoystickListener(JoyStickView joystick, SwSettings.ControlId up, SwSettings.ControlId down, SwSettings.ControlId left, SwSettings.ControlId right)
        {
            var upKey = SwSettings.GetControlKeycode(up);
            if (upKey == Android.Views.Keycode.Unknown)
            {
                var motion = SwSettings.GetControlMotion(up);
                joystick.SetOnMoveListener(new SwJoystickListener(motion));
                return;
            }

            var downKey = SwSettings.GetControlKeycode(down);
            var leftKey = SwSettings.GetControlKeycode(left);
            var rightKey = SwSettings.GetControlKeycode(right);
            joystick.SetOnMoveListener(new SwJoystickListener(upKey, downKey, leftKey, rightKey));
        }

        private void SetInputListener(View vin, SwSettings.ControlId cid)
        {
            var key = SwSettings.GetControlKeycode(cid);
            var motion = SwSettings.GetControlMotion(cid);
            var vibrate = SwSettings.GetVibrationEnable();
            vin.SetOnTouchListener(new SwButtonListener(vin, key, motion, vibrate));
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
                        {
                            SetJoystickListener((JoyStickView)nextChild,
                                SwSettings.ControlId.L_Analog_Up,
                                SwSettings.ControlId.L_Analog_Down,
                                SwSettings.ControlId.L_Analog_Left,
                                SwSettings.ControlId.L_Analog_Right);
                        }
                        break;
                    case (Resource.Id.right_joyStick):
                        {
                            SetJoystickListener((JoyStickView)nextChild,
                                SwSettings.ControlId.R_Analog_Up,
                                SwSettings.ControlId.R_Analog_Down,
                                SwSettings.ControlId.R_Analog_Left,
                                SwSettings.ControlId.R_Analog_Right);
                        }
                        break;
                    case (Resource.Id.d_pad_up):
                        SetInputListener(nextChild, SwSettings.ControlId.D_Pad_Up);
                        break;
                    case (Resource.Id.d_pad_down):
                        SetInputListener(nextChild, SwSettings.ControlId.D_Pad_Down);
                        break;
                    case (Resource.Id.d_pad_left):
                        SetInputListener(nextChild, SwSettings.ControlId.D_Pad_Left);
                        break;
                    case (Resource.Id.d_pad_right):
                        SetInputListener(nextChild, SwSettings.ControlId.D_Pad_Right);
                        break;
                    case (Resource.Id.d_pad_center):
                        SetInputListener(nextChild, SwSettings.ControlId.D_Pad_Center);
                        break;
                    case (Resource.Id.a_button):
                        SetInputListener(nextChild, SwSettings.ControlId.A_Button);
                        break;
                    case (Resource.Id.b_button):
                        SetInputListener(nextChild, SwSettings.ControlId.B_Button);
                        break;
                    case (Resource.Id.y_button):
                        SetInputListener(nextChild, SwSettings.ControlId.Y_Button);
                        break;
                    case (Resource.Id.x_button):
                        SetInputListener(nextChild, SwSettings.ControlId.X_Button);
                        break;
                    case (Resource.Id.l_button):
                        SetInputListener(nextChild, SwSettings.ControlId.L_Button);
                        break;
                    case (Resource.Id.r_button):
                        SetInputListener(nextChild, SwSettings.ControlId.R_Button);
                        break;
                    case (Resource.Id.start_button):
                        SetInputListener(nextChild, SwSettings.ControlId.Start_Button);
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
            dispatcher.Subscribe(SwSystem.MessageType.ControlUpdate, this);
            SetInputListeners((ViewGroup)keyboardView);
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
