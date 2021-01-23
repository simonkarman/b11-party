using Networking;
using System;
using System.Collections.Generic;
using UnityEngine;

public class KetelVangenClientMiniGame : ClientMiniGame {
    [SerializeField]
    private Transform root = default;
    [SerializeField]
    private int maxScore = 100;

    [SerializeField]
    private Transform bottleParent = default;
    [SerializeField]
    private Transform bottleLeftSpawnPoint = default;
    [SerializeField]
    private Transform bottleRightSpawnPoint = default;
    [SerializeField]
    private GameObject bottleKetel1Prefab = default;
    [SerializeField]
    private GameObject bottleKetel1MatuurPrefab = default;
    [SerializeField]
    private GameObject bottleSmirnoffIcePrefab = default;

    [SerializeField]
    private Transform characterParent = default;
    [SerializeField]
    private GameObject otherCharacterPrefab = default;
    [SerializeField]
    private GameObject meCharacterPrefab = default;

    private bool meHasFinished = false;
    private int score = 0;
    private KetelVangenCharacter me;
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
                me = characterInstance.GetComponent<KetelVangenCharacter>();
                me.OnHitBottle += OnHitBottle;
            }
            characterInstance.GetComponent<SpriteRenderer>().sprite = client.GetSprite();
            characters.Add(client.GetClientId(), characterInstance);
        }
    }

    private void OnPacket(Packet packet) {
        if (packet is KetelVangenSpawnedPacket bottle) {
            Spawn(bottle.GetSpawnType(), bottle.GetXPositionT(), bottle.GetSpeed());
        } else if (packet is KetelVangenCharacterUpdatedPacket characterUpdate) {
            characters[characterUpdate.GetClientId()].localPosition = characterUpdate.GetPosition();
        } else if (packet is MiniGamePlayingFinishedPacket) {
            if (!meHasFinished) {
                SetFinishedMe();
            }
        }
    }

    private void Spawn(KetelVangenSpawnedPacket.SpawnType spawnType, float xPositionT, float speed) {
        if (meHasFinished) {
            return;
        }

        GameObject prefab;
        switch (spawnType) {
        default:
        case KetelVangenSpawnedPacket.SpawnType.KETEL_1:
            prefab = bottleKetel1Prefab;
            break;
        case KetelVangenSpawnedPacket.SpawnType.KETEL_1_MATUUR:
            prefab = bottleKetel1MatuurPrefab;
            break;
        case KetelVangenSpawnedPacket.SpawnType.SMIRNOFF_ICE:
            prefab = bottleSmirnoffIcePrefab;
            break;
        }

        Transform obstacleInstance = Instantiate(prefab, bottleParent).transform;
        obstacleInstance.position = Vector3.Lerp(bottleLeftSpawnPoint.position, bottleRightSpawnPoint.position, xPositionT);
        obstacleInstance.name = spawnType.ToString() + " Bottle";
        KetelVangenBottle bottle = obstacleInstance.GetComponent<KetelVangenBottle>();
        bottle.SetSpeed(speed);
    }

    protected override void OnReadyUpImpl() {
    }

    protected override void OnPlayingImpl() {
        root.gameObject.SetActive(true);
    }

    protected override void OnPlayingEndedImpl() {
        root.gameObject.SetActive(false);
        me.OnHitBottle -= OnHitBottle;
    }

    private void OnHitBottle(int bottlePoints) {
        if (meHasFinished) {
            return;
        }

        score += bottlePoints;
        if (score < 0) {
            score = 0;
        }
        b11PartyClient.GetKarmanClient().Send(new MiniGamePlayingScorePacket(
            b11PartyClient.GetMe().GetClientId(),
            score
        ));
        if (score >= maxScore) {
            SetFinishedMe();
        }
    }

    private void SetFinishedMe() {
        b11PartyClient.GetKarmanClient().Send(new MiniGamePlayingFinishedPacket(
            b11PartyClient.GetMe().GetClientId()
        ));
        meHasFinished = true;
        me.DisableCatching();
    }

    protected override void Update() {
        base.Update();
        if (GetMode() == Mode.PLAYING) {
            b11PartyClient.GetKarmanClient().Send(new KetelVangenCharacterUpdatedPacket(
                b11PartyClient.GetMe().GetClientId(),
                me.transform.localPosition
            ));
        }
    }
}