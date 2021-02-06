using Networking;

public class ConstiBlockEnabledPacket : Packet {
    private readonly int blockIndex;
    private readonly int switchIndex;

    public ConstiBlockEnabledPacket(byte[] bytes) : base(bytes) {
        blockIndex = ReadInt();
        switchIndex = ReadInt();
    }

    public ConstiBlockEnabledPacket(int blockIndex, int switchIndex) : base(
        Bytes.Pack(Bytes.Of(blockIndex), Bytes.Of(switchIndex))
    ) {
        this.blockIndex = blockIndex;
        this.switchIndex = switchIndex;
    }

    public override void Validate() { }

    public int GetBlockIndex() {
        return blockIndex;
    }

    public int GetSwitchIndex() {
        return switchIndex;
    }
}