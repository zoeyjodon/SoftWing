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
        private ButtonBehavior behavior = ButtonBehavior.Temporary;
        private int motionId = MotionUpdateMessage.GetMotionId();
        private MessageDispatcher dispatcher;
        private bool setup_mode;
        private bool toggle_active = false;

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
            if (!setup_mode)
            {
                behavior = SwSettings.GetControlBehavior(id);
            }
        }

        ~SwButtonListener()
        {
            Log.Info(TAG, "~SwButtonListener");
        }

        public void ButtonPress()
        {
            Log.Info(TAG, "ButtonPress");
            button.SetBackgroundColor(Android.Graphics.Color.SkyBlue);
            if (vibrate_enabled)
            {
                Vibration.Vibrate(KEY_VIBRATION_TIME);
            }
            ReportEvent(ControlUpdateMessage.UpdateType.Pressed);
        }

        public void ButtonRelease()
        {
            Log.Info(TAG, "ButtonRelease");
            button.SetBackgroundColor(Android.Graphics.Color.Transparent);
            if (vibrate_enabled)
            {
                Vibration.Vibrate(KEY_VIBRATION_TIME);
            }
            ReportEvent(ControlUpdateMessage.UpdateType.Released);
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            Log.Info(TAG, "OnTouch: " + ButtonBehaviorToString(behavior));
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    if (behavior == ButtonBehavior.Temporary)
                    {
                        ButtonPress();
                    }
                    else if (behavior == ButtonBehavior.Toggle)
                    {
                        if (!toggle_active)
                        {
                            ButtonPress();
                        }
                        toggle_active = !toggle_active;
                    }
                    break;
                case MotionEventActions.Up:
                    if ((behavior == ButtonBehavior.Temporary) || (behavior == ButtonBehavior.Toggle && !toggle_active))
                    {
                        ButtonRelease();
                    }
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
