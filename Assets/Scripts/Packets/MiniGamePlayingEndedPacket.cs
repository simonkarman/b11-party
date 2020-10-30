using Networking;

public class MiniGamePlayingEndedPacket : Packet {

    public MiniGamePlayingEndedPacket(byte[] bytes) : base(bytes) { }
    public MiniGamePlayingEndedPacket() : base(new byte[0]) { }

    public override void Validate() { }
}