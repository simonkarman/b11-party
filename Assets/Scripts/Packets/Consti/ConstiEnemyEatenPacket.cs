using Networking;

public class ConstiEnemyEatenPacket : Packet {
    private readonly int enemyIndex;

    public ConstiEnemyEatenPacket(byte[] bytes) : base(bytes) {
        enemyIndex = ReadInt();
    }

    public ConstiEnemyEatenPacket(int enemyIndex) : base(Bytes.Of(enemyIndex)) {
        this.enemyIndex = enemyIndex;
    }

    public override void Validate() { }

    public int GetEnemyIndex() {
        return enemyIndex;
    }
}
