using Networking;
using System;

public class ConstiCharacterChasingPacket : Packet {
    private readonly Guid clientId;

    public ConstiCharacterChasingPacket(byte[] bytes) : base(bytes) {
        clientId = ReadGuid();
    }

    public ConstiCharacterChasingPacket(Guid clientId) : base(Bytes.Of(clientId)) {
        this.clientId = clientId;
    }

    public override void Validate() { }

    public Guid GetClientId() {
        return clientId;
    }
}
