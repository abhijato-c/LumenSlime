using UnityEngine;

public class InnerParallax : MonoBehaviour {
    public float ParallaxFactor;
    public Sprite[] InnerBackgrounds;
    public SpriteRenderer Outer;
    public Sprite[] OuterBackgrounds;
    private SpriteRenderer sr;
    private float StartY;
    void Start() {
        sr = GetComponent<SpriteRenderer>();
    }
    void LateUpdate() {
        transform.localPosition = new Vector3(
            0f,
            Mathf.Min((transform.parent.localPosition.y - StartY) * -ParallaxFactor - 10f, -10f),
            transform.localPosition.z
        );
    }

    public void SetIndex(int index, float startY) {
        StartY = startY;
        sr.sprite = InnerBackgrounds[index];
        Outer.sprite = OuterBackgrounds[index];
    }
}
