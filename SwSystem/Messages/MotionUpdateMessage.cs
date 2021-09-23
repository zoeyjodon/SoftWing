namespace SoftWing.SwSystem.Messages
{
    public struct MotionDescription
    {
        private static int idCount = 0;

        public MotionDescription(int id_in, float beginX_in, float beginY_in, float endX_in, float endY_in)
        {
            id = id_in;
            beginX = beginX_in;
            beginY = beginY_in;
            endX = endX_in;
            endY = endY_in;
        }

        public static MotionDescription InvalidMotion()
        {
            return new MotionDescription(-1, -1, -1, -1, -1);
        }

        public static int GetMotionId()
        {
            return idCount++;
        }

        public int id { get; set; }
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
