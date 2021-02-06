using Networking;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ConstiClientMiniGame : ClientMiniGame {
    [SerializeField]
    private Transform root = default;
    [SerializeField]
    private ConstiMap map = default;
    [SerializeField]
    private Transform view = default;

    [SerializeField]
    private Transform characterParent = default;
    [SerializeField]
    private GameObject otherCharacterPrefab = default;
    [SerializeField]
    private GameObject meCharacterPrefab = default;

    private float chasingDurationLeft = -1f;
    private bool deadMessageSent = false;
    private ConstiCharacter me;
    private readonly Dictionary<Guid, Transform> characters = new Dictionary<Guid, Transform>();

    protected override void OnLoadImpl() {
        root.gameObject.SetActive(false);
        b11PartyClient.OnOtherPacket += OnPacket;
        Guid meId = b11PartyClient.GetMe().GetClientId();
        foreach (var client in b11PartyClient.GetClients()) {
            bool isMe = client.GetClientId().Equals(meId);
            GameObject prefab = isMe ? meCharacterPrefab : otherCharacterPrefab;
            Transform characterInstance = Instantiate(prefab, characterParent).transform;
            characterInstance.localPosition = Vector3.zero;
            if (isMe) {
                me = characterInstance.GetComponent<ConstiCharacter>();
                me.Initialize(b11PartyClient);
            }
            characterInstance.GetComponent<SpriteRenderer>().sprite = client.GetSprite();
            characters.Add(client.GetClientId(), characterInstance);
        }
        view.gameObject.SetActive(true);
        view.parent = me.transform;
        view.localPosition = Vector3.zero;
    }

    private void OnPacket(Packet packet) {
        if (packet is ConstiCharacterSpawnsPacket characterSpawns) {
            foreach (var spawn in characterSpawns.GetSpawns()) {
                var spawnTransform = map.GetSpawns().GetChild(spawn.GetSpawnIndex());
                if (b11PartyClient.GetMe().GetClientId().Equals(spawn.GetClientId())) {
                    me.SetSpawn(spawnTransform.position);
                } else {
                    characters[spawn.GetClientId()].position = spawnTransform.position;
                }
                spawnTransform.gameObject.SetActive(false);
            }
        } else if (packet is ConstiEnemyUpdatedPacket enemyUpdated) {
            map.GetEnemies().GetChild(enemyUpdated.GetEnemyIndex()).transform.localPosition = enemyUpdated.GetPosition();
        } else if (packet is ConstiEnemyEatenPacket enemyEaten) {
            map.GetEnemies().GetChild(enemyEaten.GetEnemyIndex()).GetComponent<ConstiEnemy>().OnEaten();
        } else if (packet is ConstiCoinUpdatedPacket coinUpdated) {
            map.GetCoins().GetChild(coinUpdated.GetCoinIndex()).gameObject.SetActive(coinUpdated.GetIsActive());
        } else if (packet is ConstiPowerupUpdatedPacket powerupUpdated) {
            map.GetPowerups().GetChild(powerupUpdated.GetPowerupIndex()).gameObject.SetActive(powerupUpdated.GetIsActive());
        } else if (packet is ConstiCharacterChasingPacket characterChasing) {
            if (b11PartyClient.GetMe().GetClientId().Equals(characterChasing.GetClientId())) {
                me.StartChasing();
                foreach (Transform enemy in map.GetEnemies()) {
                    enemy.GetComponent<ConstiEnemy>().StartBeingChased();
                }
                chasingDurationLeft = ConstiServerMiniGame.ChasingDuration;
            } else {
                // TODO: decide wether we want to show that another client is chasing
            }
        } else if (packet is ConstiBlockEnabledPacket blockEnabled) {
            var block = map.GetBlocks().GetChild(blockEnabled.GetBlockIndex());
            block.gameObject.SetActive(true);
            block.GetChild(blockEnabled.GetSwitchIndex()).gameObject.SetActive(true);
        } else if (packet is ConstiBlockDisabledPacket blockDisabled) {
            var block = map.GetBlocks().GetChild(blockDisabled.GetBlockIndex());
            block.gameObject.SetActive(false);
            foreach (Transform blockChild in block) {
                blockChild.gameObject.SetActive(false);
            }
        } else if (packet is ConstiCharacterUpdatedPacket characterUpdated) {
            characters[characterUpdated.GetClientId()].localPosition = characterUpdated.GetPosition();
        } else if (packet is MiniGamePlayingFinishedPacket characterFinished) {
            if (!b11PartyClient.GetMe().GetClientId().Equals(characterFinished.GetClientId())) {
                characters[characterFinished.GetClientId()].gameObject.SetActive(false);
            }
        } else if (packet is ConstiMaxScoreReachedPacket) {
            me.SetAlive(false);
            me.enabled = false;
        }
    }

    protected override void OnReadyUpImpl() {
    }

    protected override void OnPlayingImpl() {
        root.gameObject.SetActive(true);
        me.SetMovingEnabled(false);
        // TODO: do intro (move view camera from center to position of me spawn, and after that do:
        me.SetMovingEnabled(true);
    }

    protected override void OnPlayingEndedImpl() {
        Camera.main.transform.position = Vector3.back * 10;
        root.gameObject.SetActive(false);
        b11PartyClient.OnOtherPacket -= OnPacket;
    }

    protected override void Update() {
        base.Update();
        if (chasingDurationLeft > 0f) {
            chasingDurationLeft -= Time.deltaTime;
            if (chasingDurationLeft <= 0f) {
                chasingDurationLeft = -1f;
                me.StopChasing();
                foreach (Transform enemy in map.GetEnemies()) {
                    enemy.GetComponent<ConstiEnemy>().StopBeingChased();
                }
            }
        }
        if (GetMode() == Mode.PLAYING && !deadMessageSent) {
            Camera.main.transform.position = me.transform.position + new Vector3(0, -1f, -10f);
            b11PartyClient.GetKarmanClient().Send(new ConstiCharacterUpdatedPacket(
                b11PartyClient.GetMe().GetClientId(),
                me.transform.localPosition
            ));
            if (!me.IsAlive()) {
                b11PartyClient.GetKarmanClient().Send(new MiniGamePlayingFinishedPacket(
                    b11PartyClient.GetMe().GetClientId()
                ));
                view.transform.parent = transform;
                me.gameObject.SetActive(false);
                deadMessageSent = true;
            }
        }
    }
}
