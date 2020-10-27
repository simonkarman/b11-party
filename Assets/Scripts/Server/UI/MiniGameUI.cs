using UnityEngine;
using UnityEngine.UI;

public class MiniGameUI : MonoBehaviour {
    [SerializeField]
    private Image backgroundImage = default;
    [SerializeField]
    private Text nameText = default;
    [SerializeField]
    private Text statusText = default;

    private Color completedColor = new Color(1, 1, 1, 0.1f);
    private Color notCompletedColor = new Color(1, 1, 1, 0.5f);

    public void SetFromInfo(B11PartyServer.MiniGameInfo miniGameInfo) {
        nameText.text = miniGameInfo.GetName() + "MiniGame";
        backgroundImage.color = miniGameInfo.IsCompleted() ? completedColor : notCompletedColor;
        statusText.text = miniGameInfo.IsCompleted() ? "Completed" : "Not Yet Completed";
    }
}