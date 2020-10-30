using Networking;
using System;
using UnityEngine;

public class B11BalloonServerMiniGame : ServerMiniGame {
    private static readonly Logging.Logger log = Logging.Logger.For<B11BalloonServerMiniGame>();

    [SerializeField]
    private float maxSizeBase = 1.2f;
    [SerializeField]
    private float maxRandomOffset = 0.5f;

    private B11PartyServer b11PartyServer;

    public override void OnLoad(B11PartyServer b11PartyServer) {
        this.b11PartyServer = b11PartyServer;
        this.b11PartyServer.GetKarmanServer().OnClientPackedReceivedCallback += OnPacket;
    }

    private void OnPacket(Guid clientId, Packet packet) {
        if (packet is B11BalloonSizePacket) {
            b11PartyServer.GetKarmanServer().Broadcast(packet, clientId);
        }
    }

    public override void BeginReadyUp() {
        float maxSize = maxSizeBase + UnityEngine.Random.value * maxRandomOffset;
        log.Info("Max Size of balloon minigame set to: {0}", maxSize);
        b11PartyServer.GetKarmanServer().Broadcast(new B11BalloonMaxSizePacket(maxSize));
    }

    public override void EndReadyUp() { }

    public override void BeginPlaying() { }

    public override void EndPlaying() { }

    public override void OnUnload() { }
}