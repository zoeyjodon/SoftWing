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
using System;

namespace SoftWing
{
    [Activity(Label = "InputServiceLauncher", WindowSoftInputMode = SoftInput.StateAlwaysVisible)]
    public class InputServiceLauncher : Activity
    {
        private const String TAG = "InputServiceLauncher";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "OnCreate()");
            base.OnCreate(savedInstanceState);

            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            //SetContentView(SoftWing.Resource.Layout.activity_main);

            var imm = (InputMethodManager)
                    GetSystemService(Context.InputMethodService);
            imm.ShowSoftInput(new View(this), ShowFlags.Implicit);
        }

        protected override void OnDestroy()
        {
            Log.Debug(TAG, "OnDestroy()");
            base.OnDestroy();

            var current_focus = CurrentFocus;
            if (current_focus == null)
            {
                return;
            }
            var imm = (InputMethodManager)
                    GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(current_focus.WindowToken, HideSoftInputFlags.None);
        }
    }

    [Activity(Label = "ServiceScreenSwapper")]
    public class ServiceScreenSwapper : Activity
    {
        public static bool IsActive = false;
        public static IBinder CurrentWindowToken;
        private DisplayManagerHelper mDisplayManagerHelper;
        private LgSwivelStateCallback mSwivelStateCallback;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CurrentWindowToken = CurrentFocus?.WindowToken;

            mDisplayManagerHelper = new DisplayManagerHelper(this);
            mSwivelStateCallback = new LgSwivelStateCallback(this, mDisplayManagerHelper);
            mDisplayManagerHelper.RegisterSwivelStateCallback(mSwivelStateCallback);
            MoveTaskToBack(true);
        }

        protected override void OnStart()
        {
            base.OnStart();
            IsActive = true;
        }

        protected override void OnStop()
        {
            base.OnStop();
            IsActive = false;
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
                    StartInputService();
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
            var flags = ActivityFlags.NewTask;
            intent.AddFlags(flags);
            parent_context.StartActivity(intent, options.ToBundle());
        }
    }
}