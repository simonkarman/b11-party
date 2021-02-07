using Networking;
using System;
using System.Collections.Generic;
using UnityEngine;

public class KamerdienstClientMiniGame : ClientMiniGame {
    [SerializeField]
    private Transform root = default;

    [SerializeField]
    private KamerdienstTrashCan[] trashcans = default;
    [SerializeField]
    private KamerdienstPickup[] pickups = default;
    [SerializeField]
    private KamerdienstLocation[] locations = default;
    [SerializeField]
    private Transform memberPrefab = default;

    [SerializeField]
    private Transform characterParent = default;
    [SerializeField]
    private GameObject otherCharacterPrefab = default;
    [SerializeField]
    private GameObject meCharacterPrefab = default;

    private bool meHasFinished = false;
    private KamerdienstCharacter me;
    private readonly Dictionary<Guid, KamerdienstInventory> characters = new Dictionary<Guid, KamerdienstInventory>();
    private readonly Dictionary<int, KamerdienstMember> members = new Dictionary<int, KamerdienstMember>();

    protected override void OnLoadImpl() {
        root.gameObject.SetActive(false);
        b11PartyClient.OnOtherPacket += OnPacket;
        Guid meId = b11PartyClient.GetMe().GetClientId();
        foreach (var client in b11PartyClient.GetClients()) {
            bool isMe = client.GetClientId().Equals(meId);
            GameObject prefab = isMe ? meCharacterPrefab : otherCharacterPrefab;
            Transform characterInstance = Instantiate(prefab, characterParent).transform;
            characterInstance.localPosition = Vector3.zero;
            if (isMe) {
                me = characterInstance.GetComponent<KamerdienstCharacter>();
            }
            characterInstance.GetComponent<SpriteRenderer>().sprite = client.GetSprite();
            characters.Add(client.GetClientId(), characterInstance.GetComponent<KamerdienstInventory>());
        }
        foreach (var pickup in pickups) {
            pickup.Initialize(me);
        }
        foreach (var trashcan in trashcans) {
            trashcan.Initialize(me);
        }
    }

    private void OnPacket(Packet packet) {
        if (packet is KamerdienstCharacterPositionUpdatedPacket positionUpdated) {
            characters[positionUpdated.GetClientId()].transform.localPosition = positionUpdated.GetPosition();
        } else if (packet is KamerdienstCharacterInventoryUpdatedPacket inventoryUpdated) {
            characters[inventoryUpdated.GetClientId()].SetItems(inventoryUpdated.GetItems());
        } else if (packet is KamerdienstMemberSpawnedPacket memberSpawned) {
            Spawn(memberSpawned.GetMemberId(), memberSpawned.GetLocation(), memberSpawned.GetItems(), memberSpawned.GetPoints());
        } else if (packet is KamerdienstMemberHelpedPacket memberHelped) {
            bool wasHelpedByMe = b11PartyClient.GetMe().GetClientId() == memberHelped.GetClientId();
            members[memberHelped.GetMemberId()].WasHelped(wasHelpedByMe, b11PartyClient.GetClient(memberHelped.GetClientId()).GetName());
        } else if (packet is KamerdienstMaxScoreReachedPacket) {
            if (!meHasFinished) {
                SetFinishedMe();
            }
        }
    }

    private void Spawn(int memberId, int locationIndex, KamerdienstItemType[] items, int points) {
        var location = locations[locationIndex % locations.Length];
        Transform memberInstance = Instantiate(memberPrefab, location.transform).transform;
        memberInstance.position = location.GetStart().position;
        KamerdienstMember member = memberInstance.GetComponent<KamerdienstMember>();
        member.Initialize(b11PartyClient, me, memberId, location.GetEnd().position, items, points);
        members.Add(memberId, member);
    }

    protected override void OnReadyUpImpl() {
    }

    protected override void OnPlayingImpl() {
        root.gameObject.SetActive(true);
    }

    protected override void OnPlayingEndedImpl() {
        b11PartyClient.OnOtherPacket -= OnPacket;
    }

    private void SetFinishedMe() {
        b11PartyClient.GetKarmanClient().Send(new MiniGamePlayingFinishedPacket(
            b11PartyClient.GetMe().GetClientId()
        ));
        meHasFinished = true;
        me.DisableHelping();
    }

    protected override void Update() {
        base.Update();
        if (GetMode() == Mode.PLAYING && !meHasFinished) {
            var meId = b11PartyClient.GetMe().GetClientId();
            b11PartyClient.GetKarmanClient().Send(new KamerdienstCharacterPositionUpdatedPacket(meId, me.transform.localPosition));
            if (me.CheckItemsChanged()) {
                b11PartyClient.GetKarmanClient().Send(new KamerdienstCharacterInventoryUpdatedPacket(meId, me.GetItems()));
            }
        }
    }

    protected void OnDrawGizmos() {
        Gizmos.color = Color.green;
        foreach (var pickup in pickups) {
            Gizmos.DrawWireSphere(pickup.transform.position, 1f);
        }
        Gizmos.color = Color.magenta;
        foreach (var trashcan in trashcans) {
            Gizmos.DrawWireSphere(trashcan.transform.position, 1f);
        }
        Gizmos.color = Color.yellow;
        foreach (var location in locations) {
            Gizmos.DrawLine(location.GetStart().position, location.GetEnd().position);
            Gizmos.DrawWireSphere(location.GetStart().position, 0.3f);
            Gizmos.DrawWireSphere(location.GetEnd().position, 1f);
        }
    }
}
