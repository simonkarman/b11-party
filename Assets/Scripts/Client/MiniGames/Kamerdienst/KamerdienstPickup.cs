using UnityEngine;

public class KamerdienstPickup : MonoBehaviour {
    [SerializeField]
    private KamerdienstItemType item;
    [SerializeField]
    private SpriteRenderer circle;
    [SerializeField]
    private float waitDuration = 2f;

    private float waitT;
    private KamerdienstCharacter meCharacter;

    public void Initialize(KamerdienstCharacter meCharacter) {
        this.meCharacter = meCharacter;
    }

    protected void Update() {
        waitT -= Time.deltaTime;
        bool isAvailable = waitT <= 0;

        circle.color = isAvailable
            ? new Color(0.2f, 0.6f, 0.15f, 0.4f)
            : new Color(0.6f, 0.2f, 0.15f, 0.2f);

        bool isInRangeAndAvailable = isAvailable && IsMeInRange();
        float targetScale = isInRangeAndAvailable ? 1.2f : 1f;
        float currentScale = circle.transform.localScale.x;
        if (Mathf.Abs(targetScale - currentScale) > 0.001f) {
            circle.transform.localScale = Vector3.one * Mathf.Lerp(currentScale, targetScale, 0.05f);
        } else {
            circle.transform.localScale = Vector3.one * targetScale;
        }

        if (Input.GetKeyDown(KeyCode.Space)
            && !meCharacter.IsWaiting()
            && meCharacter.CanHelp()
            && !meCharacter.GetInventory().IsFull()
            && isInRangeAndAvailable
        ) {
            waitT = waitDuration;
            meCharacter.AddItem(item);
        }
    }

    public bool IsMeInRange() {
        float range = 1f;
        return (meCharacter.transform.position - transform.position).sqrMagnitude < range * range;
    }
}
