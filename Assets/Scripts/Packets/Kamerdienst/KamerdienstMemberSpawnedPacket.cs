using Networking;
using System.Linq;

public class KamerdienstMemberSpawnedPacket : Packet {
    private readonly int memberId;
    private readonly int location;
    private readonly KamerdienstItemType[] items;
    private readonly int points;

    public KamerdienstMemberSpawnedPacket(byte[] bytes) : base(bytes) {
        memberId = ReadInt();
        location = ReadInt();
        items = ReadIntArray().Select(item => (KamerdienstItemType)item).ToArray();
        points = ReadInt();
    }

    public KamerdienstMemberSpawnedPacket(int memberId, int location, KamerdienstItemType[] items, int points) : base(
        Bytes.Pack(Bytes.Of(memberId), Bytes.Of(location), Bytes.Of(items.Select(item => (int)item).ToArray()), Bytes.Of(points))
    ) {
        this.memberId = memberId;
        this.location = location;
        this.items = items;
        this.points = points;
    }

    public override void Validate() { }

    public int GetMemberId() {
        return memberId;
    }

    public int GetLocation() {
        return location;
    }

    public KamerdienstItemType[] GetItems() {
        return items;
    }

    public int GetPoints() {
        return points;
    }
}
