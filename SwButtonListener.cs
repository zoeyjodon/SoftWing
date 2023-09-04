using Android.Util;
using Android.Views;
using System;
using SoftWing.SwSystem;
using SoftWing.SwSystem.Messages;
using static Android.Views.View;
using Xamarin.Essentials;
using static SoftWing.SwSystem.SwSettings;

namespace SoftWing
{
    public class SwButtonListener : Java.Lang.Object, IOnTouchListener
    {
        private const String TAG = "SwButtonListener";
        private TimeSpan KEY_VIBRATION_TIME = TimeSpan.FromSeconds(0.01);
        private View button;
        private bool vibrate_enabled = SwSettings.Default_Vibration_Enable;
        private ControlId id = ControlId.Unknown;
        private MotionDescription motion = MotionDescription.InvalidMotion();
        private Keycode key = Keycode.Unknown;
        private int motionId = MotionUpdateMessage.GetMotionId();
        private MessageDispatcher dispatcher;
        private bool setup_mode;

        public SwButtonListener(View button_in, ControlId id_in, bool setup_mode = false)
        {
            Log.Info(TAG, "SwButtonListener - key");
            button = button_in;
            id = id_in;
            motion = SwSettings.GetControlMotion(id);
            key = SwSettings.GetControlKeycode(id);
            dispatcher = MessageDispatcher.GetInstance();
            vibrate_enabled = SwSettings.GetVibrationEnable();
            this.setup_mode = setup_mode;
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
                    if (vibrate_enabled)
                    {
                        Vibration.Vibrate(KEY_VIBRATION_TIME);
                    }
                    ReportEvent(ControlUpdateMessage.UpdateType.Pressed);
                    break;
                case MotionEventActions.Up:
                    Log.Info(TAG, "OnTouch - Up");
                    button.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    if (vibrate_enabled)
                    {
                        Vibration.Vibrate(KEY_VIBRATION_TIME);
                    }
                    ReportEvent(ControlUpdateMessage.UpdateType.Released);
                    break;
                default:
                    break;
            }
            return true;
        }

        private void ReportEvent(ControlUpdateMessage.UpdateType update)
        {
            if (setup_mode)
            {
                dispatcher.Post(new ControlUpdateMessage(id, update, null));
            }
            else if (key != Keycode.Unknown)
            {
                dispatcher.Post(new ControlUpdateMessage(id, update, key));
            }
            else if (motion.type != MotionType.Invalid)
            {
                dispatcher.Post(new MotionUpdateMessage(motionId, motion, update == ControlUpdateMessage.UpdateType.Released));
            }
            else
            {
                Log.Info(TAG, "Warning: Unhandled button action");
            }
        }
    }
}
