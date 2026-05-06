using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// One-thumb control with tap-vs-hold:
///   • Tap (<tapThreshold seconds): wisp pulses briefly at finger position,
///     fox does NOT move. Pure visual feedback ("did the world hear me?").
///   • Hold (>=tapThreshold seconds): wisp follows finger, fox tracks wisp.
///     On release, wisp light dims smoothly to zero rather than popping out.
///
/// Works with both the Unity Input System (touch) and legacy mouse (editor testing).
/// </summary>
[RequireComponent(typeof(Light))]
public class WispController : MonoBehaviour
{
    [Header("Wisp Settings")]
    public float wispHeight     = 0.4f;
    public float followSpeed    = 12f;
    public float pulseSpeed     = 2f;
    public float pulseAmplitude = 0.3f;

    [Header("Touch input")]
    [Tooltip("Press shorter than this is treated as a tap (no fox movement).")]
    public float tapThreshold   = 0.15f;
    [Tooltip("How long the smooth-dim takes when releasing a hold.")]
    public float dimDuration    = 0.3f;
    [Tooltip("Tap pulse total duration (scale + intensity choreography).")]
    public float tapPulseDuration   = 0.4f;
    [Tooltip("Peak scale of the orb during a tap pulse (1 = no scale change).")]
    public float tapPulseScalePeak  = 1.45f;
    [Tooltip("Peak intensity multiplier of the light during a tap pulse.")]
    public float tapPulseIntensity  = 1.6f;

    [Header("References")]
    public Camera mainCamera;
    public FoxController fox;
    public ParticleSystem wispParticles;

    Light _light;
    WispMeshBuilder _meshBuilder;
    Vector3 _targetWorldPos;
    Vector3 _orbBaseScale = Vector3.one;

    // Plane at fox-ground level for raycasting finger → world
    readonly Plane _groundPlane = new Plane(Vector3.up, Vector3.zero);

    bool _isActive;            // visual is currently shown
    bool _wasPressed;          // input was pressed last frame
    float _pressDownTime;      // Time.time when current press started
    bool _foxCommanded;        // we've already set fox.WispTarget for this hold
    float _baseIntensity;
    Coroutine _activeRoutine;

