using UnityEngine;

public class MiniGameChoosePoint : MonoBehaviour {
    [SerializeField]
    private SpriteRenderer sprite = default;
    [SerializeField]
    private int picks = 0;

    public void MarkAsCompleted() {
        sprite.color = new Color(1, 1, 1, 0.2f);
    }

    public void ResetPicks() {
        picks = 0;
    }

    public void AddPick() {
        picks++;
    }

    public void RemovePick() {
        picks--;
    }

    public int GetPicks() {
        return picks;
    }
}