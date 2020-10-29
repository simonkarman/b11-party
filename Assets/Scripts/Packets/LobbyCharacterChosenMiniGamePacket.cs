using Networking;

public class LobbyCharacterChosenMiniGamePacket : Packet {

    private readonly string miniGameName;

    public LobbyCharacterChosenMiniGamePacket(byte[] bytes) : base(bytes) {
        miniGameName = ReadString();
    }

    public LobbyCharacterChosenMiniGamePacket(string miniGameName) : base(Bytes.Of(miniGameName)) {
        this.miniGameName = miniGameName;
    }

    public override void Validate() { }

    public string GetMiniGameName() {
        return miniGameName;
    }
}