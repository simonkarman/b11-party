using Networking;

public class ConstiCoinUpdatedPacket : Packet {
    private readonly int coinIndex;
    private readonly bool isActive;

    public ConstiCoinUpdatedPacket(byte[] bytes) : base(bytes) {
        coinIndex = ReadInt();
        isActive = ReadInt() > 0;
    }

    public ConstiCoinUpdatedPacket(int coinIndex, bool isActive) : base(
        Bytes.Pack(Bytes.Of(coinIndex), Bytes.Of(isActive ? 1 : 0))
    ) {
        this.coinIndex = coinIndex;
        this.isActive = isActive;
    }

    public override void Validate() { }

    public int GetCoinIndex() {
        return coinIndex;
    }

    public bool GetIsActive() {
        return isActive;
    }
}
