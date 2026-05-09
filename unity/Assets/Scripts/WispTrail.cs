using UnityEngine;

/// <summary>
/// Builds a child "WispTrail" GameObject with a TrailRenderer that emits a short
/// pale-cyan emissive trail behind the wisp as it moves. Trail emission mirrors
/// the WispLight's enabled state so it activates/deactivates in lockstep with
/// the wisp visual (no direct dependency on WispController internals).
///
/// Pattern matches WispMeshBuilder / MemoryMoteVisualBuilder / BiomeGateVisualBuilder:
/// [ExecuteAlways], idempotent EnsureBuilt, OnDestroy cleanup, ContextMenu rebuild.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Light))]
public class WispTrail : MonoBehaviour
{
    [Header("Look")]
    [Tooltip("Pale-cyan trail base color (HDR-friendly). Default #00FFFF.")]
    public Color trailColor = new Color(0f, 1f, 1f, 1f);

    [Tooltip("Emission multiplier on top of base color — drives URP bloom.")]
    public float emissionIntensity = 2.5f;

    [Header("Trail")]
    [Tooltip("Total time a trail point lives (also defines length at given speed).")]
    public float trailTime = 0.4f;

    [Tooltip("Trail width at the head (where the wisp currently is).")]
    public float startWidth = 0.05f;

    [Tooltip("Trail width at the tail (where the trail fades out).")]
    public float endWidth = 0f;

    [Tooltip("Minimum distance between trail vertices — lower = smoother but more verts.")]
    public float minVertexDistance = 0.05f;

    public TrailRenderer Trail { get; private set; }

    Light _light;
    Transform _trailGO;
    Material _trailMat;

    void Awake()
    {
        _light = GetComponent<Light>();
        EnsureBuilt();
    }

    void OnEnable()
    {
        if (_light == null) _light = GetComponent<Light>();
        EnsureBuilt();
    }

    void OnValidate()
    {
        if (Trail != null)
        {
            Trail.time = trailTime;
            Trail.startWidth = startWidth;
            Trail.endWidth = endWidth;
            Trail.minVertexDistance = minVertexDistance;
            Trail.colorGradient = BuildGradient();
        }
        if (_trailMat != null) ApplyMaterial();
    }

    void Update()
    {
        // Mirror trail emission to the wisp's Light enabled state. When WispController
        // toggles the light off (release / dim coroutine end), the trail stops emitting
        // new points but existing points continue to fade out for `trailTime` seconds.
        if (Trail != null && _light != null && Trail.emitting != _light.enabled)
            Trail.emitting = _light.enabled;
    }

    void OnDestroy()
    {
        if (_trailGO == null) return;
        if (Application.isPlaying) Destroy(_trailGO.gameObject);
        else                       DestroyImmediate(_trailGO.gameObject);
    }

    [ContextMenu("Rebuild Wisp Trail")]
    void RebuildWispTrail()
    {
        var existing = transform.Find("WispTrail");
        if (existing != null) DestroyImmediate(existing.gameObject);
        _trailGO = null;
        Trail = null;
        _trailMat = null;
        EnsureBuilt();
    }

    void EnsureBuilt()
    {
        var existing = transform.Find("WispTrail");
        if (existing != null)
        {
            _trailGO = existing;
            Trail = existing.GetComponent<TrailRenderer>();
            if (_trailMat == null && Trail != null) _trailMat = Trail.sharedMaterial;
            return;
        }

        var go = new GameObject("WispTrail");
        go.transform.SetParent(transform, worldPositionStays: false);
        go.transform.localPosition = Vector3.zero;
        _trailGO = go.transform;

        Trail = go.AddComponent<TrailRenderer>();
        Trail.time = trailTime;
        Trail.startWidth = startWidth;
        Trail.endWidth = endWidth;
        Trail.minVertexDistance = minVertexDistance;
        Trail.alignment = LineAlignment.View;          // billboards toward the camera
        Trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Trail.receiveShadows = false;
        Trail.emitting = false;                         // off until WispController turns light on
        Trail.colorGradient = BuildGradient();

        // Pick the right shader for a particle-class trail. URP/Particles/Unlit
        // is the canonical choice for emissive trails — gets bloom for free.
        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        _trailMat = new Material(shader) { name = "Wisp_Trail" };
        ApplyMaterial();
        Trail.sharedMaterial = _trailMat;
    }

    Gradient BuildGradient()
    {
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(trailColor, 0f), new GradientColorKey(trailColor, 1f) },
            new[] { new GradientAlphaKey(1f, 0f),         new GradientAlphaKey(0f, 1f) }
        );
        return g;
    }

    void ApplyMaterial()
    {
        if (_trailMat == null) return;
        _trailMat.color = trailColor;
        _trailMat.SetColor("_BaseColor", trailColor);
        _trailMat.EnableKeyword("_EMISSION");
        _trailMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        _trailMat.SetColor("_EmissionColor", trailColor * emissionIntensity);
    }
}
