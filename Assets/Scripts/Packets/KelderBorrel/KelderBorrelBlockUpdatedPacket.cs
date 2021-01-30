using Networking;
using System;

public class KelderBorrelBlockUpdatedPacket : Packet {
    private readonly Guid blockId;
    private readonly Guid clientId;
    private readonly int hitScore;

    public KelderBorrelBlockUpdatedPacket(byte[] bytes) : base(bytes) {
        blockId = ReadGuid();
        clientId = ReadGuid();
        hitScore = ReadInt();
    }

    public KelderBorrelBlockUpdatedPacket(Guid blockId, Guid clientId, int hitScore) : base(
        Bytes.Pack(Bytes.Of(blockId), Bytes.Of(clientId), Bytes.Of(hitScore))
    ) {
        this.blockId = blockId;
        this.clientId = clientId;
        this.hitScore = hitScore;
    }

    public override void Validate() { }

    public Guid GetBlockId() {
        return blockId;
    }

    public Guid GetClientId() {
        return clientId;
    }

    public int GetHitScore() {
        return hitScore;
    }
}
