using KarmanProtocol;
using Networking;
using UnityEngine;

public class B11PartyClient : MonoBehaviour {

    private KarmanClient karmanClient;

    public void StartWith(KarmanClient karmanClient) {
        this.karmanClient = karmanClient;
        karmanClient.OnPacketReceivedCallback += OnPacketReceived;
    }

    private void OnPacketReceived(Packet packet) {
        if (packet is PingPacket pingPacket) {
            PingResponsePacket pingResponsePacket = new PingResponsePacket(pingPacket.GetPingId());
            karmanClient.Send(pingResponsePacket);
        }
    }
}