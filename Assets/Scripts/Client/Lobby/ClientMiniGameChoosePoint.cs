using UnityEngine;
using UnityEngine.UI;

public class ClientMiniGameChoosePoint : MonoBehaviour {
    [SerializeField]
    private Text nameText = default;
    [SerializeField]
    private SpriteRenderer flag = default;
    [SerializeField]
    private SpriteRenderer preview = default;
    [SerializeField]
    private SpriteMask previewMask = default;
    [SerializeField]
    private Color activeColor;
    [SerializeField]
    private float activePreviewAlpha;
    [SerializeField]
    private Color deactiveColor;
    [SerializeField]
    private float deactivePreviewAlpha;
    [SerializeField]
    private Color proximityColor;
    [SerializeField]
    private float proximityPreviewAlpha;
    [SerializeField]
    private Color chosenColor;
    [SerializeField]
    private float chosenPreviewAlpha;

    private string miniGameName;
    private bool canBeChosen;

    public void Setup(string miniGameName, Sprite preview, int miniGameIndex) {
        this.miniGameName = miniGameName;
        nameText.text = miniGameName;
        this.preview.sprite = preview;
        this.preview.sortingOrder = -10 - miniGameIndex;
        previewMask.frontSortingOrder = -10 - miniGameIndex;
        previewMask.backSortingOrder = -11 - miniGameIndex;
    }

    private void SetColor(Color color, float previewAlpha) {
        flag.color = color;
        nameText.color = color;
        preview.color = new Color(1f, 1f, 1f, previewAlpha);
    }

    public void DisableForChoosing() {
        canBeChosen = false;
        SetColor(deactiveColor, deactivePreviewAlpha);
    }

    public void EnableForChoosing() {
        canBeChosen = true;
        SetColor(activeColor, activePreviewAlpha);
    }

    public void SetAsChosen() {
        SetColor(chosenColor, chosenPreviewAlpha);
    }

    protected void OnTriggerEnter2D(Collider2D other) {
        if (!canBeChosen)
            return;

        ClientLobbyCharacter character = other.GetComponent<ClientLobbyCharacter>();
        if (character != null) {
            character.SetClosestMiniGame(miniGameName);
            SetColor(proximityColor, proximityPreviewAlpha);
        }
    }

    protected void OnTriggerExit2D(Collider2D other) {
        if (!canBeChosen)
            return;

        ClientLobbyCharacter character = other.GetComponent<ClientLobbyCharacter>();
        if (character != null) {
            character.SetClosestMiniGame(null);
            SetColor(activeColor, activePreviewAlpha);
        }
    }
}