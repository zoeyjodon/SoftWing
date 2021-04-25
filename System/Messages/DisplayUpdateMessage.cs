namespace SoftWing.System.Messages
{
    class DisplayUpdateMessage : SystemMessage
    {
        public MessageType getMessageType()
        {
            return MessageType.DisplayUpdate;
        }
    }
}
