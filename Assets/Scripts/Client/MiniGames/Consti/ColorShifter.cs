using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ColorShifter : MonoBehaviour {
    [SerializeField]
    private float shiftDuration = 2f;

    private SpriteRenderer spriteRenderer;
    private Color initialColor;
    private float hue;

    protected void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialColor = spriteRenderer.color;
        hue = Random.value;
    }

    protected void Update() {
        hue += Time.deltaTime;
        if (hue >= shiftDuration) {
            hue -= shiftDuration;
        }
        spriteRenderer.color = Color.HSVToRGB(hue / shiftDuration, 0.3f, 1f);
    }

    protected void OnDisable() {
        spriteRenderer.color = initialColor;
    }
}
