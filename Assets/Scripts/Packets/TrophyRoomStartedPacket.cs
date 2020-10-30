using Networking;
using System;
using System.Collections.Generic;
using System.Linq;

public class TrophyRoomStartedPacket : Packet {

    public class Score {
        private readonly Guid clientId;
        private readonly int totalScore;

        public Score(Guid clientId, int totalScore) {
            this.clientId = clientId;
            this.totalScore = totalScore;
        }

        public Guid GetClientId() {
            return clientId;
        }

        public int GetTotalScore() {
            return totalScore;
        }
    }

    private int duration;
    private Score[] scores;

    public TrophyRoomStartedPacket(byte[] bytes) : base(bytes) {
        duration = ReadInt();
        List<Score> scores = new List<Score>();
        while (!IsDone()) {
            Guid clientId = ReadGuid();
            int totalScore = ReadInt();
            scores.Add(new Score(clientId, totalScore));
        }
        this.scores = scores.ToArray();
    }

    public TrophyRoomStartedPacket(int duration, Score[] scores) : base(AsBytes(duration, scores)) {
        this.duration = duration;
        this.scores = scores;
    }

    public override void Validate() { }

    private static byte[] AsBytes(int duration, Score[] scores) {
        byte[][] scoresAsBytes = scores
            .Select(so => Bytes.Pack(
                Bytes.Of(so.GetClientId()),
                Bytes.Of(so.GetTotalScore())
            ))
            .ToArray();
        return Bytes.Pack(Bytes.Of(duration), Bytes.Pack(scoresAsBytes));
    }

    public int GetDuration() {
        return duration;
    }

    public Score[] GetScores() {
        return scores;
    }
}