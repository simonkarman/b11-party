using KarmanProtocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class B11PartyServer : MonoBehaviour {
    public const int DEFAULT_PORT = 14641;
    private static readonly Logging.Logger log = Logging.Logger.For<B11PartyServer>();

    private bool serverStarted = false;
    private KarmanServer karmanServer;

    [SerializeField]
    private float startupDelay = 0.2f;

    [SerializeField] private LobbyPhase lobbyPhase = default;
    [SerializeField] private MiniGameLoadingPhase miniGameLoadingPhase = default;
    [SerializeField] private MiniGameReadyUpPhase miniGameReadyUpPhase = default;
    [SerializeField] private MiniGamePlayingPhase miniGamePlayingPhase = default;
    [SerializeField] private ScoreOverviewPhase scoreOverviewPhase = default;
    [SerializeField] private TrophyRoomPhase trophyRoomPhase = default;

    [SerializeField]
    private List<B11Client> clients = new List<B11Client>();

    [SerializeField]
    private List<MiniGameInfo> miniGames = new List<MiniGameInfo>();

    public Action<Phase, MonoBehaviour> OnPhaseChangedCallback;
    public Action<Guid, int> OnClientScoreChangedCallback;
    public Action<IReadOnlyList<MiniGameInfo>> OnMiniGamesChangedCallback;

    public enum Phase {
        STARTING,
        LOBBY,
        MINI_GAME_LOADING,
        MINI_GAME_READY_UP,
        MINI_GAME_PLAYING,
        SCORE_OVERVIEW,
        TROPHY_ROOM
    }

    [Serializable]
    public class B11Client {
        [SerializeField]
        private string name = default;
        [SerializeField]
        private string clientId = default;

        private int score;
        private int lastScore;
        private bool isConnected;

        public Guid GetClientId() {
            return Guid.Parse(clientId);
        }

        public string GetName() {
            return name;
        }

        public void AddScore(int amount) {
            lastScore = score;
            score += amount;
        }

        public int GetScore() {
            return score;
        }

        public int GetLastScore() {
            return lastScore;
        }

        public void SetConnected(bool isConnected) {
            this.isConnected = isConnected;
        }

        public bool IsConnected() {
            return isConnected;
        }
    }

    [Serializable]
    public class MiniGameInfo {
        [SerializeField]
        private string name = default;
        [SerializeField]
        private bool isCompleted = false;

        public string GetName() {
            return name;
        }

        public void MarkAsCompleted() {
            isCompleted = true;
        }

        public bool IsCompleted() {
            return isCompleted;
        }
    }

    private bool HasMiniGamesLeft() {
        return miniGames.Any(mg => !mg.IsCompleted());
    }

    public IReadOnlyList<B11Client> GetClients() {
        return clients;
    }

    protected void Awake() {
        karmanServer = new KarmanServer();
        karmanServer.OnRunningCallback += () => serverStarted = true;
        karmanServer.OnClientConnectedCallback += (Guid clientId) => {
            B11Client client = clients.FirstOrDefault(c => c.GetClientId().Equals(clientId));
            if (client == default(B11Client)) {
                log.Warning("A client with id {0} tried to connect, however that is not a known client id.", clientId);
                Networking.ThreadManager.ExecuteOnMainThread(() => karmanServer.Kick(clientId));
            }
            log.Warning("{0} joined!", client.GetName());
            client.SetConnected(true);
        };
        karmanServer.OnClientDisconnectedCallback += (Guid clientId) => {
            B11Client client = clients.FirstOrDefault(c => c.GetClientId().Equals(clientId));
            if (client != default(B11Client)) {
                log.Warning("{0} disconnected.", client.GetName());
                client.SetConnected(false);
            }
        };

        foreach (var monoBehaviour in new MonoBehaviour[] { lobbyPhase, miniGameLoadingPhase, miniGameReadyUpPhase, miniGamePlayingPhase, scoreOverviewPhase, trophyRoomPhase }) {
            monoBehaviour.gameObject.SetActive(false);
        }
    }

    protected void OnDestroy() {
        karmanServer.Shutdown();
    }

    protected IEnumerator Start() {
        log.Info("Starting server in {0} second(s).", startupDelay);
        yield return new WaitForSeconds(startupDelay);
        karmanServer.Start(DEFAULT_PORT);

        log.Info("Waiting for server to start on port {0}.", DEFAULT_PORT);
        while (!serverStarted) { yield return null; }
        log.Info("Server is up and running!");

        OnMiniGamesChangedCallback(miniGames);

        while (HasMiniGamesLeft()) {

            // Lobby Phase
            log.Info("Moving to LobbyPhase.");
            lobbyPhase.gameObject.SetActive(true);
            lobbyPhase.Begin(miniGames.Where(mg => !mg.IsCompleted()).Select(mg => mg.GetName()).ToArray());
            OnPhaseChangedCallback(Phase.LOBBY, lobbyPhase);
            while (lobbyPhase.IsChoosingMiniGameInProgress()) { yield return null; }
            lobbyPhase.gameObject.SetActive(false);
            MiniGameInfo chosenMiniGame = miniGames.First(mg => mg.GetName().Equals(lobbyPhase.GetChosenMiniGameName()));

            // Mini Game Loading Phase
            log.Info("Moving to MiniGameLoadingPhase, since the '{0}' minigame was chosen.", chosenMiniGame.GetName());
            miniGameLoadingPhase.gameObject.SetActive(true);
            MiniGame miniGame = miniGameLoadingPhase.Load(chosenMiniGame.GetName());
            OnPhaseChangedCallback(Phase.MINI_GAME_LOADING, miniGameLoadingPhase);
            while (miniGameLoadingPhase.HasClientsLoading()) { yield return null; }
            miniGameLoadingPhase.gameObject.SetActive(false);

            // Mini Game Ready Up Phase
            log.Info("Moving to MiniGameReadyUpPhase.");
            miniGameReadyUpPhase.gameObject.SetActive(true);
            miniGameReadyUpPhase.BeginReadyUpFor(miniGame);
            OnPhaseChangedCallback(Phase.MINI_GAME_READY_UP, miniGameReadyUpPhase);
            while (!miniGameReadyUpPhase.IsDone()) { yield return null; }
            miniGameReadyUpPhase.gameObject.SetActive(false);

            // Mini Game Playing Phase
            log.Info("Moving to MiniGamePlayingPhase.");
            miniGamePlayingPhase.gameObject.SetActive(true);
            miniGamePlayingPhase.BeginPlayingFor(miniGame);
            OnPhaseChangedCallback(Phase.MINI_GAME_PLAYING, miniGamePlayingPhase);
            while (!miniGamePlayingPhase.IsDone()) { yield return null; }
            foreach (var client in clients) {
                client.AddScore(miniGamePlayingPhase.GetScore(client.GetClientId()));
                OnClientScoreChangedCallback(client.GetClientId(), client.GetScore());
            }
            chosenMiniGame.MarkAsCompleted();
            OnMiniGamesChangedCallback(miniGames);
            miniGamePlayingPhase.gameObject.SetActive(false);

            // Score Overview Phase
            if (HasMiniGamesLeft()) {
                log.Info("Moving to ScoreOverviewPhase.");
                scoreOverviewPhase.gameObject.SetActive(true);
                scoreOverviewPhase.Show(clients);
                OnPhaseChangedCallback(Phase.SCORE_OVERVIEW, scoreOverviewPhase);
                while (!scoreOverviewPhase.IsDone()) { yield return null; }
                scoreOverviewPhase.gameObject.SetActive(false);
            }
        }

        // Trophy Room Phase
        log.Info("Moving to TrophyRoomPhase, since all minigames are done.");
        trophyRoomPhase.gameObject.SetActive(true);
        trophyRoomPhase.Begin();
    }
}