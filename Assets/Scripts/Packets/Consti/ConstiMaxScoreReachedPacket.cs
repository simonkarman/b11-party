using Networking;

public class ConstiMaxScoreReachedPacket : Packet {
    public ConstiMaxScoreReachedPacket(byte[] bytes) : base(bytes) {}
    public ConstiMaxScoreReachedPacket() : base(Bytes.Empty) {}

    public override void Validate() { }
}
