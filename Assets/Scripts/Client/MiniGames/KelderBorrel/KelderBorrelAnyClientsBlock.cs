using System;
using UnityEngine;

public class KelderBorrelAnyClientsBlock : KelderBorrelBlock {
    [SerializeField]
    private Sprite[] numberSprites;
    [SerializeField]
    private SpriteRenderer numberRenderer;

    private int hitsLeft;

    protected override void OnRegisterHit(Guid clientId) {
        hitsLeft--;
        numberRenderer.sprite = numberSprites[Mathf.Clamp(hitsLeft, 0, numberSprites.Length - 1)];
        if (hitsLeft <= 0) {
            gameObject.SetActive(false);
        }
    }

    public void SetHits(int hits) {
        hitsLeft = hits;
        numberRenderer.sprite = numberSprites[hitsLeft];
    }
}
