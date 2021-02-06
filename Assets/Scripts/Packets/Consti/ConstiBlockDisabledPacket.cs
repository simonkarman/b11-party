using Networking;

public class ConstiBlockDisabledPacket : Packet {
    private readonly int blockIndex;

    public ConstiBlockDisabledPacket(byte[] bytes) : base(bytes) {
        blockIndex = ReadInt();
    }

    public ConstiBlockDisabledPacket(int blockIndex) : base(Bytes.Of(blockIndex)) {
        this.blockIndex = blockIndex;
    }

    public override void Validate() { }

    public int GetBlockIndex() {
        return blockIndex;
    }
}
