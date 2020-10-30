using Networking;
using System;

public class MiniGamePlayingFinishedPacket : Packet {

    private Guid clientId;

    public MiniGamePlayingFinishedPacket(byte[] bytes) : base(bytes) {
        clientId = ReadGuid();
    }

    public MiniGamePlayingFinishedPacket(Guid clientId) : base(Bytes.Of(clientId)) {
        this.clientId = clientId;
    }

    public override void Validate() { }

    public Guid GetClientId() {
        return clientId;
    }
}