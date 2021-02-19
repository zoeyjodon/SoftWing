using Android.App;
using Android.Content;
using Android.InputMethodServices;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Google.Apis.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftWing
{
    [Service(Permission = "android.permission.BIND_INPUT_METHOD")]
    [IntentFilter(new[] { "android.view.InputMethod" })]
    [MetaData("android.view.im", Resource = "@xml/method")]
    public class SoftWingInput : InputMethodService, KeyboardView.IOnKeyboardActionListener
    {
        private static String TAG = "SoftWingInput";
        private static String MODE_DECIMAL = "decimal";
        private static String MODE_NUMERIC = "numeric";
        private static String MODE_QWERTY = "qwerty";
        private static int KEY_CODE_SWITCH_KEYBOARD = 1111;
        private static int KEY_CODE_BACKSPACE = -5;

        private SoftWingKeyboard keyboardView;
        private Keyboard keyboard;
        private bool caps = false;
        private bool capWords = false;

        public override View OnCreateInputView()
        {
            Log.Debug(TAG, "onCreateInputView()");

            keyboardView = (SoftWingKeyboard)LayoutInflater.Inflate(SoftWing.Resource.Layout.input, null);
            //keyboardView.OnKeyboardActionListener = this;

            return keyboardView;
        }

        private InputMethodSubtype GetCurrentInputMethodSubtype()
        {
            InputMethodManager imeManager = (InputMethodManager)ApplicationContext.GetSystemService(Context.InputMethodService);
            if (imeManager != null)
            {
                return imeManager.CurrentInputMethodSubtype;
            }
            else
            {
                return null;
            }
        }

        private IBinder GetImeToken()
        {
            Window window = Window.Window;
            if (window != null)
            {
                return window.Attributes.Token;
            }
            else
            {
                return null;
            }
        }

        public void SwipeDown()
        {
        }

        public void SwipeLeft()
        {
        }

        public void SwipeRight()
        {
        }

        public void SwipeUp()
        {
        }

        public void OnKey([GeneratedEnum] Android.Views.Keycode primaryCode, [GeneratedEnum] Android.Views.Keycode[] keyCodes)
        {
            throw new NotImplementedException();
        }

        public void OnPress([GeneratedEnum] Android.Views.Keycode primaryCode)
        {
            throw new NotImplementedException();
        }

        public void OnRelease([GeneratedEnum] Android.Views.Keycode primaryCode)
        {
            throw new NotImplementedException();
        }

        public void OnText(Java.Lang.ICharSequence text)
        {
            throw new NotImplementedException();
        }
    }
}