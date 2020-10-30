using Networking;
using System;
using System.Collections.Generic;
using System.Linq;

public class TrophyRoomStartedPacket : Packet {

    public class TrophyRoomInformation {
        private readonly Guid clientId;
        private readonly int totalScore;

        public TrophyRoomInformation(Guid clientId, int totalScore) {
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
    private TrophyRoomInformation[] trophyRooms;

    public TrophyRoomStartedPacket(byte[] bytes) : base(bytes) {
        List<TrophyRoomInformation> trophyRooms = new List<TrophyRoomInformation>();
        while (!IsDone()) {
            Guid clientId = ReadGuid();
            int totalScore = ReadInt();
            trophyRooms.Add(new TrophyRoomInformation(clientId, totalScore));
        }
        this.trophyRooms = trophyRooms.ToArray();
    }

    public TrophyRoomStartedPacket(TrophyRoomInformation[] trophyRooms) : base(AsBytes(trophyRooms)) {
        this.trophyRooms = trophyRooms;
    }

    public override void Validate() { }

    private static byte[] AsBytes(TrophyRoomInformation[] trophyRooms) {
        byte[][] trophyRoomsAsBytes = trophyRooms
            .Select(so => Bytes.Pack(
                Bytes.Of(so.GetClientId()),
                Bytes.Of(so.GetTotalScore())
            ))
            .ToArray();
        return Bytes.Pack(trophyRoomsAsBytes);
    }

    public TrophyRoomInformation[] GetTrophyRooms() {
        return trophyRooms;
    }
}