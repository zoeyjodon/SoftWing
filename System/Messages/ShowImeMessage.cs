namespace SoftWing.System.Messages
{
    class ShowImeMessage : SystemMessage
    {
        public ShowImeMessage()
        {
        }

        public MessageType getMessageType()
        {
            return MessageType.ShowIme;
        }
    }
}
