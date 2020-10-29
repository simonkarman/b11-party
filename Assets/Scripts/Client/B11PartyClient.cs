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

    public Action<B11Client> OnStartedWithCallback;
    public Action OnStoppedCallback;
    public Action<Guid, int> OnScoreChangedCallback;
    public Action<string[]> OnLobbyStartedCallback;
    public Action OnLobbyEndedCallback;

    public void StartWith(KarmanClient karmanClient) {
        foreach (var client in clients) {
            client.SetB11PartyClient(this);
        }
        enabled = true;
        this.karmanClient = karmanClient;
        me = clients.First(client => karmanClient.id.Equals(client.GetClientId()));
        karmanClient.OnPacketReceivedCallback += OnPacketReceived;
        karmanClient.OnJoinedCallback += () => { };
        karmanClient.OnConnectedCallback += () => { };
        karmanClient.OnDisconnectedCallback += () => { };
        karmanClient.OnLeftCallback += () => { };
        OnStartedWithCallback(me);
    }

    public void Stop() {
        enabled = false;
        OnStoppedCallback();
    }

    private void OnPacketReceived(Packet packet) {
        if (packet is PingPacket pingPacket) {
            PingResponsePacket pingResponsePacket = new PingResponsePacket(pingPacket.GetPingId());
            karmanClient.Send(pingResponsePacket);
        } else if (packet is LobbyStartedPacket lobbyStartedPacket) {
            OnLobbyStartedCallback(lobbyStartedPacket.GetAvailableMiniGames());
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
}