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
        private MessageDispatcher dispatcher;

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

        private void PerformGesture(MotionDescription motion, long durationMs)
        {
            Log.Info(TAG, "PerformGesture");

            Path path = new Path();
            path.MoveTo(motion.beginX, motion.beginY);
            path.LineTo(motion.endX, motion.endY);

            GestureDescription.Builder gestureBuilder = new GestureDescription.Builder();
            gestureBuilder.AddStroke(new GestureDescription.StrokeDescription(path, 0, durationMs));
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
            PerformGesture(motionUpdate.motion, 1000);
        }
    }
}