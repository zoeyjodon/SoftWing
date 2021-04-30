using Android.Util;
using Android.Views;
using System;
using SoftWing.System;
using SoftWing.System.Messages;
using static Android.Views.View;
using Android.App;
using Com.Jackandphantom.Joystickview;
using Android.Views.InputMethods;
using Android.OS;
using System.Threading.Tasks;

namespace SoftWing
{
    public class SwJoystickListener : Java.Lang.Object, JoyStickView.IOnMoveListener
    {
        private const String TAG = "SwJoystickListener";
        private const int STRENGTH_THRESHOLD = 50;

        private const double ANGLE_TOLERANCE = 67.5;

        private const int ANGLE_RIGHT = 0;
        private const double ANGLE_RIGHT_MAX = ANGLE_RIGHT + ANGLE_TOLERANCE;
        private const double ANGLE_RIGHT_MIN = 360 - ANGLE_TOLERANCE;

        private const int ANGLE_UP = 90;
        private const double ANGLE_UP_MAX = ANGLE_UP + ANGLE_TOLERANCE;
        private const double ANGLE_UP_MIN = ANGLE_UP - ANGLE_TOLERANCE;

        private const int ANGLE_LEFT = 180;
        private const double ANGLE_LEFT_MAX = ANGLE_LEFT + ANGLE_TOLERANCE;
        private const double ANGLE_LEFT_MIN = ANGLE_LEFT - ANGLE_TOLERANCE;

        private const int ANGLE_DOWN = 270;
        private const double ANGLE_DOWN_MAX = ANGLE_DOWN + ANGLE_TOLERANCE;
        private const double ANGLE_DOWN_MIN = ANGLE_DOWN - ANGLE_TOLERANCE;

        private MessageDispatcher dispatcher;
        private bool up_pressed = false;
        private bool down_pressed = false;
        private bool left_pressed = false;
        private bool right_pressed = false;

        public SwJoystickListener()
        {
            Log.Info(TAG, "SwJoystickListener");
            dispatcher = MessageDispatcher.GetInstance(new Activity());
        }

        ~SwJoystickListener()
        {
            Log.Info(TAG, "~SwJoystickListener");
        }

        public void OnMove(double angle, float strength)
        {
            if (strength < STRENGTH_THRESHOLD)
            {
                DisableRunningInputs();
                return;
            }
            HandleRightPress(angle);
            HandleLeftPress(angle);
            HandleUpPress(angle);
            HandleDownPress(angle);
        }

        private void HandleRightPress(double angle)
        {
            if ((angle < ANGLE_RIGHT_MAX) || (angle > ANGLE_RIGHT_MIN))
            {
                if (!right_pressed)
                {
                    Log.Info(TAG, "Right Pressed");
                    right_pressed = true;
                    dispatcher.Post(new ControlUpdateMessage(Keycode.DpadRight, ControlUpdateMessage.UpdateType.Pressed));
                }
            }
            else if (right_pressed)
            {
                Log.Info(TAG, "Right Released");
                right_pressed = false;
                dispatcher.Post(new ControlUpdateMessage(Keycode.DpadRight, ControlUpdateMessage.UpdateType.Released));
            }
        }

        private void HandleLeftPress(double angle)
        {
            if ((angle < ANGLE_LEFT_MAX) && (angle > ANGLE_LEFT_MIN))
            {
                if (!left_pressed)
                {
                    Log.Info(TAG, "Left Pressed");
                    left_pressed = true;
                    dispatcher.Post(new ControlUpdateMessage(Keycode.DpadLeft, ControlUpdateMessage.UpdateType.Pressed));
                }
            }
            else if (left_pressed)
            {
                Log.Info(TAG, "Left Released");
                left_pressed = false;
                dispatcher.Post(new ControlUpdateMessage(Keycode.DpadLeft, ControlUpdateMessage.UpdateType.Released));
            }
        }

        private void HandleUpPress(double angle)
        {
            if ((angle < ANGLE_UP_MAX) && (angle > ANGLE_UP_MIN))
            {
                if (!up_pressed)
                {
                    Log.Info(TAG, "Up Pressed");
                    up_pressed = true;
                    dispatcher.Post(new ControlUpdateMessage(Keycode.DpadUp, ControlUpdateMessage.UpdateType.Pressed));
                }
            }
            else if (up_pressed)
            {
                Log.Info(TAG, "Up Released");
                up_pressed = false;
                dispatcher.Post(new ControlUpdateMessage(Keycode.DpadUp, ControlUpdateMessage.UpdateType.Released));
            }
        }

        private void HandleDownPress(double angle)
        {
            if ((angle < ANGLE_DOWN_MAX) && (angle > ANGLE_DOWN_MIN))
            {
                if (!down_pressed)
                {
                    Log.Info(TAG, "Down Pressed");
                    down_pressed = true;
                    dispatcher.Post(new ControlUpdateMessage(Keycode.DpadDown, ControlUpdateMessage.UpdateType.Pressed));
                }
            }
            else if (down_pressed)
            {
                Log.Info(TAG, "Down Released");
                down_pressed = false;
                dispatcher.Post(new ControlUpdateMessage(Keycode.DpadDown, ControlUpdateMessage.UpdateType.Released));
            }
        }

        private void DisableRunningInputs()
        {
            if (up_pressed)
            {
                Log.Info(TAG, "Up Released");
                dispatcher.Post(new ControlUpdateMessage(Keycode.DpadUp, ControlUpdateMessage.UpdateType.Released));
            }
            if (down_pressed)
            {
                Log.Info(TAG, "Down Released");
                dispatcher.Post(new ControlUpdateMessage(Keycode.DpadDown, ControlUpdateMessage.UpdateType.Released));
            }
            if (left_pressed)
            {
                Log.Info(TAG, "Left Released");
                dispatcher.Post(new ControlUpdateMessage(Keycode.DpadLeft, ControlUpdateMessage.UpdateType.Released));
            }
            if (right_pressed)
            {
                Log.Info(TAG, "Right Released");
                dispatcher.Post(new ControlUpdateMessage(Keycode.DpadRight, ControlUpdateMessage.UpdateType.Released));
            }
            up_pressed = false;
            down_pressed = false;
            left_pressed = false;
            right_pressed = false;
        }
    }
}
