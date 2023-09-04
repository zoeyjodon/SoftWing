using Android.Views;
using static SoftWing.SwSystem.SwSettings;

namespace SoftWing.SwSystem.Messages
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
        public ControlId Id { get; }

        public Keycode? Key { get; }

        public ControlUpdateMessage(ControlId id_in, UpdateType update, Keycode? key)
        {
            Update = update;
            Id = id_in;
            Key = key;
        }

        public MessageType getMessageType()
        {
            return MessageType.ControlUpdate;
        }
    }
}
