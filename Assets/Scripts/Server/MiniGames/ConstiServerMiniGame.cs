using Networking;
using System;
using System.Collections.Generic;
using UnityEngine;
using CharacterSpawn = ConstiCharacterSpawnsPacket.CharacterSpawn;

public class ConstiServerMiniGame : ServerMiniGame {
    public static readonly float IntroDuration = 3.5f;
    public static readonly float ChasingDuration = 10f;

    [SerializeField]
    private int maxScore = 111;
    [SerializeField]
    private ConstiMap map;
    [SerializeField]
    private float coinInterval = 5f;
    [SerializeField]
    private float blockInterval = 5f;
    [SerializeField]
    private float powerupInterval = 10f;

    private B11PartyServer b11PartyServer;
    private bool isPlaying;
    private bool isIntro = true;
    private float introTime;
    private bool maxScoreWasReached;

    private LinkedList<int> coinSpawns;
    private float coinTime = 0f;
    private LinkedList<int> blockSpawns;
    private float blockTime = 0f;
    private LinkedList<int> powerupSpawns;
    private float powerupTime = 0f;

    public override void OnLoad(B11PartyServer b11PartyServer) {
        this.b11PartyServer = b11PartyServer;
        this.b11PartyServer.GetKarmanServer().OnClientPackedReceivedCallback += OnPacket;

        // Coins
        int numberOfCoins = map.GetCoins().childCount;
        int[] coins = new int[numberOfCoins];
        for (int coinIndex = 0; coinIndex < numberOfCoins; coinIndex++) {
            coins[coinIndex] = coinIndex;
        }
        Shuffle(coins);
        coinSpawns = new LinkedList<int>(coins);

        // Blocks
        int numberOfBlocks = map.GetBlocks().childCount;
        int[] blocks = new int[numberOfBlocks];
        for (int blockIndex = 0; blockIndex < numberOfBlocks; blockIndex++) {
            blocks[blockIndex] = blockIndex;
        }
        Shuffle(blocks);
        blockSpawns = new LinkedList<int>(blocks);

        // Powerups
        int numberOfPowerups = map.GetPowerups().childCount;
        int[] powerups = new int[numberOfPowerups];
        for (int powerupIndex = 0; powerupIndex < numberOfPowerups; powerupIndex++) {
            powerups[powerupIndex] = powerupIndex;
        }
        Shuffle(powerups);
        powerupSpawns = new LinkedList<int>(powerups);
    }

    protected void Update() {
        if (!isPlaying || maxScoreWasReached) {
            return;
        }

        // Coins
        coinTime += Time.deltaTime;
        if (coinTime >= coinInterval) {
            coinTime = 0f;
            TrySpawnCoin();
        }

        // Blocks
        blockTime += Time.deltaTime;
        if (blockTime >= blockInterval) {
            blockTime = 0f;
            TrySpawnBlock();
        }

        // Powerup
        powerupTime += Time.deltaTime;
        if (powerupTime >= powerupInterval) {
            powerupTime = 0f;
            TrySpawnPowerup();
        }

        // Enemies
        foreach (Transform enemyTransform in map.GetEnemies()) {
            b11PartyServer.GetKarmanServer().Broadcast(new ConstiEnemyUpdatedPacket(enemyTransform.GetSiblingIndex(), enemyTransform.localPosition));
        }

        // Start Enemies after intro
        introTime += Time.deltaTime;
        if (isIntro && introTime > IntroDuration) {
            isIntro = false;
            foreach (Transform enemyTransform in map.GetEnemies()) {
                var enemy = enemyTransform.GetComponent<ConstiEnemy>();
                enemy.RunAI();
            }
        }
    }

    private void TrySpawnCoin() {
        if (coinSpawns.Count > 0) {
            int coinIndex = coinSpawns.First.Value;
            coinSpawns.RemoveFirst();
            map.GetCoins().GetChild(coinIndex).gameObject.SetActive(true);
            b11PartyServer.GetKarmanServer().Broadcast(new ConstiCoinUpdatedPacket(coinIndex, true));
        }
    }

    private void TrySpawnBlock() {
        if (blockSpawns.Count > 0) {
            int blockIndex = blockSpawns.First.Value;
            int switchIndex = UnityEngine.Random.Range(0, 4);
            blockSpawns.RemoveFirst();
            map.GetBlocks().GetChild(blockIndex).gameObject.SetActive(true);
            b11PartyServer.GetKarmanServer().Broadcast(new ConstiBlockEnabledPacket(blockIndex, switchIndex));
        }
    }

