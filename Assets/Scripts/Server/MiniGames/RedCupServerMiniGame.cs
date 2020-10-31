using Networking;
using System;

public class RedCupServerMiniGame : ServerMiniGame {
    private B11PartyServer b11PartyServer;

    public override void OnLoad(B11PartyServer b11PartyServer) {
        this.b11PartyServer = b11PartyServer;
        this.b11PartyServer.GetKarmanServer().OnClientPackedReceivedCallback += OnPacket;
    }

    private void OnPacket(Guid clientId, Packet packet) {
        if (packet is RedCupBallUpdatedPacket) {
            b11PartyServer.GetKarmanServer().Broadcast(packet, clientId);
        } else if (packet is RedCupCupHitPacket) {
            b11PartyServer.GetKarmanServer().Broadcast(packet);
        }
    }

    public override void BeginReadyUp() {
    }

    public override void EndReadyUp() {
    }

    public override void BeginPlaying() {
    }

    public override void EndPlaying() {
    }

    public override void OnUnload() {
        b11PartyServer.GetKarmanServer().OnClientPackedReceivedCallback -= OnPacket;
    }
}