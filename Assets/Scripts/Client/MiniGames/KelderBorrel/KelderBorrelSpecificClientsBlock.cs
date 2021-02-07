using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KelderBorrelSpecificClientsBlock : KelderBorrelBlock {
    private readonly List<Guid> clients = new List<Guid>();

    [Serializable]
    public class PersonOverlay {
        [SerializeField]
        private GameObject root;
        [SerializeField]
        private SpriteRenderer[] spriteRenderers;

        public GameObject GetRoot() {
            return root;
        }

        public SpriteRenderer[] GetSpriteRenderers() {
            return spriteRenderers;
        }
    }

    [SerializeField]
    private PersonOverlay[] personOverlays;

    private Dictionary<Guid, SpriteRenderer> activeRenderers = new Dictionary<Guid, SpriteRenderer>();

    protected void Awake() {
        foreach (var personOverlay in personOverlays) {
            personOverlay.GetRoot().SetActive(false);
        }
    }

    protected override void OnRegisterHit(Guid clientId, bool isMe) {
        clients.Remove(clientId);
        if (activeRenderers.ContainsKey(clientId)) {
            activeRenderers[clientId].color = new Color(0.3f, 0.3f, 0.3f, 0.2f);
        }
        if (isMe) {
            foreach (var rendererKvp in activeRenderers) {
                var color = clients.Contains(rendererKvp.Key)
                    ? new Color(1f, 1f, 1f, 0.2f)
                    : new Color(0.3f, 0.3f, 0.3f, 0.2f);
                rendererKvp.Value.color = color;
            }
        }
        if (clients.Count <= 0) {
            gameObject.SetActive(false);
        }
    }

    public void SetSpecificClients(B11PartyClient b11PartyClient, Guid[] clients) {
        clients = clients.Where(clientId => clientId != Guid.Empty).ToArray();
        if (clients.Length == 0) {
            return;
        }

        var personOverlay = personOverlays.Where(po => po.GetSpriteRenderers().Length == clients.Length).FirstOrDefault();
        if (personOverlay != null) {
            personOverlay.GetRoot().SetActive(true);
            for (int clientIndex = 0; clientIndex < clients.Length; clientIndex++) {
                Guid clientId = clients[clientIndex];
                this.clients.Add(clientId);

                SpriteRenderer renderer = personOverlay.GetSpriteRenderers()[clientIndex];
                renderer.color = Color.white;
                renderer.sprite = b11PartyClient.GetClient(clientId).GetSprite();
                activeRenderers.Add(clientId, renderer);
            }
        } else {
            Debug.LogWarning($"Cannot show a person overlay on a specific clients block with {clients.Length} client(s).");
        }

        bool containsMe = clients.Contains(b11PartyClient.GetMe().GetClientId());
        if (!containsMe) {
            isHit = true;
            blockRenderer.color = new Color(1f, 1f, 1f, 0.1f);
            blockCollider.enabled = false;
            foreach (var renderer in activeRenderers.Values) {
                renderer.color = new Color(1f, 1f, 1f, 0.2f);
            }
        }
    }
}
