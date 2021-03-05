using Networking;
using System;

public class B11BalloonOrderPacket : Packet {
    private readonly Guid[] order;

    public B11BalloonOrderPacket(byte[] bytes) : base(bytes) {
        order = ReadGuidArray();
    }

    public B11BalloonOrderPacket(Guid[] order) : base(Bytes.Of(order)) {
        this.order = order;
    }

    public override void Validate() { }

    public Guid[] GetOrder() {
        return order;
    }
}
