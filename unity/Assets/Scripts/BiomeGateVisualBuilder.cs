using System.Collections;
using UnityEngine;

/// <summary>
/// Procedural arch visual for the BiomeGate — 4 warm-gold HDR-emissive pillar
/// spheres + a child "GateLight" point light. Mirrors the Builder pattern used
/// by WispMeshBuilder and MemoryMoteVisualBuilder.
///
/// On fox proximity (distance &lt; proximityRadius), spikes the GateLight
/// intensity to peakMultiplier × base, then eases back. Hysteresis (1.3×) on
/// re-entry prevents flicker when the fox lingers at the radius edge.
/// </summary>
[ExecuteAlways]
public class BiomeGateVisualBuilder : MonoBehaviour
{
    [Header("Look")]
    public Color pillarColor      = new Color(1.0f, 0.7f, 0.2f, 1.0f);
    public float emissionIntensity = 4.0f;
    public float pillarScale      = 0.4f;

    [Header("Layout")]
    [Tooltip("Horizontal half-distance from gate center to pillar.")]
    public float pillarXOffset = 4f;
    [Tooltip("Vertical offset of lower/upper pillar pair from gate center.")]
    public float pillarYStack  = 1f;

    [Header("Gate Light")]
    public Color gateLightColor      = new Color(1.0f, 0.8f, 0.4f, 1.0f);
    public float gateLightIntensity  = 1.8f;
    public float gateLightRange      = 12f;

    [Header("Proximity Pulse")]
    [Tooltip("Radius around gate within which fox triggers a pulse.")]
    public float proximityRadius   = 20f;
    [Tooltip("Peak multiplier on top of gateLightIntensity at the apex of the pulse.")]
    public float pulsePeakMultiplier = 3f;
    public float pulseRiseTime = 0.2f;
    public float pulseFadeTime = 0.6f;

    Light _gateLight;
    bool _pulsing;
    bool _foxWithin; // hysteresis state — true while fox is inside the (extended) zone
    Transform _fox;
    Material _emissiveMat;

    static readonly string[] kPillarNames =
    {
        "PillarL_Lower", "PillarL_Upper", "PillarR_Lower", "PillarR_Upper",
    };

    void Awake()
    {
        EnsureBuilt();
    }

    void OnEnable()
    {
        EnsureBuilt();
        TryFindFox();
    }

    void Start()
    {
        TryFindFox();
    }

    void OnValidate()
    {
        if (_emissiveMat != null) ApplyPillarColors();
        if (_gateLight != null)
        {
            _gateLight.color    = gateLightColor;
            _gateLight.intensity = gateLightIntensity;
            _gateLight.range    = gateLightRange;
        }
    }

    void Update()
    {
        if (_fox == null)
        {
            // Try to re-find the fox if it appeared after Start (e.g. spawn-on-load).
            TryFindFox();
            if (_fox == null) return;
        }

        float dist = Vector3.Distance(transform.position, _fox.position);

        if (!_foxWithin && !_pulsing && dist < proximityRadius)
        {
            _foxWithin = true;
            StartCoroutine(ProximityPulse());
        }
        else if (_foxWithin && dist > proximityRadius * 1.3f)
        {
            // Fox left the (extended) zone — re-arm so a fresh entry can re-fire.
            _foxWithin = false;
        }
    }

    void OnDestroy()
    {
        // Clean up spawned children so we don't leave dangling visual junk.
        DestroyChildIfExists("GateLight");
        for (int i = 0; i < kPillarNames.Length; i++) DestroyChildIfExists(kPillarNames[i]);
    }

    [ContextMenu("Rebuild Gate Visual")]
    void RebuildGateVisual()
    {
        DestroyChildIfExists("GateLight");
        for (int i = 0; i < kPillarNames.Length; i++) DestroyChildIfExists(kPillarNames[i]);
        _gateLight = null;
        _emissiveMat = null;
        EnsureBuilt();
    }

    // ─────────────────────────────────────────────────────────────────────
    // Build
    // ─────────────────────────────────────────────────────────────────────

