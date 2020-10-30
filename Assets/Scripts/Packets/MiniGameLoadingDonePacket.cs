using Networking;
using System;

public class MiniGameLoadingDonePacket : Packet {
    private readonly Guid clientId;

    public MiniGameLoadingDonePacket(byte[] bytes) : base(bytes) {
        clientId = ReadGuid();
    }

    public MiniGameLoadingDonePacket(Guid clientId) : base(Bytes.Of(clientId)) {
        this.clientId = clientId;
    }

    public override void Validate() { }

    public Guid GetClientId() {
        return clientId;
    }
}
