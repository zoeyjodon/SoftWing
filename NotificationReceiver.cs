using Android.Content;
using Android.Util;
using Android.Views.InputMethods;
using System;

namespace SoftWing
{
    class NotificationReceiver : BroadcastReceiver
    {
        private const String TAG = "NotificationReceiver";
        private SoftWingInput mIME;
        public const String ACTION_SHOW = "org.pocketworkstation.pckeyboard.SHOW";

        public NotificationReceiver(SoftWingInput ime)
        {
            mIME = ime;
            Log.Info(TAG, "NotificationReceiver created, ime=" + mIME);
        }

        public override void OnReceive(Context context, Intent intent)
        {
            String action = intent.Action;
            Log.Info(TAG, "NotificationReceiver.onReceive called, action=" + action);

            if (action.Equals(ACTION_SHOW))
            {
                InputMethodManager imm = (InputMethodManager)
                    context.GetSystemService(Context.InputMethodService);
                if (imm != null)
                {
                    imm.ShowSoftInputFromInputMethod(SoftWingInput.mToken, ShowFlags.Forced);
                }
            }
        }
    }
}