    void Awake()
    {
        _light = GetComponent<Light>();
        _baseIntensity = _light.intensity;
        _meshBuilder = GetComponent<WispMeshBuilder>();
        if (_meshBuilder != null && _meshBuilder.Renderer != null)
            _orbBaseScale = _meshBuilder.Renderer.transform.localScale;
        SetActive(false);
    }

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Update()
    {
        HandleInput();

        if (_isActive && _activeRoutine == null) // skip drift while a routine owns the values
        {
            // Smooth follow finger
            transform.position = Vector3.Lerp(
                transform.position, _targetWorldPos, Time.deltaTime * followSpeed);

            // Idle pulse (only while held — taps and dim use their own coroutines)
            if (_wasPressed)
            {
                float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
                _light.intensity = _baseIntensity * pulse;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Input
    // ─────────────────────────────────────────────────────────────────────

    void HandleInput()
    {
        bool pressed = ReadInputPressed(out Vector2 screenPos);

        if (pressed)
        {
            if (!_wasPressed) OnPressDown(screenPos);
            else              OnPressed(screenPos);
        }
        else if (_wasPressed)
        {
            OnReleased();
        }
    }

    void OnPressDown(Vector2 screenPos)
    {
        _wasPressed = true;
        _pressDownTime = Time.time;
        _foxCommanded = false;

        // Cancel any in-flight dim/tap routine — new press takes priority.
        if (_activeRoutine != null) { StopCoroutine(_activeRoutine); _activeRoutine = null; }

        // Snap visual to finger position immediately (no easing on first appearance).
        if (TryProjectToGround(screenPos, out Vector3 hit))
        {
            transform.position = hit;
            _targetWorldPos = hit;
        }
        ShowVisualWithoutFoxCommand();
    }

    void OnPressed(Vector2 screenPos)
    {
        if (TryProjectToGround(screenPos, out Vector3 hit))
            _targetWorldPos = hit;

        // After tapThreshold elapsed, this is a hold — start commanding fox.
        if (!_foxCommanded && Time.time - _pressDownTime >= tapThreshold)
        {
            _foxCommanded = true;
            if (fox != null) fox.WispTarget = transform;
        }
    }

    void OnReleased()
    {
        _wasPressed = false;
        float pressDuration = Time.time - _pressDownTime;

        // Stop fox immediately on any release — even taps don't keep fox running.
        if (fox != null) fox.WispTarget = null;

        if (pressDuration < tapThreshold)
        {
            // Quick tap — pulse-only feedback, no full hide-fade afterwards.
            _activeRoutine = StartCoroutine(TapPulseRoutine());
        }
        else
        {
            // Hold release — smooth-dim the light to zero, then deactivate.
            _activeRoutine = StartCoroutine(DimAndDeactivateRoutine());
        }
    }

    bool ReadInputPressed(out Vector2 screenPos)
    {
        screenPos = default;
#if ENABLE_INPUT_SYSTEM
        var touchscreen = Touchscreen.current;
        if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
        {
            screenPos = touchscreen.primaryTouch.position.ReadValue();
            return true;
        }
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.isPressed)
        {
            screenPos = mouse.position.ReadValue();
            return true;
        }
        return false;
#else
        if (Input.touchCount > 0)
        {
            screenPos = Input.GetTouch(0).position;
            return true;
        }
        if (Input.GetMouseButton(0))
        {
            screenPos = (Vector2)Input.mousePosition;
            return true;
        }
        return false;
#endif
    }

    bool TryProjectToGround(Vector2 screenPos, out Vector3 worldPos)
    {
        worldPos = default;
        if (mainCamera == null) return false;
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0));
        if (_groundPlane.Raycast(ray, out float dist))
        {
            var hit = ray.GetPoint(dist);
            worldPos = new Vector3(hit.x, wispHeight, hit.z);
            return true;
        }
        return false;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Visibility helpers
    // ─────────────────────────────────────────────────────────────────────

    void ShowVisualWithoutFoxCommand()
    {
        SetActiveVisual(true);
        _light.intensity = _baseIntensity;
        if (_meshBuilder != null && _meshBuilder.Renderer != null)
            _meshBuilder.Renderer.transform.localScale = _orbBaseScale;
    }

    void SetActive(bool active)
    {
        SetActiveVisual(active);
        if (fox != null) fox.WispTarget = active ? transform : null;
    }

    void SetActiveVisual(bool active)
    {
        _isActive = active;
        _light.enabled = active;
        if (_meshBuilder != null && _meshBuilder.Renderer != null)
            _meshBuilder.Renderer.enabled = active;
        if (wispParticles != null)
        {
            if (active) wispParticles.Play();
            else        wispParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Coroutines — tap pulse + smooth dim
    // ─────────────────────────────────────────────────────────────────────

    IEnumerator TapPulseRoutine()
    {
        // Sin-arc choreography: scale and intensity both rise to a peak then return.
        // The wisp stays visible the whole pulse, then fades in the last 25%.
        float t = 0f;
        Vector3 baseScale = _orbBaseScale;
        Transform orb = (_meshBuilder != null && _meshBuilder.Renderer != null)
                        ? _meshBuilder.Renderer.transform : null;

        while (t < tapPulseDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / tapPulseDuration);

            // 0..1 sin half-arc — peak at k=0.5
            float arc = Mathf.Sin(k * Mathf.PI);

            // Scale: 1.0 → tapPulseScalePeak → 1.0
            float scale = Mathf.Lerp(1f, tapPulseScalePeak, arc);
            if (orb != null) orb.localScale = baseScale * scale;

            // Intensity: full → peak → fade. Fade kicks in last quarter.
            float fade = (k > 0.75f) ? Mathf.Lerp(1f, 0f, (k - 0.75f) * 4f) : 1f;
            float bright = Mathf.Lerp(1f, tapPulseIntensity, arc) * fade;
            _light.intensity = _baseIntensity * bright;

            yield return null;
        }

        if (orb != null) orb.localScale = baseScale;
        _light.intensity = _baseIntensity;
        SetActiveVisual(false);
        _activeRoutine = null;
    }

    IEnumerator DimAndDeactivateRoutine()
    {
        float startIntensity = _light.intensity;
        float t = 0f;
        while (t < dimDuration)
        {
            t += Time.deltaTime;
            _light.intensity = Mathf.Lerp(startIntensity, 0f, t / dimDuration);
            yield return null;
        }
        SetActiveVisual(false);
        _light.intensity = _baseIntensity; // restore for next show
        _activeRoutine = null;
    }
}
