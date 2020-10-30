using Networking;
using System;

public class MiniGameReadyUpReadyPacket : Packet {

    private Guid clientId;

    public MiniGameReadyUpReadyPacket(byte[] bytes) : base(bytes) {
        clientId = ReadGuid();
    }

    public MiniGameReadyUpReadyPacket(Guid clientId) : base(Bytes.Of(clientId)) {
        this.clientId = clientId;
    }

    public override void Validate() { }

    public Guid GetClientId() {
        return clientId;
    }
}