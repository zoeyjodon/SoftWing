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

        public MotionUpdateMessage(MotionDescription motion_in)
        {
            motion = motion_in;
        }

        public MessageType getMessageType()
        {
            return MessageType.MotionUpdate;
        }
    }
}
