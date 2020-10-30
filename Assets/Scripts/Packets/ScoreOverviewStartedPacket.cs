using Networking;
using System;
using System.Collections.Generic;
using System.Linq;

public class ScoreOverviewStartedPacket : Packet {

    public class ScoreOverviewInformation {
        private readonly Guid clientId;
        private readonly int lastAddedScore;

        public ScoreOverviewInformation(Guid clientId, int lastAddedScore) {
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
    private ScoreOverviewInformation[] scoreOverviews;

    public ScoreOverviewStartedPacket(byte[] bytes) : base(bytes) {
        List<ScoreOverviewInformation> scoreOverviews = new List<ScoreOverviewInformation>();
        while (!IsDone()) {
            Guid clientId = ReadGuid();
            int lastAddedScore = ReadInt();
            scoreOverviews.Add(new ScoreOverviewInformation(clientId, lastAddedScore));
        }
        this.scoreOverviews = scoreOverviews.ToArray();
    }

    public ScoreOverviewStartedPacket(ScoreOverviewInformation[] scoreOverviews) : base(AsBytes(scoreOverviews)) {
        this.scoreOverviews = scoreOverviews;
    }

    public override void Validate() { }

    private static byte[] AsBytes(ScoreOverviewInformation[] scoreOverviews) {
        byte[][] scoreOverviewsAsBytes = scoreOverviews
            .Select(so => Bytes.Pack(
                Bytes.Of(so.GetClientId()),
                Bytes.Of(so.GetLastAddedScore())
            ))
            .ToArray();
        return Bytes.Pack(scoreOverviewsAsBytes);
    }

    public ScoreOverviewInformation[] GetScoreOverviews() {
        return scoreOverviews;
    }
}