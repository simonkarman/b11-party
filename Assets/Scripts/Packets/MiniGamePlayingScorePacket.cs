using Networking;
using System;

public class MiniGamePlayingScorePacket : Packet {

    private readonly Guid clientId;
    private readonly int score;

    public MiniGamePlayingScorePacket(byte[] bytes) : base(bytes) {
        clientId = ReadGuid();
        score = ReadInt();
    }

    public MiniGamePlayingScorePacket(Guid clientId, int score) : base(
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
