using KarmanProtocol;
using Networking;
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

    private readonly Dictionary<Guid, LobbyCharacter> characters = new Dictionary<Guid, LobbyCharacter>();
    private KarmanServer server;

    private void OnDisconnect(Guid clientId) {
        characters[clientId].SetNotChosen();
    }

    private void OnPacket(Guid clientId, Packet packet) {
        if (!gameObject.activeInHierarchy) {
            return;
        }

        if (packet is LobbyCharacterUpdatedPacket updatePacket) {
            if (updatePacket.GetClientId().Equals(clientId)) {
                characters[clientId].transform.position = updatePacket.GetPosition();
                server.Broadcast(updatePacket, clientId);
            }

        } else if (packet is LobbyCharacterChosenMiniGamePacket chosenPacket) {
            log.Info(characters[clientId].name + " has chosen " + chosenPacket.GetMiniGameName());
            characters[clientId].SetChosen(chosenPacket.GetMiniGameName());
        }
    }

    public void Begin(string[] miniGameNames) {
        foreach (var client in b11PartyServer.GetClients()) {
            Transform characterObject = Instantiate(lobbyCharacterPrefab, transform).transform;
            characterObject.name = client.GetName();
            characterObject.position = UnityEngine.Random.insideUnitCircle;
            LobbyCharacter character = characterObject.GetComponent<LobbyCharacter>();
            character.Setup(client.GetName());
            characters.Add(client.GetClientId(), character);
        }

        server = b11PartyServer.GetKarmanServer();
        server.OnClientDisconnectedCallback += OnDisconnect;
        server.OnClientPackedReceivedCallback += OnPacket;
    }

    public bool IsChoosingMiniGameInProgress() {
        // TODO: replace with correct logic again later
        // int characterThatHaveNotChosen = characters.Values.Count(character => !character.HasChosen());
        // return characters.Count == 0 || characterThatHaveNotChosen > 0;
        return characters.Count == 0 || characters.Values.All(character => !character.HasChosen());
    }

    public string GetChosenMiniGameName() {
        Dictionary<string, int> numberOfVotesPerMiniGame = new Dictionary<string, int>();
        foreach (var character in characters) {
            string chosenMiniGame = character.Value.GetChosen();
            if (chosenMiniGame != null) {
                if (!numberOfVotesPerMiniGame.ContainsKey(chosenMiniGame)) {
                    numberOfVotesPerMiniGame.Add(chosenMiniGame, 0);
                }
                numberOfVotesPerMiniGame[chosenMiniGame]++;
            }
            Destroy(character.Value.gameObject);
        }
        log.Info("Votes: {0}", string.Join(", ", numberOfVotesPerMiniGame.Select(miniGameAndVotes => string.Format("{0}: {1}", miniGameAndVotes.Key, miniGameAndVotes.Value))));
        characters.Clear();
        server.OnClientDisconnectedCallback -= OnDisconnect;
        server.OnClientPackedReceivedCallback -= OnPacket;
        return numberOfVotesPerMiniGame.OrderByDescending(miniGameAndVotes => miniGameAndVotes.Value).First().Key;
    }
}