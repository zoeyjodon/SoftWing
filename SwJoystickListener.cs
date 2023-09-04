using Android.Util;
using System;
using SoftWing.SwSystem;
using SoftWing.SwSystem.Messages;
using static SoftWing.SwSystem.SwSettings;
using static Android.Views.View;
using Android.Views;
using Android.Graphics;
using Android.Runtime;

namespace SoftWing
{
    public class SwJoystickListener : Java.Lang.Object, IOnTouchListener, ISurfaceHolderCallback
    {
        private const String TAG = "SwJoystickListener";

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
        private int surface_width;
        private int surface_height;

        public SwJoystickListener(SurfaceView surface, ControlId id_in, bool setup_mode = false)
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

            InitJoystickView(surface);
        }

        private void InitJoystickView(SurfaceView surface)
        {
            surfacePaintOuter.StrokeWidth = STROKE_WIDTH;
            surfacePaintOuter.Color = Color.White;

            surfacePaintMiddle.StrokeWidth = STROKE_WIDTH;
            surfacePaintMiddle.Color = Color.Black;

            surfacePaintInner.StrokeWidth = STROKE_WIDTH;
            surfacePaintInner.Color = Color.White;

            surface.Holder.AddCallback(this);
        }

        ~SwJoystickListener()
        {
            Log.Info(TAG, "~SwJoystickListener");
        }

        public void OnMove(double angle, float strength)
        {
            bool motionComplete = strength == 0;
            var angleMotion = CalculateMotion(angle, strength);

            if (!MotionHasChanged(angleMotion))
            {
                return;
            }

            if ((motion.type != MotionType.Invalid) && (!setup_mode))
            {
                // Handle motion inputs
                dispatcher.Post(new MotionUpdateMessage(motionId, angleMotion, motionComplete));
            }
            else
            {
                // Handle key inputs
                // Clip key inputs
                if (strength < MOTION_FORCE_INCREMENT_PERCENT)
                {
                    angleMotion.endX = motion.beginX;
                    angleMotion.endY = motion.beginY;
                }
                OnMoveKeyDirection(
                    id_right, 
                    id_left, 
                    right_keycode, 
                    left_keycode, 
                    motion.beginX, 
                    lastMotion.endX, 
                    angleMotion.endX
                );
                OnMoveKeyDirection(
                    id_down, 
                    id_up, 
                    down_keycode, 
                    up_keycode, 
                    motion.beginY, 
                    lastMotion.endY, 
                    angleMotion.endY
                );
            }
            lastMotion = angleMotion;
        }

        private void OnMoveKeyDirection(
            ControlId positive, 
            ControlId negative, 
            Keycode positive_key, 
            Keycode negative_key, 
            float baseBegin, 
            float lastEnd, 
            float currentEnd
        )
        {
            if ((currentEnd > baseBegin) && (lastEnd <= baseBegin))
            {
                if (lastEnd < baseBegin)
                {
                    dispatcher.Post(new ControlUpdateMessage(negative, ControlUpdateMessage.UpdateType.Released, negative_key));
                }
                dispatcher.Post(new ControlUpdateMessage(positive, ControlUpdateMessage.UpdateType.Pressed, positive_key));
            }
            else if ((currentEnd < baseBegin) && (lastEnd >= baseBegin))
            {
                if (lastEnd > baseBegin)
                {
                    dispatcher.Post(new ControlUpdateMessage(positive, ControlUpdateMessage.UpdateType.Released, positive_key));
                }
                dispatcher.Post(new ControlUpdateMessage(negative, ControlUpdateMessage.UpdateType.Pressed, negative_key));
            }
            else if ((currentEnd == baseBegin) && (lastEnd != baseBegin))
            {
                if (lastEnd > baseBegin)
                {
                    dispatcher.Post(new ControlUpdateMessage(positive, ControlUpdateMessage.UpdateType.Released, positive_key));
                }
                else
                {
                    dispatcher.Post(new ControlUpdateMessage(negative, ControlUpdateMessage.UpdateType.Released, negative_key));
                }
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
            var invalid_motion = MotionDescription.InvalidMotion();
            // Ignore initial position setup
            if ((invalid_motion.endX == lastMotion.endX) && (invalid_motion.endY == invalid_motion.endY))
            {
                lastMotion = motion;
            }
            return (motion.endX != lastMotion.endX) ||
                   (motion.endY != lastMotion.endY);
        }

        private void JoystickBackground(Canvas canvas)
        {
            canvas.DrawColor(0, BlendMode.Clear);
            var surface_radius = Math.Min(surface_width / 2, surface_height / 2);
            canvas.DrawCircle(surface_radius, surface_radius, surface_radius, surfacePaintOuter);
            canvas.DrawCircle(surface_radius, surface_radius, surface_radius - 10, surfacePaintMiddle);
        }

        private void ResetJoystick(ISurfaceHolder surfaceHolder)
        {
            Log.Info(TAG, "ResetJoystick");
            var canvas = surfaceHolder.LockCanvas();

            JoystickBackground(canvas);
            canvas.DrawCircle(surface_width / 2, surface_height / 2, SURFACE_RADIUS_INNER, surfacePaintInner);

            surfaceHolder.UnlockCanvasAndPost(canvas);

            OnMove(0, 0);
        }

        private void MoveJoystick(ISurfaceHolder surfaceHolder, MotionEvent e)
        {
            var canvas = surfaceHolder.LockCanvas();

            JoystickBackground(canvas);

            var center_x = surface_width / 2;
            var center_y = surface_height / 2;
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
            var surface = (SurfaceView) v;

            switch (e.Action)
            {
                case MotionEventActions.Up:
                    ResetJoystick(surface.Holder);
                    break;
                default:
                    MoveJoystick(surface.Holder, e);
                    break;
            }
            return true;
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            surface_width = holder.SurfaceFrame.Width();
            surface_height = holder.SurfaceFrame.Height();
            ResetJoystick(holder);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
        }
    }
}
