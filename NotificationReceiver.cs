using Android.Content;
using Android.Util;
using System;

namespace SoftWing
{
    class NotificationReceiver : BroadcastReceiver
    {
        private const String TAG = "NotificationReceiver";
        public const String ACTION_SHOW = "org.pocketworkstation.pckeyboard.SHOW";
        private SwSystem.MessageDispatcher dispatcher;

        public NotificationReceiver()
        {
            dispatcher = SwSystem.MessageDispatcher.GetInstance();
        }

        public override void OnReceive(Context context, Intent intent)
        {
            String action = intent.Action;
            Log.Info(TAG, "NotificationReceiver.onReceive called, action=" + action);

            if (action.Equals(ACTION_SHOW))
            {
                dispatcher.Post(new SwSystem.Messages.ShowImeMessage());
            }
        }
    }
}