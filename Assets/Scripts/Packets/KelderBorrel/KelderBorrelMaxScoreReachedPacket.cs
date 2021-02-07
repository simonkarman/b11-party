using Networking;

public class KelderBorrelMaxScoreReachedPacket : Packet {
    public KelderBorrelMaxScoreReachedPacket(byte[] bytes) : base(bytes) {}
    public KelderBorrelMaxScoreReachedPacket() : base(Bytes.Empty) {}

    public override void Validate() { }
}
