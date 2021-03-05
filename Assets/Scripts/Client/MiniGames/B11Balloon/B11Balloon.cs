using System;
using UnityEngine;

public class B11Balloon : MonoBehaviour {
    private float currentSize;
    private float max = 1f;
    private bool rotationDirection;
    private float shakeIntensity = 0.1f;
    private float rotation = 0f;

    [SerializeField]
    private GameObject explosionPrefab;

    private Transform currentExplosion;

    public void Reset(float min, float max) {
        if (currentExplosion != null) {
            Destroy(currentExplosion.gameObject);
            currentExplosion = null;
        }
        SetSize(min);
        this.max = max;
        rotation = 0f;
    }

    public void SetSize(float size) {
        currentSize = size;
        transform.localScale = Vector3.one * size;
        shakeIntensity = 1 - Mathf.Pow(1f - (size / max), 2f);
    }

    public void Pop() {
        SetSize(max);
        currentExplosion = Instantiate(explosionPrefab, transform.parent, false).transform;
    }

    public bool WouldInflateCauseAPop(float inflateAmount) {
        return currentSize + inflateAmount >= max;
    }

    public float GetSize() {
        return currentSize;
    }

    protected void Update() {
        float rot = Time.deltaTime;
        rotation += (rotationDirection ? 1 : -1) * rot;
        float maxRotation = 1 - Mathf.Pow(shakeIntensity, 3f) * 0.8f;
        if (rotation >= maxRotation || rotation <= -maxRotation) {
            rotationDirection = !rotationDirection;
            rotation = Mathf.Clamp(rotation, -maxRotation, maxRotation);
        }
        transform.rotation = Quaternion.Euler(0f, 0f, rotation * 40f);
    }
}
