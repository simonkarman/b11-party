using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientScoreOverviewPhase : MonoBehaviour {
    [SerializeField]
    private B11PartyClient b11PartyClient = default;
    [SerializeField]
    private GameObject root = default;
    [SerializeField]
    public Text timeLeftText = default;
    [SerializeField]
    private Transform scoreOverviewClientsUIRoot = default;
    [SerializeField]
    private GameObject scoreOverviewClientUIPrefab = default;

    private bool isWatching;
    private float timeWatching;
    private int maxScore;
    private readonly Dictionary<Guid, ScoreOverviewClientUI> scoreOverviewClientUIs = new Dictionary<Guid, ScoreOverviewClientUI>();

    protected void Awake() {
        root.SetActive(false);
        timeLeftText.gameObject.SetActive(false);
        b11PartyClient.OnScoreOverviewStartedCallback += OnStarted;
        b11PartyClient.OnScoreOverviewEndedCallback += OnEnded;
    }

    private void OnStarted(ScoreOverviewStartedPacket.ScoreOverviewInformation[] scoreInformation) {
        root.SetActive(true);
        timeLeftText.gameObject.SetActive(true);
        isWatching = true;
        timeWatching = 0f;
        maxScore = int.MinValue;
        foreach (var clientScore in scoreInformation) {
            B11PartyClient.B11Client client = b11PartyClient.GetClient(clientScore.GetClientId());
            Transform clientObject = Instantiate(scoreOverviewClientUIPrefab, scoreOverviewClientsUIRoot).transform;
            clientObject.name = client.GetName();
            ScoreOverviewClientUI scoreOverviewClientUI = clientObject.GetComponent<ScoreOverviewClientUI>();
            scoreOverviewClientUI.SetFrom(client, clientScore.GetLastAddedScore());
            scoreOverviewClientUI.SetCurrent(0f);
            scoreOverviewClientUIs.Add(client.GetClientId(), scoreOverviewClientUI);

            if (clientScore.GetLastAddedScore() > maxScore) {
                maxScore = clientScore.GetLastAddedScore();
            }
        }
        foreach (var scoreOverviewUI in scoreOverviewClientUIs.Values) {
            scoreOverviewUI.SetMaxScore(maxScore);
        }
    }

    protected void Update() {
        if (isWatching) {
            timeWatching += Time.deltaTime;
            float timeT = Mathf.Clamp01((timeWatching - 2) / (ScoreOverviewPhase.WATCH_DURATION - 5));
            float currentScore = maxScore * timeT;
            foreach (var clientUI in scoreOverviewClientUIs.Values) {
                clientUI.SetCurrent(currentScore);
            }
            float timeLeft = ScoreOverviewPhase.WATCH_DURATION - timeWatching;
            if (timeLeft < 0f) {
                timeLeft = 0f;
                isWatching = false;
                timeLeftText.gameObject.SetActive(false);
            }
            timeLeftText.text = string.Format("Continuing in {0} second(s)...", timeLeft.ToString("0"));
        }
    }

    private void OnEnded() {
        root.SetActive(false);
        foreach (Transform child in scoreOverviewClientsUIRoot.transform) {
            Destroy(child.gameObject);
        }
        isWatching = false;
        timeLeftText.gameObject.SetActive(false);
    }
}