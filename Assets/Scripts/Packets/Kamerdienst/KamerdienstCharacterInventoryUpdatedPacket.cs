using Networking;
using System;
using System.Linq;

public class KamerdienstCharacterInventoryUpdatedPacket : Packet {
    private readonly Guid clientId;
    private readonly KamerdienstItemType[] items;

    public KamerdienstCharacterInventoryUpdatedPacket(byte[] bytes) : base(bytes) {
        clientId = ReadGuid();
        items = ReadIntArray().Select(item => (KamerdienstItemType)item).ToArray();
    }

    public KamerdienstCharacterInventoryUpdatedPacket(Guid clientId, KamerdienstItemType[] items) : base(
        Bytes.Pack(Bytes.Of(clientId), Bytes.Of(items.Select(item => (int)item).ToArray()))
    ) {
        this.clientId = clientId;
        this.items = items;
    }

    public override void Validate() {}

    public Guid GetClientId() {
        return clientId;
    }

    public KamerdienstItemType[] GetItems() {
        return items;
    }
}
