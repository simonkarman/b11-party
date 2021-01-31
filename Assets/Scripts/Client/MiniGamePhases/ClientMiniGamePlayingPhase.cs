using System;
using System.Collections.Generic;
using UnityEngine;

public class ClientMiniGamePlayingPhase : MonoBehaviour {
    [SerializeField]
    private B11PartyClient b11PartyClient = default;
    [SerializeField]
    private GameObject root = default;

    [SerializeField]
    private Transform playingClientsUIRoot = default;
    [SerializeField]
    private GameObject playingClientUIPrefab = default;

    private readonly Dictionary<Guid, PlayingClientUI> playingClientUIs = new Dictionary<Guid, PlayingClientUI>();

    protected void Awake() {
        root.SetActive(false);
        b11PartyClient.OnMiniGamePlayingStartedCallback += OnStarted;
        b11PartyClient.OnMiniGamePlayingFinishedCallback += OnFinished;
        b11PartyClient.OnMiniGamePlayingScoreCallback += OnScore;
        b11PartyClient.OnMiniGamePlayingEndedCallback += OnEnded;
    }

    private void OnStarted() {
        root.SetActive(true);
        foreach (var client in b11PartyClient.GetClients()) {
            Transform clientObject = Instantiate(playingClientUIPrefab, playingClientsUIRoot).transform;
            clientObject.name = client.GetName();
            PlayingClientUI playingClientUI = clientObject.GetComponent<PlayingClientUI>();
            playingClientUI.SetFrom(client);
            playingClientUIs.Add(client.GetClientId(), playingClientUI);
        }
        ClientMiniGame currentMiniGame = b11PartyClient.GetCurrentMiniGame();
        currentMiniGame.OnPlaying();
    }

    private void OnFinished(Guid clientId) {
        playingClientUIs[clientId].SetFinished();
    }

    private void OnScore(Guid clientId, int score) {
        playingClientUIs[clientId].SetScore(score);
    }

    private void OnEnded() {
        root.SetActive(false);
        foreach (Transform child in playingClientsUIRoot.transform) {
            Destroy(child.gameObject);
        }
        playingClientUIs.Clear();
        b11PartyClient.GetCurrentMiniGame().OnPlayingEnded();
    }
}