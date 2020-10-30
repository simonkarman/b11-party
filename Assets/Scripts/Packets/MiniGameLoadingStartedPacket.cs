using Networking;

public class MiniGameLoadingStartedPacket : Packet {

    private readonly string miniGameName;

    public MiniGameLoadingStartedPacket(byte[] bytes) : base(bytes) {
        miniGameName = ReadString();
    }

    public MiniGameLoadingStartedPacket(string miniGameName) : base(Bytes.Of(miniGameName)) {
        this.miniGameName = miniGameName;
    }

    public override void Validate() { }

    public string GetMiniGameName() {
        return miniGameName;
    }
}