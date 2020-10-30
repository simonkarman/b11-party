using UnityEngine;

public abstract class ServerMiniGame : MonoBehaviour {
    public abstract void OnLoad(B11PartyServer b11PartyServer);
    public abstract void OnUnload();
    public abstract void BeginReadyUp();
    public abstract void EndReadyUp();
    public abstract void BeginPlaying();
    public abstract void EndPlaying();
}