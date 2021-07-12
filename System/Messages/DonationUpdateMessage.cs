namespace SoftWing.System.Messages
{
    class DonationUpdateMessage : SystemMessage
    {
        public enum UpdateType : byte
        {
            SetupComplete,
        }
        public UpdateType DonationType { get; }

        public DonationUpdateMessage(UpdateType type)
        {
            DonationType = type;
        }

        public MessageType getMessageType()
        {
            return MessageType.DonationUpdate;
        }
    }
}
