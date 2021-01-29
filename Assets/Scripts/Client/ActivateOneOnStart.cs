using UnityEngine;

public class ActivateOneOnStart : MonoBehaviour {
    [SerializeField]
    private GameObject[] options;

    protected void Start() {
        int randomIndex = Random.Range(0, options.Length);
        options[randomIndex].SetActive(true);
    }
}
