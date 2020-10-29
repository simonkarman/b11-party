using Networking;
using UnityEngine;

public class LobbyCharacterUpdatedPacket : Packet {

    private readonly Vector2 position;

    public LobbyCharacterUpdatedPacket(byte[] bytes) : base(bytes) {
        position = ReadVector2();
    }

    public LobbyCharacterUpdatedPacket(Vector2 position) : base(Bytes.Of(position)) {
        this.position = position;
    }

    public override void Validate() { }

    public Vector2 GetPosition() {
        return position;
    }
}
