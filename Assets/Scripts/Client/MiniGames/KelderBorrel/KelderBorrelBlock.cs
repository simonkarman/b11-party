using System;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class KelderBorrelBlock : MonoBehaviour {
    private Guid me;
    protected Guid blockId;
    private B11PartyClient b11PartyClient;

    protected bool isHit = false;

    [SerializeField]
    private Sprite[] blockSprites;
    [SerializeField]
    protected SpriteRenderer blockRenderer;
    [SerializeField]
    protected Collider2D blockCollider;

    public void Initialize(B11PartyClient b11PartyClient, Guid me, Guid blockId, KelderBorrelBlockPosition position) {
        this.b11PartyClient = b11PartyClient;
        this.me = me;
        this.blockId = blockId;
        if (blockSprites != null && blockSprites.Length > 0) {
            blockRenderer.sprite = blockSprites[Random.Range(0, blockSprites.Length)];
        }
        transform.localPosition = new Vector3(position.GetBlockX() * 1.75f, position.GetLineNumber() * 1.16667f, 0f);
    }

    public bool IsHit() {
        return isHit;
    }

    public void Hit() {
        if (!isHit) {
            isHit = true;
            b11PartyClient.GetKarmanClient().Send(new KelderBorrelBlockHitPacket(blockId));
        }
    }

    public void RegisterHit(Guid clientId, int hitScore) {
        bool isMe = clientId == me;
        if (isMe) {
            if (hitScore > 0) {
                ShowScore(hitScore);
            }
            blockRenderer.color = new Color(1f, 1f, 1f, 0.1f);
            blockCollider.enabled = false;
        }
        OnRegisterHit(clientId, isMe);
    }
    protected abstract void OnRegisterHit(Guid clientId, bool isMe);

    private void ShowScore(int hitScore) {
        Debug.Log("You hit a block for a score of " + hitScore);
    }
}
