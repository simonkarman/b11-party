using KarmanProtocol;
using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MiniGamePlayingPhase : MonoBehaviour {
    [SerializeField]
    private Text playingPhaseText = default;
    [SerializeField]
    private B11PartyServer b11PartyServer = default;

    private readonly Dictionary<Guid, bool> clientPlayingStatusses = new Dictionary<Guid, bool>();
    private readonly Dictionary<Guid, int> clientPlayingScores = new Dictionary<Guid, int>();
    private ServerMiniGame miniGame;
    private KarmanServer server;

    public void BeginPlayingFor(ServerMiniGame miniGame) {
        foreach (var client in b11PartyServer.GetClients()) {
            clientPlayingStatusses.Add(client.GetClientId(), false);
            clientPlayingScores.Add(client.GetClientId(), 0);
        }
        server = b11PartyServer.GetKarmanServer();
        server.OnClientPackedReceivedCallback += OnPacket;

        this.miniGame = miniGame;
        miniGame.BeginPlaying();

        UpdateText();
    }

    private void OnPacket(Guid clientId, Packet packet) {
        if (packet is MiniGamePlayingFinishedPacket finishedPacket) {
            if (finishedPacket.GetClientId().Equals(clientId)) {
                clientPlayingStatusses[clientId] = true;
                UpdateText();
                server.Broadcast(finishedPacket);
            }
        } else if (packet is MiniGamePlayingScorePacket scorePacket) {
            if (scorePacket.GetClientId().Equals(clientId)) {
                clientPlayingScores[clientId] = scorePacket.GetScore();
                server.Broadcast(scorePacket);
            }
        }
    }

    private void UpdateText() {
        playingPhaseText.text = string.Format(
            "Playing {0}... {1}/{2} (scores={3})",
            miniGame.name,
            clientPlayingStatusses.Values.Count(status => status == false),
            clientPlayingStatusses.Count,
            string.Join(", ", clientPlayingScores.Values)
        );
    }

    public bool IsAClientStillPlaying() {
        return clientPlayingStatusses.Values.Any(status => status == false);
    }

    public int GetScore(Guid clientId) {
        return clientPlayingScores[clientId];
    }

    public void End() {
        miniGame.EndPlaying();
        miniGame = null;
        playingPhaseText.text = "Mini Game Playing";
        clientPlayingStatusses.Clear();
        clientPlayingScores.Clear();
        server.OnClientPackedReceivedCallback -= OnPacket;
        server = null;
    }
}
