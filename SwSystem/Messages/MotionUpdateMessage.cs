namespace SoftWing.SwSystem.Messages
{
    public enum MotionType
    {
        Invalid = -1,
        Tap = 0,
        Swipe = 1
    }

    public struct MotionDescription
    {

        public MotionDescription(MotionType type_in, float beginX_in, float beginY_in, float endX_in, float endY_in)
        {
            type = type_in;
            beginX = beginX_in;
            beginY = beginY_in;
            endX = endX_in;
            endY = endY_in;
        }

        public static MotionDescription InvalidMotion()
        {
            return new MotionDescription(MotionType.Invalid, -1, -1, -1, -1);
        }

        public MotionType type { get; set; }
        public float beginX { get; set; }
        public float beginY { get; set; }
        public float endX { get; set; }
        public float endY { get; set; }
    }

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
