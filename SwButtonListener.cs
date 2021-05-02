using Android.Util;
using Android.Views;
using System;
using SoftWing.System;
using SoftWing.System.Messages;
using static Android.Views.View;
using Android.App;
using Xamarin.Essentials;

namespace SoftWing
{
    public class SwButtonListener : Java.Lang.Object, IOnTouchListener
    {
        private const String TAG = "SwButtonListener";
        private TimeSpan KEY_VIBRATION_TIME = TimeSpan.FromSeconds(0.01);
        private View button;
        private Keycode key;
        private MessageDispatcher dispatcher;

        public SwButtonListener(View button_in, Keycode key_in)
        {
            Log.Info(TAG, "SwButtonListener");
            button = button_in;
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
                    button.SetBackgroundColor(Android.Graphics.Color.SkyBlue);
                    Vibration.Vibrate(KEY_VIBRATION_TIME);
                    dispatcher.Post(new ControlUpdateMessage(key, ControlUpdateMessage.UpdateType.Pressed));
                    break;
                case MotionEventActions.Up:
                    Log.Info(TAG, "OnTouch - Up");
                    button.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    Vibration.Vibrate(KEY_VIBRATION_TIME);
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
