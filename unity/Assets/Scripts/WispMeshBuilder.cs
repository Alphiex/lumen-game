using UnityEngine;

/// <summary>
/// Procedural visual for the wisp light: a small URP-emissive sphere child.
/// Mirrors the FoxMeshBuilder pattern — runs in [ExecuteAlways] so the visual
/// is present in editor scene view without entering Play mode.
///
/// WispController.SetActive(bool) toggles this builder's MeshRenderer so the
/// orb appears/disappears in lockstep with the Light.
/// </summary>
[ExecuteAlways]
public class WispMeshBuilder : MonoBehaviour
{
    [Header("Look")]
    [Tooltip("Pale cyan-gold base color (HDR-friendly). Default #B8F0FF.")]
    public Color baseColor = new Color(0.722f, 0.941f, 1.0f, 1.0f);

    [Tooltip("Emission multiplier on top of base color — drives URP bloom.")]
    public float emissionIntensity = 2.5f;

    [Tooltip("Sphere local scale.")]
    public float orbScale = 0.18f;

    /// <summary>Renderer for the orb child — used by WispController to toggle visibility.</summary>
    public MeshRenderer Renderer { get; private set; }

    Material _emissiveMat;

    void Awake()
    {
        EnsureBuilt();
    }

    void OnValidate()
    {
        // In editor, refresh material color when properties change
        if (_emissiveMat != null) ApplyColors();
        if (Renderer != null) Renderer.transform.localScale = Vector3.one * orbScale;
    }

    void EnsureBuilt()
    {
        var existing = transform.Find("WispOrb");
        if (existing != null)
        {
            Renderer = existing.GetComponent<MeshRenderer>();
            if (_emissiveMat == null && Renderer != null) _emissiveMat = Renderer.sharedMaterial;
            return;
        }

        // Build the orb child
        var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name = "WispOrb";
        // Strip the auto-added collider — orb is purely visual
        var col = orb.GetComponent<Collider>();
        if (col != null) DestroyImmediate(col);

        orb.transform.SetParent(transform, worldPositionStays: false);
        orb.transform.localPosition = Vector3.zero;
        orb.transform.localScale = Vector3.one * orbScale;

        Renderer = orb.GetComponent<MeshRenderer>();

        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        _emissiveMat = new Material(shader) { name = "Wisp_Emissive" };
        ApplyColors();
        Renderer.sharedMaterial = _emissiveMat;
        // Drop shadows on the orb — emissive things shouldn't cast strong shadows
        Renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    void ApplyColors()
    {
        if (_emissiveMat == null) return;
        _emissiveMat.color = baseColor;                                 // _BaseColor on URP/Lit
        _emissiveMat.SetColor("_BaseColor", baseColor);
        _emissiveMat.EnableKeyword("_EMISSION");
        _emissiveMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        // HDR emission — base × intensity
        var hdr = baseColor * emissionIntensity;
        _emissiveMat.SetColor("_EmissionColor", hdr);
    }
}
