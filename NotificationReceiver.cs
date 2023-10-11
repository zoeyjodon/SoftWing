using Android.Content;
using Android.Util;
using SoftWing.SwSystem;
using System;

namespace SoftWing
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    class NotificationReceiver : BroadcastReceiver
    {
        private const String TAG = "NotificationReceiver";
        public const String ACTION_SHOW = "com.jodonlucas.softwing.SHOW";
        private SwSystem.MessageDispatcher dispatcher;
        private string profile;

        public NotificationReceiver()
        {
            dispatcher = SwSystem.MessageDispatcher.GetInstance();
            profile = SwSettings.Default_Keymap_Filename;
        }

        public NotificationReceiver(string profile_in)
        {
            dispatcher = SwSystem.MessageDispatcher.GetInstance();
            profile = profile_in;
        }

        public string ProfileActionString {get { return ACTION_SHOW + "." + profile; } } 

        public override void OnReceive(Context context, Intent intent)
        {
            String action = intent.Action;
            Log.Info(TAG, "NotificationReceiver.onReceive called, profile=" + profile);

            if (action.Equals(ProfileActionString))
            {
                SwSettings.SetSelectedKeymap(profile);
                dispatcher.Post(new SwSystem.Messages.ShowImeMessage());
            }
        }
    }
}