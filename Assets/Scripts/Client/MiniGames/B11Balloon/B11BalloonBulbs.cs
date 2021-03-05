using UnityEngine;

public class B11BalloonBulbs : MonoBehaviour {
    private int bulbsLeft;

    [SerializeField]
    private SpriteRenderer[] bulbs;
    [SerializeField]
    private Color bulbUsedColor;
    [SerializeField]
    private Color bulbAvailableColor;

    public void Reset() {
        bulbsLeft = bulbs.Length;
        foreach (var bulb in bulbs) {
            bulb.color = bulbAvailableColor;
        }
    }

    public bool HasAllBulbsLeft() {
        return bulbsLeft == bulbs.Length;
    }

    public bool HasNoBulbsLeft() {
        return bulbsLeft == 0;
    }

    public void UseOne() {
        bulbsLeft -= 1;
        bulbs[bulbsLeft].color = bulbUsedColor;
    }
}
