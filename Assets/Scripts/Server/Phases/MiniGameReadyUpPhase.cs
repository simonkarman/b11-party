using KarmanProtocol;
using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MiniGameReadyUpPhase : MonoBehaviour {
    [SerializeField]
    private Text readyUpPhaseText = default;
    [SerializeField]
    private B11PartyServer b11PartyServer = default;

    private readonly Dictionary<Guid, bool> clientReadyStatusses = new Dictionary<Guid, bool>();
    private ServerMiniGame miniGame;
    private KarmanServer server;

    public void BeginReadyUpFor(ServerMiniGame miniGame) {
        foreach (var client in b11PartyServer.GetClients()) {
            if (!client.IsConnected()) {
                continue;
            }
            clientReadyStatusses.Add(client.GetClientId(), false);
        }
        server = b11PartyServer.GetKarmanServer();
        server.OnClientPackedReceivedCallback += OnPacket;

        this.miniGame = miniGame;
        miniGame.BeginReadyUp();

        UpdateText();
    }

    public bool IsWaitingForReadyUp() {
        return clientReadyStatusses.Values.Any(status => status == false);
    }

    private void UpdateText() {
        readyUpPhaseText.text = string.Format(
            "{0}... {1}/{2}",
            miniGame.name,
            clientReadyStatusses.Values.Count(status => status == false),
            clientReadyStatusses.Count
        );
    }

    private void OnPacket(Guid clientId, Packet packet) {
        if (packet is MiniGameReadyUpReadyPacket readyPacket) {
            if (readyPacket.GetClientId().Equals(clientId)) {
                clientReadyStatusses[clientId] = true;
                UpdateText();
                server.Broadcast(readyPacket);
            }
        }
    }

    public void End() {
        miniGame.EndReadyUp();
        miniGame = null;
        readyUpPhaseText.text = "Mini Game Ready Up";
        clientReadyStatusses.Clear();
        server.OnClientPackedReceivedCallback -= OnPacket;
        server = null;
    }
}
 