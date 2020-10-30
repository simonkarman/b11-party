using UnityEngine;
using UnityEngine.UI;

public class PlayingClientUI : MonoBehaviour {
    [SerializeField]
    public Text nameText = default;
    [SerializeField]
    public Text scoreText = default;
    [SerializeField]
    public Image image = default;

    [SerializeField]
    public Text statusText = default;
    [SerializeField]
    public Image background = default;
    [SerializeField]
    public Color doneColor = Color.green;

    public void SetFrom(B11PartyClient.B11Client client) {
        image.sprite = client.GetSprite();
        nameText.text = client.GetName();
    }

    public void SetFinished() {
        scoreText.text = "??";
        statusText.text = "Finished!";
        background.color = doneColor;
    }

    public void SetScore(int score) {
        if (scoreText) {
            scoreText.text = score.ToString();
        }
    }
}