namespace SoftWing.System.Messages
{
    public class ControlUpdateMessage : SystemMessage
    {
        public enum ControlType : byte
        {
            Invalid,
            Down,
            Up,
            Left,
            Right,
            Center
        }

        public enum UpdateType : byte
        {
            Invalid,
            Pressed,
            Held,
            Released,
        }

        private UpdateType _update_type;
        private ControlType _control_type;

        public ControlUpdateMessage(ControlType control_type, UpdateType update_type)
        {
            _update_type = update_type;
            _control_type = control_type;
        }

        public UpdateType getUpdateType()
        {
            return _update_type;
        }

        public ControlType getControlType()
        {
            return _control_type;
        }

        public MessageType getMessageType()
        {
            return MessageType.ControlUpdate;
        }
    }
}
