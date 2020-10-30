using Networking;

public class ScoreOverviewEndedPacket : Packet {

    public ScoreOverviewEndedPacket(byte[] bytes) : base(bytes) { }
    public ScoreOverviewEndedPacket() : base(new byte[0]) { }

    public override void Validate() { }
}