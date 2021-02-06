using Networking;
using UnityEngine;

public class ConstiEnemyUpdatedPacket : Packet {
    private readonly int enemyIndex;
    private readonly Vector2 position;

    public ConstiEnemyUpdatedPacket(byte[] bytes) : base(bytes) {
        enemyIndex = ReadInt();
        position = ReadVector2();
    }

    public ConstiEnemyUpdatedPacket(int enemyIndex, Vector2 position) : base(
        Bytes.Pack(Bytes.Of(enemyIndex), Bytes.Of(position))
    ) {
        this.enemyIndex = enemyIndex;
        this.position = position;
    }

    public override void Validate() { }

    public int GetEnemyIndex() {
        return enemyIndex;
    }

    public Vector2 GetPosition() {
        return position;
    }
}
