using Android.Util;
using Android.Views;
using System;
using SoftWing.SwSystem;
using SoftWing.SwSystem.Messages;
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
        private const int STRENGTH_THRESHOLD = 25;

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

        private const int MOTION_ANGLE_INCREMENT_DEGREES = 30;
        private const int MOTION_FORCE_INCREMENT_PERCENT = 33;

        private Keycode up_keycode = Keycode.Unknown;
        private Keycode down_keycode = Keycode.Unknown;
        private Keycode left_keycode = Keycode.Unknown;
        private Keycode right_keycode = Keycode.Unknown;
        private MotionDescription motion = MotionDescription.InvalidMotion();
        private MotionDescription lastMotion = MotionDescription.InvalidMotion();
        private int motionId = MotionUpdateMessage.GetMotionId();
        private MessageDispatcher dispatcher;
        private bool up_pressed = false;
        private bool down_pressed = false;
        private bool left_pressed = false;
        private bool right_pressed = false;

        public SwJoystickListener(Keycode up_keycode_in, Keycode down_keycode_in, Keycode left_keycode_in, Keycode right_keycode_in)
        {
            Log.Info(TAG, "SwJoystickListener");
            up_keycode = up_keycode_in;
            down_keycode = down_keycode_in;
            left_keycode = left_keycode_in;
            right_keycode = right_keycode_in;
            dispatcher = MessageDispatcher.GetInstance(new Activity());
        }
        public SwJoystickListener(MotionDescription motion_in)
        {
            Log.Info(TAG, "SwJoystickListener");
            motion = motion_in;
            dispatcher = MessageDispatcher.GetInstance(new Activity());
        }

        ~SwJoystickListener()
        {
            Log.Info(TAG, "~SwJoystickListener");
        }

        public void OnMove(double angle, float strength)
        {
            if (up_keycode == Keycode.Unknown)
            {
                HandleMotion(angle, strength);
                return;
            }

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

        private bool TopRegionActive(double angle)
        {
            return (angle < ANGLE_UP_MAX) && (angle > ANGLE_UP_MIN);
        }

        private bool BottomRegionActive(double angle)
        {
            return (angle < ANGLE_DOWN_MAX) && (angle > ANGLE_DOWN_MIN);
        }

        private bool RightRegionActive(double angle)
        {
            return (angle < ANGLE_RIGHT_MAX) || (angle > ANGLE_RIGHT_MIN);
        }

        private bool LeftRegionActive(double angle)
        {
            return (angle < ANGLE_LEFT_MAX) && (angle > ANGLE_LEFT_MIN);
        }

        private double ClipAngle(double angle)
        {
            angle = (angle + (MOTION_ANGLE_INCREMENT_DEGREES / 2)) % 360;
            angle = (int)(angle / MOTION_ANGLE_INCREMENT_DEGREES) * MOTION_ANGLE_INCREMENT_DEGREES;
            return angle;
        }

        private double ClipStrength(double strength)
        {
            strength = strength + (MOTION_FORCE_INCREMENT_PERCENT / 2);
            strength = (int)(strength / MOTION_FORCE_INCREMENT_PERCENT) * MOTION_FORCE_INCREMENT_PERCENT;
            return strength;
        }

        private MotionDescription CalculateMotion(double angle, float strength)
        {
            var diffX2 = Math.Pow((motion.endX - motion.beginX), 2);
            var diffY2 = Math.Pow((motion.endY - motion.beginY), 2);
            var distance = Math.Sqrt(diffX2 + diffY2);
            double strengthMod = distance * strength / 100.0;

            if (motion.type != MotionType.Tap)
            {
                // Clip joystick controls to avoid choppy behavior
                strengthMod = ClipStrength(strengthMod);
                angle = ClipAngle(angle);
            }
            double angleRad = Math.PI * angle / 180.0;
            float endX = motion.beginX + (float)(strengthMod * Math.Cos(angleRad));
            float endY = motion.beginY - (float)(strengthMod * Math.Sin(angleRad));
            return new MotionDescription(motion.type, motion.beginX, motion.beginY, endX, endY);
        }

        private bool MotionHasChanged(MotionDescription motion)
        {
            return (motion.endX != lastMotion.endX) ||
                   (motion.endY != lastMotion.endY);
        }

        private void HandleMotion(double angle, float strength)
        {
            bool motionComplete = strength == 0;
            var angleMotion = CalculateMotion(angle, strength);

            if (MotionHasChanged(angleMotion))
            {
                dispatcher.Post(new MotionUpdateMessage(motionId, angleMotion, motionComplete));
                lastMotion = angleMotion;
            }
        }

        private void HandleRightPress(double angle)
        {
            if (RightRegionActive(angle))
            {
                if (!right_pressed)
                {
                    Log.Info(TAG, "Right Pressed");
                    right_pressed = true;
                    dispatcher.Post(new ControlUpdateMessage(right_keycode, ControlUpdateMessage.UpdateType.Pressed));
                }
            }
            else if (right_pressed)
            {
                Log.Info(TAG, "Right Released");
                right_pressed = false;
                dispatcher.Post(new ControlUpdateMessage(right_keycode, ControlUpdateMessage.UpdateType.Released));
            }
        }

        private void HandleLeftPress(double angle)
        {
            if (LeftRegionActive(angle))
            {
                if (!left_pressed)
                {
                    Log.Info(TAG, "Left Pressed");
                    left_pressed = true;
                    dispatcher.Post(new ControlUpdateMessage(left_keycode, ControlUpdateMessage.UpdateType.Pressed));
                }
            }
            else if (left_pressed)
            {
                Log.Info(TAG, "Left Released");
                left_pressed = false;
                dispatcher.Post(new ControlUpdateMessage(left_keycode, ControlUpdateMessage.UpdateType.Released));
            }
        }

        private void HandleUpPress(double angle)
        {
            if (TopRegionActive(angle))
            {
                if (!up_pressed)
                {
                    Log.Info(TAG, "Up Pressed");
                    up_pressed = true;
                    dispatcher.Post(new ControlUpdateMessage(up_keycode, ControlUpdateMessage.UpdateType.Pressed));
                }
            }
            else if (up_pressed)
            {
                Log.Info(TAG, "Up Released");
                up_pressed = false;
                dispatcher.Post(new ControlUpdateMessage(up_keycode, ControlUpdateMessage.UpdateType.Released));
            }
        }

        private void HandleDownPress(double angle)
        {
            if (BottomRegionActive(angle))
            {
                if (!down_pressed)
                {
                    Log.Info(TAG, "Down Pressed");
                    down_pressed = true;
                    dispatcher.Post(new ControlUpdateMessage(down_keycode, ControlUpdateMessage.UpdateType.Pressed));
                }
            }
            else if (down_pressed)
            {
                Log.Info(TAG, "Down Released");
                down_pressed = false;
                dispatcher.Post(new ControlUpdateMessage(down_keycode, ControlUpdateMessage.UpdateType.Released));
            }
        }

        private void DisableRunningInputs()
        {
            if (up_pressed)
            {
                Log.Info(TAG, "Up Released");
                dispatcher.Post(new ControlUpdateMessage(up_keycode, ControlUpdateMessage.UpdateType.Released));
            }
            if (down_pressed)
            {
                Log.Info(TAG, "Down Released");
                dispatcher.Post(new ControlUpdateMessage(down_keycode, ControlUpdateMessage.UpdateType.Released));
            }
            if (left_pressed)
            {
                Log.Info(TAG, "Left Released");
                dispatcher.Post(new ControlUpdateMessage(left_keycode, ControlUpdateMessage.UpdateType.Released));
            }
            if (right_pressed)
            {
                Log.Info(TAG, "Right Released");
                dispatcher.Post(new ControlUpdateMessage(right_keycode, ControlUpdateMessage.UpdateType.Released));
            }
            up_pressed = false;
            down_pressed = false;
            left_pressed = false;
            right_pressed = false;
        }
    }
}
