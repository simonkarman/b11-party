using Networking;
using System;

public class KelderBorrelBlockHitPacket : Packet {
    private readonly Guid blockId;

    public KelderBorrelBlockHitPacket(byte[] bytes) : base(bytes) {
        blockId = ReadGuid();
    }

    public KelderBorrelBlockHitPacket(Guid blockId) : base(Bytes.Pack(Bytes.Of(blockId))) {
        this.blockId = blockId;
    }

    public override void Validate() { }

    public Guid GetBlockId() {
        return blockId;
    }
}
