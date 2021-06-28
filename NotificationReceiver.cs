using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views.InputMethods;
using System;

namespace SoftWing
{
    class NotificationReceiver : BroadcastReceiver
    {
        private const String TAG = "NotificationReceiver";
        public const String ACTION_SHOW = "org.pocketworkstation.pckeyboard.SHOW";
        public const int SHOW_IME_DELAY_MS = 500;

        public NotificationReceiver()
        {
        }

        public override void OnReceive(Context context, Intent intent)
        {
            String action = intent.Action;
            Log.Info(TAG, "NotificationReceiver.onReceive called, action=" + action);

            if (action.Equals(ACTION_SHOW))
            {
                SwDisplayManager.UseSwKeyboard();
                // Give the IME time to update
                new Handler().PostDelayed(delegate
                {
                    InputMethodManager input_manager = (InputMethodManager)
                        context.GetSystemService(Context.InputMethodService);
                    if (input_manager != null)
                    {
                        input_manager.ShowSoftInputFromInputMethod(SoftWingInput.InputSessionToken, ShowFlags.Forced);
                    }
                }, SHOW_IME_DELAY_MS);
            }
        }
    }
}