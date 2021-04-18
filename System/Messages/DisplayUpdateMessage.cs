using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftWing.System.Messages
{
    class DisplayUpdateMessage : SystemMessage
    {
        public MessageType getMessageType()
        {
            return MessageType.DisplayUpdate;
        }
    }
}