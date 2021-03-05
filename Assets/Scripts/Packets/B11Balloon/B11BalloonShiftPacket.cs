using Networking;

public class B11BalloonShiftPacket : Packet {
    public B11BalloonShiftPacket(byte[] bytes) : base(bytes) {}
    public B11BalloonShiftPacket() : base(Bytes.Empty) {}

    public override void Validate() { }
}
