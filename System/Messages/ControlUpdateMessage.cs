namespace SoftWing.System.Messages
{
    class ControlUpdateMessage : SystemMessage
    {
        public enum UpdateType : byte
        {
            Invalid,
            DownPressed,
            DownHeld,
            DownReleased,
        }

        private UpdateType _update_type;

        public ControlUpdateMessage(UpdateType update_type)
        {
            _update_type = update_type;
        }

        public UpdateType getUpdateType()
        {
            return _update_type;
        }

        public MessageType getMessageType()
        {
            return MessageType.ControlUpdate;
        }
    }
}
