using UnityEngine;

/// <summary>
/// Procedural low-poly fox body built from Unity primitives.
/// Spawns once on Awake and parents all parts under this GameObject as children
/// named so ProceduralFoxAnimator can find them by name (Body, Head, EarL/R, TailTip, LegFL/FR/BL/BR).
///
/// Replace this with a real fox FBX later by deleting the spawned children and dragging in a rigged mesh.
/// </summary>
[ExecuteAlways]
public class FoxMeshBuilder : MonoBehaviour
{
    [Header("Look")]
    public Color furColor    = new Color(0.82f, 0.42f, 0.12f);
    public Color bellyColor  = new Color(0.95f, 0.92f, 0.85f);
    public Color noseColor   = new Color(0.10f, 0.08f, 0.08f);

    Material _furMat;
    Material _bellyMat;
    Material _noseMat;

    void Awake()
    {
        if (transform.Find("FoxBody") != null) return; // already built
        Build();
    }

    void Build()
    {
        EnsureMaterials();

        // Body: capsule rotated so its long axis is along Z (forward).
        var body = Spawn(PrimitiveType.Capsule, "FoxBody",
            pos: new Vector3(0f, 0.30f, 0f),
            scale: new Vector3(0.40f, 0.22f, 0.22f),
            euler: new Vector3(90f, 0f, 0f),
            mat: _furMat);

        // Head
        var head = Spawn(PrimitiveType.Sphere, "Head",
            pos: new Vector3(0f, 0.52f, 0.22f),
            scale: new Vector3(0.18f, 0.16f, 0.16f),
            mat: _furMat);

        // Snout
        Spawn(PrimitiveType.Cube, "Snout",
            pos: new Vector3(0f, 0.50f, 0.34f),
            scale: new Vector3(0.08f, 0.06f, 0.12f),
            mat: _noseMat,
            parent: head.transform);

        // Ears — slight outward tilt
        Spawn(PrimitiveType.Cube, "EarL",
            pos: new Vector3(-0.10f, 0.64f, 0.22f),
            scale: new Vector3(0.06f, 0.12f, 0.03f),
            euler: new Vector3(0f, 0f, 15f),
            mat: _furMat);

        Spawn(PrimitiveType.Cube, "EarR",
            pos: new Vector3(0.10f, 0.64f, 0.22f),
            scale: new Vector3(0.06f, 0.12f, 0.03f),
            euler: new Vector3(0f, 0f, -15f),
            mat: _furMat);

        // Tail
        Spawn(PrimitiveType.Sphere, "TailBase",
            pos: new Vector3(0f, 0.32f, -0.22f),
            scale: new Vector3(0.10f, 0.10f, 0.10f),
            mat: _furMat);

        Spawn(PrimitiveType.Sphere, "TailTip",
            pos: new Vector3(0f, 0.44f, -0.34f),
            scale: new Vector3(0.14f, 0.14f, 0.14f),
            mat: _bellyMat); // fluffy white tip

        // Legs — front and back, left and right
        Spawn(PrimitiveType.Capsule, "LegFL",
            pos: new Vector3(-0.10f, 0.12f, 0.18f),
            scale: new Vector3(0.07f, 0.12f, 0.07f),
            mat: _furMat);
        Spawn(PrimitiveType.Capsule, "LegFR",
            pos: new Vector3(0.10f, 0.12f, 0.18f),
            scale: new Vector3(0.07f, 0.12f, 0.07f),
            mat: _furMat);
        Spawn(PrimitiveType.Capsule, "LegBL",
            pos: new Vector3(-0.10f, 0.12f, -0.16f),
            scale: new Vector3(0.07f, 0.12f, 0.07f),
            mat: _furMat);
        Spawn(PrimitiveType.Capsule, "LegBR",
            pos: new Vector3(0.10f, 0.12f, -0.16f),
            scale: new Vector3(0.07f, 0.12f, 0.07f),
            mat: _furMat);
    }

    GameObject Spawn(PrimitiveType type, string name, Vector3 pos, Vector3 scale,
                     Material mat, Vector3 euler = default, Transform parent = null)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        var col = go.GetComponent<Collider>();
        if (col != null) DestroyImmediate(col); // colliders only on the root

        var t = go.transform;
        t.SetParent(parent != null ? parent : transform, worldPositionStays: false);
        t.localPosition = pos;
        t.localEulerAngles = euler;
        t.localScale = scale;

        var r = go.GetComponent<MeshRenderer>();
        if (r != null && mat != null) r.sharedMaterial = mat;
        return go;
    }

    void EnsureMaterials()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        _furMat   = new Material(shader) { name = "Fox_Fur",   color = furColor };
        _bellyMat = new Material(shader) { name = "Fox_Belly", color = bellyColor };
        _noseMat  = new Material(shader) { name = "Fox_Nose",  color = noseColor };
    }
}
