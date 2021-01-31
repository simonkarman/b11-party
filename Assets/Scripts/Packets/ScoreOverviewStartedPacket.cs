using Networking;
using System;
using System.Collections.Generic;
using System.Linq;

public class ScoreOverviewStartedPacket : Packet {

    public class Score {
        private readonly Guid clientId;
        private readonly int lastAddedScore;

        public Score(Guid clientId, int lastAddedScore) {
            this.clientId = clientId;
            this.lastAddedScore = lastAddedScore;
        }

        public Guid GetClientId() {
            return clientId;
        }

        public int GetLastAddedScore() {
            return lastAddedScore;
        }
    }

    private readonly string miniGameName;
    private readonly int duration;
    private readonly Score[] scores;

    public ScoreOverviewStartedPacket(byte[] bytes) : base(bytes) {
        int miniGameNameLength = ReadInt();
        miniGameName = ReadString(miniGameNameLength);
        duration = ReadInt();
        List<Score> scores = new List<Score>();
        while (!IsDone()) {
            Guid clientId = ReadGuid();
            int lastAddedScore = ReadInt();
            scores.Add(new Score(clientId, lastAddedScore));
        }
        this.scores = scores.ToArray();
    }

    public ScoreOverviewStartedPacket(string miniGameName, int duration, Score[] scores) : base(AsBytes(miniGameName, duration, scores)) {
        this.duration = duration;
        this.scores = scores;
    }

    public override void Validate() { }

    private static byte[] AsBytes(string miniGameName, int duration, Score[] scores) {
        byte[][] scoresAsBytes = scores
            .Select(so => Bytes.Pack(
                Bytes.Of(so.GetClientId()),
                Bytes.Of(so.GetLastAddedScore())
            ))
            .ToArray();
        byte[] miniGameNameBytes = Bytes.Of(miniGameName);
        return Bytes.Pack(Bytes.Of(miniGameName.Length), miniGameNameBytes, Bytes.Of(duration), Bytes.Pack(scoresAsBytes));
    }

    public string GetMiniGameName() {
        return miniGameName;
    }

    public int GetDuration() {
        return duration;
    }

    public Score[] GetScores() {
        return scores;
    }
}