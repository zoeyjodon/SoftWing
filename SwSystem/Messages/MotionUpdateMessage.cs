namespace SoftWing.SwSystem.Messages
{
    public class MotionUpdateMessage : SystemMessage
    {
        private static int idCount = 0;
        public int id { get; }
        public MotionDescription motion { get; }
        public bool cancel_requested { get; }

        public static int GetMotionId()
        {
            return idCount++;
        }

        public MotionUpdateMessage(int id_in, MotionDescription motion_in, bool cancel_requested_in = false)
        {
            id = id_in;
            motion = motion_in;
            cancel_requested = cancel_requested_in;
        }

        public MessageType getMessageType()
        {
            return MessageType.MotionUpdate;
        }
    }
}
