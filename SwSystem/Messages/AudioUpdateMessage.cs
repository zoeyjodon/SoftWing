namespace SoftWing.SwSystem.Messages
{
    class AudioUpdateMessage : SystemMessage
    {
        public enum AudioType
        {
            SwingOpen,
            SwingClose
        }
        public string AudioPath { get; }
        public AudioType Type { get; }

        public AudioUpdateMessage(string path, AudioType type)
        {
            AudioPath = path;
            Type = type;
        }

        public MessageType getMessageType()
        {
            return MessageType.AudioUpdate;
        }
    }
}
