using Android.App;
using Android.Content;
using Android.InputMethodServices;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Java.Interop;
using System;

namespace SoftWing
{
    [Service(Label = "SoftWingInput", Permission = "android.permission.BIND_INPUT_METHOD")]
    [IntentFilter(new[] { "android.view.InputMethod" })]
    [MetaData("android.view.im", Resource = "@xml/method")]
    public class SoftWingInput : InputMethodService
    {
        private const String TAG = "SoftWingInput";

        public override View OnCreateInputView()
        {
            Log.Debug(TAG, "onCreateInputView()");

            var keyboardView = LayoutInflater.Inflate(SoftWing.Resource.Layout.input, null);

            return keyboardView;
        }

        public override void OnStartInputView(EditorInfo info, bool restarting)
        {
            base.OnStartInputView(info, restarting);

            // If we aren't running the swapper yet, we should be
            if (!ServiceScreenSwapper.IsActive)
            {
                var intent = new Intent(this, typeof(ServiceScreenSwapper));
                var flags = ActivityFlags.NewTask;
                intent.AddFlags(flags);
                StartActivity(intent);
            }
        }

        [Export("testButtonClicked")]
        public void testButtonClicked(View v)
        {
            Log.Debug(TAG, "testButtonClicked()");
            var ic = CurrentInputConnection;
            ic.CommitText("BUTTON", 1);
        }
    }
}