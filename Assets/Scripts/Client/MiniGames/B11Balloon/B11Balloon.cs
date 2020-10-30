using System;
using UnityEngine;
using UnityEngine.U2D;

public class B11Balloon : MonoBehaviour {
    [Serializable]
    public class EmojiInfo {
        [SerializeField]
        private float fromSize = default;
        [SerializeField]
        private Sprite emoji = default;

        public float GetFromSize() {
            return fromSize;
        }

        public Sprite GetEmoji() {
            return emoji;
        }
    }

    [SerializeField]
    private SpriteShapeRenderer balloonRenderer = default;

    [SerializeField]
    private SpriteRenderer clientSpriteRenderer = default;

    [SerializeField]
    private EmojiInfo[] emojiInfos = default;

    [SerializeField]
    private Sprite poppedEmoji = default;
    [SerializeField]
    private SpriteRenderer emojiRenderer = default;

    [SerializeField]
    private float maxXSize = 1.0f;

    private float startSize;
    private float maxSize;

    public void ShowEmoji(bool isMe) {
        emojiRenderer.gameObject.SetActive(isMe);
    }

    public void SetStartSize(float startSize) {
        this.startSize = startSize;
        SetSize(startSize);
    }

    public void SetMaxSize(float maxSize) {
        this.maxSize = maxSize;
    }

    public void SetFinished() {
        balloonRenderer.color = new Color(1, 1, 1, 0.3f);
    }

    public void Pop() {
        balloonRenderer.color = Color.clear;
        emojiRenderer.sprite = poppedEmoji;
    }

    public void SetSize(float size) {
        Vector3 sizev3 = Vector3.one * size;
        if (sizev3.x > maxXSize) {
            sizev3.x = maxXSize;
        }
        transform.localScale = sizev3;

        // Set emoij based on startSize maxSize
        float emojiT = Mathf.InverseLerp(startSize, maxSize, size);
        Sprite emojiSprite = null;
        for (int i = 0; i < emojiInfos.Length; i++) {
            if (emojiT > emojiInfos[i].GetFromSize()) {
                emojiSprite = emojiInfos[i].GetEmoji();
            }
        }
        emojiRenderer.sprite = emojiSprite;
    }

    public void SetClientSprite(Sprite sprite) {
        clientSpriteRenderer.sprite = sprite;
    }
}