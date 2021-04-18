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
using System;

namespace SoftWing
{
    [Activity(Label = "MainInputLauncher")]
    [MetaData("com.lge.special_display", Value = "true")]
    [MetaData("android.allow_multiple_resumed_activities", Value = "true")]
    [MetaData("com.android.internal.R.bool.config_perDisplayFocusEnabled", Value = "true")]
    public class MainInputLauncher : Activity
    {
        private const String TAG = "MainInputLauncher";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "OnCreate()");
            base.OnCreate(savedInstanceState);

            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            SetContentView(Resource.Layout.input);
        }

        protected override void OnStart()
        {
            base.OnStart();
            MoveTaskToBack(true);
        }

        [Export("testButtonClicked")]
        public void testButtonClicked(View v)
        {
            Log.Debug(TAG, "testButtonClicked()");
            //ServiceScreenSwapper.Dispatcher.Post(new System.Messages.DisplayUpdateMessage());
            //SoftWingInput.ClickTestButton(this);
        }

        protected override void OnDestroy()
        {
            Log.Debug(TAG, "OnDestroy()");
            base.OnDestroy();
        }
    }

    [Activity(Label = "InputServiceLauncher")]
    [MetaData("com.lge.special_display", Value = "true")]
    [MetaData("android.allow_multiple_resumed_activities", Value = "true")]
    [MetaData("com.android.internal.R.bool.config_perDisplayFocusEnabled", Value = "true")]
    public class InputServiceLauncher : Activity
    {
        private const String TAG = "InputServiceLauncher";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "OnCreate()");
            base.OnCreate(savedInstanceState);

            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            SetContentView(Resource.Layout.input);
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        [Export("testButtonClicked")]
        public void testButtonClicked(View v)
        {
            Log.Debug(TAG, "testButtonClicked()");


            ServiceScreenSwapper.RunningSwapperActivity.StartNewActivity();
            //ServiceScreenSwapper.Dispatcher.Post(new System.Messages.DisplayUpdateMessage());
            //SoftWingInput.ClickTestButton(this);
        }

        protected override void OnDestroy()
        {
            Log.Debug(TAG, "OnDestroy()");
            base.OnDestroy();
        }
    }

    [Activity(Label = "ServiceScreenSwapper")]
    [MetaData("com.lge.special_display", Value = "true")]
    [MetaData("android.allow_multiple_resumed_activities", Value = "true")]
    [MetaData("com.android.internal.R.bool.config_perDisplayFocusEnabled", Value = "true")]
    public class ServiceScreenSwapper : Activity
    {
        public static DisplayManagerHelper mDisplayManagerHelper;
        private LgSwivelStateCallback mSwivelStateCallback;
        public static ServiceScreenSwapper RunningSwapperActivity;
        public static System.MessageDispatcher Dispatcher = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            mDisplayManagerHelper = new DisplayManagerHelper(this);
            mSwivelStateCallback = new LgSwivelStateCallback(this, mDisplayManagerHelper);
            mDisplayManagerHelper.RegisterSwivelStateCallback(mSwivelStateCallback);
            MoveTaskToBack(true);
            RunningSwapperActivity = this;
            if (Dispatcher == null)
            {
                Dispatcher = new System.MessageDispatcher(this);
                Dispatcher.Subscribe(System.MessageType.DisplayUpdate, NotificationReceiver.Instance);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            RunningSwapperActivity = null;
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        public void StartNewActivity()
        {
            var intent = new Intent(this, typeof(MainInputLauncher));
            ActivityOptions options = ActivityOptions.MakeBasic();
            // set Display ID where your activity will be launched
            options.SetLaunchDisplayId(mDisplayManagerHelper.CoverDisplayId);
            var flags = ActivityFlags.NewTask | ActivityFlags.MultipleTask | ActivityFlags.ClearTop;
            intent.AddFlags(flags);
            StartActivity(intent, options.ToBundle());
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
        private Activity parent_context;
        private DisplayManagerHelper parent_manager;
        private const int DISPATCH_DELAY_MS = 500;

        public LgSwivelStateCallback(Activity _parent_context, DisplayManagerHelper _parent_manager)
        {
            parent_context = _parent_context;
            parent_manager = _parent_manager;
        }

        public override void OnSwivelStateChanged(int state)
        {
            switch (state)
            {
                case DisplayManagerHelper.SwivelStart:
                    // Swivel start
                    Log.Debug(TAG, "Swivel Open start");
                    KillActiveInputActivity();
                    break;
                case DisplayManagerHelper.SwivelEnd:
                    // Swivel complete
                    Log.Debug(TAG, "Swivel Open end");
                    StartInputService();
                    break;
                case DisplayManagerHelper.NonSwivelStart:
                    // Non Swivel start
                    Log.Debug(TAG, "Swivel Closed start");
                    KillActiveInputActivity();
                    break;
                case DisplayManagerHelper.NonSwivelEnd:
                    // Non Swivel complete
                    Log.Debug(TAG, "Swivel Closed end");
                    //StartInputService();
                    break;
                default:
                    // default value
                    break;
            }
        }

        private void KillActiveInputActivity()
        {
            Log.Debug(TAG, "Killing the active input activity");
            var intent = new Intent(parent_context, typeof(InputServiceLauncher));
            parent_context.StopService(intent);
        }

        private void StartInputService()
        {
            Log.Debug(TAG, "Starting a new input service");
            var intent = new Intent(parent_context, typeof(InputServiceLauncher));
            ActivityOptions options = ActivityOptions.MakeBasic();
            // set Display ID where your activity will be launched
            if (parent_manager.SwivelState == DisplayManagerHelper.SwivelSwiveled)
            {
                options.SetLaunchDisplayId(parent_manager.MultiDisplayId);
            }
            else
            {
                options.SetLaunchDisplayId(parent_manager.CoverDisplayId);
            }
            var flags = ActivityFlags.NewTask | ActivityFlags.MultipleTask;
            intent.AddFlags(flags);
            parent_context.StartActivity(intent, options.ToBundle());
        }
    }
}