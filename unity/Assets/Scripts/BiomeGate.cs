using UnityEngine;

/// <summary>
/// Biome gate — when the fox enters this trigger, the loop completes successfully.
/// Place a large trigger collider (box or cylinder) at the end of the biome path.
/// </summary>
public class BiomeGate : MonoBehaviour
{
    [Header("FX")]
    public ParticleSystem completionBurst;
    public Light gateLight;
    public float gatePulseSpeed = 1.5f;
    public float gatePulseAmplitude = 0.4f;

    bool _triggered;
    float _baseLightIntensity;

    void Start()
    {
        if (gateLight) _baseLightIntensity = gateLight.intensity;
    }

    void Update()
    {
        // Gate pulses gently to draw attention
        if (gateLight && !_triggered)
        {
            float pulse = 1f + Mathf.Sin(Time.time * gatePulseSpeed) * gatePulseAmplitude;
            gateLight.intensity = _baseLightIntensity * pulse;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Fox")) return;

        _triggered = true;

        if (completionBurst)
            completionBurst.Play();

        // Stop gate pulse
        if (gateLight) gateLight.intensity = _baseLightIntensity * 2f;

        // Freeze fox
        var fox = other.GetComponent<FoxController>();
        fox?.FreezeForLoopEnd();

        DaylightManager.Instance?.CompleteLoop();
    }
}
