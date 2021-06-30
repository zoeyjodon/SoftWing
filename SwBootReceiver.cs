using Android.App;
using Android.Content;
using Android.Util;
using Android.Widget;
using System;

namespace SoftWing
{
    [BroadcastReceiver(Exported = true,
                       Enabled = true,
                       Name = "com.jodonlucas.softwing.SoftWing.SwBootReceiver",
                       DirectBootAware = true)]
    [IntentFilter(new[] { Intent.ActionUserUnlocked, Intent.ActionBootCompleted, Intent.ActionScreenOn }, Priority = 1000)]
    class SwBootReceiver : BroadcastReceiver
    {
        private const String TAG = "autostart";

        public SwBootReceiver()
        {
            Log.Info(TAG, "autostart");
        }

        public override void OnReceive(Context context, Intent intent)
        {
            SwDisplayManager.StartSwDisplayManager(context);
        }
    }
}