using Networking;

public class LobbyEndedPacket : Packet {

    public LobbyEndedPacket(byte[] bytes) : base(bytes) { }
    public LobbyEndedPacket() : base(new byte[0]) { }

    public override void Validate() { }
}