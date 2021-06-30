namespace SoftWing.System.Messages
{
    class DisplayUpdateMessage : SystemMessage
    {
        public int SwivelState;

        public DisplayUpdateMessage(int swivel_state)
        {
            SwivelState = swivel_state;
        }

        public MessageType getMessageType()
        {
            return MessageType.DisplayUpdate;
        }
    }
}
