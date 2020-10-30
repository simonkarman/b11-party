using Networking;

public class MiniGameLoadingEndedPacket : Packet {

    public MiniGameLoadingEndedPacket(byte[] bytes) : base(bytes) { }
    public MiniGameLoadingEndedPacket() : base(new byte[0]) { }

    public override void Validate() { }
}