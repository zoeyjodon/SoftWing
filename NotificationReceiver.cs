using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftWing
{
    class NotificationReceiver : BroadcastReceiver
    {
        static String TAG = "PCKeyboard/Notification";
        static public String ACTION_SHOW = "org.pocketworkstation.pckeyboard.SHOW";

        private SoftWingInput mIME;

        public NotificationReceiver(SoftWingInput ime)
        {
            //super();
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