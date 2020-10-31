using UnityEngine;

public class RedCupTable : MonoBehaviour {
    [SerializeField]
    private SpriteRenderer player = default;

    [SerializeField]
    protected Transform ballSpawnPoint = default;
    [SerializeField]
    protected Transform ball = default;
    [SerializeField]
    protected Transform[] cups = default;

    public virtual void SetFrom(B11PartyClient.B11Client client) {
        ball.transform.position = ballSpawnPoint.position;
        player.sprite = client.GetSprite();
    }

    public void SetCupHit(int cupId) {
        cups[cupId].gameObject.SetActive(false);
    }

    public void SetBallPosition(Vector2 position) {
        ball.transform.position = position;
    }

    public void SetFinished() {
        ball.transform.position = ballSpawnPoint.position;
        ball.gameObject.SetActive(false);
    }
}