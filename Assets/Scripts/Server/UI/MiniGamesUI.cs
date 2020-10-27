using System;
using System.Collections.Generic;
using UnityEngine;

public class MiniGamesUI : MonoBehaviour {
    [SerializeField]
    private B11PartyServer b11PartyServer = default;
    [SerializeField]
    private GameObject miniGameUIPrefab = default;
    [SerializeField]
    private Transform body = default;

    private Dictionary<string, MiniGameUI> miniGameUIs = new Dictionary<string, MiniGameUI>();

    protected void Start() {
        b11PartyServer.OnMiniGamesChangedCallback += OnMiniGamesChanged;
    }

    private void OnMiniGamesChanged(IReadOnlyList<B11PartyServer.MiniGameInfo> miniGames) {
        foreach (var miniGameInfo in miniGames) {
            if (!miniGameUIs.TryGetValue(miniGameInfo.GetName(), out MiniGameUI miniGameUI)) {
                Transform miniGameUIObject = Instantiate(miniGameUIPrefab, body).transform;
                miniGameUIObject.name = miniGameUIPrefab.name + " " + miniGameInfo.GetName();
                miniGameUIObject.SetAsLastSibling();
                miniGameUI = miniGameUIObject.GetComponent<MiniGameUI>();
            }
            miniGameUI.SetFromInfo(miniGameInfo);
        }
    }
}