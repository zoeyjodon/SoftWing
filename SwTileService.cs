using System;
using Android.App;
using Android.Content;
using Android.Util;
using Android.Service.QuickSettings;

namespace SoftWing
{
    [Service(Label = "SoftWingTile", Permission = Android.Manifest.Permission.BindQuickSettingsTile, Icon = "@mipmap/ic_launcher_foreground", Exported = true)]
    [IntentFilter(new[] { ActionQsTile })]
    class SwTileService : TileService
    {
        private const String TAG = "SwTileService";
        private SwSystem.MessageDispatcher dispatcher;

        public SwTileService()
        {
            dispatcher = SwSystem.MessageDispatcher.GetInstance();
        }

        public override void OnClick()
        {
            base.OnClick();

            if (!IsLocked)
            {
                dispatcher.Post(new SwSystem.Messages.ShowImeMessage());
            }
        }

        //First time tile is added to quick settings
        public override void OnTileAdded()
        {
            base.OnTileAdded();
        }

        //Called each time tile is visible
        public override void OnStartListening()
        {
            base.OnStartListening();
        }

        //Called when tile is no longer visible
        public override void OnStopListening()
        {
            base.OnStopListening();
        }

        //Called when tile is removed by the user
        public override void OnTileRemoved()
        {
            base.OnTileRemoved();
        }
    }
}
