using System;
using UnityEngine;

public class B11BalloonDeadZone : MonoBehaviour {
    internal void Add(SpriteRenderer spriteRenderer) {
        spriteRenderer.gameObject.SetActive(false);
    }
}
