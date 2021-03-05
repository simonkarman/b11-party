using Networking;

public class B11BalloonPoppedPacket : Packet {
    public B11BalloonPoppedPacket(byte[] bytes) : base(bytes) {}
    public B11BalloonPoppedPacket() : base(Bytes.Empty) {}

    public override void Validate() { }
}
