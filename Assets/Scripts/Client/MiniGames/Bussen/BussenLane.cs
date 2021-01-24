using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BussenLane : MonoBehaviour {
    public const int LaneWidth = 11;

    [SerializeField]
    protected Transform content;
    [SerializeField]
    private Text lineIndexText;
    [SerializeField]
    private Text lineIndexSpecialText;

    protected int LaneIndex { get; private set; }
    private bool isFadingOut = false;
    private float fade = 0.001f;
    private readonly AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    protected void Awake() {
        content.localScale = Vector3.one * fadeCurve.Evaluate(fade);
    }

    public void Initialize(int index) {
        LaneIndex = index;

        bool isSpecial = index % 11 == 0;
        lineIndexText.text = index.ToString();
        lineIndexText.gameObject.SetActive(!isSpecial);
        lineIndexSpecialText.text = index.ToString();
        lineIndexSpecialText.gameObject.SetActive(isSpecial);
    }

    public int GetIndex() {
        return LaneIndex;
    }

    public abstract void SetFrom(int seed, int amount, float multiplier);

    public void StartFadeOut() {
        isFadingOut = true;
    }

    private void Update() {
        if (!isFadingOut) {
            if (fade < 1f) {
                fade += Time.deltaTime * 2f;
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

    protected int[] AllLinePositions() {
        int[] arr = new int[LaneWidth];
        for (int i = 0; i < LaneWidth; i++) {
            arr[i] = i - (LaneWidth / 2);
        }
        return arr;
    }

    protected int[] NoneCenterLinePositions() {
        List<int> positions = new List<int>();
        for (int i = 0; i < LaneWidth; i++) {
            int value = i - (LaneWidth / 2);
            if (Mathf.Abs(value) >= 2) {
                positions.Add(value);
            }
        }
        return positions.ToArray();
    }

    protected T[] Shuffle<T>(T[] input, System.Random random) {
        int m = input.Length;
        while (m > 0) {
            int i = random.Next(m--);
            T t = input[m];
            input[m] = input[i];
            input[i] = t;
        }
        return input;
    }

    [ContextMenu("Test Shuffle")]
    public void Example() {
        int seed = new System.Random().Next(300000);
        for (int i = 0; i < 10; i++) {
            var r = new System.Random(seed);
            Debug.Log($"Shuffled: [{string.Join(", ", Shuffle(AllLinePositions(), r))}]");
            seed = r.Next(4300000);
        }
    }
}
