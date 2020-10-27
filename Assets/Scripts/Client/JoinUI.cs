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
        private string passcode;
        [SerializeField]
        private string clientId;

        public string GetPasscode() {
            return passcode;
        }

        public Guid GetClientId() {
            return Guid.Parse(clientId);
        }
    }

    [SerializeField]
    private Button connectButton = default;
    [SerializeField]
    private InputField connectionStringInput = default;
    [SerializeField]
    private InputField passcodeInput = default;
    [SerializeField]
    private Passcode[] passcodes = default;

    private KarmanClient karmanClient;

    protected void Awake() {
        Logging.Logger.AddAppender(new Logging.UnityDebugAppender(Logging.LogLevel.INFO));
        OnLeft();
        connectButton.interactable = false;
    }

    public void OnInputChanged() {
        connectButton.interactable = passcodeInput.text.Equals("se-rver") || passcodeInput.text.Length == 7 && connectionStringInput.text.Length > 3 && passcodes.Any(passcode => passcode.GetPasscode().ToLower().Equals(passcodeInput.text.ToLower()));
    }

    public void OnConnectButtonClicked() {
        if (passcodeInput.text.Equals("se-rver")) {
            SceneManager.LoadScene("Server");
        }

        passcodeInput.interactable = false;
        connectionStringInput.interactable = false;
        connectButton.interactable = false;
        Guid clientId = passcodes.FirstOrDefault(passcode => passcode.GetPasscode().ToLower().Equals(passcodeInput.text.ToLower())).GetClientId();
        Debug.LogFormat("Provided passcode resulted in the following client id: {0}", clientId);
        connectButton.GetComponentInChildren<Text>().text = "Trying to connect...";
        karmanClient = new KarmanClient(clientId);
        karmanClient.OnJoinedCallback += OnJoined;
        karmanClient.OnConnectedCallback += () => { };
        karmanClient.OnDisconnectedCallback += () => { };
        karmanClient.OnLeftCallback += OnLeft;
        karmanClient.Start(connectionStringInput.text, B11PartyServer.DEFAULT_PORT);
    }

    private void OnLeft() {
        gameObject.SetActive(true);
        karmanClient = null;
        passcodeInput.interactable = true;
        connectionStringInput.interactable = true;
        connectButton.interactable = true;
        connectButton.GetComponentInChildren<Text>().text = "Connect!";
    }

    private void OnJoined() {
        gameObject.SetActive(false);
    }

    protected void OnDestroy() {
        karmanClient.Leave();
    }
}