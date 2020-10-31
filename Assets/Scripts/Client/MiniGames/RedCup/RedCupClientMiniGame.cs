using Networking;
using System;
using System.Collections.Generic;
using UnityEngine;

public class RedCupClientMiniGame : ClientMiniGame {
    [SerializeField]
    private Transform root = default;

    [SerializeField]
    private Transform tablesParent = default;
    [SerializeField]
    private Transform tablesLeftSpawnPoint = default;
    [SerializeField]
    private Transform tablesRightSpawnPoint = default;
    [SerializeField]
    private GameObject meTablePrefab = default;
    [SerializeField]
    private GameObject otherTablePrefab = default;

    private bool meHasFinished = false;
    private int score = 0;
    private MeRedCupTable me;
    private readonly Dictionary<Guid, RedCupTable> tables = new Dictionary<Guid, RedCupTable>();

    protected override void OnLoadImpl() {
        root.gameObject.SetActive(false);
        b11PartyClient.OnOtherPacket += OnPacket;
        Guid meId = b11PartyClient.GetMe().GetClientId();
        float numberOfClientsMinusOne = b11PartyClient.GetClients().Count - 1f;
        int currentClient = 0;
        foreach (var client in b11PartyClient.GetClients()) {
            bool isMe = client.GetClientId().Equals(meId);
            GameObject prefab = isMe ? meTablePrefab : otherTablePrefab;
            Transform tableInstance = Instantiate(prefab, tablesParent).transform;
            float clientT = currentClient / numberOfClientsMinusOne;
            tableInstance.position = Vector3.Lerp(tablesLeftSpawnPoint.position, tablesRightSpawnPoint.position, clientT);
            RedCupTable redCupTable = tableInstance.GetComponent<RedCupTable>();
            redCupTable.SetFrom(client);
            if (isMe) {
                me = (MeRedCupTable)redCupTable;
                me.OnHitCup += OnHitCup;
            }
            tables.Add(client.GetClientId(), redCupTable);
            currentClient++;
        }
    }

    private void OnPacket(Packet packet) {
        if (packet is RedCupCupHitPacket cupHit) {
            tables[cupHit.GetClientId()].SetCupHit(cupHit.GetCupId());
        } else if (packet is RedCupBallUpdatedPacket ballUpdate) {
            tables[ballUpdate.GetClientId()].SetBallPosition(ballUpdate.GetPosition());
        } else if (packet is MiniGamePlayingFinishedPacket finishedPacket) {
            if (!meHasFinished) {
                SetFinishedMe();
            }
            tables[finishedPacket.GetClientId()].SetFinished();
        }
    }

    protected void FixedUpdate() {
        b11PartyClient.GetKarmanClient().Send(new RedCupBallUpdatedPacket(
            b11PartyClient.GetMe().GetClientId(),
            me.GetBallPosition()
        ));
    }

    protected override void OnReadyUpImpl() {
    }

    protected override void OnPlayingImpl() {
        root.gameObject.SetActive(true);
        me.BeginPlaying();
    }

    protected override void OnPlayingEndedImpl() {
        root.gameObject.SetActive(false);
        me.OnHitCup -= OnHitCup;
        b11PartyClient.OnOtherPacket -= OnPacket;
    }

    private void OnHitCup(int cupId) {
        if (meHasFinished) {
            return;
        }

        score += 10;
        b11PartyClient.GetKarmanClient().Send(new MiniGamePlayingScorePacket(
            b11PartyClient.GetMe().GetClientId(),
            score
        ));
        b11PartyClient.GetKarmanClient().Send(new RedCupCupHitPacket(
            b11PartyClient.GetMe().GetClientId(),
            cupId
        ));
    }

    private void SetFinishedMe() {
        me.EndPlaying();
        b11PartyClient.GetKarmanClient().Send(new MiniGamePlayingFinishedPacket(
            b11PartyClient.GetMe().GetClientId()
        ));
        meHasFinished = true;
    }
}