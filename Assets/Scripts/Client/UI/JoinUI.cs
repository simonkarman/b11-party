using KarmanProtocol;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class JoinUI : MonoBehaviour {

    [Serializable]
    public class Passcode {
        [SerializeField]
        private string passcode = default;
        [SerializeField]
        private string clientId = default;

        public string GetPasscode() {
            return passcode;
        }

        public Guid GetClientId() {
            return Guid.Parse(clientId);
        }
    }

    [SerializeField]
    private Transform root = default;
    [SerializeField]
    private Button connectButton = default;
    [SerializeField]
    private InputField connectionStringInput = default;
    [SerializeField]
    private InputField passcodeInput = default;
    [SerializeField]
    private Passcode[] passcodes = default;
    [SerializeField]
    private B11PartyClient b11PartyClient = default;

    private KarmanClient karmanClient;

    protected void Awake() {
        Logging.Logger.ClearAppenders();
        Logging.Logger.AddAppender(new Logging.UnityDebugAppender(Logging.LogLevel.INFO));
    }

    protected void Start() {
        OnLeft();
        connectButton.interactable = false;
    }

    public void OnInputChanged() {
        bool isServer = passcodeInput.text.Equals("serv-er11");
        bool isClient = (
            (passcodeInput.text.Length == 9 || (FeatureToggles.SingleLetterLogin && passcodeInput.text.Length == 1))
            && connectionStringInput.text.Length > 3
            && passcodes.Any(passcode => passcode.GetPasscode().ToLower().StartsWith(passcodeInput.text.ToLower()))
        );
        connectButton.interactable = isServer || isClient;
    }

    public void OnConnectButtonClicked() {
        if (passcodeInput.text.Equals("serv-er11")) {
            SceneManager.LoadScene("Server");
            return;
        }

        passcodeInput.interactable = false;
        connectionStringInput.interactable = false;
        connectButton.interactable = false;
        Guid clientId = passcodes.FirstOrDefault(passcode => passcode.GetPasscode().ToLower().StartsWith(passcodeInput.text.ToLower())).GetClientId();
        Debug.LogFormat("Provided passcode resulted in the following client id: {0}", clientId);
        connectButton.GetComponentInChildren<Text>().text = "Trying to connect...";
        karmanClient = new KarmanClient(clientId, B11PartyServer.GAME_ID, clientId);
        karmanClient.OnJoinedCallback += OnJoined;
        karmanClient.OnConnectedCallback += () => { };
        karmanClient.OnDisconnectedCallback += () => { };
        karmanClient.OnLeftCallback += () => {
            OnLeft();
            if (Application.isEditor) {
                Application.Quit();
            } else {
                SceneManager.LoadScene("Client");
            }
        };
        karmanClient.Start(connectionStringInput.text, B11PartyServer.DEFAULT_PORT);
    }

    private void OnLeft() {
        root.gameObject.SetActive(true);
        karmanClient = null;
        passcodeInput.interactable = true;
        connectionStringInput.interactable = true;
        connectButton.interactable = true;
        connectButton.GetComponentInChildren<Text>().text = "Connect!";
        b11PartyClient.Stop();
    }

    private void OnJoined() {
        root.gameObject.SetActive(false);
        b11PartyClient.StartWith(karmanClient);
    }

    protected void OnDestroy() {
        if (karmanClient != null) {
            karmanClient.Leave();
        }
    }
}