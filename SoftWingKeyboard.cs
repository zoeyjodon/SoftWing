using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftWing
{
    public class SoftWingKeyboard : View, View.IOnClickListener
    {
        public SoftWingKeyboard(Context context) :
            base(context)
        {
            Initialize();
        }

        public SoftWingKeyboard(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public SoftWingKeyboard(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        public void OnClick(View v)
        {
            throw new NotImplementedException();
        }

        private void Initialize()
        {
        }
    }
}