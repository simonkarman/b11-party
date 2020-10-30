using Networking;
using System;

public class B11BalloonSizePacket : Packet {

    private readonly Guid clientId;
    private readonly float size;

    public B11BalloonSizePacket(byte[] bytes) : base(bytes) {
        clientId = ReadGuid();
        size = ReadFloat();
    }

    public B11BalloonSizePacket(Guid clientId, float size) : base(
        Bytes.Pack(Bytes.Of(clientId), Bytes.Of(size))
    ) {
        this.clientId = clientId;
        this.size = size;
    }

    public override void Validate() { }

    public Guid GetClientId() {
        return clientId;
    }

    public float GetSize() {
        return size;
    }
}
