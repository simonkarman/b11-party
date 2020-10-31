using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ClientMiniGameReadyUpPhase : MonoBehaviour {
    [SerializeField]
    private B11PartyClient b11PartyClient = default;
    [SerializeField]
    private GameObject root = default;

    [SerializeField]
    private Transform readyUpClientsUIRoot = default;
    [SerializeField]
    private GameObject readyUpClientUIPrefab = default;

    [SerializeField]
    private Text titleText = default;
    [SerializeField]
    private Text descriptionText = default;
    [SerializeField]
    private Text controlSchemaText = default;
    [SerializeField]
    private Image previewImage = default;

    [Serializable]
    public class MiniGameReadyUpInformation {
        [SerializeField]
        private string name = default;
        [SerializeField, TextArea(3, 10)]
        private string description = default;
        [SerializeField]
        private string controlSchema = default;
        [SerializeField]
        private Sprite preview = default;

        public string GetName() {
            return name;
        }

        public string GetDescription() {
            return description;
        }

        public string GetControlSchema() {
            return controlSchema;
        }

        public Sprite GetPreview() {
            return preview;
        }
    }
    [SerializeField]
    private MiniGameReadyUpInformation[] miniGames = default;

    private readonly Dictionary<Guid, ReadyUpClientUI> readyUpClientUIs = new Dictionary<Guid, ReadyUpClientUI>();

    protected void Awake() {
        root.SetActive(false);
        b11PartyClient.OnMiniGameReadyUpStartedCallback += OnStarted;
        b11PartyClient.OnMiniGameReadyUpReadyCallback += OnDone;
        b11PartyClient.OnMiniGameReadyUpEndedCallback += OnEnded;
    }

    private void OnStarted() {
        root.SetActive(true);
        foreach (var client in b11PartyClient.GetClients()) {
            Transform clientObject = Instantiate(readyUpClientUIPrefab, readyUpClientsUIRoot).transform;
            clientObject.name = client.GetName();
            ReadyUpClientUI readyUpClientUI = clientObject.GetComponent<ReadyUpClientUI>();
            readyUpClientUI.SetFrom(client);
            readyUpClientUIs.Add(client.GetClientId(), readyUpClientUI);
        }
        ClientMiniGame currentMiniGame = b11PartyClient.GetCurrentMiniGame();
        string miniGameName = currentMiniGame.GetMiniGameName();
        MiniGameReadyUpInformation readyUpInformation = miniGames.First(miniGame => miniGame.GetName().Equals(miniGameName));
        titleText.text = readyUpInformation.GetName();
        descriptionText.text = readyUpInformation.GetDescription();
        controlSchemaText.text = readyUpInformation.GetControlSchema();
        previewImage.sprite = readyUpInformation.GetPreview();
        currentMiniGame.OnReadyUp();
    }

    private void OnDone(Guid clientId) {
        readyUpClientUIs[clientId].SetReady();
    }

    private void OnEnded() {
        root.SetActive(false);
        foreach (Transform child in readyUpClientsUIRoot.transform) {
            Destroy(child.gameObject);
        }
        readyUpClientUIs.Clear();
    }


}