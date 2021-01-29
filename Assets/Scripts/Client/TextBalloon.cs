using System;
using UnityEngine;
using UnityEngine.UI;

public class TextBalloon : MonoBehaviour {
    [SerializeField]
    private Text text;

    public void SetText(string text) {
        this.text.text = text;
    }
}
