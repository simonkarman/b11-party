using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LaneType = BussenLaneSpawnedPacket.LaneType;

public class BussenClientMiniGame : ClientMiniGame {
    [SerializeField]
    private Transform root = default;
    [SerializeField]
    private Transform contentRoot = default;

    [SerializeField]
    private Transform laneParent = default;
    [SerializeField]
    private GameObject grassLanePrefab = default;
    [SerializeField]
    private GameObject roadLanePrefab = default;

    public BussenLane GetLaneAt(int laneIndex) {
        return lanes.FirstOrDefault(lane => lane.GetIndex() == laneIndex);
    }

    [SerializeField]
    private GameObject waterLanePrefab = default;
    [SerializeField]
    private GameObject lavaLanePrefab = default;

    [SerializeField]
    private Transform characterParent = default;
    [SerializeField]
    private GameObject otherCharacterPrefab = default;
    [SerializeField]
    private GameObject meCharacterPrefab = default;

    private int lastLaneIndex = 0;
    private bool deadMessageSent = false;
    private int lastScoreSent = -1;
    private BussenControllableCharacter me;
    private readonly Dictionary<Guid, Transform> characters = new Dictionary<Guid, Transform>();
    private readonly LinkedList<BussenLane> lanes = new LinkedList<BussenLane>();

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
                me = characterInstance.GetComponent<BussenControllableCharacter>();
                me.SetBussenClientMiniGame(this);
            }
            characterInstance.GetComponent<SpriteRenderer>().sprite = client.GetSprite();
            characters.Add(client.GetClientId(), characterInstance);
        }
    }

    private void OnPacket(Packet packet) {
        if (packet is BussenLaneSpawnedPacket lane) {
            Spawn(lane);
        } else if (packet is BussenCharacterUpdatedPacket characterUpdate) {
            characters[characterUpdate.GetClientId()].localPosition = characterUpdate.GetPosition();
        } else if (packet is BussenLastLaneUpdatedPacket lastLaneUpdate) {
            UpdateLastLaneIndex(lastLaneUpdate.GetLastLaneIndex());
        } else if (packet is MiniGamePlayingFinishedPacket characterFinished) {
            if (!b11PartyClient.GetMe().GetClientId().Equals(characterFinished.GetClientId())) {
                characters[characterFinished.GetClientId()].GetComponent<BussenCharacter>().Kill();
            }
        }
    }

    private void UpdateLastLaneIndex(int lastLaneIndex) {
        this.lastLaneIndex = lastLaneIndex;
        while (lanes.Count > 0 && lanes.First.Value.GetIndex() < lastLaneIndex) {
            lanes.First.Value.StartFadeOut();
            lanes.RemoveFirst();
        }
        if (me.GetLaneIndex() < lastLaneIndex) {
            me.Kill();
        }
    }

    private void Spawn(BussenLaneSpawnedPacket lane) {
        GameObject lanePrefab = (lane.GetLaneType()) switch {
            LaneType.Road => roadLanePrefab,
            LaneType.Water => waterLanePrefab,
            LaneType.Lava => lavaLanePrefab,
            _ => grassLanePrefab,
        };
        Transform laneInstance = Instantiate(lanePrefab, laneParent).transform;
        laneInstance.localPosition = Vector3.up * lane.GetIndex();
        laneInstance.name = $"Lane {lane.GetLaneType()} at {lane.GetIndex()}";
        BussenLane bussenLane = laneInstance.GetComponent<BussenLane>();
        bussenLane.Initialize(lane.GetIndex());
        bussenLane.SetFrom(lane.GetSeed(), lane.GetAmount(), lane.GetMultiplier());
        lanes.AddLast(bussenLane);
        me.SetMaxLaneIndex(lane.GetIndex());
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
        if (GetMode() == Mode.PLAYING || GetMode() == Mode.DONE) {
            Vector3 current = contentRoot.localPosition;
            contentRoot.localPosition = new Vector3(current.x, Mathf.Lerp(current.y, -lastLaneIndex, 0.05f), current.z);
        }
        if (GetMode() == Mode.PLAYING && !deadMessageSent) {
            b11PartyClient.GetKarmanClient().Send(new BussenCharacterUpdatedPacket(
                b11PartyClient.GetMe().GetClientId(),
                me.transform.localPosition
            ));
            if (me.IsAlive()) {
                int score = me.GetLaneIndex();
                if (lastScoreSent < score) {
                    b11PartyClient.GetKarmanClient().Send(new MiniGamePlayingScorePacket(
                        b11PartyClient.GetMe().GetClientId(),
                        score
                    ));
                    lastScoreSent = score;
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
