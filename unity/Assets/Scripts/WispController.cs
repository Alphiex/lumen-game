using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// One-thumb control: touch/hold anywhere → wisp appears at finger world position,
/// fox tracks the wisp. Release → wisp hides, fox stops.
/// Works with both Unity Input System (touch) and mouse (editor testing).
/// </summary>
[RequireComponent(typeof(Light))]
public class WispController : MonoBehaviour
{
    [Header("Wisp Settings")]
    public float wispHeight = 0.4f;
    public float followSpeed = 12f;
    public float pulseSpeed = 2f;
    public float pulseAmplitude = 0.3f;

    [Header("References")]
    public Camera mainCamera;
    public FoxController fox;
    public ParticleSystem wispParticles;

    Light _light;
    Vector3 _targetWorldPos;
    bool _isActive;
    float _baseIntensity;

    // Plane at fox-ground level for ray casting finger → world
    readonly Plane _groundPlane = new Plane(Vector3.up, Vector3.zero);

    void Awake()
    {
        _light = GetComponent<Light>();
        _baseIntensity = _light.intensity;
        SetActive(false);
    }

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Update()
    {
        HandleInput();

        if (_isActive)
        {
            // Smooth follow
            transform.position = Vector3.Lerp(
                transform.position, _targetWorldPos, Time.deltaTime * followSpeed);

            // Idle pulse
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
            _light.intensity = _baseIntensity * pulse;
        }
    }

    void HandleInput()
    {
#if ENABLE_INPUT_SYSTEM
        // New Input System path
        var touchscreen = Touchscreen.current;
        if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
        {
            Vector2 screenPos = touchscreen.primaryTouch.position.ReadValue();
            MoveWispToScreen(screenPos);
            return;
        }

        // Mouse fallback (editor)
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.isPressed)
        {
            MoveWispToScreen(mouse.position.ReadValue());
            return;
        }
#else
        // Legacy Input path
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            MoveWispToScreen(t.position);
            return;
        }
        if (Input.GetMouseButton(0))
        {
            MoveWispToScreen(Input.mousePosition);
            return;
        }
#endif
        // No input — hide wisp
        if (_isActive) SetActive(false);
    }

    void MoveWispToScreen(Vector2 screenPos)
    {
        if (!_isActive) SetActive(true);

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0));
        if (_groundPlane.Raycast(ray, out float dist))
        {
            Vector3 hit = ray.GetPoint(dist);
            _targetWorldPos = new Vector3(hit.x, wispHeight, hit.z);
        }
    }

    void SetActive(bool active)
    {
        _isActive = active;
        _light.enabled = active;
        if (wispParticles)
        {
            if (active) wispParticles.Play();
            else wispParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        // Tell the fox
        if (fox != null)
            fox.WispTarget = active ? transform : null;
    }
}
