using System;
using System.Collections.Generic;
using UnityEngine;

public class ClientTrophyRoomPhase : MonoBehaviour {
    [SerializeField]
    private B11PartyClient b11PartyClient = default;
    [SerializeField]
    private GameObject root = default;
    [SerializeField]
    private Transform trophyRoomClientsUIRoot = default;
    [SerializeField]
    private GameObject trophyRoomClientUIPrefab = default;

    private int watchDuration;
    private bool isWatching;
    private float timeWatching;
    private int maxScore;
    private readonly Dictionary<Guid, TrophyRoomClientUI> trophyRoomClientUIs = new Dictionary<Guid, TrophyRoomClientUI>();

    protected void Awake() {
        root.SetActive(false);
        b11PartyClient.OnTrophyRoomStartedCallback += OnStarted;
    }

    private void OnStarted(int watchDuration, TrophyRoomStartedPacket.Score[] totalScoreInformation) {
        root.SetActive(true);
        this.watchDuration = watchDuration;
        isWatching = true;
        timeWatching = 0f;
        maxScore = int.MinValue;
        foreach (var clientScore in totalScoreInformation) {
            B11PartyClient.B11Client client = b11PartyClient.GetClient(clientScore.GetClientId());
            Transform clientObject = Instantiate(trophyRoomClientUIPrefab, trophyRoomClientsUIRoot).transform;
            clientObject.name = client.GetName();
            TrophyRoomClientUI trophyRoomClientUI = clientObject.GetComponent<TrophyRoomClientUI>();
            trophyRoomClientUI.SetFrom(client, clientScore.GetTotalScore());
            trophyRoomClientUI.SetCurrent(0f);
            trophyRoomClientUIs.Add(client.GetClientId(), trophyRoomClientUI);

            if (clientScore.GetTotalScore() > maxScore) {
                maxScore = clientScore.GetTotalScore();
            }
        }
        foreach (var scoreOverviewUI in trophyRoomClientUIs.Values) {
            scoreOverviewUI.SetMaxScore(maxScore);
        }
    }

    protected void Update() {
        if (isWatching) {
            timeWatching += Time.deltaTime;
            float timeT = Mathf.Clamp01((timeWatching - 2) / (watchDuration - 5));
            float currentScore = maxScore * timeT;
            foreach (var clientUI in trophyRoomClientUIs.Values) {
                clientUI.SetCurrent(currentScore);
            }
        }
    }
}