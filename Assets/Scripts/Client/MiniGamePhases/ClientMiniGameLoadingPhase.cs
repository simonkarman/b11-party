using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ClientMiniGameLoadingPhase : MonoBehaviour {
    [SerializeField]
    private B11PartyClient b11PartyClient = default;
    [SerializeField]
    private GameObject root = default;
    [SerializeField]
    private Transform loadingClientsUIRoot = default;
    [SerializeField]
    private GameObject loadingClientUIPrefab = default;
    [SerializeField]
    private Text miniGameTitleText = default;

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

    private readonly Dictionary<Guid, LoadingClientUI> loadingClientUIs = new Dictionary<Guid, LoadingClientUI>();

    protected void Awake() {
        root.SetActive(false);
        b11PartyClient.OnMiniGameLoadingStartedCallback += OnStarted;
        b11PartyClient.OnMiniGameLoadingDoneCallback += OnDone;
        b11PartyClient.OnMiniGameLoadingEndedCallback += OnEnded;
    }

    private void OnStarted(string miniGameName) {
        root.SetActive(true);
        foreach (var client in b11PartyClient.GetClients()) {
            Transform clientObject = Instantiate(loadingClientUIPrefab, loadingClientsUIRoot).transform;
            clientObject.name = client.GetName();
            LoadingClientUI loadingClientUI = clientObject.GetComponent<LoadingClientUI>();
            loadingClientUI.SetFrom(client);
            loadingClientUIs.Add(client.GetClientId(), loadingClientUI);
        }
        miniGameTitleText.text = miniGameName;
        MiniGameLoadingInformation loadingInformation = miniGames.First(miniGame => miniGame.GetName().Equals(miniGameName));
        Transform miniGameObject = Instantiate(loadingInformation.GetPrefab()).transform;
        miniGameObject.localPosition = Vector3.zero;
        miniGameObject.name = miniGameName;
        ClientMiniGame clientMiniGame = miniGameObject.GetComponent<ClientMiniGame>();
        clientMiniGame.SetMiniGameName(miniGameName);
        clientMiniGame.OnLoad(b11PartyClient);
        b11PartyClient.SetCurrentMiniGame(clientMiniGame);
    }

    private void OnDone(Guid clientId) {
        loadingClientUIs[clientId].SetDone();
    }

    private void OnEnded() {
        root.SetActive(false);
        foreach (Transform child in loadingClientsUIRoot.transform) {
            Destroy(child.gameObject);
        }
        loadingClientUIs.Clear();
    }
}