using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreOverviewPhase : MonoBehaviour {
    public const int WATCH_DURATION = 20;
    [SerializeField]
    private Text scoreOverviewPhaseText = default;
    

    private bool isWatching = false;
    private float watchTimeLeft;

    public void Begin(IReadOnlyList<B11PartyServer.B11Client> clients) {
        isWatching = true;
        watchTimeLeft = WATCH_DURATION;
    }

    public bool InProgress() {
        return isWatching;
    }

    private void UpdateText() {
        scoreOverviewPhaseText.text = string.Format(
            "Score Overview {0}/{1}",
            watchTimeLeft.ToString("0"),
            WATCH_DURATION
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