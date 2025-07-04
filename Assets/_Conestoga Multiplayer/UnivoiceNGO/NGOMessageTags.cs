namespace Adrenak.UniVoice.Networks {
    /// <summary>
    /// The different types of messages we send over NGO 
    /// to implement the <see cref="IAudioClient{T}"/> and <see cref="IAudioServer{T}"/>
    /// interfaces
    /// </summary>
    public class NGOMessageTags {
        public const string PEER_INIT = "PEER_INIT";
        public const string PEER_JOINED = "PEER_JOINED";
        public const string PEER_LEFT = "PEER_LEFT";
        public const string AUDIO_FRAME = "AUDIO_FRAME";
        public const string VOICE_SETTINGS = "VOICE_SETTINGS";
    }
}
