using KarmanProtocol;
using Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class B11PartyServer : MonoBehaviour {
    public static readonly int DEFAULT_PORT = 14641;
    public static readonly Guid GAME_ID = Guid.Parse("11117d77-6145-4732-b30a-fd6f4812e251");
    private static readonly Logging.Logger log = Logging.Logger.For<B11PartyServer>();

    private bool serverStarted = false;
    private KarmanServer karmanServer;

    public KarmanServer GetKarmanServer() {
        return karmanServer;
    }

    [SerializeField]
    private float startupDelay = 0.2f;
    [SerializeField]
    private float pingInterval = 2.5f;

    [SerializeField] private LobbyPhase lobbyPhase = default;
    [SerializeField] private MiniGameLoadingPhase miniGameLoadingPhase = default;
    [SerializeField] private MiniGameReadyUpPhase miniGameReadyUpPhase = default;
    [SerializeField] private MiniGamePlayingPhase miniGamePlayingPhase = default;
    [SerializeField] private ScoreOverviewPhase scoreOverviewPhase = default;
    [SerializeField] private TrophyRoomPhase trophyRoomPhase = default;

    [SerializeField]
    private List<B11Client> clients = new List<B11Client>();
    private Dictionary<Guid, B11Client> clientsById = new Dictionary<Guid, B11Client>();

    [SerializeField]
    private List<MiniGameInfo> miniGames = new List<MiniGameInfo>();

    public Action<Phase, MonoBehaviour> OnPhaseChangedCallback;
    public Action<Guid, int> OnClientScoreChangedCallback;
    public Action<Guid, int> OnClientPingChangedCallback;
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
        private B11PartyServer b11PartyServer;
        [SerializeField]
        private string name = default;
        [SerializeField]
        private string clientId = default;
        [SerializeField]
        private Sprite sprite = default;

        private int lastAddedScore;
        private int score;
        private bool connected = false;

        public void SetB11PartyServer(B11PartyServer b11PartyServer, int score) {
            this.b11PartyServer = b11PartyServer;
            this.score = score;
        }

        public Guid GetClientId() {
            return Guid.Parse(clientId);
        }

        public string GetName() {
            return name;
        }

        public void AddScore(int amount) {
            lastAddedScore = amount;
            score += amount;
            b11PartyServer.OnClientScoreChangedCallback(GetClientId(), score);
            BroadcastScoreToClients();
        }

        public void BroadcastScoreToClients() {
            if (connected) {
                b11PartyServer.GetKarmanServer().Broadcast(new ClientScoreChangedPacket(GetClientId(), score));
            }
        }

        public int GetLastAddedScore() {
            return lastAddedScore;
        }

        public int GetScore() {
            return score;
        }

        public void MarkConnected() {
            connected = true;
            SetPing(999);
        }

        public bool IsConnected() {
            return connected;
        }

        public void SetPing(int ping) {
            if (!connected) {
                log.Warning("Cannot set ping of client {0} since it is not connected (anymore)", clientId);
                return;
            }
            if (ping < 0) {
                connected = false;
            }
            b11PartyServer.OnClientPingChangedCallback(GetClientId(), ping);
        }

        public Sprite GetSprite() {
            return sprite;
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
        if (PlayerPrefs.HasKey(PLAYER_PREFS_MINIGAMES)) {
            string miniGameReloadString = PlayerPrefs.GetString(PLAYER_PREFS_MINIGAMES);
            log.Info("Reloading from player prefs: {0}", miniGameReloadString);
            string[] miniGamesThatAreNotCompleted = miniGameReloadString.Split(',');
            foreach (var miniGameInfo in miniGames) {
                if (!miniGamesThatAreNotCompleted.Contains(miniGameInfo.GetName())) {
                    miniGameInfo.MarkAsCompleted();
                }
            }
        }

        foreach (var client in clients) {
            string key = GetClientScorePlayerPrefKey(client.GetClientId());
            int score = int.Parse(PlayerPrefs.GetString(GetClientScorePlayerPrefKey(client.GetClientId()), "0"));
            client.SetB11PartyServer(this, score);
            clientsById.Add(client.GetClientId(), client);
        }

        karmanServer = new KarmanServer(GAME_ID);
        karmanServer.OnRunningCallback += () => serverStarted = true;
        karmanServer.OnShutdownCallback += () => log.Error("Server could not start.");
        karmanServer.OnClientJoinedCallback += (Guid clientId) => { };
        karmanServer.OnClientConnectedCallback += (Guid clientId) => {
            B11Client client = clients.FirstOrDefault(c => c.GetClientId().Equals(clientId));
            if (client == default(B11Client)) {
                log.Warning("A client with id {0} tried to connect, however that is not a known client id.", clientId);
                ThreadManager.ExecuteOnMainThread(() => karmanServer.Kick(clientId));
            }
            if (!lobbyPhase.gameObject.activeSelf) {
                log.Warning("A client with id {0} tried to connect, however the server is currently not in the LobbyPhases.", clientId);
                ThreadManager.ExecuteOnMainThread(() => karmanServer.Kick(clientId));
            }
            log.Warning("{0} joined!", client.GetName());
            client.MarkConnected();
            karmanServer.Send(client.GetClientId(), new LobbyStartedPacket(GetAvailableMiniGames()));
            client.BroadcastScoreToClients();
        };
        karmanServer.OnClientDisconnectedCallback += (Guid clientId) => {
            B11Client client = clients.FirstOrDefault(c => c.GetClientId().Equals(clientId));
            if (client != default(B11Client)) {
                log.Warning("{0} disconnected.", client.GetName());
                client.SetPing(-1);
            }
        };
        karmanServer.OnClientLeftCallback += (Guid clientId) => {
            B11Client client = clients.FirstOrDefault(c => c.GetClientId().Equals(clientId));
            if (client != default(B11Client)) {
                log.Warning("{0} left.", client.GetName());
                client.SetPing(-1);
            }
        };
        karmanServer.OnClientPackedReceivedCallback += OnClientPacketReceived;

        foreach (var monoBehaviour in new MonoBehaviour[] { lobbyPhase, miniGameLoadingPhase, miniGameReadyUpPhase, miniGamePlayingPhase, scoreOverviewPhase, trophyRoomPhase }) {
            monoBehaviour.gameObject.SetActive(false);
        }
    }

    private void OnClientPacketReceived(Guid clientId, Packet packet) {
        if (packet is PingResponsePacket pingResponsePacket) {
            OnClientPingResponsePacketReceived(clientId, pingResponsePacket);
        }
    }

    protected void OnDestroy() {
        karmanServer.Shutdown();
    }

    private string[] GetAvailableMiniGames() {
        return miniGames.Where(mg => !mg.IsCompleted()).Select(mg => mg.GetName()).ToArray();
    }

    protected IEnumerator Start() {
        log.Info("Starting server in {0} second(s).", startupDelay);
        yield return new WaitForSeconds(startupDelay);
        karmanServer.Start(DEFAULT_PORT);

        log.Info("Waiting for server to start on port {0}.", DEFAULT_PORT);
        OnPhaseChangedCallback(Phase.STARTING, this);
        while (!serverStarted) { yield return null; }
        log.Info("Server is up and running!");

        foreach (var client in clients) {
            OnClientScoreChangedCallback(client.GetClientId(), client.GetScore());
        }
        OnMiniGamesChangedCallback(miniGames);

        while (HasMiniGamesLeft()) {

            // Lobby Phase
            OnPhaseChangedCallback(Phase.LOBBY, lobbyPhase);
            log.Info("Moving to LobbyPhase.");
            lobbyPhase.gameObject.SetActive(true);
            lobbyPhase.Begin(GetAvailableMiniGames());
            karmanServer.Broadcast(new LobbyStartedPacket(GetAvailableMiniGames()));
            while (lobbyPhase.IsChoosingMiniGameInProgress()) { yield return null; }
            karmanServer.Broadcast(new LobbyEndedPacket());
            yield return new WaitForSeconds(0.5f);
            lobbyPhase.gameObject.SetActive(false);
            string chosenMiniGameName = lobbyPhase.GetChosenMiniGameName();
            MiniGameInfo chosenMiniGame = miniGames.First(mg => mg.GetName().Equals(chosenMiniGameName));

            // Sync scores after leaving the lobby
            foreach (var client in clients) {
                client.BroadcastScoreToClients();
            }

            // Mini Game Loading Phase
            OnPhaseChangedCallback(Phase.MINI_GAME_LOADING, miniGameLoadingPhase);
            log.Info("Moving to MiniGameLoadingPhase, since the '{0}' minigame was chosen.", chosenMiniGame.GetName());
            miniGameLoadingPhase.gameObject.SetActive(true);
            ServerMiniGame miniGame = miniGameLoadingPhase.Load(chosenMiniGame.GetName());
            karmanServer.Broadcast(new MiniGameLoadingStartedPacket(chosenMiniGame.GetName()));
            while (miniGameLoadingPhase.HasClientsLoading()) { yield return null; }
            karmanServer.Broadcast(new MiniGameLoadingEndedPacket());
            yield return new WaitForSeconds(0.5f);
            miniGameLoadingPhase.End();
            miniGameLoadingPhase.gameObject.SetActive(false);

            // Mini Game Ready Up Phase
            OnPhaseChangedCallback(Phase.MINI_GAME_READY_UP, miniGameReadyUpPhase);
            log.Info("Moving to MiniGameReadyUpPhase.");
            miniGameReadyUpPhase.gameObject.SetActive(true);
            miniGameReadyUpPhase.BeginReadyUpFor(miniGame);
            karmanServer.Broadcast(new MiniGameReadyUpStartedPacket());
            while (miniGameReadyUpPhase.IsWaitingForReadyUp()) { yield return null; }
            karmanServer.Broadcast(new MiniGameReadyUpEndedPacket());
            yield return new WaitForSeconds(0.5f);
            miniGameReadyUpPhase.End();
            miniGameReadyUpPhase.gameObject.SetActive(false);

            // Mini Game Playing Phase
            OnPhaseChangedCallback(Phase.MINI_GAME_PLAYING, miniGamePlayingPhase);
            log.Info("Moving to MiniGamePlayingPhase.");
            miniGamePlayingPhase.gameObject.SetActive(true);
            miniGamePlayingPhase.BeginPlayingFor(miniGame);
            karmanServer.Broadcast(new MiniGamePlayingStartedPacket());
            while (miniGamePlayingPhase.IsAClientStillPlaying()) { yield return null; }
            karmanServer.Broadcast(new MiniGamePlayingEndedPacket());
            yield return new WaitForSeconds(1.0f);
            foreach (var client in clients) {
                client.AddScore(miniGamePlayingPhase.GetScore(client.GetClientId()));
                OnClientScoreChangedCallback(client.GetClientId(), client.GetScore());
            }
            chosenMiniGame.MarkAsCompleted();
            OnMiniGamesChangedCallback(miniGames);
            SaveStateToPlayerPrefs();
            miniGamePlayingPhase.End();
            miniGamePlayingPhase.gameObject.SetActive(false);
            miniGame.OnUnload();
            Destroy(miniGame.gameObject);

            // Score Overview Phase
            if (HasMiniGamesLeft()) {
                log.Info("Moving to ScoreOverviewPhase.");
                OnPhaseChangedCallback(Phase.SCORE_OVERVIEW, scoreOverviewPhase);
                scoreOverviewPhase.gameObject.SetActive(true);
                scoreOverviewPhase.Show(clients);
                karmanServer.Broadcast(new ScoreOverviewStartedPacket(GetScoreOverviewInformation()));
                while (!scoreOverviewPhase.IsDone()) { yield return null; }
                karmanServer.Broadcast(new ScoreOverviewEndedPacket());
                yield return new WaitForSeconds(0.5f);
                scoreOverviewPhase.End();
                scoreOverviewPhase.gameObject.SetActive(false);
            }
        }

        // Trophy Room Phase
        log.Info("Moving to TrophyRoomPhase, since all minigames are done.");
        OnPhaseChangedCallback(Phase.TROPHY_ROOM, trophyRoomPhase);
        trophyRoomPhase.gameObject.SetActive(true);
        trophyRoomPhase.Begin(clients);
        karmanServer.Broadcast(new TrophyRoomStartedPacket(GetTrophyRoomInformation()));
    }

    private ScoreOverviewStartedPacket.ScoreOverviewInformation[] GetScoreOverviewInformation() {
        return clients
            .Select(client => new ScoreOverviewStartedPacket.ScoreOverviewInformation(
                client.GetClientId(),
                client.GetLastAddedScore()
            ))
            .ToArray();
    }

    private TrophyRoomStartedPacket.TrophyRoomInformation[] GetTrophyRoomInformation() {
        return clients
            .Select(client => new TrophyRoomStartedPacket.TrophyRoomInformation(
                client.GetClientId(),
                client.GetScore()
            ))
            .ToArray();
    }

    public class PingMoment {
        private readonly Guid pingId;
        private readonly float sendMoment;
        private int numberOfPingsInProgress = 0;

        public PingMoment() {
            pingId = Guid.NewGuid();
            sendMoment = Time.realtimeSinceStartup;
        }

        public PingPacket GetPingPacket() {
            numberOfPingsInProgress++;
            return new PingPacket(pingId);
        }

        public bool ResolveOne(out int ping) {
            ping = Mathf.CeilToInt((Time.realtimeSinceStartup - sendMoment) * 1000);
            return --numberOfPingsInProgress <= 0;
        }

        public Guid GetPingId() {
            return pingId;
        }

        public int GetNumberOfPingsInProgress() {
            return numberOfPingsInProgress;
        }
    }

    private readonly Dictionary<Guid, PingMoment> pingMoments = new Dictionary<Guid, PingMoment>();
    private float nextPingMoment;
    protected void FixedUpdate() {
        if (nextPingMoment <= Time.realtimeSinceStartup) {
            nextPingMoment += pingInterval;
            PingMoment pingMoment = new PingMoment();
            pingMoments.Add(pingMoment.GetPingId(), pingMoment);
            foreach (var client in clients) {
                if (client.IsConnected()) {
                    karmanServer.Send(client.GetClientId(), pingMoment.GetPingPacket());
                }
            }
            if (pingMoment.GetNumberOfPingsInProgress() == 0) {
                pingMoments.Remove(pingMoment.GetPingId());
                log.Trace("Ping skipped since no clients are connected");
            } else {
                log.Trace("Send ping packet with ping id {0} to {1} client(s)", pingMoment.GetPingId(), pingMoment.GetNumberOfPingsInProgress());
            }
        }
    }

    private void OnClientPingResponsePacketReceived(Guid clientId, PingResponsePacket pingResponsePacket) {
        Guid pingId = pingResponsePacket.GetPingId();
        if (pingMoments[pingId].ResolveOne(out int ping)) {
            pingMoments.Remove(pingId);
        }
        log.Trace("Client {0} has a latency with the server of {1} milliseconds.", clientId, ping);
        clientsById[clientId].SetPing(ping);
    }

    private static readonly string PLAYER_PREFS_MINIGAMES = "b11-minigames";
    private static readonly string PLAYER_PREFS_CLIENT = "b11-client";
    private float saveTime = 5f;
    private float escapeTime = 1.2f;
    protected void Update() {
        if (Input.GetKey(KeyCode.Escape)) {
            escapeTime -= Time.deltaTime;
            if (escapeTime < 0f) {
                ServerReload(Input.GetKey(KeyCode.R));
            }
        } else {
            escapeTime = 1.2f;
        }
        saveTime -= Time.deltaTime;
        if (saveTime < 0f) {
            saveTime += 5f;
            SaveStateToPlayerPrefs();
        }
        if (Input.GetKeyDown(KeyCode.I)) {
            foreach (var client in clients) {
                client.AddScore(1);
            }
        }
    }

    private void ServerReload(bool reset = false) {
        SaveStateToPlayerPrefs();
        if (reset) {
            PlayerPrefs.DeleteAll();
        }
        SceneManager.LoadScene("Server");
    }

    private void SaveStateToPlayerPrefs() {
        PlayerPrefs.SetString(PLAYER_PREFS_MINIGAMES, string.Join(",", GetAvailableMiniGames()));
        foreach (var client in clients) {
            string key = GetClientScorePlayerPrefKey(client.GetClientId());
            string value = client.GetScore().ToString();
            PlayerPrefs.SetString(key, value);
        }
        PlayerPrefs.Save();
    }

    private static string GetClientScorePlayerPrefKey(Guid clientId) {
        return string.Format("{0}-{1}", PLAYER_PREFS_CLIENT, clientId.ToString().Substring(4));
    }
}