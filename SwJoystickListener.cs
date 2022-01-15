using Android.Util;
using System;
using SoftWing.SwSystem;
using SoftWing.SwSystem.Messages;
using Android.App;
using Com.Jackandphantom.Joystickview;
using static SoftWing.SwSystem.SwSettings;

namespace SoftWing
{
    public class SwJoystickListener : Java.Lang.Object, JoyStickView.IOnMoveListener
    {
        private const String TAG = "SwJoystickListener";

        private const int MOTION_FORCE_INCREMENT_PERCENT = 33;

        private ControlId id = ControlId.Unknown;
        private MotionDescription motion = MotionDescription.InvalidMotion();
        private MotionDescription lastMotion = MotionDescription.InvalidMotion();
        private int motionAngleIncrementDegrees = 90;
        private int motionId = MotionUpdateMessage.GetMotionId();
        private MessageDispatcher dispatcher;

        public SwJoystickListener(ControlId id_in)
        {
            Log.Info(TAG, "SwJoystickListener");
            id = id_in;
            dispatcher = MessageDispatcher.GetInstance();
        }

        public SwJoystickListener(MotionDescription motion_in)
        {
            Log.Info(TAG, "SwJoystickListener");
            motion = motion_in;
            motionAngleIncrementDegrees = 360 / motion.directionCount;
            dispatcher = MessageDispatcher.GetInstance();
        }

        ~SwJoystickListener()
        {
            Log.Info(TAG, "~SwJoystickListener");
        }

        public void OnMove(double angle, float strength)
        {
            if (id != ControlId.Unknown)
            {
                dispatcher.Post(new ControlUpdateMessage(id, ControlUpdateMessage.UpdateType.Pressed));
            }
            else if (motion.type != MotionType.Invalid)
            {
                HandleMotion(angle, strength);
            }
            else
            {
                Log.Info(TAG, "Warning: Unhandled joystick action");
            }
        }

        private double ClipAngle(double angle)
        {
            angle = (angle + (motionAngleIncrementDegrees / 2)) % 360;
            angle = (int)(angle / motionAngleIncrementDegrees) * motionAngleIncrementDegrees;
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
            return new MotionDescription(motion.type, motion.beginX, motion.beginY, endX, endY, motion.directionCount);
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
    }
}
