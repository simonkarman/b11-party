using Networking;

public class ConstiPowerupUpdatedPacket : Packet {
    private readonly int powerupIndex;
    private readonly bool isActive;

    public ConstiPowerupUpdatedPacket(byte[] bytes) : base(bytes) {
        powerupIndex = ReadInt();
        isActive = ReadInt() > 0;
    }

    public ConstiPowerupUpdatedPacket(int powerupIndex, bool isActive) : base(
        Bytes.Pack(Bytes.Of(powerupIndex), Bytes.Of(isActive ? 1 : 0))
    ) {
        this.powerupIndex = powerupIndex;
        this.isActive = isActive;
    }

    public override void Validate() { }

    public int GetPowerupIndex() {
        return powerupIndex;
    }

    public bool GetIsActive() {
        return isActive;
    }
}
