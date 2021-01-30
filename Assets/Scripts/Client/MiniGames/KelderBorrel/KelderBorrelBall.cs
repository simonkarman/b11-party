using UnityEngine;

public class KelderBorrelBall : MonoBehaviour {
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    public void SetSprite(Sprite sprite) {
        spriteRenderer.sprite = sprite;
    }
}
