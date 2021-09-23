using System;
using Android.App;
using Android.Content;
using Android.Util;
using Android.AccessibilityServices;
using Android.Views.Accessibility;
using Android.Graphics;
using static Android.AccessibilityServices.AccessibilityService;
using SoftWing.SwSystem;
using SoftWing.SwSystem.Messages;

namespace SoftWing
{
    class SwGestureCallback : GestureResultCallback
    {
        private const String TAG = "SwGestureCallback";

        public SwGestureCallback()
        {
            Log.Info(TAG, "SwGestureCallback()");
        }

        public override void OnCancelled(GestureDescription gestureDescription)
        {
            Log.Info(TAG, "OnCancelled");
            base.OnCancelled(gestureDescription);
        }

        public override void OnCompleted(GestureDescription gestureDescription)
        {
            Log.Info(TAG, "OnCompleted");
            base.OnCompleted(gestureDescription);
        }
    }

    [Service(Label = "SoftWingAccessibility", Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE")]
    [IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
    [MetaData("android.accessibilityservice", Resource = "@xml/accessibility_service_config")]
    class SoftWingAccessibility : AccessibilityService, MessageSubscriber
    {
        private const String TAG = "SoftWingAccessibility";
        private const long FIRST_STROKE_DURATION_MS = 1;
        private const long HOLD_STROKE_DURATION_MS = 0x0FFFFFFFFFFFFFFF;

        private MessageDispatcher dispatcher;
        private MotionDescription lastMotion = new MotionDescription(0, 0, 0, 0);

        public override void OnCreate()
        {
            Log.Info(TAG, "OnCreate");
            base.OnCreate();
            dispatcher = MessageDispatcher.GetInstance();
            dispatcher.Subscribe(MessageType.MotionUpdate, this);
        }

        protected override void OnServiceConnected()
        {
            Log.Info(TAG, "OnServiceConnected");
            base.OnServiceConnected();
        }

        public override bool OnUnbind(Intent intent)
        {
            Log.Info(TAG, "OnUnbind");
            return base.OnUnbind(intent);
        }

        private void CancelGesture()
        {
            Log.Info(TAG, "CancelGesture");

            Path path = new Path();
            path.MoveTo(lastMotion.endX, lastMotion.endY);
            path.LineTo(lastMotion.endX, lastMotion.endY);

            var stroke = new GestureDescription.StrokeDescription(path, 0, 1, false);

            GestureDescription.Builder gestureBuilder = new GestureDescription.Builder();
            gestureBuilder.AddStroke(stroke);
            if (DispatchGesture(gestureBuilder.Build(), new SwGestureCallback(), null))
            {
                Log.Info(TAG, "Dispatch Success!");
            }
            else
            {
                Log.Info(TAG, "Dispatch Failure");
            }
        }

        private void PerformGesture(MotionDescription motion)
        {
            Log.Info(TAG, "PerformGesture");

            lastMotion = motion;

            Path firstPath = new Path();
            firstPath.MoveTo(motion.beginX, motion.beginY);
            firstPath.LineTo(motion.endX, motion.endY);

            Path holdPath = new Path();
            holdPath.MoveTo(motion.endX, motion.endY);
            holdPath.LineTo(motion.endX, motion.endY);

            var stroke = new GestureDescription.StrokeDescription(firstPath, 0, FIRST_STROKE_DURATION_MS, true);
            stroke.ContinueStroke(holdPath, FIRST_STROKE_DURATION_MS, HOLD_STROKE_DURATION_MS, true);

            GestureDescription.Builder gestureBuilder = new GestureDescription.Builder();
            gestureBuilder.AddStroke(stroke);
            if (DispatchGesture(gestureBuilder.Build(), new SwGestureCallback(), null))
            {
                Log.Info(TAG, "Dispatch Success!");
            }
            else
            {
                Log.Info(TAG, "Dispatch Failure");
            }

        }

        public override void OnAccessibilityEvent(AccessibilityEvent e)
        {
            Log.Info(TAG, "OnAccessibilityEvent " + e.ToString());
        }

        public override void OnInterrupt()
        {
            Log.Info(TAG, "OnInterrupt");
        }

        public void Accept(SystemMessage message)
        {
            if (message.getMessageType() != MessageType.MotionUpdate)
            {
                return;
            }
            var motionUpdate = (MotionUpdateMessage)message;
            if (motionUpdate.cancel_requested)
            {
                CancelGesture();
            }
            else
            {
                PerformGesture(motionUpdate.motion);
            }
        }
    }
}