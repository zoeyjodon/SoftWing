using Android.App;
using Android.Content;
using Android.InputMethodServices;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Com.Lge.Display;
using SoftWing.System;
using System;

namespace SoftWing
{
    [Activity(Label = "SwDisplayManager", Theme = "@android:style/Theme.NoDisplay")]
    public class SwDisplayManager : Activity, System.MessageSubscriber
    {
        private const String TAG = "SwDisplayManager";
        private DisplayManagerHelper lg_display_manager;
        private LgSwivelStateCallback swivel_state_cb;
        private MessageDispatcher dispatcher;
        private static SwDisplayManager instance;


        public static void StartSwDisplayManager(Context calling_context)
        {
            if (instance != null)
            {
                return;
            }
            var intent = new Intent(calling_context, typeof(SwDisplayManager));
            var flags = ActivityFlags.NewTask | ActivityFlags.MultipleTask | ActivityFlags.ClearTop;
            intent.AddFlags(flags);
            calling_context.StartActivity(intent);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug(TAG, "OnCreate");
            base.OnCreate(savedInstanceState);

            lg_display_manager = new DisplayManagerHelper(this);
            instance = this;

            dispatcher = MessageDispatcher.GetInstance(this);
            dispatcher.Subscribe(System.MessageType.DisplayUpdate, this);

            swivel_state_cb = new LgSwivelStateCallback();
            lg_display_manager.RegisterSwivelStateCallback(swivel_state_cb);

        }

        protected override void OnDestroy()
        {
            Log.Debug(TAG, "OnDestroy");
            base.OnDestroy();
        }

        protected override void OnStart()
        {
            Log.Debug(TAG, "OnStart");
            base.OnStart();
        }

        protected override void OnResume()
        {
            Finish();
            base.OnResume();
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
                    imm.SetInputMethod(SoftWingInput.InputSessionToken, InputMethod.Id);
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
                    imm.SetInputMethod(SoftWingInput.InputSessionToken, InputMethod.Id);
                    return;
                }
            }
        }

        public void Accept(SystemMessage message)
        {
            Log.Debug(TAG, "Accept");
            if (lg_display_manager.SwivelState == DisplayManagerHelper.SwivelSwiveled)
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
        private bool ignore_transition = true;

        public LgSwivelStateCallback()
        {
            dispatcher = MessageDispatcher.GetInstance(new Activity());
        }

        public override void OnSwivelStateChanged(int state)
        {
            Log.Debug(TAG, "OnSwivelStateChanged");
            // The callback manager runs once on startup to report the initial state.
            // We only want updates if that state changes.
            if (ignore_transition)
            {
                Log.Debug(TAG, "Ignoring first swivel action");
                ignore_transition = false;
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