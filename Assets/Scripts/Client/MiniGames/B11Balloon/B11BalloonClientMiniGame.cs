using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class B11BalloonClientMiniGame : ClientMiniGame {
    private const int SORTING_ORDER_WALL = 8;

    public class Location {
        private readonly int sortingOrder;
        private readonly float size;
        private readonly Vector3 position;

        private Location(int sortingOrder, float size, Vector3 position) {
            this.sortingOrder = sortingOrder;
            this.size = size;
            this.position = position;
        }

        public static Location From(int sortingOrder, Transform transform) {
            return new Location(sortingOrder, transform.localScale.x, transform.position);
        }

        public static Location From(SpriteRenderer spriteRenderer) {
            return From(spriteRenderer.sortingOrder, spriteRenderer.transform);
        }

        public int GetSortingOrder() {
            return sortingOrder;
        }

        public float GetSize() {
            return size;
        }

        public Vector3 GetPosition() {
            return position;
        }

        public static Location Lerp(Location a, Location b, float t) {
            return new Location(
                Mathf.RoundToInt(Mathf.Lerp(a.sortingOrder, b.sortingOrder, t)),
                Mathf.Lerp(a.size, b.size, t),
                Vector3.Lerp(a.position, b.position, t)
            );
        }

        public void ApplyTo(SpriteRenderer spriteRenderer) {
            spriteRenderer.sortingOrder = sortingOrder;
            spriteRenderer.transform.position = position;
            spriteRenderer.transform.localScale = size * Vector3.one;
        }
    }

    [SerializeField]
    private Transform root = default;

    [SerializeField]
    private Transform characterParent = default;
    [SerializeField]
    private GameObject otherCharacterPrefab = default;
    [SerializeField]
    private GameObject meCharacterPrefab = default;

    [SerializeField]
    private B11Balloon balloon = default;
    [SerializeField]
    private B11BalloonBulbs bulbs = default;
    [SerializeField]
    private B11BalloonDeadZone deadZone = default;

    [SerializeField]
    private Transform queueLastTransform;
    [SerializeField]
    private Transform queueFirstTransform;
    [SerializeField]
    private Transform buttonPressTransform;

    private Location buttonPressLocation;
    private Location queueLastLocation;
    private Location queueFirstLocation;

    private readonly AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    private bool isMoving = false;
    private readonly float movingDuration = 2f;
    private float movingTime;

    private readonly float inFrontMaxDuration = 8f;
    private float inFrontTime;
    private bool isInFront = false;
    private readonly float inFrontCooldownDuration = 0.4f;
    private float inFrontCooldownTime;
    
    private readonly Dictionary<Guid, SpriteRenderer> characters = new Dictionary<Guid, SpriteRenderer>();
    private readonly Dictionary<Guid, Location> charactersFrom = new Dictionary<Guid, Location>();
    private readonly Dictionary<Guid, Location> charactersTo = new Dictionary<Guid, Location>();
    private readonly LinkedList<Guid> order = new LinkedList<Guid>();

    protected override void OnLoadImpl() {
        root.gameObject.SetActive(false);
        b11PartyClient.OnOtherPacket += OnPacket;
        Guid meId = b11PartyClient.GetMe().GetClientId();
        foreach (var client in b11PartyClient.GetClients()) {
            bool isMe = client.GetClientId().Equals(meId);
            GameObject prefab = isMe ? meCharacterPrefab : otherCharacterPrefab;
            Transform characterInstance = Instantiate(prefab, characterParent).transform;
            characterInstance.localPosition = Vector3.zero;
            SpriteRenderer character = characterInstance.GetComponent<SpriteRenderer>();
            characterInstance.GetComponent<SpriteRenderer>().sprite = client.GetSprite();
            character.sortingOrder = SORTING_ORDER_WALL - 1;
            characters.Add(client.GetClientId(), character);
        }

        buttonPressLocation = Location.From(SORTING_ORDER_WALL + 1, buttonPressTransform);
        queueLastLocation = Location.From(1, queueLastTransform);
        queueFirstLocation = Location.From(SORTING_ORDER_WALL - 1, queueFirstTransform);
    }

    private void OnPacket(Packet packet) {
        if (packet is B11BalloonOrderPacket orderPacket) {
            // Hide all clients that are not playing
            foreach (var client in b11PartyClient.GetClients()) {
                if (!orderPacket.GetOrder().Contains(client.GetClientId())) {
                    characters[client.GetClientId()].gameObject.SetActive(false);
                }
            }
            // Create linked list of the playing clients
            foreach (var clientId in orderPacket.GetOrder()) {
                order.AddLast(clientId);
            }
        } else if (packet is B11BalloonStartRoundPacket startRoundPacket) {
            balloon.Reset(startRoundPacket.GetBalloonSizeMin(), startRoundPacket.GetBalloonSizeMax());
            bulbs.Reset();
            StartShiftCharacters();
        } else if (packet is B11BalloonShiftPacket) {
            order.AddLast(order.First.Value);
            order.RemoveFirst();
            bulbs.Reset();
            StartShiftCharacters();
        } else if (packet is B11BalloonInflatePacket inflatePacket) {
            if (!isInFront) {
                bulbs.UseOne();
            }
            balloon.SetSize(inflatePacket.GetSize());
        } else if (packet is B11BalloonPoppedPacket) {
            var playerPopped = order.First.Value;
            order.RemoveFirst();
            balloon.Pop();
            deadZone.Add(characters[playerPopped]);
            if (playerPopped == b11PartyClient.GetMe().GetClientId() || order.Count < 2) {
                // If the balloon popped on you, or you're the last player left,
                // then send to the server that you're finished
                b11PartyClient.GetKarmanClient().Send(new MiniGamePlayingFinishedPacket(
                    b11PartyClient.GetMe().GetClientId()
                ));
            }
        }
    }

    private void StartShiftCharacters() {
        charactersFrom.Clear();
        charactersTo.Clear();
        int orderId = 0;
        foreach (var clientId in order) {
            if (orderId == 0) {
                // If now first, directly jump to front sorting layer and move towards the button pressed location
                charactersFrom.Add(clientId, Location.From(buttonPressLocation.GetSortingOrder(), characters[clientId].transform));
                charactersTo.Add(clientId, buttonPressLocation);
            } else if (orderId == order.Count - 1) {
                // If now last, directly jump to back sorting layer and move towards the last spot in the queue
                charactersFrom.Add(clientId, Location.From(queueLastLocation.GetSortingOrder(), characters[clientId].transform));
                charactersTo.Add(clientId, queueLastLocation);
            } else {
                // If in queue, them move towards the position in queue
                float orderT = 0f;
                if (order.Count - 2 > 0) {
                    orderT = ((float)orderId - 1) / (order.Count - 2);
                }
                charactersFrom.Add(clientId, Location.From(characters[clientId]));
                charactersTo.Add(clientId, Location.Lerp(queueFirstLocation, queueLastLocation, orderT));
            }
            orderId++;
        }
        isMoving = true;
        movingTime = 0f;
        isInFront = false;
        inFrontTime = 0f;
        inFrontCooldownTime = 0f;
    }

    protected override void OnReadyUpImpl() {
    }

    protected override void OnPlayingImpl() {
        root.gameObject.SetActive(true);
    }

    protected override void OnPlayingEndedImpl() {
        b11PartyClient.OnOtherPacket -= OnPacket;
    }

    protected override void Update() {
        base.Update();
        
        if (GetMode() == Mode.PLAYING) {
            if (isMoving) {
                // Move each client in order to its target position slowly
                movingTime += Time.deltaTime;
                float moveT = movingTime / movingDuration;
                if (moveT >= 1f) {
                    // Once done moving, if me is the client at the button, start the choice process
                    moveT = 1f;
                    isMoving = false;
                    if (b11PartyClient.GetMe().GetClientId().Equals(order.First.Value)) {
                        isInFront = true;
                    }
                }
                float moveTEvaluated = moveCurve.Evaluate(moveT);
                foreach (var clientId in order) {
                    Location currentLocation = Location.Lerp(charactersFrom[clientId], charactersTo[clientId], moveTEvaluated);
                    currentLocation.ApplyTo(characters[clientId]);
                }
            }

            if (isInFront) {
                // Wait for cooldown
                inFrontCooldownTime += Time.deltaTime;
                if (inFrontCooldownTime >= inFrontCooldownDuration) {

                    // If in front for to long, then shift to the next player
                    inFrontTime += Time.deltaTime;
                    if (inFrontTime >= inFrontMaxDuration) {
                        b11PartyClient.GetKarmanClient().Send(new B11BalloonShiftPacket());
                        isInFront = false;
                    }

                    // Force a inflate when no bulbs have been used
                    if (bulbs.HasAllBulbsLeft()) {
                        SendInflate();
                    } else if (bulbs.HasNoBulbsLeft()) {
                        SendShift();
                    // Based on player choice, inflate or shift the queue
                    } else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Space)) {
                        SendInflate();
                    } else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
                        SendShift();
                    }
                }
            }
        }
    }

    private void SendShift() {
        inFrontCooldownTime = 0f;
        isInFront = false;
        b11PartyClient.GetKarmanClient().Send(new B11BalloonShiftPacket());
    }

    private void SendInflate() {
        inFrontCooldownTime = 0f;
        float minInflateAmount = 0.02f;
        float maxInflateAmount = 0.06f;
        float inflateAmount = (Random.Range(minInflateAmount, maxInflateAmount) + Random.Range(minInflateAmount, maxInflateAmount)) / 2f;
        if (balloon.WouldInflateCauseAPop(inflateAmount)) {
            isInFront = false;
            b11PartyClient.GetKarmanClient().Send(new B11BalloonPoppedPacket());
        } else {
            b11PartyClient.GetKarmanClient().Send(new B11BalloonInflatePacket(balloon.GetSize() + inflateAmount));
        }
        bulbs.UseOne();
    }
}
