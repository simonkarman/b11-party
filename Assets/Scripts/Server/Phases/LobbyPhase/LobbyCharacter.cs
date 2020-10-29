using System;
using UnityEngine;

public class LobbyCharacter : MonoBehaviour {
    [SerializeField]
    private SpriteRenderer spriteRenderer = default;
    [SerializeField]
    private Sprite pabloSprite = default;
    [SerializeField]
    private Sprite thomasSprite = default;
    [SerializeField]
    private Sprite yorickSprite = default;
    [SerializeField]
    private Sprite robinSprite = default;
    [SerializeField]
    private Sprite simonSprite = default;
    [SerializeField]
    private Sprite rogierSprite = default;

    private string miniGame;
    private bool hasChosen = false;

    public bool HasChosen() {
        return hasChosen;
    }

    public void SetNotChosen() {
        miniGame = null;
        hasChosen = false;
    }

    public void SetChosen(string miniGame) {
        this.miniGame = miniGame;
        hasChosen = true;
    }

    public string GetChosen() {
        return miniGame;
    }

    private Sprite GetSprite(string clientName) {
        switch (clientName) {
        case "Pablo":
            return pabloSprite;
        case "Thomas":
            return thomasSprite;
        case "Yorick":
            return yorickSprite;
        case "Robin":
            return robinSprite;
        case "Simon":
            return simonSprite;
        case "Rogier":
            return rogierSprite;
        }
        return null;
    }

    public void Setup(string clientName) {
        spriteRenderer.sprite = GetSprite(clientName);
    }
}