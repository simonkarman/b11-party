using KarmanProtocol;
using Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class B11PartyServer : MonoBehaviour {
    public static readonly int DEFAULT_PORT = 14641;
    public static readonly Guid GAME_ID = Guid.Parse("11117d77-6145-4732-b30a-fd6f4812e251");
    private static readonly Logging.Logger log = Logging.Logger.For<B11PartyServer>();

    private bool serverStarted = false;
    private KarmanServer karmanServer;

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

        private int score;
        private bool connected = false;

        public void SetB11PartyServer(B11PartyServer b11PartyServer) {
            this.b11PartyServer = b11PartyServer;
        }

        public Guid GetClientId() {
            return Guid.Parse(clientId);
        }

        public string GetName() {
            return name;
        }

        public void AddScore(int amount) {
            score += amount;
            b11PartyServer.OnClientScoreChangedCallback(GetClientId(), score);
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
        foreach (var client in clients) {
            client.SetB11PartyServer(this);
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
            log.Warning("{0} joined!", client.GetName());
            client.MarkConnected();
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

    protected IEnumerator Start() {
        log.Info("Starting server in {0} second(s).", startupDelay);
        yield return new WaitForSeconds(startupDelay);
        karmanServer.Start(DEFAULT_PORT);

        log.Info("Waiting for server to start on port {0}.", DEFAULT_PORT);
        OnPhaseChangedCallback(Phase.STARTING, this);
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
}