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
using System.Collections.Generic;

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
        private const long GESTURE_START_DELAY_MS = 50;
        private const long FIRST_STROKE_DURATION_MS = 10;
        private const long CONTINUOUS_STROKE_DURATION_MS = 500;
        private long HOLD_STROKE_DURATION_MS = GestureDescription.MaxGestureDuration / 10;

        private MessageDispatcher dispatcher;
        private Dictionary<int, MotionDescription> activeMotions = new Dictionary<int, MotionDescription>();

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

        private List<GestureDescription.StrokeDescription> GenerateTap(MotionDescription motion)
        {
            Path firstPath = new Path();
            firstPath.MoveTo(motion.beginX, motion.beginY);
            firstPath.LineTo(motion.endX, motion.endY);

            Path holdPath = new Path();
            holdPath.MoveTo(motion.endX, motion.endY);
            holdPath.LineTo(motion.endX, motion.endY);

            var stroke = new GestureDescription.StrokeDescription(holdPath, 0, HOLD_STROKE_DURATION_MS, false);
            return new List<GestureDescription.StrokeDescription> { stroke };
        }

        private List<GestureDescription.StrokeDescription> GenerateSwipe(MotionDescription motion)
        {
            Path firstPath = new Path();
            firstPath.MoveTo(motion.beginX, motion.beginY);
            firstPath.LineTo(motion.endX, motion.endY);

            Path holdPath = new Path();
            holdPath.MoveTo(motion.endX, motion.endY);
            holdPath.LineTo(motion.endX, motion.endY);

            var stroke = new GestureDescription.StrokeDescription(firstPath, GESTURE_START_DELAY_MS, FIRST_STROKE_DURATION_MS, true);
            stroke.ContinueStroke(holdPath, GESTURE_START_DELAY_MS + FIRST_STROKE_DURATION_MS, HOLD_STROKE_DURATION_MS, false);
            return new List<GestureDescription.StrokeDescription> { stroke };
        }

        private List<GestureDescription.StrokeDescription> GenerateContinuous(MotionDescription motion)
        {
            var output = new List<GestureDescription.StrokeDescription>();

            Path swipePath = new Path();
            swipePath.MoveTo(motion.beginX, motion.beginY);
            swipePath.LineTo(motion.endX, motion.endY);

            var stroke = new GestureDescription.StrokeDescription(swipePath, 0, CONTINUOUS_STROKE_DURATION_MS, false);
            output.Add(stroke);

            // Add a callback to add more strokes for "continuous" behavior
            new Android.OS.Handler(Android.OS.Looper.MainLooper).PostDelayed(delegate
            {
                if (activeMotions.ContainsValue(motion))
                {
                    RunActiveGestures();
                }
            }, CONTINUOUS_STROKE_DURATION_MS);
            return output;
        }

        private List<GestureDescription.StrokeDescription> GenerateStroke(MotionDescription motion)
        {
            switch (motion.type)
            {
                case MotionType.Swipe:
                    return GenerateSwipe(motion);
                case MotionType.Continuous:
                    return GenerateContinuous(motion);
                default:
                    return GenerateTap(motion);
            }
        }

        private List<GestureDescription.StrokeDescription> GenerateActiveStrokeList()
        {
            var output = new List<GestureDescription.StrokeDescription>();
            foreach (var motion in activeMotions.Values)
            {
                output.AddRange(GenerateStroke(motion));
            }
            return output;
        }

        private void RunActiveGestures()
        {
            var strokes = GenerateActiveStrokeList();

            GestureDescription.Builder gestureBuilder = new GestureDescription.Builder();
            foreach (var stroke in strokes)
            {
                gestureBuilder.AddStroke(stroke);
            }
            DispatchGesture(gestureBuilder.Build(), new SwGestureCallback(), null);
        }

        private void CancelGesture(int id)
        {
            Log.Info(TAG, "CancelGesture");

            if (activeMotions.Count == 1)
            {
                Path path = new Path();
                path.MoveTo(activeMotions[id].endX, activeMotions[id].endY);
                path.LineTo(activeMotions[id].endX, activeMotions[id].endY);
                var stroke = new GestureDescription.StrokeDescription(path, 0, 1, false);
                GestureDescription.Builder gestureBuilder = new GestureDescription.Builder();
                gestureBuilder.AddStroke(stroke);
                DispatchGesture(gestureBuilder.Build(), new SwGestureCallback(), null);
                activeMotions.Remove(id);
                return;
            }
            activeMotions.Remove(id);
            RunActiveGestures();

        }

        private void PerformGesture(int id, MotionDescription motion)
        {
            Log.Info(TAG, "PerformGesture");

            activeMotions.Remove(id);
            activeMotions.Add(id, motion);
            RunActiveGestures();
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
                CancelGesture(motionUpdate.id);
            }
            else
            {
                PerformGesture(motionUpdate.id, motionUpdate.motion);
            }
        }
    }
}