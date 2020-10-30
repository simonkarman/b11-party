using Networking;
using System;
using UnityEngine;

public class BatavierenServerMiniGame : ServerMiniGame {
    private static readonly Logging.Logger log = Logging.Logger.For<BatavierenServerMiniGame>();

    private B11PartyServer b11PartyServer;

    [SerializeField]
    private float startSpeed = 1f;
    [SerializeField]
    private float acceleration = 0.1f;
    [SerializeField]
    private float airChance = 0.2f;
    [SerializeField]
    private float startSpawnInterval = 3f;
    [SerializeField]
    private float spawnIntervalRandomness = 0.4f;
    [SerializeField]
    private float spawnIntervalChangeRate = -0.1f;

    private float currentSpeed;
    private float currentSpawnInterval;
    private float durationUntilNextSpawn;
    private bool isPlaying;

    public override void OnLoad(B11PartyServer b11PartyServer) {
        this.b11PartyServer = b11PartyServer;
        this.b11PartyServer.GetKarmanServer().OnClientPackedReceivedCallback += OnPacket;
        isPlaying = false;
    }

    private void OnPacket(Guid clientId, Packet packet) {
        if (packet is BatavierenCharacterUpdatedPacket) {
            b11PartyServer.GetKarmanServer().Broadcast(packet, clientId);
        }
    }

    public override void BeginReadyUp() {
    }

    public override void EndReadyUp() {
    }

    public override void BeginPlaying() {
        currentSpeed = startSpeed;
        currentSpawnInterval = startSpawnInterval;
        isPlaying = true;
    }

    public override void EndPlaying() {
        isPlaying = false;
    }

    public override void OnUnload() {
    }

    protected void Update() {
        if (isPlaying) {

            durationUntilNextSpawn -= Time.deltaTime;
            if (durationUntilNextSpawn < 0) {
                durationUntilNextSpawn += currentSpawnInterval;
                durationUntilNextSpawn += spawnIntervalRandomness * UnityEngine.Random.value;
                BatavierenObstacleSpawnedPacket.Mode mode = UnityEngine.Random.value < airChance
                    ? BatavierenObstacleSpawnedPacket.Mode.AIR
                    : BatavierenObstacleSpawnedPacket.Mode.GROUND;
                log.Info("Spawning a {0} obstacle with a speed of {1}", mode, currentSpeed);
                b11PartyServer.GetKarmanServer().Broadcast(new BatavierenObstacleSpawnedPacket(
                     mode,
                     currentSpeed
                ));

                currentSpeed += acceleration;
                currentSpawnInterval += spawnIntervalChangeRate;
                if (currentSpawnInterval <= 0.7f) {
                    currentSpawnInterval = 0.9f;
                }
            }
        }
    }
}