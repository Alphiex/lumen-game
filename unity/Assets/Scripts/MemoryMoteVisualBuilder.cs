using UnityEngine;

/// <summary>
/// Procedural emissive orb visual for a MemoryMote — a child sphere with a URP
/// HDR-emissive material that bobs gently in place. Mirrors WispMeshBuilder.
///
/// [ExecuteAlways] so the orb appears in the editor scene view without entering
/// Play mode. Builds idempotently: re-enabling the script doesn't duplicate the orb.
/// </summary>
[ExecuteAlways]
public class MemoryMoteVisualBuilder : MonoBehaviour
{
    [Header("Look")]
    [Tooltip("Pale-gold base color. Default warm amber.")]
    public Color baseColor = new Color(1.0f, 0.85f, 0.3f, 1.0f);

    [Tooltip("Emission multiplier on top of base color — drives URP bloom.")]
    public float emissionIntensity = 3.5f;

    [Tooltip("Sphere local scale.")]
    public float orbScale = 0.3f;

    [Header("Bob")]
    [Tooltip("Cycles per second of the local-Y bob.")]
    public float bobHz = 0.7f;

    [Tooltip("Vertical amplitude of the bob in world units.")]
    public float bobAmplitude = 0.08f;

    public MeshRenderer Renderer { get; private set; }

    Material _emissiveMat;
    Transform _orb;
    float _orbBaseY;
    float _phaseOffset;

    void Awake()
    {
        // Stagger each mote's bob phase so all 10 don't rise/fall in unison.
        // siblingIndex × 0.41 rad ≈ golden-angle-ish spread across 2π for 10 motes.
        _phaseOffset = transform.GetSiblingIndex() * 0.41f;
        EnsureBuilt();
    }

    void OnEnable()
    {
        // Scripts disable/re-enable in [ExecuteAlways] when scripts recompile —
        // re-cache child if it survived.
        EnsureBuilt();
    }

    void OnValidate()
    {
        if (_emissiveMat != null) ApplyColors();
        if (_orb != null) _orb.localScale = Vector3.one * orbScale;
    }

    void Update()
    {
        if (_orb == null) return;
        float dy = Mathf.Sin(Time.time * bobHz * Mathf.PI * 2f + _phaseOffset) * bobAmplitude;
        _orb.localPosition = new Vector3(0f, _orbBaseY + dy, 0f);
    }

    void OnDestroy()
    {
        // Clean up the spawned orb if the script (or its GO) is destroyed,
        // so we don't leave a dangling visual child behind.
        if (_orb != null)
        {
            if (Application.isPlaying) Destroy(_orb.gameObject);
            else                       DestroyImmediate(_orb.gameObject);
        }
    }

    [ContextMenu("Rebuild Mote Orb")]
    void RebuildMoteOrb()
    {
        var existing = transform.Find("MoteOrb");
        if (existing != null) DestroyImmediate(existing.gameObject);
        _orb = null;
        EnsureBuilt();
    }

    void EnsureBuilt()
    {
        var existing = transform.Find("MoteOrb");
        if (existing != null)
        {
            _orb = existing;
            Renderer = existing.GetComponent<MeshRenderer>();
            if (_emissiveMat == null && Renderer != null) _emissiveMat = Renderer.sharedMaterial;
            _orbBaseY = 0f;
            return;
        }

        var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name = "MoteOrb";
        var col = orb.GetComponent<Collider>();
        if (col != null) DestroyImmediate(col); // Mote already has the trigger SphereCollider on its root

        orb.transform.SetParent(transform, worldPositionStays: false);
        orb.transform.localPosition = Vector3.zero;
        orb.transform.localScale = Vector3.one * orbScale;

        _orb = orb.transform;
        _orbBaseY = 0f;
        Renderer = orb.GetComponent<MeshRenderer>();

        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        _emissiveMat = new Material(shader) { name = "Mote_Emissive" };
        ApplyColors();
        Renderer.sharedMaterial = _emissiveMat;
        Renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    void ApplyColors()
    {
        if (_emissiveMat == null) return;
        _emissiveMat.color = baseColor;
        _emissiveMat.SetColor("_BaseColor", baseColor);
        _emissiveMat.EnableKeyword("_EMISSION");
        _emissiveMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        var hdr = baseColor * emissionIntensity;
        _emissiveMat.SetColor("_EmissionColor", hdr);
    }
}
