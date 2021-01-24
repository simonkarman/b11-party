using Networking;
using System;
using UnityEngine;

public class BussenServerMiniGame : ServerMiniGame {
    private B11PartyServer b11PartyServer;

    private readonly int numberOfLanes = 7;
    private int laneIndex = 0;
    private float currentLaneInterval = 2.5f;
    private float durationUntilNextLane = 3f;
    private bool isPlaying = false;

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
                index, BussenLaneSpawnedPacket.LaneType.GRASS, UnityEngine.Random.Range(int.MinValue, int.MaxValue), 3, 1f
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
                     BussenLaneSpawnedPacket.LaneType.ROAD,
                     UnityEngine.Random.Range(int.MinValue, int.MaxValue),
                     UnityEngine.Random.Range(3, 7),
                     UnityEngine.Random.Range(1f, 1.4f)
                ));

                int lastLaneIndex = laneIndex - numberOfLanes;
                if (lastLaneIndex > 0) {
                    b11PartyServer.GetKarmanServer().Broadcast(new BussenLastLaneUpdatedPacket(lastLaneIndex));
                }

                currentLaneInterval -= 0.07f;
                if (currentLaneInterval < 0.5f) {
                    currentLaneInterval = 0.5f;
                }
                laneIndex++;
            }
        }
    }
}