    private void TrySpawnPowerup() {
        if (powerupSpawns.Count > 0) {
            int powerupIndex = powerupSpawns.First.Value;
            powerupSpawns.RemoveFirst();
            map.GetPowerups().GetChild(powerupIndex).gameObject.SetActive(true);
            b11PartyServer.GetKarmanServer().Broadcast(new ConstiPowerupUpdatedPacket(powerupIndex, true));
        }
    }

    private void OnPacket(Guid clientId, Packet packet) {
        if (maxScoreWasReached) {
            return;
        }

        if (packet is ConstiCharacterUpdatedPacket characterUpdated) {
            if (clientId.Equals(characterUpdated.GetClientId())) {
                b11PartyServer.GetKarmanServer().Broadcast(characterUpdated, clientId);
            }
            return;
        } else if (packet is ConstiCoinUpdatedPacket coinUpdated) {
            int coinIndex = coinUpdated.GetCoinIndex();
            if (!coinSpawns.Contains(coinIndex)) {
                b11PartyServer.GetMiniGamePlayingPhase().AddScore(clientId, 3);
                coinSpawns.AddLast(coinIndex);
                map.GetCoins().GetChild(coinIndex).gameObject.SetActive(false);
                b11PartyServer.GetKarmanServer().Broadcast(coinUpdated);
                if (coinSpawns.Count == 1) {
                    coinTime = 0f;
                }
            }
        } else if (packet is ConstiBlockDisabledPacket blockDisabled) {
            int blockIndex = blockDisabled.GetBlockIndex();
            if (!blockSpawns.Contains(blockIndex)) {
                b11PartyServer.GetMiniGamePlayingPhase().AddScore(clientId, 1);
                blockSpawns.AddLast(blockIndex);
                map.GetBlocks().GetChild(blockIndex).gameObject.SetActive(false);
                b11PartyServer.GetKarmanServer().Broadcast(blockDisabled);
                if (blockSpawns.Count == 1) {
                    blockTime = 0f;
                }
            }
        } else if (packet is ConstiPowerupUpdatedPacket powerupUpdated) {
            int powerupIndex = powerupUpdated.GetPowerupIndex();
            if (!powerupSpawns.Contains(powerupIndex)) {
                powerupSpawns.AddLast(powerupIndex);
                map.GetPowerups().GetChild(powerupIndex).gameObject.SetActive(false);
                b11PartyServer.GetKarmanServer().Broadcast(powerupUpdated);
                b11PartyServer.GetKarmanServer().Broadcast(new ConstiCharacterChasingPacket(clientId));
                if (powerupSpawns.Count == 1) {
                    powerupTime = 0f;
                }
            }
        } else if (packet is ConstiEnemyEatenPacket enemyEaten) {
            int enemyIndex = enemyEaten.GetEnemyIndex();
            var enemy = map.GetEnemies().GetChild(enemyIndex).GetComponent<ConstiEnemy>();
            if (!enemy.IsDead()) {
                enemy.OnEaten();
                b11PartyServer.GetMiniGamePlayingPhase().AddScore(clientId, 11);
                b11PartyServer.GetKarmanServer().Broadcast(enemyEaten);
            }
        }

        if (isPlaying) {
            if (b11PartyServer.GetMiniGamePlayingPhase().GetScore(clientId) >= maxScore) {
                maxScoreWasReached = true;
                b11PartyServer.GetKarmanServer().Broadcast(new ConstiMaxScoreReachedPacket());
            }
        }
    }

    public override void BeginReadyUp() {
        List<CharacterSpawn> spawns = new List<CharacterSpawn>();
        int[] spawnIndices = Shuffle(new int[] { 0, 1, 2, 3, 4, 5 });
        var clients = b11PartyServer.GetClients();
        for (int index = 0; index < clients.Count; index++) {
            spawns.Add(new CharacterSpawn(clients[index].GetClientId(), spawnIndices[index]));
        }
        b11PartyServer.GetKarmanServer().Broadcast(new ConstiCharacterSpawnsPacket(spawns.ToArray()));

        // Spawn 12 coins, 6 blocks, and 1 powerup
        for (int i = 0; i < 6; i++) {
            TrySpawnCoin();
            TrySpawnCoin();
            TrySpawnBlock();
        }
        TrySpawnPowerup();
    }

    public override void EndReadyUp() {
    }

    public override void BeginPlaying() {
        isPlaying = true;
    }

    public override void EndPlaying() {
        isPlaying = false;
    }

    public override void OnUnload() {
        b11PartyServer.GetKarmanServer().OnClientPackedReceivedCallback -= OnPacket;
    }

    public static T[] Shuffle<T>(T[] input) {
        int m = input.Length;
        while (m > 0) {
            int i = UnityEngine.Random.Range(0, m--);
            T t = input[m];
            input[m] = input[i];
            input[i] = t;
        }
        return input;
    }
}