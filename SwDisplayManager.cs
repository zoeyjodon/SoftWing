using Android.App;
using Android.Content;
using Android.InputMethodServices;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Com.Lge.Display;
using Google;
using Java.Interop;
using Java.IO;
using SoftWing.System;
using System;

namespace SoftWing
{
    [Activity(Label = "DisplayFocusActivity")]
    [MetaData("com.lge.special_display", Value = "true")]
    [MetaData("android.allow_multiple_resumed_activities", Value = "true")]
    [MetaData("com.android.internal.R.bool.config_perDisplayFocusEnabled", Value = "true")]
    public class DisplayFocusActivity : Activity
    {
        private const String TAG = "DisplayFocusActivity";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "OnCreate()");
            base.OnCreate(savedInstanceState);

            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            SetContentView(Resource.Layout.activity_main);
            MoveTaskToBack(true);
        }

        protected override void OnStart()
        {
            Log.Debug(TAG, "OnStart()");
            base.OnStart();
            Finish();
        }

        protected override void OnDestroy()
        {
            Log.Debug(TAG, "OnDestroy()");
            base.OnDestroy();
        }
    }

    [Activity(Label = "SwDisplayManager")]
    [MetaData("com.lge.special_display", Value = "true")]
    [MetaData("android.allow_multiple_resumed_activities", Value = "true")]
    [MetaData("com.android.internal.R.bool.config_perDisplayFocusEnabled", Value = "true")]
    public class SwDisplayManager : Activity, System.MessageSubscriber
    {
        private const String TAG = "SwDisplayManager";
        public static DisplayManagerHelper mDisplayManagerHelper;
        private LgSwivelStateCallback mSwivelStateCallback;
        public static SwDisplayManager Instance;
        public static MessageDispatcher Dispatcher = null;
        public static int FocusedDisplay = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "OnCreate");
            base.OnCreate(savedInstanceState);

            mDisplayManagerHelper = new DisplayManagerHelper(this);
            MoveTaskToBack(true);
            Instance = this;
            if (Dispatcher == null)
            {
                Dispatcher = new System.MessageDispatcher(this);
                Dispatcher.Subscribe(System.MessageType.DisplayUpdate, this);
            }
            mSwivelStateCallback = new LgSwivelStateCallback(Dispatcher);
            mDisplayManagerHelper.RegisterSwivelStateCallback(mSwivelStateCallback);
        }

        protected override void OnDestroy()
        {
            Log.Debug(TAG, "OnDestroy");
            base.OnDestroy();
            mDisplayManagerHelper = null;
            Dispatcher = null;
            Instance = null;
        }

        protected override void OnStart()
        {
            Log.Debug(TAG, "OnStart");
            base.OnStart();
        }

        public void FocusOnDisplay(int displayId)
        {
            Log.Debug(TAG, "FocusOnDisplay(" + displayId.ToString() + ")");
            var intent = new Intent(this, typeof(DisplayFocusActivity));
            ActivityOptions options = ActivityOptions.MakeBasic();
            // set Display ID where your activity will be launched
            options.SetLaunchDisplayId(displayId);
            var flags = ActivityFlags.NewTask | ActivityFlags.MultipleTask | ActivityFlags.ClearTop;
            intent.AddFlags(flags);
            StartActivity(intent, options.ToBundle());
            FocusedDisplay = displayId;
        }

        public void UseLgKeyboard()
        {
            InputMethodManager imm = (InputMethodManager)
                GetSystemService(InputMethodService);

            foreach (var InputMethod in imm.EnabledInputMethodList)
            {
                Log.Debug(TAG, "InputMethod: " + InputMethod.Id.ToString());
                if (InputMethod.Id.Contains("LgeImeImpl"))
                {
                    Log.Debug(TAG, "Setting Input Method");
                    imm.SetInputMethod(SoftWingInput.mToken, InputMethod.Id);
                    return;
                }
            }
        }

        public void UseSwKeyboard()
        {
            InputMethodManager imm = (InputMethodManager)
                GetSystemService(InputMethodService);

            foreach (var InputMethod in imm.EnabledInputMethodList)
            {
                Log.Debug(TAG, "InputMethod: " + InputMethod.Id.ToString());
                if (InputMethod.Id.Contains("SoftWingInput"))
                {
                    Log.Debug(TAG, "Setting Input Method");
                    imm.SetInputMethod(SoftWingInput.mToken, InputMethod.Id);
                    return;
                }
            }
        }

        public void Accept(SystemMessage message)
        {
            Log.Debug(TAG, "Accept");
            Log.Debug(TAG, "mDisplayManagerHelper.SwivelState = " + mDisplayManagerHelper.SwivelState.ToString());
            if (mDisplayManagerHelper.SwivelState == DisplayManagerHelper.SwivelSwiveled)
            {
                UseLgKeyboard();
            }
        }
    }

    /**
     * Updates the swivel state based on the callback actions.
     *
     * @param state The state of swivel, e.g. SWIVEL_START, SWIVEL_END, etc.
     */
    public class LgSwivelStateCallback : DisplayManagerHelper.SwivelStateCallback
    {
        private const String TAG = "LgSwivelStateCallback";
        private MessageDispatcher dispatcher;
        private bool IgnoreTransition = true;

        public LgSwivelStateCallback(MessageDispatcher _dispatcher)
        {
            dispatcher = _dispatcher;
        }

        public override void OnSwivelStateChanged(int state)
        {
            Log.Debug(TAG, "OnSwivelStateChanged");
            // The callback manager runs once on startup to report the initial state.
            // We only want updates if that state changes.
            if (IgnoreTransition)
            {
                Log.Debug(TAG, "Ignoring first swivel action");
                IgnoreTransition = false;
                return;
            }
            switch (state)
            {
                case DisplayManagerHelper.SwivelStart:
                    // Swivel start
                    Log.Debug(TAG, "Swivel Open start");
                    break;
                case DisplayManagerHelper.SwivelEnd:
                    // Swivel complete
                    Log.Debug(TAG, "Swivel Open end");
                    dispatcher.Post(new System.Messages.DisplayUpdateMessage());
                    break;
                case DisplayManagerHelper.NonSwivelStart:
                    // Non Swivel start
                    Log.Debug(TAG, "Swivel Closed start");
                    break;
                case DisplayManagerHelper.NonSwivelEnd:
                    // Non Swivel complete
                    Log.Debug(TAG, "Swivel Closed end");
                    dispatcher.Post(new System.Messages.DisplayUpdateMessage());
                    break;
                default:
                    // default value
                    break;
            }
        }
    }
}