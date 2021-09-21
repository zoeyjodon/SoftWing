namespace SoftWing.SwSystem.Messages
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
