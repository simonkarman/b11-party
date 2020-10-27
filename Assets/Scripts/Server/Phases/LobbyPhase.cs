using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LobbyPhase : MonoBehaviour {
    private static readonly Logging.Logger log = Logging.Logger.For<LobbyPhase>();

    [SerializeField]
    private B11PartyServer b11PartyServer = default;
    [SerializeField]
    private GameObject lobbyCharacterPrefab = default;

    private readonly List<LobbyCharacter> characters = new List<LobbyCharacter>();

    public void Begin(string[] miniGameNames) {
        MiniGameChoosePoint[] choosePoints = GetComponentsInChildren<MiniGameChoosePoint>();
        int count = 0;
        foreach (var point in choosePoints) {
            point.ResetPicks();
            bool canBeChosen = miniGameNames.Any(name => name.Equals(point.name));
            if (!canBeChosen) {
                point.MarkAsCompleted();
            } else {
                count++;
            }
        }
        if (count != miniGameNames.Length) {
            log.Error("One or more miniGameNames that should be able to get chosen are not possible to be chosen in the LobbyPhase.");
        }

        foreach (var client in b11PartyServer.GetClients()) {
            Transform characterObject = Instantiate(lobbyCharacterPrefab, transform).transform;
            characterObject.name = client.GetName();
            characterObject.position = UnityEngine.Random.insideUnitCircle * 5f;
            LobbyCharacter character = characterObject.GetComponent<LobbyCharacter>();
            character.Setup(client.GetName());
            characters.Add(character);
        }
    }

    public bool IsChoosingMiniGameInProgress() {
        int characterThatHaveNotChosen = characters.Count(character => !character.HasChosen());
        return characters.Count == 0 || characterThatHaveNotChosen > 0;
    }

    public string GetChosenMiniGameName() {
        foreach (var character in characters) {
            Destroy(character);
        }
        characters.Clear();
        return GetComponentsInChildren<MiniGameChoosePoint>().OrderByDescending(point => point.GetPicks()).FirstOrDefault().name;
    }
}