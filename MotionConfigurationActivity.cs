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
        private const int SURFACE_RADIUS = 50;
        private const int STROKE_WIDTH = 50;

        private ISurfaceHolder surfaceHolder = null;
        private Paint surfacePaint = new Paint(PaintFlags.AntiAlias);
        private MotionDescription motion = MotionDescription.InvalidMotion();

        public static Android.Net.Uri BackgroundImageUri = null;
        public static List<SwSettings.ControlId> controls;
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

            surfacePaint.StrokeWidth = STROKE_WIDTH;

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
            return (motionType != MotionType.Tap) || (controls.Count > 1);
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
            alert.SetTitle("Select Starting Touch Point");
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
            canvas.DrawCircle(e.GetX(), e.GetY(), SURFACE_RADIUS, surfacePaint);
            if (motion.beginX > -1)
            {
                canvas.DrawLine(motion.beginX, motion.beginY, e.GetX(), e.GetY(), surfacePaint);
                canvas.DrawCircle(motion.beginX, motion.beginY, SURFACE_RADIUS, surfacePaint);
            }
            surfaceHolder.UnlockCanvasAndPost(canvas);
        }

        private void CommitMotionAndFinish()
        {
            foreach (var control in controls)
            {
                SwSettings.SetControlMotion(control, motion);
            }
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
            MovePointMarker(e);
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
            return true;
        }
    }
}
