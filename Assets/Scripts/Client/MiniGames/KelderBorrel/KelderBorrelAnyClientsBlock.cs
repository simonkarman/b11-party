using System;
using UnityEngine;

public class KelderBorrelAnyClientsBlock : KelderBorrelBlock {
    [SerializeField]
    private Sprite[] numberSprites;
    [SerializeField]
    private SpriteRenderer numberRenderer;

    private int hitsLeft;

    protected override void OnRegisterHit(Guid clientId, bool isMe) {
        hitsLeft--;
        numberRenderer.sprite = numberSprites[Mathf.Clamp(hitsLeft, 0, numberSprites.Length - 1)];
        if (hitsLeft <= 0) {
            gameObject.SetActive(false);
        } else if (isMe) {
            numberRenderer.color = new Color(1f, 1f, 1f, 0.2f);
        }
    }

    public void SetHits(int hits) {
        hitsLeft = hits;
        numberRenderer.sprite = numberSprites[hitsLeft];
    }
}
