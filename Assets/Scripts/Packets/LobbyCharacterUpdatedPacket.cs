using Networking;
using System;
using UnityEngine;

public class LobbyCharacterUpdatedPacket : Packet {

    private readonly Guid clientId;
    private readonly Vector2 position;

    public LobbyCharacterUpdatedPacket(byte[] bytes) : base(bytes) {
        clientId = ReadGuid();
        position = ReadVector2();
    }

    public LobbyCharacterUpdatedPacket(Guid clientId, Vector2 position) : base(
        Bytes.Pack(Bytes.Of(clientId), Bytes.Of(position))
    ) {
        this.clientId = clientId;
        this.position = position;
    }

    public override void Validate() { }

    public Guid GetClientId() {
        return clientId;
    }

    public Vector2 GetPosition() {
        return position;
    }
}
