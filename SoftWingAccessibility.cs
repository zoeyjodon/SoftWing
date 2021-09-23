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
        private const long FIRST_STROKE_DURATION_MS = 1;
        private const long HOLD_STROKE_DURATION_MS = 0x0FFFFFFFFFFFFFFF;

        private MessageDispatcher dispatcher;
        private List<MotionDescription> activeMotions = new List<MotionDescription>();

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

        private GestureDescription.StrokeDescription GenerateStroke(MotionDescription motion)
        {
            Path firstPath = new Path();
            firstPath.MoveTo(motion.beginX, motion.beginY);
            firstPath.LineTo(motion.endX, motion.endY);

            Path holdPath = new Path();
            holdPath.MoveTo(motion.endX, motion.endY);
            holdPath.LineTo(motion.endX, motion.endY);

            var stroke = new GestureDescription.StrokeDescription(firstPath, 0, FIRST_STROKE_DURATION_MS, true);
            stroke.ContinueStroke(holdPath, FIRST_STROKE_DURATION_MS, HOLD_STROKE_DURATION_MS, true);

            return stroke;
        }

        private List<GestureDescription.StrokeDescription> GenerateActiveStrokeList()
        {
            var output = new List<GestureDescription.StrokeDescription>();
            foreach (var motion in activeMotions)
            {
                output.Add(GenerateStroke(motion));
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
                path.MoveTo(activeMotions[0].endX, activeMotions[0].endY);
                path.LineTo(activeMotions[0].endX, activeMotions[0].endY);
                var stroke = new GestureDescription.StrokeDescription(path, 0, 1, false);
                GestureDescription.Builder gestureBuilder = new GestureDescription.Builder();
                gestureBuilder.AddStroke(stroke);
                DispatchGesture(gestureBuilder.Build(), new SwGestureCallback(), null);
                activeMotions.RemoveAt(0);
                return;
            }
            for (int i = 0; i < activeMotions.Count; i++)
            {
                if (activeMotions[i].id == id)
                {
                    activeMotions.RemoveAt(i);
                    break;
                }
            }
            RunActiveGestures();

        }

        private void PerformGesture(MotionDescription motion)
        {
            Log.Info(TAG, "PerformGesture");

            for (int i = 0; i < activeMotions.Count; i++)
            {
                if (activeMotions[i].id == motion.id)
                {
                    activeMotions.RemoveAt(i);
                    break;
                }
            }
            activeMotions.Add(motion);
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
                CancelGesture(motionUpdate.motion.id);
            }
            else
            {
                PerformGesture(motionUpdate.motion);
            }
        }
    }
}