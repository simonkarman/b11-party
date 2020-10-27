using UnityEngine;
using UnityEngine.UI;

public class ServerInformationUI : MonoBehaviour {
    [SerializeField]
    private Text informationText = default;

    protected void Start() {
        string text = informationText.text;
        informationText.text = string.Format(
            text,
            Application.isEditor ? "EDITOR" : Application.buildGUID,
            KarmanProtocol.KarmanServer.PROTOCOL_VERSION
        );
    }
}