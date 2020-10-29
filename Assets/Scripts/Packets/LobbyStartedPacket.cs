using Networking;

public class LobbyStartedPacket : Packet {

    private readonly string[] availableMiniGames;

    public LobbyStartedPacket(byte[] bytes) : base(bytes) {
        availableMiniGames = ReadString().Split(',');
    }

    public LobbyStartedPacket(string[] availableMiniGames) : base(Bytes.Of(string.Join(",", availableMiniGames))) {
        this.availableMiniGames = availableMiniGames;
    }

    public override void Validate() { }

    public string[] GetAvailableMiniGames() {
        return availableMiniGames;
    }
}
