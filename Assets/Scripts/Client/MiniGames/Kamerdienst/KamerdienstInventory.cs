using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KamerdienstInventory : MonoBehaviour {
    [SerializeField]
    private Image[] images;

    [SerializeField]
    private Sprite bitsSprite;
    [SerializeField]
    private Sprite beerSprite;
    [SerializeField]
    private Sprite bananaSprite;
    [SerializeField]
    private Sprite coffeeSprite;
    [SerializeField]
    private Sprite cookieSprite;

    private KamerdienstItemType[] items = new KamerdienstItemType[0];

    protected void Awake() {
        SetItems(items);
    }

    public void SetItems(KamerdienstItemType[] items) {
        this.items = items;
        UpdateUI();
    }

    public bool IsEmpty() {
        return items.Length == 0;
    }

    public bool IsFull() {
        return items.Length == images.Length;
    }

    private void UpdateUI() {
        for (int i = 0; i < images.Length; i++) {
            bool hasItem = i < items.Length;
            images[i].gameObject.SetActive(hasItem);
            if (hasItem) {
                Sprite sprite = ToSprite(items[i]);
                images[i].sprite = sprite;
            }
        }
    }

    private Sprite ToSprite(KamerdienstItemType itemType) {
        switch (itemType) {
        default:
        case KamerdienstItemType.Bits:
            return bitsSprite;
        case KamerdienstItemType.Beer:
            return beerSprite;
        case KamerdienstItemType.Banana:
            return bananaSprite;
        case KamerdienstItemType.Coffee:
            return coffeeSprite;
        case KamerdienstItemType.Cookie:
            return cookieSprite;
        }
    }

    public static bool IsMatch(KamerdienstInventory a, KamerdienstInventory b) {
        if (a.items.Length != b.items.Length) {
            return false;
        }
        Dictionary<KamerdienstItemType, int> counts = new Dictionary<KamerdienstItemType, int>();
        // Add 1 for each from a
        for (int i = 0; i < a.items.Length; i++) {
            var itemA = a.items[i];
            if (!counts.ContainsKey(itemA)) {
                counts[itemA] = 0;
            }
            counts[itemA]++;
        }
        // Substract 1 for each from b
        for (int i = 0; i < b.items.Length; i++) {
            var itemB = b.items[i];
            if (!counts.ContainsKey(itemB)) {
                return false;
            }
            counts[itemB]--;
            if (counts[itemB] == 0) {
                counts.Remove(itemB);
            }
        }
        // Left should be 0
        return counts.Count == 0;
    }
}
