using Networking;
using System;
using System.Collections.Generic;
using UnityEngine;

public class KamerdienstServerMiniGame : ServerMiniGame {
    [SerializeField]
    private int maxScore;

    private B11PartyServer b11PartyServer;

    private class MemberInfo {
        private readonly int memberId;
        private readonly int location;
        private readonly int points;
        private Guid clientId = Guid.Empty;

        public MemberInfo(int memberId, int location, int points) {
            this.memberId = memberId;
            this.location = location;
            this.points = points;
        }

        public bool TrySetHelpedBy(Guid clientId) {
            if (this.clientId != Guid.Empty) {
                return false;
            }
            this.clientId = clientId;
            return true;
        }

        public KamerdienstMemberHelpedPacket GetHelpedPacket() {
            return new KamerdienstMemberHelpedPacket(memberId, clientId);
        }

        public int GetLocation() {
            return location;
        }

        public int GetPoints() {
            return points;
        }
    }

    private readonly Dictionary<int, MemberInfo> members = new Dictionary<int, MemberInfo>();
    private readonly int numberOfLocations = 6;
    private bool maxScoreWasReached;

    public override void OnLoad(B11PartyServer b11PartyServer) {
        this.b11PartyServer = b11PartyServer;
        this.b11PartyServer.GetKarmanServer().OnClientPackedReceivedCallback += OnPacket;
        maxScoreWasReached = false;
    }

    private readonly IReadOnlyDictionary<int, int> lengthToPoints = new Dictionary<int, int>() {
        { 1, 2 },
        { 2, 4 },
        { 3, 7 },
        { 4, 11 },
    };

    private void Spawn(int location) {
        var itemCount = members.Count < 11 ? UnityEngine.Random.Range(1, 3) : UnityEngine.Random.Range(2, 5);
        var items = new KamerdienstItemType[itemCount];
        for (int itemIndex = 0; itemIndex < items.Length; itemIndex++) {
            items[itemIndex] = (KamerdienstItemType)UnityEngine.Random.Range(0, 5);
        }
        int points = lengthToPoints[items.Length];
        int memberId = members.Count;
        members.Add(memberId, new MemberInfo(memberId, location, points));
        b11PartyServer.GetKarmanServer().Broadcast(new KamerdienstMemberSpawnedPacket(
            memberId, location, items, points
        ));
    }

    private void OnPacket(Guid clientId, Packet packet) {
        if (packet is KamerdienstCharacterPositionUpdatedPacket || packet is KamerdienstCharacterInventoryUpdatedPacket) {
            b11PartyServer.GetKarmanServer().Broadcast(packet, clientId);
        } else if (packet is KamerdienstMemberHelpedPacket memberHelped) {
            if (maxScoreWasReached || clientId != memberHelped.GetClientId() || !members.TryGetValue(memberHelped.GetMemberId(), out MemberInfo member)) {
                return;
            }

            if (member.TrySetHelpedBy(clientId)) {
                var playingPhase = b11PartyServer.GetMiniGamePlayingPhase();
                playingPhase.AddScore(clientId, member.GetPoints());
                if (playingPhase.GetScore(clientId) >= maxScore) {
                    maxScoreWasReached = true;
                    b11PartyServer.GetKarmanServer().Broadcast(new KamerdienstMaxScoreReachedPacket());
                }
            }
            b11PartyServer.GetKarmanServer().Broadcast(member.GetHelpedPacket());
            Spawn(member.GetLocation());
        }
    }

    public override void BeginReadyUp() {
    }

    public override void EndReadyUp() {
    }

    public override void BeginPlaying() {
        for (int location = 0; location < numberOfLocations; location++) {
            Spawn(location);
        }
    }

    public override void EndPlaying() {
    }

    public override void OnUnload() {
        b11PartyServer.GetKarmanServer().OnClientPackedReceivedCallback -= OnPacket;
    }
}