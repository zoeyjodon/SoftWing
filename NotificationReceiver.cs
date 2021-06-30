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
        private System.MessageDispatcher dispatcher;

        public NotificationReceiver()
        {
            dispatcher = System.MessageDispatcher.GetInstance();
        }

        public override void OnReceive(Context context, Intent intent)
        {
            String action = intent.Action;
            Log.Info(TAG, "NotificationReceiver.onReceive called, action=" + action);

            if (action.Equals(ACTION_SHOW))
            {
                dispatcher.Post(new System.Messages.ShowImeMessage());
            }
        }
    }
}