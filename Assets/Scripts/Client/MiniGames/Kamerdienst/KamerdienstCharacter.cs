using System.Collections.Generic;
using UnityEngine;

public class KamerdienstCharacter : MonoBehaviour {
    [SerializeField]
    private KamerdienstInventory inventory;
    [SerializeField]
    private Rigidbody2D rb2d;
    [SerializeField]
    private float moveSpeed = 1f;

    private int waitingOnMemberHelp = -1;
    private bool canHelp = true;
    private bool itemsChanged = false;
    private readonly List<KamerdienstItemType> items = new List<KamerdienstItemType>();

    protected void Update() {
        if (canHelp && !IsWaiting()) {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector2 input = new Vector3(horizontal, vertical, 0f);
            if (input.sqrMagnitude > 1f) {
                input.Normalize();
            }
            rb2d.AddForce(input * moveSpeed * Time.deltaTime, ForceMode2D.Impulse);
        }
    }

    public void DisableHelping() {
        canHelp = false;
        rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public bool CheckItemsChanged() {
        bool ret = itemsChanged;
        itemsChanged = false;
        return ret;
    }

    public void AddItem(KamerdienstItemType item) {
        items.Add(item);
        itemsChanged = true;
        inventory.SetItems(items.ToArray());
    }

    public void ClearItems() {
        itemsChanged = true;
        items.Clear();
        inventory.SetItems(items.ToArray());
    }

    public KamerdienstItemType[] GetItems() {
        return items.ToArray();
    }

    public KamerdienstInventory GetInventory() {
        return inventory;
    }

    public bool IsWaiting() {
        return waitingOnMemberHelp != -1;
    }

    public void WaitOn(int memberId) {
        waitingOnMemberHelp = memberId;
    }

    public bool IsWaitingOn(int memberId) {
        return waitingOnMemberHelp == memberId;
    }

    public void StopWaiting() {
        waitingOnMemberHelp = -1;
    }

    public bool CanHelp() {
        return canHelp;
    }
}
