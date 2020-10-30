using UnityEngine;

public abstract class ServerMiniGame : MonoBehaviour {
    public abstract void OnLoad(B11PartyServer b11PartyServer);
    public abstract void OnUnload();
}