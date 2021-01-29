using UnityEngine;
using UnityEngine.UI;

public class KamerdienstMember : MonoBehaviour {
    [SerializeField]
    private Text scoreText;
    [SerializeField]
    private KamerdienstInventory inventory;
    [SerializeField]
    private TextBalloon helpedByTextBalloon;

    private float moveDuration = 3f;
    private float moveT = 0f;
    private bool moving = true;
    private bool wasHelped = false;
    private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private B11PartyClient client;
    private KamerdienstCharacter meCharacter;
    private int memberId;
    private Vector3 startPosition;
    private Vector3 locationPosition;
    private KamerdienstItemType[] items;

    public void Initialize(B11PartyClient client, KamerdienstCharacter meCharacter, int memberId, Vector3 locationPosition, KamerdienstItemType[] items, int points) {
        this.client = client;
        this.meCharacter = meCharacter;
        this.memberId = memberId;
        this.locationPosition = locationPosition;
        this.items = items;

        startPosition = transform.position;
        scoreText.text = points.ToString();
        inventory.SetItems(items);
        helpedByTextBalloon.gameObject.SetActive(false);
    }

    protected void Update() {
        // Move slowly towards (away from) location position
        if (moving) {
            moveT += Time.deltaTime / moveDuration;
            if (moveT > 1f) {
                moving = false;
                moveT = 1f;
                if (wasHelped) {
                    gameObject.SetActive(false);
                }
            }
            var from = wasHelped ? locationPosition : startPosition;
            var to = wasHelped ? startPosition : locationPosition;
            transform.position = Vector3.Lerp(from, to, moveCurve.Evaluate(moveT));
        }

        bool isInRangeAndMatching = !moving && !wasHelped && IsMeInRange() && KamerdienstInventory.IsMatch(meCharacter.GetInventory(), inventory);
        float targetScale = isInRangeAndMatching ? 1.2f : 1f;
        float currentScale = transform.localScale.x;
        if (Mathf.Abs(targetScale - currentScale) > 0.001f) {
            transform.localScale = Vector3.one * Mathf.Lerp(currentScale, targetScale, 0.05f);
        } else {
            transform.localScale = Vector3.one * targetScale;
        }

        if (Input.GetKeyDown(KeyCode.Space)
            && !meCharacter.IsWaiting()
            && meCharacter.CanHelp()
            && isInRangeAndMatching
        ) {
            meCharacter.WaitOn(memberId);
            client.GetKarmanClient().Send(new KamerdienstMemberHelpedPacket(memberId, client.GetMe().GetClientId()));
        }
    }

    public bool IsMeInRange() {
        float range = 1f;
        return (meCharacter.transform.position - transform.position).sqrMagnitude < range * range;
    }

    public void WasHelped(bool wasHelpedByMe, string nameOfHelper) {
        if (!wasHelped) {
            moveT = 0f;
            moving = true;
            wasHelped = true;

            if (meCharacter.IsWaitingOn(memberId)) {
                meCharacter.StopWaiting();
                if (wasHelpedByMe) {
                    meCharacter.ClearItems();
                }
            }
            helpedByTextBalloon.gameObject.SetActive(true);
            helpedByTextBalloon.SetText($"Bedankt, {nameOfHelper}");
        }
    }
}
