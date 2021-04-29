using Android.Util;
using Android.Views;
using System;
using SoftWing.System;
using SoftWing.System.Messages;
using static Android.Views.View;
using Android.App;

namespace SoftWing
{
    public class SwButtonListener : Java.Lang.Object, IOnTouchListener
    {
        private const String TAG = "SwButtonListener";
        private Keycode key;
        private MessageDispatcher dispatcher;

        public SwButtonListener(Keycode key_in)
        {
            Log.Info(TAG, "SwButtonListener");
            key = key_in;
            dispatcher = MessageDispatcher.GetInstance(new Activity());
        }

        ~SwButtonListener()
        {
            Log.Info(TAG, "~SwButtonListener");
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    Log.Info(TAG, "OnTouch - Down");
                    dispatcher.Post(new ControlUpdateMessage(key, ControlUpdateMessage.UpdateType.Pressed));
                    break;
                case MotionEventActions.Up:
                    Log.Info(TAG, "OnTouch - Up");
                    dispatcher.Post(new ControlUpdateMessage(key, ControlUpdateMessage.UpdateType.Released));
                    break;
                default:
                    Log.Info(TAG, "OnTouch - Other");
                    break;
            }
            return true;
        }
    }
}
