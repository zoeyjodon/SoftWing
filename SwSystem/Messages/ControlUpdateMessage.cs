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

        public ControlUpdateMessage(ControlId id_in, UpdateType update)
        {
            Update = update;
            Id = id_in;
        }

        public MessageType getMessageType()
        {
            return MessageType.ControlUpdate;
        }
    }
}
