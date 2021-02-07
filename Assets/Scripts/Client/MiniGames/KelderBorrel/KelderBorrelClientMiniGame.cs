using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KelderBorrelClientMiniGame : ClientMiniGame {
    [SerializeField]
    private Transform root = default;
    [SerializeField]
    private Transform blockRoot = default;
    [SerializeField]
    private GameObject blockSpecificClientsPrefab = default;
    [SerializeField]
    private GameObject blockAnyClientsPrefab = default;

    [SerializeField]
    private GameObject bar = default;

    [SerializeField]
    private Transform ballParent = default;
    [SerializeField]
    private GameObject ballOtherPrefab = default;
    [SerializeField]
    private GameObject ballMePrefab = default;

    [SerializeField]
    private float blockMoveSpeed = 0.1f;
    [SerializeField]
    private float blockLineY = 0.4f;

    private bool deadMessageSent = false;
    private KelderBorrelBallMe me;
    private readonly Dictionary<Guid, KelderBorrelBall> balls = new Dictionary<Guid, KelderBorrelBall>();
    private readonly Dictionary<Guid, KelderBorrelBlock> blocks = new Dictionary<Guid, KelderBorrelBlock>();

    protected override void OnLoadImpl() {
        root.gameObject.SetActive(false);
        b11PartyClient.OnOtherPacket += OnPacket;
        Guid meId = b11PartyClient.GetMe().GetClientId();
        foreach (var client in b11PartyClient.GetClients()) {
            bool isMe = client.GetClientId().Equals(meId);
            GameObject prefab = isMe ? ballMePrefab : ballOtherPrefab;
            Transform ballInstance = Instantiate(prefab, ballParent).transform;
            ballInstance.localPosition = Vector3.zero;
            if (isMe) {
                me = ballInstance.GetComponent<KelderBorrelBallMe>();
                me.Initialize(bar.transform);
            }
            var ball = ballInstance.GetComponent<KelderBorrelBall>();
            ball.SetSprite(client.GetSprite());
            balls.Add(client.GetClientId(), ball);
        }
    }

    private void OnPacket(Packet packet) {
        if (packet is KelderBorrelBallUpdatedPacket ballUpdate) {
            balls[ballUpdate.GetClientId()].transform.localPosition = ballUpdate.GetPosition();
        } else if (packet is KelderBorrelBlockSpawnedPacket spawnBlock) {
            int hits = spawnBlock.GetHits();
            KelderBorrelBlock block;
            if (hits == -1) {
                block = SpawnSpecificClientsBlock(spawnBlock.GetClients());
            } else {
                block = SpawnAnyClientsBlock(hits);
            }
            block.Initialize(b11PartyClient, b11PartyClient.GetMe().GetClientId(), spawnBlock.GetBlockId(), spawnBlock.GetPosition());
            blocks.Add(spawnBlock.GetBlockId(), block);
        } else if (packet is KelderBorrelBlockUpdatedPacket updateBlock) {
            blocks[updateBlock.GetBlockId()].RegisterHit(updateBlock.GetClientId(), updateBlock.GetHitScore());
        } else if (packet is MiniGamePlayingFinishedPacket characterFinished) {
            bool isMe = b11PartyClient.GetMe().GetClientId().Equals(characterFinished.GetClientId());
            if (!isMe) {
                balls[characterFinished.GetClientId()].gameObject.SetActive(false);
            }
        } else if (packet is KelderBorrelMaxScoreReachedPacket) {
            if (!deadMessageSent) {
                Die();
            }
        }
    }

    private KelderBorrelBlock SpawnSpecificClientsBlock(Guid[] clients) {
        Transform blockInstance = Instantiate(blockSpecificClientsPrefab, blockRoot).transform;
        var block = blockInstance.GetComponent<KelderBorrelSpecificClientsBlock>();
        block.SetSpecificClients(b11PartyClient, clients);
        return block;
    }

    private KelderBorrelBlock SpawnAnyClientsBlock(int hits) {
        Transform blockInstance = Instantiate(blockAnyClientsPrefab, blockRoot).transform;
        var block = blockInstance.GetComponent<KelderBorrelAnyClientsBlock>();
        block.SetHits(hits);
        return block;
    }

    protected override void OnReadyUpImpl() {
    }

    protected override void OnPlayingImpl() {
        root.gameObject.SetActive(true);
    }

    protected override void OnPlayingEndedImpl() {
        b11PartyClient.OnOtherPacket -= OnPacket;
    }

    protected override void Update() {
        base.Update();
        if (GetMode() == Mode.PLAYING || GetMode() == Mode.DONE) {
            blockRoot.localPosition += Vector3.down * blockMoveSpeed * Time.deltaTime;
        }
        if (GetMode() == Mode.PLAYING && !deadMessageSent) {
            b11PartyClient.GetKarmanClient().Send(new KelderBorrelBallUpdatedPacket(
                b11PartyClient.GetMe().GetClientId(),
                me.transform.localPosition
            ));
            HideTooLowBlocksHitByMe();
            if (IsAnyBlockTooLow()) {
                Die();
            }
        }
    }

    private void Die() {
        deadMessageSent = true;
        b11PartyClient.GetKarmanClient().Send(new MiniGamePlayingFinishedPacket(
            b11PartyClient.GetMe().GetClientId()
        ));
        me.gameObject.SetActive(false);
        bar.SetActive(false);
    }

    private void HideTooLowBlocksHitByMe() {
        foreach (var block in blocks.Values.Where(block =>
        block.gameObject.activeInHierarchy
        && block.IsHit()
        && (block.transform.position.y < blockLineY + 1f))) {
            block.gameObject.SetActive(false);
        }
    }

    private bool IsAnyBlockTooLow() {
        return blocks.Values.Any(block =>
            block.gameObject.activeInHierarchy
            && !block.IsHit()
            && (block.transform.position.y < blockLineY)
        );
    }

    protected void OnDrawGizmos() {
        var center = Vector3.up * blockLineY;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(center + Vector3.right * 10, center + Vector3.left * 10);
    }
}
