using UnityEngine;
using UnityEngine.UI;

public class ClientMiniGameChoosePoint : MonoBehaviour {
    [SerializeField]
    private Text nameText = default;
    [SerializeField]
    private SpriteRenderer spriteRenderer = default;
    [SerializeField]
    private Color activeColor = Color.white;
    [SerializeField]
    private Color deactiveColor = new Color(1, 1, 1, 0.2f);
    [SerializeField]
    private Color proximityColor = new Color(1, 0.9f, 0.8f);
    [SerializeField]
    private Color chosenColor = Color.green;

    private string miniGameName;
    private bool canBeChosen;

    public void Setup(string miniGameName) {
        this.miniGameName = miniGameName;
        nameText.text = miniGameName;
    }

    private void SetColor(Color color) {
        spriteRenderer.color = color;
        nameText.color = color;
    }

    public void DisableForChoosing() {
        canBeChosen = false;
        SetColor(deactiveColor);
    }

    public void EnableForChoosing() {
        canBeChosen = true;
        SetColor(activeColor);
    }

    public void SetAsChosen() {
        SetColor(chosenColor);
    }

    protected void OnTriggerEnter2D(Collider2D other) {
        if (!canBeChosen)
            return;

        ClientLobbyCharacter character = other.GetComponent<ClientLobbyCharacter>();
        if (character != null) {
            character.SetClosestMiniGame(miniGameName);
            SetColor(proximityColor);
        }
    }

    protected void OnTriggerExit2D(Collider2D other) {
        if (!canBeChosen)
            return;

        ClientLobbyCharacter character = other.GetComponent<ClientLobbyCharacter>();
        if (character != null) {
            character.SetClosestMiniGame(null);
            SetColor(activeColor);
        }
    }
}