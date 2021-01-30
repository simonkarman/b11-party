using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class KelderBorrelServerMiniGame : ServerMiniGame {
    private static readonly Logging.Logger log = Logging.Logger.For<KelderBorrelServerMiniGame>();

    private class ClientIdRandomizer {
        private readonly B11PartyServer b11PartyServer;
        private readonly LinkedList<Guid> clientIds = new LinkedList<Guid>();

        private void Shuffle<T>(T[] input) {
            int m = input.Length;
            while (m > 0) {
                int i = Random.Range(0, m--);
                T t = input[m];
                input[m] = input[i];
                input[i] = t;
            }
        }

        public ClientIdRandomizer(B11PartyServer b11PartyServer) {
            this.b11PartyServer = b11PartyServer;
        }

        public Guid GetNext(int depth = 10) {
            if (depth <= 0) {
                log.Error("Infinite loop occurred...");
                return Guid.Empty;
            }

            if (clientIds.Count == 0) {
                Guid[] guids = b11PartyServer.GetClients()
                    .Select(client => client.GetClientId())
                    .Where(clientId => b11PartyServer.GetMiniGamePlayingPhase().IsClientStillPlaying(clientId))
                    .ToArray();
                Shuffle(guids);
                for (int i = 0; i < guids.Length; i++) {
                    clientIds.AddFirst(guids[i]);
                }
            }
            Guid id = clientIds.First.Value;
            clientIds.RemoveFirst();
            if (!b11PartyServer.GetMiniGamePlayingPhase().IsClientStillPlaying(id)) {
                return GetNext(depth - 1);
            }
            return id;
        }
    }

    private class BlockInfo  {
        private readonly int hits;
        private readonly List<Guid> clients;

        public BlockInfo(int hits) {
            this.hits = hits;
            clients = new List<Guid>();
        }

        public BlockInfo(List<Guid> clients) {
            this.clients = clients;
            hits = -1;
        }

        public int TryHitBy(Guid clientId) {
            if (hits > 0) {
                // Every client can hit it ONCE until it has been hit 'hits' times
                if (clients.Contains(clientId) || clients.Count >= hits) {
                    return 0;
                }
                int score = hits - clients.Count;
                clients.Add(clientId);
                return score;

            } else {
                // Only clients in the clients list can hit it until that list is empty
                int index = clients.IndexOf(clientId);
                if (index == -1) {
                    return 0;
                }
                int score = clients.Count;
                clients.RemoveAt(index);
                return score;
            }
        }

        public bool IsDone() {
            if (hits > 0) {
                // Every client can hit it ONCE until it has been hit 'hits' times
                return clients.Count >= hits;
            } else {
                // Only clients in the clients list can hit it until that list is empty
                return clients.Count == 0;
            }
        }
    }

    private B11PartyServer b11PartyServer;
    private ClientIdRandomizer clientIdRandomizer;

    private readonly Dictionary<Guid, BlockInfo> blocks = new Dictionary<Guid, BlockInfo>();
    private readonly int lineWidth = 8;
    private int currentLineNumber = 0;
    private readonly float spawnDuration = 10f;
    private float spawnTime;

    public override void OnLoad(B11PartyServer b11PartyServer) {
        this.b11PartyServer = b11PartyServer;
        this.b11PartyServer.GetKarmanServer().OnClientPackedReceivedCallback += OnPacket;
        clientIdRandomizer = new ClientIdRandomizer(b11PartyServer);
    }

    private void OnPacket(Guid clientId, Packet packet) {
        if (packet is KelderBorrelBallUpdatedPacket) {
            b11PartyServer.GetKarmanServer().Broadcast(packet, clientId);
        } else if (packet is KelderBorrelBlockHitPacket blockHit) {
            if (!blocks.TryGetValue(blockHit.GetBlockId(), out BlockInfo block)) {
                return;
            }
            int hitScore = block.TryHitBy(clientId);
            if (hitScore > 0) {
                var playingPhase = b11PartyServer.GetMiniGamePlayingPhase();
                playingPhase.AddScore(clientId, hitScore);
                b11PartyServer.GetKarmanServer().Broadcast(new KelderBorrelBlockUpdatedPacket(
                    blockHit.GetBlockId(), clientId, hitScore
                ));
            }
        } else if (packet is MiniGamePlayingFinishedPacket finishedPacket) {
            if (finishedPacket.GetClientId() != clientId) {
                return;
            }
            // If a player is finished,
            //   then try to hit every block that that player could still hit,
            //   so that the other players can continue playing.
            foreach (var blockKvp in blocks) {
                var block = blockKvp.Value;
                if (block.IsDone()) {
                    continue;
                }
                int hitScore = block.TryHitBy(clientId);
                if (hitScore <= 0) {
                    continue;
                }
                b11PartyServer.GetKarmanServer().Broadcast(new KelderBorrelBlockUpdatedPacket(
                    blockKvp.Key, clientId, 0
                ));
            }
        }
    }

    public override void BeginReadyUp() {
    }

    public override void EndReadyUp() {
    }

    public override void BeginPlaying() {
        for (int i = 0; i < 3; i++) {
            SpawnLineOfBlocks();
        }
    }

    private void SpawnLineOfBlocks() {
        if (!b11PartyServer.GetMiniGamePlayingPhase().IsAClientStillPlaying()) {
            return;
        }
        int maxAmount = b11PartyServer.GetMiniGamePlayingPhase().GetNumberOfClientsStillPlaying();
        float r = Mathf.Max(0.2f, 0.5f - (currentLineNumber / 100f));
        for (int x = 0; x < lineWidth; x++) {
            if (Random.value < r) {
                continue;
            }
            Guid blockId = Guid.NewGuid();
            KelderBorrelBlockPosition position = new KelderBorrelBlockPosition(currentLineNumber, x);
            BlockInfo blockInfo;
            KelderBorrelBlockSpawnedPacket packet;
            if (currentLineNumber <= 3 || Random.value < 0.7f) {
                int hits = Mathf.Min(maxAmount, Random.Range(1, 4) + (currentLineNumber > 3 ? Random.Range(0, Mathf.Max(0, maxAmount - 2)) : 0));
                blockInfo = new BlockInfo(hits);
                packet = new KelderBorrelBlockSpawnedPacket(blockId, position, hits);
            } else {
                int numberOfClients = Mathf.Min(maxAmount, Mathf.Clamp(Mathf.FloorToInt(Mathf.Pow(Random.value, 2.6f) * 4f) + 1, 1, 4));
                List<Guid> clients = new List<Guid>();
                for (int i = 0; i < numberOfClients; i++) {
                    clients.Add(clientIdRandomizer.GetNext());
                }
                blockInfo = new BlockInfo(clients);
                packet = new KelderBorrelBlockSpawnedPacket(blockId, position, clients.ToArray());
            }
            blocks.Add(blockId, blockInfo);
            b11PartyServer.GetKarmanServer().Broadcast(packet);
        }
        currentLineNumber++;
    }

    public override void EndPlaying() {
    }

    public override void OnUnload() {
        b11PartyServer.GetKarmanServer().OnClientPackedReceivedCallback -= OnPacket;
    }

    protected void Update() {
        spawnTime -= Time.deltaTime;
        if (spawnTime <= 0f) {
            spawnTime += spawnDuration;
            SpawnLineOfBlocks();
        }
    }
}