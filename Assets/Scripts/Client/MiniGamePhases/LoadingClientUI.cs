﻿using UnityEngine;
using UnityEngine.UI;

public class LoadingClientUI : MonoBehaviour {
    [SerializeField]
    public Text nameText = default;
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

    public void SetDone() {
        statusText.text = "Done";
        background.color = doneColor;
    }
}