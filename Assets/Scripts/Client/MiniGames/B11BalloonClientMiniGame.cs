using KarmanProtocol;
using System.Collections;
using UnityEngine;

public class B11BalloonClientMiniGame : ClientMiniGame {
    private B11PartyClient b11PartyClient;

    public override void OnLoad(B11PartyClient b11PartyClient) {
        this.b11PartyClient = b11PartyClient;
        StartCoroutine(LoadingSequence());
    }

    private IEnumerator LoadingSequence() {
        Debug.LogWarning("Not Yet Implemented");
        yield return new WaitForSeconds(Random.value * 5f + 1f);
        KarmanClient client = b11PartyClient.GetKarmanClient();
        client.Send(new MiniGameLoadingDonePacket(client.id));
    }
}