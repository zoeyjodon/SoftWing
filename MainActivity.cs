using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using System;
using AndroidX.Core.Content;
using Android;
using Android.Content;

namespace SoftWing
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private const String TAG = "MainActivity";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);


            // If we aren't running the swapper yet, we should be
            if (!ServiceScreenSwapper.IsActive)
            {
                var intent = new Intent(this, typeof(ServiceScreenSwapper));
                var flags = ActivityFlags.NewTask | ActivityFlags.MultipleTask | ActivityFlags.ClearTop;
                intent.AddFlags(flags);
                StartActivity(intent);
            }

            //iget-object p2, p0, Lcom/fishstix/gameboard/g;->a:Lcom/fishstix/gameboard/GameBoard;
            //iget-object p2, p2, Lcom/fishstix/gameboard/GameBoard;->a:Landroid/os/IBinder;
            //const/4 v0, 0x2
            //invoke-virtual {p1, p2, v0}, Landroid/view/inputmethod/InputMethodManager;->showSoftInputFromInputMethod(Landroid/os/IBinder;I)V
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}