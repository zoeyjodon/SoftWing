using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using SoftWing.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftWing
{
    class NotificationReceiver : BroadcastReceiver, System.MessageSubscriber
    {
        static String TAG = "PCKeyboard/Notification";
        static public String ACTION_SHOW = "org.pocketworkstation.pckeyboard.SHOW";
        private SoftWingInput mIME;
        public static NotificationReceiver Instance = null;
        public static Context calling_context = null;

        public NotificationReceiver(SoftWingInput ime)
        {
            //super();
            mIME = ime;
            Log.Info(TAG, "NotificationReceiver created, ime=" + mIME);
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public override void OnReceive(Context context, Intent intent)
        {
            String action = intent.Action;
            Log.Info(TAG, "NotificationReceiver.onReceive called, action=" + action);

            calling_context = context;

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

        public void Accept(SystemMessage message)
        {
            InputMethodManager imm = (InputMethodManager)
                calling_context.GetSystemService(Context.InputMethodService);

            if (imm != null)
            {
                imm.ShowSoftInputFromInputMethod(SoftWingInput.mToken, ShowFlags.Forced);
            }
        }
    }
}