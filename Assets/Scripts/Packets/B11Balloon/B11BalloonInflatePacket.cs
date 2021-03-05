using Networking;

public class B11BalloonInflatePacket : Packet {
    private readonly float size;

    public B11BalloonInflatePacket(byte[] bytes) : base(bytes) {
        size = ReadFloat();
    }

    public B11BalloonInflatePacket(float size) : base(Bytes.Of(size)) {
        this.size = size;
    }

    public override void Validate() { }

    public float GetSize() {
        return size;
    }
}
