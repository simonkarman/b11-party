using System;
using UnityEngine;
using UnityEngine.UI;

public class ScoreOverviewClientUI : MonoBehaviour {
    [SerializeField]
    public Text nameText = default;
    [SerializeField]
    public Image image = default;
    [SerializeField]
    public Text scoreText = default;
    [SerializeField]
    public Slider slider = default;

    private float score;

    public void SetFrom(B11PartyClient.B11Client client, float score) {
        image.sprite = client.GetSprite();
        nameText.text = client.GetName();
        scoreText.text = "0";
        this.score = score;
        slider.maxValue = score;
        slider.value = 0;
    }

    public void SetMaxScore(float maxScore) {
        slider.maxValue = maxScore;
    }

    public void SetCurrent(float _currentScore) {
        float cappedCurrentScore = Mathf.Min(_currentScore, score);
        slider.value = cappedCurrentScore;
        scoreText.text = Mathf.RoundToInt(cappedCurrentScore).ToString();
    }
}