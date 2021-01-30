using Networking;
using System;

public class KelderBorrelBlockSpawnedPacket : Packet {
    private readonly Guid blockId;
    private readonly KelderBorrelBlockPosition position;
    private readonly int hits;
    private readonly Guid[] clients;

    public KelderBorrelBlockSpawnedPacket(byte[] bytes) : base(bytes) {
        blockId = ReadGuid();
        int lineNumber = ReadInt();
        int blockX = ReadInt();
        position = new KelderBorrelBlockPosition(lineNumber, blockX);
        hits = ReadInt();
        if (hits == -1) {
            clients = ReadGuidArray();
        } else {
            clients = new Guid[0];
        }
    }

    public KelderBorrelBlockSpawnedPacket(Guid blockId, KelderBorrelBlockPosition position, int hits) : base(
        Bytes.Pack(Bytes.Of(blockId), Bytes.Of(position.GetLineNumber()), Bytes.Of(position.GetBlockX()), Bytes.Of(hits))
    ) {
        this.blockId = blockId;
        this.position = position;
        this.hits = hits;
        clients = new Guid[0];
    }

    public KelderBorrelBlockSpawnedPacket(Guid blockId, KelderBorrelBlockPosition position, Guid[] clients) : base(
        Bytes.Pack(Bytes.Of(blockId), Bytes.Of(position.GetLineNumber()), Bytes.Of(position.GetBlockX()), Bytes.Of(-1), Bytes.Of(clients))
    ) {
        this.blockId = blockId;
        this.position = position;
        this.clients = clients;
        hits = -1;
    }

    public override void Validate() { }

    public Guid GetBlockId() {
        return blockId;
    }

    public KelderBorrelBlockPosition GetPosition() {
        return position;
    }

    public int GetHits() {
        return hits;
    }

    public Guid[] GetClients() {
        return clients;
    }
}
