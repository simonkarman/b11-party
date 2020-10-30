using Networking;
using System;

public class B11BalloonMaxSizePacket : Packet {

    private readonly float maxSize;

    public B11BalloonMaxSizePacket(byte[] bytes) : base(bytes) {
        maxSize = ReadFloat();
    }

    public B11BalloonMaxSizePacket(float maxSize) : base(Bytes.Of(maxSize)) {
        this.maxSize = maxSize;
    }

    public override void Validate() { }

    public float GetMaxSize() {
        return maxSize;
    }
}
