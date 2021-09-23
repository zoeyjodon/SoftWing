namespace SoftWing.SwSystem.Messages
{
    public struct MotionDescription
    {
        public MotionDescription(float beginX_in, float beginY_in, float endX_in, float endY_in)
        {
            beginX = beginX_in;
            beginY = beginY_in;
            endX = endX_in;
            endY = endY_in;
        }

        public float beginX { get; set; }
        public float beginY { get; set; }
        public float endX { get; set; }
        public float endY { get; set; }
    }

    public class MotionUpdateMessage : SystemMessage
    {
        public MotionDescription motion { get; }
        public bool cancel_requested { get; }

        public MotionUpdateMessage(MotionDescription motion_in, bool cancel_requested_in = false)
        {
            motion = motion_in;
            cancel_requested = cancel_requested_in;
        }

        public MessageType getMessageType()
        {
            return MessageType.MotionUpdate;
        }
    }
}
