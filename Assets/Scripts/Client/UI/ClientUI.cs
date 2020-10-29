using System;
using UnityEngine;
using UnityEngine.UI;

public class ClientUI : MonoBehaviour {
    [SerializeField]
    public B11PartyClient b11PartyClient;
    [SerializeField]
    public GameObject root;

    [SerializeField]
    private Image image = default;
    [SerializeField]
    private Text nameText = default;
    [SerializeField]
    private Text scoreText = default;

    private Guid meClientId;

    protected void Awake() {
        root.SetActive(false);
        b11PartyClient.OnStartedWithCallback += (B11PartyClient.B11Client me) => {
            meClientId = me.GetClientId();
            image.sprite = me.GetSprite();
            nameText.text = me.GetName();
            scoreText.text = me.GetScore().ToString();
            root.SetActive(true);
        };
        b11PartyClient.OnStoppedCallback += () => {
            root.SetActive(false);
        };
        b11PartyClient.OnScoreChangedCallback += (Guid guid, int score) => {
            if (guid.Equals(meClientId)) {
                scoreText.text = score.ToString();
            }
        };
    }
}