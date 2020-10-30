using KarmanProtocol;
using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MiniGameLoadingPhase : MonoBehaviour {
    [SerializeField]
    private Text loadingPhaseText = default;
    [SerializeField]
    private B11PartyServer b11PartyServer = default;

    [Serializable]
    public class MiniGameLoadingInformation {
        [SerializeField]
        private string name = default;
        [SerializeField]
        private GameObject prefab = default;

        public string GetName() {
            return name;
        }

        public GameObject GetPrefab() {
            return prefab;
        }
    }
    [SerializeField]
    private MiniGameLoadingInformation[] miniGames = default;

    private readonly Dictionary<Guid, bool> clientLoadStatusses = new Dictionary<Guid, bool>();
    private ServerMiniGame miniGame;
    private KarmanServer server;

    public ServerMiniGame Load(string miniGameName) {
        MiniGameLoadingInformation miniGameInfo = miniGames.Where(mgi => mgi.GetName().Equals(miniGameName)).First();
        Transform miniGameObject = Instantiate(miniGameInfo.GetPrefab()).transform;
        miniGameObject.name = "Loading " + miniGameInfo.GetName();
        miniGameObject.localPosition = Vector3.zero;

        foreach (var client in b11PartyServer.GetClients()) {
            clientLoadStatusses.Add(client.GetClientId(), false);
        }
        server = b11PartyServer.GetKarmanServer();
        server.OnClientPackedReceivedCallback += OnPacket;

        miniGame = miniGameObject.GetComponent<ServerMiniGame>();
        miniGame.OnLoad(b11PartyServer);

        UpdateText();
        return miniGame;
    }

    private void UpdateText() {
        loadingPhaseText.text = string.Format(
            "{0}... {1}/{2}",
            miniGame.name,
            clientLoadStatusses.Values.Count(status => status == false),
            clientLoadStatusses.Count
        );
    }

    private void OnPacket(Guid clientId, Packet packet) {
        if (packet is MiniGameLoadingDonePacket miniGameLoadingDonePacket) {
            if (miniGameLoadingDonePacket.GetClientId().Equals(clientId)) {
                clientLoadStatusses[clientId] = true;
                UpdateText();
                server.Broadcast(miniGameLoadingDonePacket, clientId);
            }
        }
    }

    public bool HasClientsLoading() {
        return clientLoadStatusses.Values.Any(status => status == false);
    }

    public void End() {
        miniGame.name = miniGame.name.Replace("Loading", "MiniGame");
        miniGame = null;
        loadingPhaseText.text = "Mini Game Loading";
        clientLoadStatusses.Clear();
        server.OnClientPackedReceivedCallback -= OnPacket;
        server = null;
    }
}