    void EnsureBuilt()
    {
        // Pillars
        var pillarPositions = new Vector3[]
        {
            new Vector3(-pillarXOffset, -pillarYStack, 0f),
            new Vector3(-pillarXOffset,  pillarYStack, 0f),
            new Vector3( pillarXOffset, -pillarYStack, 0f),
            new Vector3( pillarXOffset,  pillarYStack, 0f),
        };

        for (int i = 0; i < kPillarNames.Length; i++)
        {
            var t = transform.Find(kPillarNames[i]);
            if (t == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = kPillarNames[i];
                go.tag = "Untagged";
                var col = go.GetComponent<Collider>();
                if (col != null) DestroyImmediate(col);
                go.transform.SetParent(transform, worldPositionStays: false);
                go.transform.localPosition = pillarPositions[i];
                go.transform.localScale = Vector3.one * pillarScale;

                var r = go.GetComponent<MeshRenderer>();
                if (_emissiveMat == null) _emissiveMat = MakeEmissiveMaterial();
                r.sharedMaterial = _emissiveMat;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        // Single shared material — find/cache it from any pillar so OnValidate works.
        if (_emissiveMat == null)
        {
            var first = transform.Find(kPillarNames[0]);
            if (first != null)
            {
                var r = first.GetComponent<MeshRenderer>();
                if (r != null) _emissiveMat = r.sharedMaterial;
            }
        }

        // Gate light
        var lightChild = transform.Find("GateLight");
        if (lightChild == null)
        {
            var lightGO = new GameObject("GateLight");
            lightGO.transform.SetParent(transform, worldPositionStays: false);
            lightGO.transform.localPosition = Vector3.zero;
            _gateLight = lightGO.AddComponent<Light>();
        }
        else
        {
            _gateLight = lightChild.GetComponent<Light>();
        }
        _gateLight.type = LightType.Point;
        _gateLight.color = gateLightColor;
        _gateLight.intensity = gateLightIntensity;
        _gateLight.range = gateLightRange;
        _gateLight.shadows = LightShadows.None;
    }

    Material MakeEmissiveMaterial()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var mat = new Material(shader) { name = "GatePillar_Emissive" };
        ApplyPillarColors(mat);
        return mat;
    }

    void ApplyPillarColors() => ApplyPillarColors(_emissiveMat);

    void ApplyPillarColors(Material mat)
    {
        if (mat == null) return;
        mat.color = pillarColor;
        mat.SetColor("_BaseColor", pillarColor);
        mat.EnableKeyword("_EMISSION");
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        mat.SetColor("_EmissionColor", pillarColor * emissionIntensity);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Proximity pulse
    // ─────────────────────────────────────────────────────────────────────

    void TryFindFox()
    {
        if (_fox != null) return;
        var go = GameObject.FindWithTag("Fox");
        if (go != null) _fox = go.transform;
    }

    IEnumerator ProximityPulse()
    {
        _pulsing = true;
        if (_gateLight == null) { _pulsing = false; yield break; }

        float baseI = gateLightIntensity;
        float peakI = baseI * pulsePeakMultiplier;

        // Rise — linear lerp from base to peak.
        float t = 0f;
        while (t < pulseRiseTime)
        {
            t += Time.deltaTime;
            _gateLight.intensity = Mathf.Lerp(baseI, peakI, Mathf.Clamp01(t / pulseRiseTime));
            yield return null;
        }
        _gateLight.intensity = peakI;

        // Fade — linear back to base over fadeTime.
        t = 0f;
        while (t < pulseFadeTime)
        {
            t += Time.deltaTime;
            _gateLight.intensity = Mathf.Lerp(peakI, baseI, Mathf.Clamp01(t / pulseFadeTime));
            yield return null;
        }
        _gateLight.intensity = baseI;
        _pulsing = false;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────

    void DestroyChildIfExists(string childName)
    {
        var t = transform.Find(childName);
        if (t == null) return;
        if (Application.isPlaying) Destroy(t.gameObject);
        else                       DestroyImmediate(t.gameObject);
    }
}
