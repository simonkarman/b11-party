using KarmanProtocol;
using System.Collections;
using UnityEngine;

public abstract class ClientMiniGame : MonoBehaviour {
    protected enum Mode {
        NEW,
        LOADING,
        READY_UP,
        PLAYING,
        DONE
    }

    protected string miniGameName;
    protected B11PartyClient b11PartyClient;
    private Mode mode = Mode.NEW;
    private bool ready = false;

    public void SetMiniGameName(string miniGameName) {
        this.miniGameName = miniGameName;
    }

    public string GetMiniGameName() {
        return miniGameName;
    }

    protected Mode GetMode() {
        return mode;
    }

    public void OnLoad(B11PartyClient b11PartyClient) {
        mode = Mode.LOADING;
        this.b11PartyClient = b11PartyClient;
        StartCoroutine(LoadingSequence());
    }
    protected abstract void OnLoadImpl();

    public void OnReadyUp() {
        mode = Mode.READY_UP;
        OnReadyUpImpl();
    }
    protected abstract void OnReadyUpImpl();

    public void OnPlaying() {
        mode = Mode.PLAYING;
        OnPlayingImpl();
    }
    protected abstract void OnPlayingImpl();

    private IEnumerator LoadingSequence() {
        yield return new WaitForSeconds(Random.value * 4f + 2f);
        KarmanClient client = b11PartyClient.GetKarmanClient();
        client.Send(new MiniGameLoadingDonePacket(client.id));
    }

    protected void Update() {
        if (mode == Mode.READY_UP && !ready && Input.GetKeyDown(KeyCode.Space)) {
            KarmanClient client = b11PartyClient.GetKarmanClient();
            client.Send(new MiniGameReadyUpReadyPacket(client.id));
            ready = true;
        }
    }

    public abstract void OnPlayingEndedImpl();
}