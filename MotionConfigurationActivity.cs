using Android.App;
using Android.OS;
using Android.Support.V7.App;
using System;
using Android.Util;
using Android.Graphics;
using Android.Views;
using Android.Content.PM;
using SoftWing.SwSystem.Messages;
using static Android.Views.View;
using Android.Widget;
using SoftWing.SwSystem;
using System.Collections.Generic;

namespace SoftWing
{
    [Activity(Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Landscape, LaunchMode = LaunchMode.SingleTask)]
    public class MotionConfigurationActivity : AppCompatActivity, IOnTouchListener
    {
        private const String TAG = "MotionConfigurationActivity";
        private const int SURFACE_RADIUS_OUTER = 120;
        private const int SURFACE_RADIUS_INNER = 100;
        private const int STROKE_WIDTH_OUTER = 50;
        private const int STROKE_WIDTH_INNER = 30;

        private ISurfaceHolder surfaceHolder = null;
        private Paint surfacePaintOuter = new Paint(PaintFlags.AntiAlias);
        private Paint surfacePaintInner = new Paint(PaintFlags.AntiAlias);
        private MotionDescription motion = MotionDescription.InvalidMotion();

        public static Android.Net.Uri BackgroundImageUri = null;
        public static SwSettings.ControlId control;
        public static MotionType motionType = MotionType.Invalid;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "OnCreate()");
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            RequestWindowFeature(WindowFeatures.NoTitle);
            var uiOptions = SystemUiFlags.HideNavigation |
                 SystemUiFlags.LayoutHideNavigation |
                 SystemUiFlags.LayoutFullscreen |
                 SystemUiFlags.Fullscreen |
                 SystemUiFlags.LayoutStable |
                 SystemUiFlags.ImmersiveSticky;
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;

            surfacePaintOuter.StrokeWidth = STROKE_WIDTH_OUTER;
            surfacePaintOuter.Color = Color.Black;

            surfacePaintInner.StrokeWidth = STROKE_WIDTH_INNER;
            surfacePaintInner.Color = Color.White;

            SetContentView(Resource.Layout.motion_configuration);
        }

        protected override void OnStart()
        {
            base.OnStart();

            var motionSurface = FindViewById<ImageView>(Resource.Id.motionConfigurationImage);
            motionSurface.SetImageURI(BackgroundImageUri);

            ConfigureMotionDrawSurface();
            if (multiplePointsRequired())
            {
                PromptUserForSwipeBegin();
            }
            else
            {
                PromptUserForTap();
            }
        }

        private bool multiplePointsRequired()
        {
            bool isAnalogControl = (control == SwSettings.ControlId.L_Analog) || (control == SwSettings.ControlId.R_Analog);
            return (motionType != MotionType.Tap) || isAnalogControl;
        }

        private void PromptUserForSwipeBegin()
        {
            Log.Debug(TAG, "PromptUserForSwipeBegin()");

            Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
            var alert = dialog.Create();
            alert.SetTitle("Select Touch Origin");
            var message = "Tap the origin point for the touch action.\nThis could be the center of a joystick, or the beginning of a swpie motion\n";

            alert.SetMessage(message);
            alert.SetButton("Continue", (c, ev) => { });
            alert.Show();
        }

        private void PromptUserForSwipeEnd()
        {
            Log.Debug(TAG, "PromptUserForSwipeEnd()");

            Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
            var alert = dialog.Create();
            alert.SetTitle("Select Touch End");
            var message = "Tap the end point for the touch action.\nThis could be the outer edge of a joystick, or the end of a swpie motion\n";

            alert.SetMessage(message);
            alert.SetButton("Continue", (c, ev) => { });
            alert.Show();
        }

        private void PromptUserForTap()
        {
            Log.Debug(TAG, "PromptUserForTap()");

            Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
            var alert = dialog.Create();
            alert.SetTitle("Select Touch Point");
            var message = "Tap the point on the screen where you want the tap action to occur\n";

            alert.SetMessage(message);
            alert.SetButton("Continue", (c, ev) => { });
            alert.Show();
        }

        private void ConfigureMotionDrawSurface()
        {
            var motionSurface = FindViewById<SurfaceView>(Resource.Id.motionConfigurationSurface);
            motionSurface.SetOnTouchListener(this);
            motionSurface.SetZOrderOnTop(true);
            surfaceHolder = motionSurface.Holder;
            surfaceHolder.SetFormat(Format.Transparent);
        }

        private void MovePointMarker(MotionEvent e)
        {
            var canvas = surfaceHolder.LockCanvas();
            canvas.DrawColor(0, BlendMode.Clear);

            if (motion.beginX > -1)
            {
                canvas.DrawLine(motion.beginX, motion.beginY, e.GetX(), e.GetY(), surfacePaintOuter);
                canvas.DrawLine(motion.beginX, motion.beginY, e.GetX(), e.GetY(), surfacePaintInner);

                canvas.DrawCircle(motion.beginX, motion.beginY, SURFACE_RADIUS_OUTER / 2, surfacePaintOuter);
                canvas.DrawCircle(motion.beginX, motion.beginY, SURFACE_RADIUS_INNER / 2, surfacePaintInner);
            }
            if (e.Action != MotionEventActions.Up)
            {
                canvas.DrawCircle(e.GetX(), e.GetY(), SURFACE_RADIUS_OUTER, surfacePaintOuter);
                canvas.DrawCircle(e.GetX(), e.GetY(), SURFACE_RADIUS_INNER, surfacePaintInner);
            }
            surfaceHolder.UnlockCanvasAndPost(canvas);
        }

        private void CommitMotionAndFinish()
        {
            SwSettings.SetControlMotion(control, motion);
            Finish();
        }

        private void EndPointSetting(MotionEvent e)
        {
            motion.type = motionType;
            if (motion.beginX == -1)
            {
                motion.beginX = e.GetX();
                motion.beginY = e.GetY();
                if (multiplePointsRequired())
                {
                    PromptUserForSwipeEnd();
                }
                else
                {
                    motion.endX = motion.beginX;
                    motion.endY = motion.beginY;
                    CommitMotionAndFinish();
                }
            }
            else
            {
                motion.endX = e.GetX();
                motion.endY = e.GetY();
                CommitMotionAndFinish();
            }
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Up:
                    Log.Info(TAG, "OnTouch - Up");
                    EndPointSetting(e);
                    break;
                default:
                    Log.Info(TAG, "OnTouch - Other");
                    break;
            }
            MovePointMarker(e);
            return true;
        }
    }
}
