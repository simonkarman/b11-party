using Networking;

public class MiniGamePlayingStartedPacket : Packet {

    public MiniGamePlayingStartedPacket(byte[] bytes) : base(bytes) { }
    public MiniGamePlayingStartedPacket() : base(new byte[0]) { }

    public override void Validate() { }
}