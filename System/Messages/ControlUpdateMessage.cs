using Android.Views;

namespace SoftWing.System.Messages
{
    public class ControlUpdateMessage : SystemMessage
    {
        public enum UpdateType : byte
        {
            Invalid,
            Pressed,
            Released,
        }

        public UpdateType Update { get; }
        public Keycode Key { get; }

        public ControlUpdateMessage(Keycode key, UpdateType update)
        {
            Update = update;
            Key = key;
        }

        public MessageType getMessageType()
        {
            return MessageType.ControlUpdate;
        }
    }
}
