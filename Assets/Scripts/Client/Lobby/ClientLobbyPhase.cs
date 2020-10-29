﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class ClientLobbyPhase : MonoBehaviour {
    [SerializeField]
    private B11PartyClient b11PartyClient = default;
    [SerializeField]
    private GameObject root = default;
    [SerializeField]
    private Transform miniGameRoot = default;
    [SerializeField]
    private Transform charactersRoot = default;
    [SerializeField]
    private GameObject clientLobbyCharacterPrefab = default;
    [SerializeField]
    private GameObject miniGameChoosePointPrefab = default;
    [SerializeField]
    private string[] miniGameNames = default;

    private ClientLobbyCharacter me;
    private readonly Dictionary<Guid, SpriteRenderer> otherClients = new Dictionary<Guid, SpriteRenderer>();
    private readonly Dictionary<string, ClientMiniGameChoosePoint> miniGames = new Dictionary<string, ClientMiniGameChoosePoint>();

    protected void Awake() {
        root.SetActive(false);
        b11PartyClient.OnStartedWithCallback += (B11PartyClient.B11Client meInfo) => {
            // Create a character for each client that is not me
            foreach (var client in b11PartyClient.GetClients()) {
                if (client.GetClientId().Equals(meInfo.GetClientId())) {
                    continue;
                }
                Transform clientObject = new GameObject(client.GetName()).transform;
                clientObject.parent = charactersRoot;
                clientObject.localPosition = UnityEngine.Random.insideUnitCircle;
                clientObject.localScale = Vector3.one * 0.8f;
                SpriteRenderer clientSpriteRenderer = clientObject.gameObject.AddComponent<SpriteRenderer>();
                clientSpriteRenderer.sprite = client.GetSprite();
                clientSpriteRenderer.color = new Color(1, 1, 1, 0.5f);
                otherClients.Add(client.GetClientId(), clientSpriteRenderer);
            }

            // Create me
            Transform meObject = Instantiate(clientLobbyCharacterPrefab, charactersRoot).transform;
            meObject.localPosition = Vector3.zero;
            me = meObject.GetComponent<ClientLobbyCharacter>();
            me.name = meInfo.GetName();
            me.OnChosenMiniGameCallback += OnChosenMiniGame;

            // Create all minigames
            if (miniGameRoot.childCount != miniGameNames.Length) {
                throw new Exception("The number of mini games is not the same as the number of locations for mini games.");
            } else {
                for (int miniGameIndex = 0; miniGameIndex < miniGameRoot.childCount; miniGameIndex++) {
                    Transform location = miniGameRoot.GetChild(miniGameIndex);
                    Transform miniGameObject = Instantiate(miniGameChoosePointPrefab, location).transform;
                    miniGameObject.localPosition = Vector3.zero;
                    miniGameObject.name = miniGameNames[miniGameIndex];
                    ClientMiniGameChoosePoint miniGame = miniGameObject.GetComponent<ClientMiniGameChoosePoint>();
                    miniGame.Setup(miniGameNames[miniGameIndex]);
                    miniGames.Add(miniGameNames[miniGameIndex], miniGame);
                }
            }
        };
        b11PartyClient.OnLobbyStartedCallback += (string[] miniGameNames) => {
            root.SetActive(true);
            me.Reset();
            foreach (var miniGame in miniGames.Values) {
                miniGame.DisableForChoosing();
            }
            foreach (var miniGameName in miniGameNames) {
                miniGames[miniGameName].EnableForChoosing();
            }
        };
        b11PartyClient.OnLobbyEndedCallback += () => {
            root.SetActive(false);
        };
    }

    protected void LateUpdate() {
        if (me.isActiveAndEnabled) {
            b11PartyClient.GetKarmanClient().Send(new LobbyCharacterUpdatedPacket(me.transform.localPosition));
        }
    }

    private void OnChosenMiniGame(string miniGameName) {
        miniGames[miniGameName].SetAsChosen();
        Debug.Log("Chosen minigame: " + miniGameName);
        b11PartyClient.GetKarmanClient().Send(new LobbyCharacterChosenMiniGamePacket(miniGameName));
    }
}