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

        private const int ANGLE_RIGHT = 0;
        private double ANGLE_RIGHT_MAX;
        private double ANGLE_RIGHT_MIN;

        private const int ANGLE_UP = 90;
        private double ANGLE_UP_MAX;
        private double ANGLE_UP_MIN;

        private const int ANGLE_LEFT = 180;
        private double ANGLE_LEFT_MAX;
        private double ANGLE_LEFT_MIN;

        private const int ANGLE_DOWN = 270;
        private double ANGLE_DOWN_MAX;
        private double ANGLE_DOWN_MIN;

        private const int SURFACE_RADIUS_INNER = 50;
        private const int STROKE_WIDTH = 15;
        private const int MOTION_FORCE_INCREMENT_PERCENT = 33;

        private Paint surfacePaintOuter = new Paint(PaintFlags.AntiAlias);
        private Paint surfacePaintMiddle= new Paint(PaintFlags.AntiAlias);
        private Paint surfacePaintInner = new Paint(PaintFlags.AntiAlias);

        private ControlId id = ControlId.Unknown;
        private ControlId id_up = ControlId.Unknown;
        private ControlId id_down = ControlId.Unknown;
        private ControlId id_left = ControlId.Unknown;
        private ControlId id_right = ControlId.Unknown;
        private Keycode up_keycode = Keycode.Unknown;
        private Keycode down_keycode = Keycode.Unknown;
        private Keycode left_keycode = Keycode.Unknown;
        private Keycode right_keycode = Keycode.Unknown;
        private MotionDescription motion = MotionDescription.InvalidMotion();
        private MotionDescription lastMotion = MotionDescription.InvalidMotion();
        private int motionAngleIncrementDegrees = 45;
        private int motionId = MotionUpdateMessage.GetMotionId();
        private MessageDispatcher dispatcher;
        private bool setup_mode;
        private bool up_pressed = false;
        private bool down_pressed = false;
        private bool left_pressed = false;
        private bool right_pressed = false;

        public SwJoystickListener(ControlId id_in, bool setup_mode = false)
        {
            Log.Info(TAG, "SwJoystickListener");
            id = id_in;
            id_up = ANALOG_TO_DIRECTION_MAP[id_in][AnalogDirection.Up];
            id_down = ANALOG_TO_DIRECTION_MAP[id_in][AnalogDirection.Down];
            id_left = ANALOG_TO_DIRECTION_MAP[id_in][AnalogDirection.Left];
            id_right = ANALOG_TO_DIRECTION_MAP[id_in][AnalogDirection.Right];
            motion = GetControlMotion(id);
            dispatcher = MessageDispatcher.GetInstance();
            this.setup_mode = setup_mode;
            if (setup_mode)
            {
                motion.directionCount = 8;
                motion.beginX = 100;
                motion.endX = 200;
                motion.beginY = 100;
                motion.endY = 200;
            }
            else if (motion.type == MotionType.Invalid)
            {
                up_keycode = GetControlKeycode(id_up);
                down_keycode = GetControlKeycode(id_down);
                left_keycode = GetControlKeycode(id_left);
                right_keycode = GetControlKeycode(id_right);
                motion.directionCount = 8;
                motion.beginX = 100;
                motion.endX = 200;
                motion.beginY = 100;
                motion.endY = 200;
            }
            motionAngleIncrementDegrees = 360 / motion.directionCount;

            InitJoystickView();
            InitJoystickTolerance();
        }

        private void InitJoystickTolerance()
        {
            ANGLE_RIGHT_MAX = ANGLE_RIGHT + motionAngleIncrementDegrees;
            ANGLE_RIGHT_MIN = 360 - motionAngleIncrementDegrees;

            ANGLE_UP_MAX = ANGLE_UP + motionAngleIncrementDegrees;
            ANGLE_UP_MIN = ANGLE_UP - motionAngleIncrementDegrees;

            ANGLE_LEFT_MAX = ANGLE_LEFT + motionAngleIncrementDegrees;
            ANGLE_LEFT_MIN = ANGLE_LEFT - motionAngleIncrementDegrees;

            ANGLE_DOWN_MAX = ANGLE_DOWN + motionAngleIncrementDegrees;
            ANGLE_DOWN_MIN = ANGLE_DOWN - motionAngleIncrementDegrees;
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

        private void HandleRightPress(double angle)
        {
            if (RightRegionActive(angle))
            {
                if (!right_pressed)
                {
                    Log.Info(TAG, "Right Pressed");
                    right_pressed = true;
                    dispatcher.Post(new ControlUpdateMessage(id_right, ControlUpdateMessage.UpdateType.Pressed, right_keycode));
                }
            }
            else if (right_pressed)
            {
                Log.Info(TAG, "Right Released");
                right_pressed = false;
                dispatcher.Post(new ControlUpdateMessage(id_right, ControlUpdateMessage.UpdateType.Released, right_keycode));
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
                    dispatcher.Post(new ControlUpdateMessage(id_left, ControlUpdateMessage.UpdateType.Pressed, left_keycode));
                }
            }
            else if (left_pressed)
            {
                Log.Info(TAG, "Left Released");
                left_pressed = false;
                dispatcher.Post(new ControlUpdateMessage(id_left, ControlUpdateMessage.UpdateType.Released, left_keycode));
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
                    dispatcher.Post(new ControlUpdateMessage(id_up, ControlUpdateMessage.UpdateType.Pressed, up_keycode));
                }
            }
            else if (up_pressed)
            {
                Log.Info(TAG, "Up Released");
                up_pressed = false;
                dispatcher.Post(new ControlUpdateMessage(id_up, ControlUpdateMessage.UpdateType.Released, up_keycode));
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
                    dispatcher.Post(new ControlUpdateMessage(id_down, ControlUpdateMessage.UpdateType.Pressed, down_keycode));
                }
            }
            else if (down_pressed)
            {
                Log.Info(TAG, "Down Released");
                down_pressed = false;
                dispatcher.Post(new ControlUpdateMessage(id_down, ControlUpdateMessage.UpdateType.Released, down_keycode));
            }
        }

        private void DisableRunningInputs()
        {
            if (up_pressed)
            {
                Log.Info(TAG, "Up Released");
                dispatcher.Post(new ControlUpdateMessage(id, ControlUpdateMessage.UpdateType.Released, up_keycode));
            }
            if (down_pressed)
            {
                Log.Info(TAG, "Down Released");
                dispatcher.Post(new ControlUpdateMessage(id, ControlUpdateMessage.UpdateType.Released, down_keycode));
            }
            if (left_pressed)
            {
                Log.Info(TAG, "Left Released");
                dispatcher.Post(new ControlUpdateMessage(id, ControlUpdateMessage.UpdateType.Released, left_keycode));
            }
            if (right_pressed)
            {
                Log.Info(TAG, "Right Released");
                dispatcher.Post(new ControlUpdateMessage(id, ControlUpdateMessage.UpdateType.Released, right_keycode));
            }
            up_pressed = false;
            down_pressed = false;
            left_pressed = false;
            right_pressed = false;
        }

        public void OnMove(double angle, float strength)
        {
            Log.Info(TAG, "OnMove: " + angle.ToString() + ", " + strength.ToString());
            bool motionComplete = strength == 0;
            var angleMotion = CalculateMotion(angle, strength);

            if (!MotionHasChanged(angleMotion))
            {
                return;
            }
            lastMotion = angleMotion;

            if ((motion.type != MotionType.Invalid) && (!setup_mode))
            {
                dispatcher.Post(new MotionUpdateMessage(motionId, angleMotion, motionComplete));
                return;
            }

            // Handle key inputs
            if (strength < MOTION_FORCE_INCREMENT_PERCENT)
            {
                DisableRunningInputs();
                return;
            }
            HandleRightPress(angle);
            HandleLeftPress(angle);
            HandleUpPress(angle);
            HandleDownPress(angle);
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

            angle = angle * (-180 / Math.PI);
            if (angle < 0)
            {
                angle = 360 + angle;
            }
            OnMove(angle, 100 * (float)current_radius / surface_radius);
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
