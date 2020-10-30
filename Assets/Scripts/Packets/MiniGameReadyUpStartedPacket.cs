using Networking;

public class MiniGameReadyUpStartedPacket : Packet {

    public MiniGameReadyUpStartedPacket(byte[] bytes) : base(bytes) { }
    public MiniGameReadyUpStartedPacket() : base(new byte[0]) { }

    public override void Validate() { }
}