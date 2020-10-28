using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {
    [SerializeField]
    private Image image = default;
    [SerializeField]
    private Text nameText = default;
    [SerializeField]
    private Text statusText = default;
    [SerializeField]
    private Text idText = default;
    [SerializeField]
    private Text scoreText = default;

    public void SetFrom(B11PartyServer.B11Client client) {
        image.sprite = client.GetSprite();
        nameText.text = client.GetName();
        statusText.text = "NOT CONNECTED";
        idText.text = client.GetClientId().ToString();
        scoreText.text = "0";
    }

    public void SetPing(int ping) {
        statusText.text = ping < 0 ? "NOT CONNECTED" : string.Format("{0}ms", ping);
    }

    public void SetScore(int score) {
        scoreText.text = score.ToString();
    }
}