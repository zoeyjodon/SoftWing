using Android.Net;

namespace SoftWing.SwSystem.Messages
{
    class AudioUpdateMessage : SystemMessage
    {
        public enum AudioType
        {
            SwingOpen,
            SwingClose
        }
        public Uri AudioPath { get; }
        public AudioType Type { get; }

        public AudioUpdateMessage(Uri path, AudioType type)
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
