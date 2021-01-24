using Networking;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using LaneType = BussenLaneSpawnedPacket.LaneType;

public class BussenServerMiniGame : ServerMiniGame {
    private B11PartyServer b11PartyServer;

    private readonly int numberOfLanes = 8;
    private int laneIndex = 0;
    private float currentLaneInterval = 2.25f;
    private float durationUntilNextLane = 1f;
    private bool isPlaying = false;

    // Noise Based Lane Type
    /*private bool noiseInit = true;
    private Vector2 noiseStart;
    private Vector2 noiseDirection;

    public LaneType GetCurrentLaneType() {
        if (noiseInit) {
            noiseInit = false;
            noiseStart = Random.insideUnitCircle * 1000f;
            noiseDirection = Random.insideUnitCircle.normalized;
        }
        Vector2 location = noiseStart + noiseDirection * laneIndex * 0.17f;
        float t = Mathf.Clamp01(Mathf.PerlinNoise(location.x, location.y));
        if (t < 0.33f) {
            return LaneType.Road;
        } else if (t < 0.66f) {
            return LaneType.Grass;
        } else {
            return LaneType.Water;
        }
    }*/

    // Segment Based Lane Type
    private LaneType current;
    private int numberOfLaneTypeFollowing = 0;
    public LaneType UpdateCurrentLaneType() {
        if (numberOfLaneTypeFollowing-- >= 0) {
            return current;
        }
        // Always add grass after each other lane type (after lane 100 decrease grass size)
        if (current != LaneType.Grass) {
            current = LaneType.Grass;
            numberOfLaneTypeFollowing = 1 + Random.Range(0, 4) + (laneIndex < 100 ? Random.Range(0, 3) : 0);
            return current;
        }
        // Ensure only small roads at the start
        if (laneIndex < 30) {
            numberOfLaneTypeFollowing = Random.Range(0, 2);
            current = LaneType.Road;
            return current;
        }
        // After the start, pick one at random (after lane 60, increase difficulty):
        float r = Random.value + (laneIndex > 60 ? Random.value * 0.2f : 0f);
        // 45% (25% later) chance on a small road
        if (r < 0.45f) {
            numberOfLaneTypeFollowing = Random.Range(0, 3);
            current = LaneType.Road;
            return current;
        }
        // 25% chance on a small river
        if (r < 0.70f) {
            numberOfLaneTypeFollowing = Random.Range(0, 2);
            current = LaneType.Water;
            return current;
        }
        // 20% chance on a large road
        if (r < 0.90f) {
            numberOfLaneTypeFollowing = Random.Range(3, 6);
            current = LaneType.Road;
            return current;
        }
        // 10% chance on a large river
        if (r < 1.00f) {
            numberOfLaneTypeFollowing = Random.Range(2, 5);
            current = LaneType.Water;
            return current;
        }
        // 0% (20% later) chance on lava river
        numberOfLaneTypeFollowing = 3;
        current = LaneType.Lava;
        return current;
    }

    public override void OnLoad(B11PartyServer b11PartyServer) {
        this.b11PartyServer = b11PartyServer;
        this.b11PartyServer.GetKarmanServer().OnClientPackedReceivedCallback += OnPacket;
        isPlaying = false;
    }

    private void OnPacket(Guid clientId, Packet packet) {
        if (packet is BussenCharacterUpdatedPacket) {
            b11PartyServer.GetKarmanServer().Broadcast(packet, clientId);
        }
    }

    public override void BeginReadyUp() {
        for (int index = 0; index < (numberOfLanes - 2); index++) {
            b11PartyServer.GetKarmanServer().Broadcast(new BussenLaneSpawnedPacket(
                index, LaneType.Grass, Random.Range(int.MinValue, int.MaxValue), 3, 1f
            ));
            laneIndex++;
        }
        b11PartyServer.GetKarmanServer().Broadcast(new BussenLastLaneUpdatedPacket(0));
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

    protected void Update() {
        if (isPlaying) {
            durationUntilNextLane -= Time.deltaTime;
            if (durationUntilNextLane < 0) {
                durationUntilNextLane += currentLaneInterval;

                b11PartyServer.GetKarmanServer().Broadcast(new BussenLaneSpawnedPacket(
                     laneIndex,
                     UpdateCurrentLaneType(),
                     Random.Range(int.MinValue, int.MaxValue),
                     Random.Range(2, 4 + (laneIndex / 111)) + (current == LaneType.Road ? Random.Range(0, 2) : 0),
                     Random.Range(1f, 1.4f + (laneIndex * 0.005f)) + (current == LaneType.Lava ? laneIndex * 0.005f : 0f)
                ));

                int lastLaneIndex = laneIndex - numberOfLanes;
                if (lastLaneIndex > 0) {
                    b11PartyServer.GetKarmanServer().Broadcast(new BussenLastLaneUpdatedPacket(lastLaneIndex));
                }

                currentLaneInterval -= 0.035f;
                if (currentLaneInterval < 0.6f) {
                    currentLaneInterval = 0.6f;
                }
                laneIndex++;
            }
        }
    }
}
