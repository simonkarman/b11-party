using Networking;
using System;
using System.Collections.Generic;
using UnityEngine;

public class B11BalloonClientMiniGame : ClientMiniGame {
    [SerializeField]
    private Transform root = default;
    [SerializeField]
    private float growSpeed = 0.2f;
    [SerializeField]
    private float startSize = 0.6f;

    [SerializeField]
    private Transform balloonParent = default;
    [SerializeField]
    private Transform balloonLeftPosition = default;
    [SerializeField]
    private Transform balloonRightPosition = default;
    [SerializeField]
    private GameObject balloonPrefab = default;

    private bool growingStarted;
    private bool stillGrowing;
    private bool balloonPopped;
    private float currentSize;
    private float maxSize;
    private readonly Dictionary<Guid, B11Balloon> balloons = new Dictionary<Guid, B11Balloon>();
    private B11Balloon meBalloon;

    protected void Start() {
        root.gameObject.SetActive(false);
    }

    protected override void OnLoadImpl() {
        Guid me = b11PartyClient.GetMe().GetClientId();
        int numberOfClients = b11PartyClient.GetClients().Count;
        int clientIndex = 0;
        foreach (var client in b11PartyClient.GetClients()) {
            Transform balloonInstance = Instantiate(balloonPrefab, balloonParent).transform;
            float clientT = clientIndex / (float)(numberOfClients - 1);
            balloonInstance.position = Vector3.Lerp(balloonLeftPosition.position, balloonRightPosition.position, clientT);

            B11Balloon balloon = balloonInstance.GetComponent<B11Balloon>();
            balloon.SetStartSize(startSize);
            balloons.Add(client.GetClientId(), balloon);
            bool isMe = client.GetClientId().Equals(me);
            balloon.ShowEmoji(isMe);
            balloon.SetClientSprite(client.GetSprite());
            if (isMe) {
                meBalloon = balloon;
            }
            clientIndex++;
        }
        currentSize = startSize;
        maxSize = 99f;
        b11PartyClient.OnMiniGamePlayingFinishedCallback += OnBalloonFinished;
        b11PartyClient.OnOtherPacket += OnOtherPacket;
    }

    private void OnOtherPacket(Packet packet) {
        if (packet is B11BalloonMaxSizePacket maxSizePacket) {
            foreach (var balloon in balloons.Values) {
                balloon.SetMaxSize(maxSizePacket.GetMaxSize());
            };
            maxSize = maxSizePacket.GetMaxSize();

        } else if (packet is B11BalloonSizePacket sizePacket) {
            balloons[sizePacket.GetClientId()].SetSize(sizePacket.GetSize());
        }
    }

    protected override void OnReadyUpImpl() { }

    private void OnBalloonFinished(Guid clientId) {
        balloons[clientId].SetFinished();
    }

    protected override void OnPlayingImpl() {
        root.gameObject.SetActive(true);
        stillGrowing = true;
        balloonPopped = false;
    }

    protected override void OnPlayingEndedImpl() {
        root.gameObject.SetActive(false);
        b11PartyClient.OnMiniGamePlayingFinishedCallback -= OnBalloonFinished;
        b11PartyClient.OnOtherPacket -= OnOtherPacket;
        float scoreT = Mathf.InverseLerp(startSize, maxSize, currentSize);
        int score = balloonPopped ? 0 : Mathf.RoundToInt(111f * scoreT);
        b11PartyClient.GetKarmanClient().Send(new MiniGamePlayingScorePacket(
            b11PartyClient.GetMe().GetClientId(),
            score
        ));

    }

    private void SendFinishedPacket() {
        b11PartyClient.GetKarmanClient().Send(new MiniGamePlayingFinishedPacket(
            b11PartyClient.GetMe().GetClientId()
        ));
    }

    protected override void Update() {
        base.Update();
        if (GetMode() == Mode.PLAYING) {
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
                growingStarted = true;
            }

            if (stillGrowing && growingStarted) {
                currentSize += Time.deltaTime * growSpeed;
                meBalloon.SetSize(currentSize);
                if (currentSize >= maxSize) {
                    currentSize = maxSize;
                    balloonPopped = true;
                    stillGrowing = false;
                    SendFinishedPacket();
                    meBalloon.Pop();
                }
            }

            if (stillGrowing && Input.GetKeyDown(KeyCode.Space) && currentSize > startSize + 0.2f) {
                stillGrowing = false;
                SendFinishedPacket();
                meBalloon.SetFinished();
            }

            if (stillGrowing) {
                b11PartyClient.GetKarmanClient().Send(new B11BalloonSizePacket(
                    b11PartyClient.GetMe().GetClientId(),
                    currentSize
                ));
            }
        }
    }
}