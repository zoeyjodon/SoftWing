using Android.Util;
using System;
using SoftWing.SwSystem;
using SoftWing.SwSystem.Messages;
using Android.App;
using static SoftWing.SwSystem.SwSettings;
using static Android.Views.View;
using Android.Views;
using Android.Graphics;
using static AndroidX.Core.Content.PM.ShortcutInfoCompat;

namespace SoftWing
{
    public class SwJoystickListener : Java.Lang.Object, IOnTouchListener
    {
        private const String TAG = "SwJoystickListener";

        private const int SURFACE_RADIUS_INNER = 50;
        private const int STROKE_WIDTH = 15;
        private const int MOTION_FORCE_INCREMENT_PERCENT = 33;

        private Paint surfacePaintOuter = new Paint(PaintFlags.AntiAlias);
        private Paint surfacePaintMiddle= new Paint(PaintFlags.AntiAlias);
        private Paint surfacePaintInner = new Paint(PaintFlags.AntiAlias);

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
            InitJoystickView();
        }

        public SwJoystickListener(MotionDescription motion_in)
        {
            Log.Info(TAG, "SwJoystickListener");
            motion = motion_in;
            motionAngleIncrementDegrees = 360 / motion.directionCount;
            dispatcher = MessageDispatcher.GetInstance();
            InitJoystickView();
        }

        private void InitJoystickView()
        {
            surfacePaintOuter.StrokeWidth = STROKE_WIDTH;
            surfacePaintOuter.Color = Color.White;

            surfacePaintMiddle.StrokeWidth = STROKE_WIDTH;
            surfacePaintMiddle.Color = Color.Black;

            surfacePaintInner.StrokeWidth = STROKE_WIDTH;
            surfacePaintInner.Color = Color.White;
        }

        ~SwJoystickListener()
        {
            Log.Info(TAG, "~SwJoystickListener");
        }

        public void OnMove(double angle, float strength)
        {
            Log.Info(TAG, "OnMove: " + angle.ToString() + ", " + strength.ToString());
            if (id != ControlId.Unknown)
            {
                dispatcher.Post(new ControlUpdateMessage(id, ControlUpdateMessage.UpdateType.Pressed, null));
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

        private void JoystickBackground(SurfaceView surface, Canvas canvas)
        {
            Log.Info(TAG, "JoystickBackground");

            canvas.DrawColor(0, BlendMode.Clear);
            var surface_radius = Math.Min(surface.Width / 2, surface.Height / 2);
            canvas.DrawCircle(surface_radius, surface_radius, surface_radius, surfacePaintOuter);
            canvas.DrawCircle(surface_radius, surface_radius, surface_radius - 10, surfacePaintMiddle);
        }

        private void ResetJoystick(SurfaceView surface)
        {
            Log.Info(TAG, "ResetJoystick");
            var surfaceHolder = surface.Holder;
            var canvas = surfaceHolder.LockCanvas();

            JoystickBackground(surface, canvas);
            canvas.DrawCircle(surface.Width / 2, surface.Height / 2, SURFACE_RADIUS_INNER, surfacePaintInner);

            surfaceHolder.UnlockCanvasAndPost(canvas);

            OnMove(0, 0);
        }

        private void MoveJoystick(SurfaceView surface, MotionEvent e)
        {
            Log.Info(TAG, "MoveJoystick");
            var surfaceHolder = surface.Holder;
            var canvas = surfaceHolder.LockCanvas();

            JoystickBackground(surface, canvas);

            var center_x = surface.Width / 2;
            var center_y = surface.Height / 2;
            var current_x = e.GetX();
            var current_y = e.GetY();
            var adjusted_x = current_x - center_x;
            var adjusted_y = current_y - center_y;

            var surface_radius = Math.Min(center_x, center_y) - SURFACE_RADIUS_INNER;
            var current_radius = Math.Sqrt((adjusted_x * adjusted_x) + (adjusted_y * adjusted_y));
            var angle = Math.Atan2(adjusted_y, adjusted_x);
            if (current_radius > surface_radius)
            {
                current_radius = surface_radius;
                current_x = (surface_radius * (float)Math.Cos(angle)) + center_x;
                current_y = (surface_radius * (float)Math.Sin(angle)) + center_y;
            }

            canvas.DrawCircle(current_x, current_y, SURFACE_RADIUS_INNER, surfacePaintInner);

            surfaceHolder.UnlockCanvasAndPost(canvas);

            OnMove(angle * (-180 / Math.PI), 100 * (float)current_radius / surface_radius);
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            Log.Info(TAG, "OnTouch");
            var surface = (SurfaceView) v;

            switch (e.Action)
            {
                case MotionEventActions.Up:
                    ResetJoystick(surface);
                    break;
                default:
                    MoveJoystick(surface, e);
                    break;
            }
            return true;
        }
    }
}
