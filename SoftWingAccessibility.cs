using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.AccessibilityServices;
using Android.Views.Accessibility;
using Android.Graphics;
using static Android.AccessibilityServices.AccessibilityService;

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
    class SoftWingAccessibility : AccessibilityService
    {
        private const String TAG = "SoftWingAccessibility";
        private static SoftWingAccessibility instance = null;

        public override void OnCreate()
        {
            Log.Info(TAG, "OnCreate");
            instance = this;
            base.OnCreate();
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

        public static void PerformGesture()
        {
            Log.Info(TAG, "PerformGesture");
            if (instance == null)
            {
                Log.Info(TAG, "instance invalid");
                return;
            }
            Path path = new Path();
            path.MoveTo(0, 0);
            path.LineTo(500, 500);

            GestureDescription.Builder gestureBuilder = new GestureDescription.Builder();
            gestureBuilder.AddStroke(new GestureDescription.StrokeDescription(path, 0, 3000));
            if (instance.DispatchGesture(gestureBuilder.Build(), new SwGestureCallback(), null))
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
    }
}