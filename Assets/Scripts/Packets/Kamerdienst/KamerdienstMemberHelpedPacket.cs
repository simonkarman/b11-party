using Networking;
using System;

public class KamerdienstMemberHelpedPacket : Packet {
    private readonly int memberId;
    private readonly Guid clientId;

    public KamerdienstMemberHelpedPacket(byte[] bytes) : base(bytes) {
        memberId = ReadInt();
        clientId = ReadGuid();
    }

    public KamerdienstMemberHelpedPacket(int memberId, Guid clientId) : base(
        Bytes.Pack(Bytes.Of(memberId), Bytes.Of(clientId))
    ) {
        this.memberId = memberId;
        this.clientId = clientId;
    }

    public override void Validate() { }

    public int GetMemberId() {
        return memberId;
    }

    public Guid GetClientId() {
        return clientId;
    }
}
