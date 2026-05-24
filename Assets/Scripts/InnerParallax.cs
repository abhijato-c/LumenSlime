using UnityEngine;

public class InnerParallax : MonoBehaviour {
    public float ParallaxFactor;
    public float StartY;
    void LateUpdate() {
        transform.localPosition = new Vector3(
            0f,
            StartY - (transform.parent.localPosition.y * ParallaxFactor),
            transform.localPosition.z
        );
    }
}
