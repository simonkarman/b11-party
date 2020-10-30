using Networking;
using System;

public class ClientScoreChangedPacket : Packet {

    private readonly Guid clientId;
    private readonly int score;

    public ClientScoreChangedPacket(byte[] bytes) : base(bytes) {
        clientId = ReadGuid();
        score = ReadInt();
    }

    public ClientScoreChangedPacket(Guid clientId, int score) : base(
        Bytes.Pack(Bytes.Of(clientId), Bytes.Of(score))
    ) {
        this.clientId = clientId;
        this.score = score;
    }

    public override void Validate() { }

    public Guid GetClientId() {
        return clientId;
    }

    public int GetScore() {
        return score;
    }
}
