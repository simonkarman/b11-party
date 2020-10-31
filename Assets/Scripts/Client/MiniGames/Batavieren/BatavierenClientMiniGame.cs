using Networking;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BatavierenClientMiniGame : ClientMiniGame {
    [SerializeField]
    private Transform root = default;

    [SerializeField]
    private Transform obstacleAirParent = default;
    [SerializeField]
    private Transform obstacleGroundParent = default;
    [SerializeField]
    private GameObject obstacleGroundPrefab = default;
    [SerializeField]
    private GameObject obstacleAirPrefab = default;

    [SerializeField]
    private Transform characterParent = default;
    [SerializeField]
    private GameObject otherCharacterPrefab = default;
    [SerializeField]
    private GameObject meCharacterPrefab = default;

    [SerializeField]
    private float scoreSpeed = 3f;

    private bool deadMessageSent = false;
    private int lastScoreSent = -1;
    private float score = 0f;
    private BatavierenCharacter me;
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
                me = characterInstance.GetComponent<BatavierenCharacter>();
            }
            characters.Add(client.GetClientId(), characterInstance);
        }
    }

    private void OnPacket(Packet packet) {
        if (packet is BatavierenObstacleSpawnedPacket obstacle) {
            Spawn(obstacle.GetMode(), obstacle.GetSpeed());
        } else if (packet is BatavierenCharacterUpdatedPacket characterUpdate) {
            characters[characterUpdate.GetClientId()].localPosition = characterUpdate.GetPosition();
        }
    }

    private void Spawn(BatavierenObstacleSpawnedPacket.Mode mode, float speed) {
        Transform spawnPoint;
        GameObject prefab;
        switch (mode) {
        default:
        case BatavierenObstacleSpawnedPacket.Mode.GROUND:
            spawnPoint = obstacleGroundParent;
            prefab = obstacleGroundPrefab;
            break;
        case BatavierenObstacleSpawnedPacket.Mode.AIR:
            spawnPoint = obstacleAirParent;
            prefab = obstacleAirPrefab;
            break;
        }

        Transform obstacleInstance = Instantiate(prefab, spawnPoint).transform;
        obstacleInstance.localPosition = Vector3.zero;
        obstacleInstance.name = mode.ToString() + " Obstacle";
        BatavierenObstacle obstacle = obstacleInstance.GetComponent<BatavierenObstacle>();
        obstacle.SetSpeed(speed);
    }

    protected override void OnReadyUpImpl() {
    }

    protected override void OnPlayingImpl() {
        root.gameObject.SetActive(true);
    }

    protected override void OnPlayingEndedImpl() {
        root.gameObject.SetActive(false);
        b11PartyClient.OnOtherPacket -= OnPacket;
    }

    protected override void Update() {
        base.Update();
        if (GetMode() == Mode.PLAYING && !deadMessageSent) {
            b11PartyClient.GetKarmanClient().Send(new BatavierenCharacterUpdatedPacket(
                b11PartyClient.GetMe().GetClientId(),
                me.transform.localPosition
            ));
            if (me.IsAlive()) {
                score += Time.deltaTime * scoreSpeed;
                int scoreAsInt = Mathf.FloorToInt(score);
                if (lastScoreSent != score) {
                    b11PartyClient.GetKarmanClient().Send(new MiniGamePlayingScorePacket(
                        b11PartyClient.GetMe().GetClientId(),
                        scoreAsInt
                    ));
                    lastScoreSent = scoreAsInt;
                }
            } else {
                b11PartyClient.GetKarmanClient().Send(new MiniGamePlayingFinishedPacket(
                    b11PartyClient.GetMe().GetClientId()
                ));
                deadMessageSent = true;
            }
        }
    }
}