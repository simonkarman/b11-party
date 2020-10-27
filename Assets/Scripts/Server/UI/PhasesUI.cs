using UnityEngine;
using UnityEngine.UI;

public class PhasesUI : MonoBehaviour {
    [SerializeField]
    private B11PartyServer b11PartyServer = default;

    [SerializeField]
    private Image lobby = default;
    [SerializeField]
    private Image miniGameLoading = default;
    [SerializeField]
    private Image miniGameReadyUp = default;
    [SerializeField]
    private Image miniGamePlaying = default;
    [SerializeField]
    private Image scoreOverview = default;
    [SerializeField]
    private Image trophyRoom = default;

    [SerializeField]
    private Color selectedColor = Color.green;
    [SerializeField]
    private Color deselectedColor = Color.white;

    protected void Start() {
        b11PartyServer.OnPhaseChangedCallback += OnPhaseChanged;
    }

    private void OnPhaseChanged(B11PartyServer.Phase phase, MonoBehaviour phaseBehaviour) {
        foreach (Image image in new[] { lobby, miniGameLoading, miniGameReadyUp, miniGamePlaying, scoreOverview, trophyRoom }) {
            image.color = deselectedColor;
        }
        switch (phase) {
        case B11PartyServer.Phase.STARTING:
            break;
        case B11PartyServer.Phase.LOBBY:
            lobby.color = selectedColor;
            break;
        case B11PartyServer.Phase.MINI_GAME_LOADING:
            miniGameLoading.color = selectedColor;
            break;
        case B11PartyServer.Phase.MINI_GAME_READY_UP:
            miniGameReadyUp.color = selectedColor;
            break;
        case B11PartyServer.Phase.MINI_GAME_PLAYING:
            miniGamePlaying.color = selectedColor;
            break;
        case B11PartyServer.Phase.SCORE_OVERVIEW:
            scoreOverview.color = selectedColor;
            break;
        case B11PartyServer.Phase.TROPHY_ROOM:
            trophyRoom.color = selectedColor;
            break;
        }
    }
}