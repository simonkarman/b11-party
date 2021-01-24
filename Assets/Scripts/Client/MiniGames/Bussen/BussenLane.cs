using UnityEngine;
using UnityEngine.UI;

public abstract class BussenLane : MonoBehaviour {
    public const int LaneWidth = 11;

    [SerializeField]
    private Transform content;
    [SerializeField]
    private Text lineIndexText;
    [SerializeField]
    private Text lineIndexSpecialText;

    private int index;
    private bool isFadingOut = false;
    private float fade = 0.001f;
    private readonly AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    protected void Awake() {
        content.localScale = Vector3.one * fadeCurve.Evaluate(fade);
    }

    public void SetIndex(int index) {
        this.index = index;

        bool isSpecial = index % 11 == 0;
        lineIndexText.text = index.ToString();
        lineIndexText.gameObject.SetActive(!isSpecial);
        lineIndexSpecialText.text = index.ToString();
        lineIndexSpecialText.gameObject.SetActive(isSpecial);
    }

    public int GetIndex() {
        return index;
    }

    public abstract void SetFrom(int seed, int amount, float multiplier);

    public void StartFadeOut() {
        isFadingOut = true;
    }

    private void Update() {
        if (!isFadingOut) {
            if (fade < 1f) {
                fade += Time.deltaTime;
                if (fade > 1f) {
                    fade = 1f;
                }
            }
        }
        if (isFadingOut) {
            if (fade > 0f) {
                fade -= Time.deltaTime;
                if (fade <= 0.01f) {
                    fade = 0.01f;
                    Destroy(gameObject);
                }
            }
        }
        content.localScale = Vector3.one * fadeCurve.Evaluate(fade);
    }
}
