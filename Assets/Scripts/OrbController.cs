using UnityEngine;
using UnityEngine.Rendering.Universal;

public class OrbController : MonoBehaviour {
    public float speed;
    public float MinIntensity;
    public float MaxIntensity;
    public float MinRange;
    public float MaxRange;

    private Light2D Light;
    void Start() {
        Light = GetComponent<Light2D>();
    }

    void Update() {
        float wave = (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f;
        Light.intensity = Mathf.Lerp(MinIntensity, MaxIntensity, wave);
        Light.pointLightOuterRadius = Mathf.Lerp(MinRange, MaxRange, wave);
    }
}
