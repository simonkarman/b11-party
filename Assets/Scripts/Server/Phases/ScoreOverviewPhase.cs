using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreOverviewPhase : MonoBehaviour {
    [SerializeField]
    private Text scoreOverviewPhaseText = default;

    private int watchDuration;
    private bool isWatching = false;
    private float watchTimeLeft;

    public void Begin(int watchDuration, IReadOnlyList<B11PartyServer.B11Client> clients) {
        this.watchDuration = watchDuration;
        isWatching = true;
        watchTimeLeft = watchDuration;
    }

    public bool InProgress() {
        return isWatching;
    }

    private void UpdateText() {
        scoreOverviewPhaseText.text = string.Format(
            "Score Overview {0}/{1}",
            watchTimeLeft.ToString("0"),
            watchDuration
        );
    }

    public void End() {
        scoreOverviewPhaseText.text = "Score Overview";
    }

    protected void Update() {
        if (isWatching) {
            watchTimeLeft -= Time.deltaTime;
            if (watchTimeLeft <= 0f) {
                watchTimeLeft = 0f;
                isWatching = false;
            }
            UpdateText();
        }
    }
}