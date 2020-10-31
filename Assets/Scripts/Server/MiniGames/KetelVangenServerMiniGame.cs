using Networking;
using System;
using UnityEngine;

public class KetelVangenServerMiniGame : ServerMiniGame {
    private B11PartyServer b11PartyServer;

    [SerializeField]
    private float startSpeed = 1f;
    [SerializeField]
    private float acceleration = 0.1f;
    [SerializeField]
    private float ketelMatuurChance = 0.2f;
    [SerializeField]
    private float smirnoffIceChance = 0.2f;
    [SerializeField]
    private float startSpawnInterval = 0.3f;
    [SerializeField]
    private float spawnIntervalRandomness = 0.4f;

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
        if (packet is KetelVangenCharacterUpdatedPacket) {
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
                KetelVangenSpawnedPacket.SpawnType spawnType = KetelVangenSpawnedPacket.SpawnType.KETEL_1;
                float randomValue = UnityEngine.Random.value;
                if (randomValue > (1 - smirnoffIceChance)) {
                    spawnType = KetelVangenSpawnedPacket.SpawnType.SMIRNOFF_ICE;
                } else if (randomValue > (1 - smirnoffIceChance - ketelMatuurChance)) {
                    spawnType = KetelVangenSpawnedPacket.SpawnType.KETEL_1_MATUUR;
                }
                b11PartyServer.GetKarmanServer().Broadcast(new KetelVangenSpawnedPacket(
                     spawnType,
                     UnityEngine.Random.value,
                     currentSpeed
                ));

                currentSpeed += acceleration;
                if (currentSpeed > 6f) {
                    currentSpeed = 3f;
                }
            }
        }
    }
}