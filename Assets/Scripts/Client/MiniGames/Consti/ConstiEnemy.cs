using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ColorShifter), typeof(Collider2D))]
public class ConstiEnemy : MonoBehaviour {
    [SerializeField]
    private Slider deadSlider;

    private ColorShifter colorShifter;
    private bool localEaten = false;
    private bool dead = false;
    private float deadTime;

    protected void Start() {
        colorShifter = GetComponent<ColorShifter>();
        colorShifter.enabled = false;
    }

    public void StartBeingChased() {
        colorShifter.enabled = true;
    }

    public void StopBeingChased() {
        colorShifter.enabled = false;
    }

    public void OnEaten() {
        dead = true;
        deadSlider.value = 1f;
        transform.localScale = Vector3.one * 0.3f;
        deadSlider.gameObject.SetActive(true);
    }

    public bool IsDead() {
        return dead;
    }

    protected void Update() {
        if (dead) {
            deadTime += Time.deltaTime;
            deadSlider.value = deadTime / ConstiServerMiniGame.ChasingDuration;
            if (deadTime >= ConstiServerMiniGame.ChasingDuration) {
                deadTime = 0f;
                dead = false;
                localEaten = false;
                transform.localScale = Vector3.one;
                deadSlider.gameObject.SetActive(false);
            }
        }
    }

    public bool IsLocalEaten() {
        return localEaten;
    }

    public void MarkLocalEaten() {
        localEaten = true;
    }
}
