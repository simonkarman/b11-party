using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class B11BalloonServerMiniGame : ServerMiniGame {
    private static readonly Logging.Logger log = Logging.Logger.For<B11BalloonServerMiniGame>();
    private B11PartyServer b11PartyServer;

    private readonly LinkedList<Guid> order = new LinkedList<Guid>();
    private bool isCoutingDownForNextRound;
    private float countingDownTime;
    private readonly float countingDownDuration = 3f;

    private void Shuffle<T>(T[] input) {
        int m = input.Length;
        while (m > 0) {
            int i = Random.Range(0, m--);
            T t = input[m];
            input[m] = input[i];
            input[i] = t;
        }
    }

    public override void OnLoad(B11PartyServer b11PartyServer) {
        this.b11PartyServer = b11PartyServer;
        b11PartyServer.GetKarmanServer().OnClientPackedReceivedCallback += OnPacket;
    }

    public override void BeginReadyUp() {
        Guid[] order = b11PartyServer.GetClients()
            .Where(client => client.IsConnected())
            .Select(client => client.GetClientId())
            .ToArray();
        Shuffle(order);
        b11PartyServer.GetKarmanServer().Broadcast(new B11BalloonOrderPacket(order));
        foreach (var clientId in order) {
            this.order.AddLast(clientId);
        }
    }

    public override void EndReadyUp() {
    }

    private void OnPacket(Guid clientId, Packet packet) {
        if (packet is B11BalloonShiftPacket) {
            if (clientId != order.First.Value) {
                log.Warning("Client {0} just sent a {1}, however that client is not at the button right now, so the packet is ignored.", clientId, packet.GetType().Name);
                return;
            }
            b11PartyServer.GetKarmanServer().Broadcast(packet);
            order.AddLast(order.First.Value);
            order.RemoveFirst();
        } else if (packet is B11BalloonInflatePacket) {
            if (clientId != order.First.Value) {
                log.Warning("Client {0} just sent a {1}, however that client is not at the button right now, so the packet is ignored.", clientId, packet.GetType().Name);
                return;
            }
            // Give all players still in the order 1 point, but the player pressing the button 3 instead.
            foreach (var clientIdInQueue in order) {
                int amount = clientIdInQueue == clientId ? 3 : 1;
                b11PartyServer.GetMiniGamePlayingPhase().AddScore(clientIdInQueue, amount);
            }
            b11PartyServer.GetKarmanServer().Broadcast(packet);
        } else if (packet is B11BalloonPoppedPacket) {
            if (clientId != order.First.Value) {
                log.Warning("Client {0} just sent a {1}, however that client is not at the button right now, so the packet is ignored.", clientId, packet.GetType().Name);
                return;
            }
            order.RemoveFirst();
            b11PartyServer.GetKarmanServer().Broadcast(packet);
            isCoutingDownForNextRound = order.Count > 1;
            countingDownTime = 0f;

            // If we're now not couting down, this means there is a last person standing
            // Add 11 bonus points to that person
            if (!isCoutingDownForNextRound) {
                b11PartyServer.GetMiniGamePlayingPhase().AddScore(order.First.Value, 11);
            }
        }
    }

    public override void BeginPlaying() {
        SendStartRoundPacket(0.1f);
    }

    public override void EndPlaying() {
    }

    public override void OnUnload() {
        b11PartyServer.GetKarmanServer().OnClientPackedReceivedCallback -= OnPacket;
    }

    protected void Update() {
        if (isCoutingDownForNextRound) {
            countingDownTime += Time.deltaTime;
            if (countingDownTime >= countingDownDuration) {
                isCoutingDownForNextRound = false;

                float minMin = 0.2f;
                float maxMin = 0.6f;
                SendStartRoundPacket((Random.Range(minMin, maxMin) + Random.Range(minMin, maxMin)) / 2f);
            }
        }
    }

    private void SendStartRoundPacket(float min) {
        float maxT = 1f - Mathf.Pow(1f - Random.value, 2f);
        float max = Mathf.Lerp(0.55f, 1f, maxT);
        b11PartyServer.GetKarmanServer().Broadcast(new B11BalloonStartRoundPacket(min, max));
    }
}
