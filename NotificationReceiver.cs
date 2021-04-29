using Android.Content;
using Android.Util;
using Android.Views.InputMethods;
using System;

namespace SoftWing
{
    class NotificationReceiver : BroadcastReceiver
    {
        private const String TAG = "NotificationReceiver";
        public const String ACTION_SHOW = "org.pocketworkstation.pckeyboard.SHOW";

        public NotificationReceiver()
        {
        }

        public override void OnReceive(Context context, Intent intent)
        {
            String action = intent.Action;
            Log.Info(TAG, "NotificationReceiver.onReceive called, action=" + action);

            if (action.Equals(ACTION_SHOW))
            {
                InputMethodManager input_manager = (InputMethodManager)
                    context.GetSystemService(Context.InputMethodService);
                if (input_manager != null)
                {
                    input_manager.ShowSoftInputFromInputMethod(SoftWingInput.InputSessionToken, ShowFlags.Forced);
                }
            }
        }
    }
}