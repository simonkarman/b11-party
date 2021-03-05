using Networking;

public class B11BalloonStartRoundPacket : Packet {
    private readonly float balloonSizeMin;
    private readonly float balloonSizeMax;

    public B11BalloonStartRoundPacket(byte[] bytes) : base(bytes) {
        balloonSizeMin = ReadFloat();
        balloonSizeMax = ReadFloat();
    }

    public B11BalloonStartRoundPacket(float balloonSizeMin, float balloonSizeMax) : base(
        Bytes.Pack(Bytes.Of(balloonSizeMin), Bytes.Of(balloonSizeMax))
    ) {
        this.balloonSizeMin = balloonSizeMin;
        this.balloonSizeMax = balloonSizeMax;
    }

    public override void Validate() { }

    public float GetBalloonSizeMin() {
        return balloonSizeMin;
    }

    public float GetBalloonSizeMax() {
        return balloonSizeMax;
    }
}
