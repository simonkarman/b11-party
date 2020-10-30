using KarmanProtocol;
using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class B11PartyClient : MonoBehaviour {

    [Serializable]
    public class B11Client {
        [SerializeField]
        private string name = default;
        [SerializeField]
        private string clientId = default;
        [SerializeField]
        private Sprite sprite = default;

        private B11PartyClient b11PartyClient;
        private int score;

        public string GetName() {
            return name;
        }

        public Guid GetClientId() {
            return Guid.Parse(clientId);
        }

        public Sprite GetSprite() {
            return sprite;
        }

        public void SetScore(int score) {
            this.score = score;
            b11PartyClient.OnScoreChangedCallback(GetClientId(), score);
        }

        public int GetScore() {
            return score;
        }

        public void SetB11PartyClient(B11PartyClient b11PartyClient) {
            this.b11PartyClient = b11PartyClient;
        }
    }

    private KarmanClient karmanClient;

    [SerializeField]
    private List<B11Client> clients = default;
    private B11Client me;
    private ClientMiniGame currentMiniGame;

    public Action<B11Client> OnStartedWithCallback;
    public Action OnStoppedCallback;
    public Action<Guid, int> OnScoreChangedCallback;
    public Action<string[]> OnLobbyStartedCallback;
    public Action OnLobbyEndedCallback;
    public Action<string> OnMiniGameLoadingStartedCallback;
    public Action<Guid> OnMiniGameLoadingDoneCallback;
    public Action OnMiniGameLoadingEndedCallback;
    public Action OnMiniGameReadyUpStartedCallback;
    public Action<Guid> OnMiniGameReadyUpReadyCallback;
    public Action OnMiniGameReadyUpEndedCallback;
    public Action OnMiniGamePlayingStartedCallback;
    public Action<Guid> OnMiniGamePlayingFinishedCallback;
    public Action<Guid, int> OnMiniGamePlayingScoreCallback;
    public Action OnMiniGamePlayingEndedCallback;
    public Action<ScoreOverviewStartedPacket.ScoreOverviewInformation[]> OnScoreOverviewStartedCallback;
    public Action OnScoreOverviewEndedCallback;
    public Action<Packet> OnOtherPacket;

    public void StartWith(KarmanClient karmanClient) {
        foreach (var client in clients) {
            client.SetB11PartyClient(this);
        }
        enabled = true;
        this.karmanClient = karmanClient;
        me = GetClient(karmanClient.id);
        karmanClient.OnPacketReceivedCallback += OnPacketReceived;
        karmanClient.OnJoinedCallback += () => { };
        karmanClient.OnConnectedCallback += () => { };
        karmanClient.OnDisconnectedCallback += () => { };
        karmanClient.OnLeftCallback += () => { };
        OnStartedWithCallback(me);
        OnOtherPacket += (Packet packet) => {};
    }

    public B11Client GetClient(Guid clientId) {
        return clients.First(client => client.GetClientId().Equals(clientId));
    }

    public void Stop() {
        enabled = false;
        OnStoppedCallback();
    }

    private void OnPacketReceived(Packet packet) {
        // Ping
        if (packet is PingPacket pingPacket) {
            PingResponsePacket pingResponsePacket = new PingResponsePacket(pingPacket.GetPingId());
            karmanClient.Send(pingResponsePacket);
        }

        // Score
        else if (packet is ClientScoreChangedPacket clientScoreChangedPacket) {
            GetClient(clientScoreChangedPacket.GetClientId()).SetScore(clientScoreChangedPacket.GetScore());
        }

        // Lobby
        else if (packet is LobbyStartedPacket lobbyStartedPacket) {
            OnLobbyStartedCallback(lobbyStartedPacket.GetAvailableMiniGames());
        } else if (packet is LobbyEndedPacket) {
            OnLobbyEndedCallback();
        }
        // Mini Game Loading
        else if (packet is MiniGameLoadingStartedPacket miniGameLoadingStartedPacket) {
            OnMiniGameLoadingStartedCallback(miniGameLoadingStartedPacket.GetMiniGameName());
        } else if (packet is MiniGameLoadingDonePacket miniGameLoadingDonePacket) {
            OnMiniGameLoadingDoneCallback(miniGameLoadingDonePacket.GetClientId());
        } else if (packet is MiniGameLoadingEndedPacket) {
            OnMiniGameLoadingEndedCallback();
        }
        // Mini Game Ready Up
        else if (packet is MiniGameReadyUpStartedPacket miniGameReadyUpStartedPacket) {
            OnMiniGameReadyUpStartedCallback();
        } else if (packet is MiniGameReadyUpReadyPacket miniGameReadyUpReadyPacket) {
            OnMiniGameReadyUpReadyCallback(miniGameReadyUpReadyPacket.GetClientId());
        } else if (packet is MiniGameReadyUpEndedPacket) {
            OnMiniGameReadyUpEndedCallback();
        }
        // Mini Game Playing
        else if (packet is MiniGamePlayingStartedPacket miniGamePlayingStartedPacket) {
            OnMiniGamePlayingStartedCallback();
        } else if (packet is MiniGamePlayingFinishedPacket miniGamePlayingFinishedPacket) {
            OnMiniGamePlayingFinishedCallback(miniGamePlayingFinishedPacket.GetClientId());
        } else if (packet is MiniGamePlayingScorePacket miniGamePlayingScorePacket) {
            OnMiniGamePlayingScoreCallback(miniGamePlayingScorePacket.GetClientId(), miniGamePlayingScorePacket.GetScore());
        } else if (packet is MiniGamePlayingEndedPacket) {
            OnMiniGamePlayingEndedCallback();
        }
        // Score Overview
        else if (packet is ScoreOverviewStartedPacket scoreOverviewStartedPacket) {
            OnScoreOverviewStartedCallback(scoreOverviewStartedPacket.GetScoreOverviews());
        } else if (packet is ScoreOverviewEndedPacket scoreOverviewEndedPacket) {
            OnScoreOverviewEndedCallback();
        }

        // Other Packets
        else {
            OnOtherPacket(packet);
        }
    }

    public IReadOnlyList<B11Client> GetClients() {
        return clients;
    }

    public B11Client GetMe() {
        return me;
    }

    public KarmanClient GetKarmanClient() {
        return karmanClient;
    }

    public void SetCurrentMiniGame(ClientMiniGame currentMiniGame) {
        this.currentMiniGame = currentMiniGame;
    }

    public ClientMiniGame GetCurrentMiniGame() {
        return currentMiniGame;
    }
}