using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayersUI : MonoBehaviour {
    [SerializeField]
    private B11PartyServer b11PartyServer = default;
    [SerializeField]
    private GameObject playerUIPrefab = default;
    [SerializeField]
    private Transform body = default;

    private Dictionary<Guid, PlayerUI> playerUIs = new Dictionary<Guid, PlayerUI>();

    protected void Start() {
        foreach (var client in b11PartyServer.GetClients()) {
            Transform playerUIObject = Instantiate(playerUIPrefab, body).transform;
            playerUIObject.SetAsLastSibling();
            playerUIObject.name = string.Format("{0} [{1}]", client.GetName(), client.GetClientId());
            PlayerUI playerUI = playerUIObject.GetComponent<PlayerUI>();
            playerUI.SetFrom(client);
            playerUIs.Add(client.GetClientId(), playerUI);
        }
        b11PartyServer.OnClientPingChangedCallback += (Guid clientId, int ping) => {
            playerUIs[clientId].SetPing(ping);
        };
        b11PartyServer.OnClientScoreChangedCallback += (Guid clientId, int score) => {
            playerUIs[clientId].SetScore(score);
        };
    }
}