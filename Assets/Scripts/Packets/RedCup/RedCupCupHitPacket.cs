using Networking;
using System;

public class RedCupCupHitPacket : Packet {

    private readonly Guid clientId;
    private readonly int cupId;

    public RedCupCupHitPacket(byte[] bytes) : base(bytes) {
        clientId = ReadGuid();
        cupId = ReadInt();
    }

    public RedCupCupHitPacket(Guid clientId, int cupId) : base(
        Bytes.Pack(Bytes.Of(clientId), Bytes.Of(cupId))
    ) {
        this.clientId = clientId;
        this.cupId = cupId;
    }

    public override void Validate() { }

    public Guid GetClientId() {
        return clientId;
    }

    public int GetCupId() {
        return cupId;
    }
}
