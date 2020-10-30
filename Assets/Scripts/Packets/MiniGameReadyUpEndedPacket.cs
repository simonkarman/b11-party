using Networking;

public class MiniGameReadyUpEndedPacket : Packet {

    public MiniGameReadyUpEndedPacket(byte[] bytes) : base(bytes) { }
    public MiniGameReadyUpEndedPacket() : base(new byte[0]) { }

    public override void Validate() { }
}