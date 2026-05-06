using UnityEngine;

/// <summary>
/// Drives transform-only procedural animation on the primitive fox built by
/// FoxMeshBuilder. Replaces real bone-skin animation for the prototype phase.
///
/// Knobs you'll want to tune in Unity once you can see it in motion:
///   • idleBreathHz / idleBreathAmp  — how alive the fox feels when standing still
///   • legCycleHzAtRun                — fast cycle = scrappy, slow cycle = stately
///   • bodyRockAmpAtRun               — body tilt during run; 0 = stiff, 0.06 = lively
///   • lookAroundAngleDeg / lookHz    — head sweep size and rate during the look state
/// </summary>
public class ProceduralFoxAnimator : MonoBehaviour
{
    [Header("Idle")]
    public float idleBreathHz   = 0.4f;
    public float idleBreathAmp  = 0.003f;
    public float earFlickPeriod = 3.0f;
    public float tailSwayHz     = 0.3f;
    public float tailSwayAmpDeg = 8f;

    [Header("Walk / Run")]
    [Tooltip("Speed (m/s) below which the fox is treated as idle.")]
    public float walkThreshold       = 0.1f;
    [Tooltip("Speed at which leg cycle reaches its max frequency.")]
    public float runReferenceSpeed   = 4f;
    public float legCycleHzAtIdle    = 0.0f;
    public float legCycleHzAtRun     = 3.0f;
    public float legSwingDegAtRun    = 35f;
    public float bodyRockAmpAtRun    = 0.04f;
    public float tailBobAmpAtRun     = 0.05f;

    [Header("LookAround")]
    public float lookAroundAngleDeg  = 25f;
    public float lookHz              = 0.5f;
    public float earPerkLerp         = 6f;

    // Cached refs
    Transform _body, _head, _earL, _earR, _tailBase, _tailTip;
    Transform _legFL, _legFR, _legBL, _legBR;

    // Originals
    Vector3 _bodyPos0, _headEuler0, _earLEuler0, _earREuler0, _tailBaseEuler0, _tailTipPos0;
    Vector3 _legFLEuler0, _legFREuler0, _legBLEuler0, _legBREuler0;

    // State
    float _speed;
    bool  _lookAround;
    float _earPerkBlend;          // 0..1 lerp toward perked ears during lookAround
    float _nextEarFlickAt;
    float _earFlickPhase;

    void Awake()
    {
        _body     = transform.Find("FoxBody");
        _head     = transform.Find("Head");
        _earL     = transform.Find("EarL");
        _earR     = transform.Find("EarR");
        _tailBase = transform.Find("TailBase");
        _tailTip  = transform.Find("TailTip");
        _legFL    = transform.Find("LegFL");
        _legFR    = transform.Find("LegFR");
        _legBL    = transform.Find("LegBL");
        _legBR    = transform.Find("LegBR");

        if (_body)     _bodyPos0       = _body.localPosition;
        if (_head)     _headEuler0     = _head.localEulerAngles;
        if (_earL)     _earLEuler0     = _earL.localEulerAngles;
        if (_earR)     _earREuler0     = _earR.localEulerAngles;
        if (_tailBase) _tailBaseEuler0 = _tailBase.localEulerAngles;
        if (_tailTip)  _tailTipPos0    = _tailTip.localPosition;
        if (_legFL)    _legFLEuler0    = _legFL.localEulerAngles;
        if (_legFR)    _legFREuler0    = _legFR.localEulerAngles;
        if (_legBL)    _legBLEuler0    = _legBL.localEulerAngles;
        if (_legBR)    _legBREuler0    = _legBR.localEulerAngles;

        _nextEarFlickAt = Time.time + Random.Range(1f, earFlickPeriod);
    }

    public void SetSpeed(float speed) => _speed = speed;
    public void SetLookAround(bool on) => _lookAround = on;

    void Update()
    {
        bool moving = _speed > walkThreshold;

        AnimateLegs(moving);
        AnimateBody(moving);
        AnimateTail(moving);
        AnimateEars();
        AnimateHead();
    }

    void AnimateLegs(bool moving)
    {
        // Cycle frequency scales linearly with speed up to runReferenceSpeed.
        float cycleHz = moving
            ? Mathf.Lerp(legCycleHzAtIdle, legCycleHzAtRun,
                         Mathf.Clamp01(_speed / runReferenceSpeed))
            : 0f;
        float phase = Time.time * cycleHz * Mathf.PI * 2f;
        // Trot gait: front-left + back-right swing together; front-right + back-left opposite.
        float swingAmp = moving ? legSwingDegAtRun * Mathf.Clamp01(_speed / runReferenceSpeed) : 0f;
        float a = Mathf.Sin(phase)              * swingAmp;
        float b = Mathf.Sin(phase + Mathf.PI)   * swingAmp; // 180° offset

        if (_legFL) _legFL.localEulerAngles = _legFLEuler0 + new Vector3(a, 0, 0);
        if (_legBR) _legBR.localEulerAngles = _legBREuler0 + new Vector3(a, 0, 0);
        if (_legFR) _legFR.localEulerAngles = _legFREuler0 + new Vector3(b, 0, 0);
        if (_legBL) _legBL.localEulerAngles = _legBLEuler0 + new Vector3(b, 0, 0);
    }

    void AnimateBody(bool moving)
    {
        if (_body == null) return;

        // Idle: subtle breathing bob along Y.
        float bob = Mathf.Sin(Time.time * idleBreathHz * Mathf.PI * 2f) * idleBreathAmp;

        // Run: side-to-side rock — z-roll oscillating with leg phase.
        float rock = 0f;
        if (moving)
        {
            float speedFrac = Mathf.Clamp01(_speed / runReferenceSpeed);
            rock = Mathf.Sin(Time.time * legCycleHzAtRun * Mathf.PI * 2f) * bodyRockAmpAtRun * speedFrac;
        }

        _body.localPosition = _bodyPos0 + new Vector3(0f, bob, 0f);
        _body.localEulerAngles = new Vector3(90f, 0f, rock * Mathf.Rad2Deg); // body keeps its base 90° rotation
    }

    void AnimateTail(bool moving)
    {
        if (_tailBase != null)
        {
            // Side-to-side sway — slow when idle, slightly faster when moving.
            float hz = moving ? tailSwayHz * 1.6f : tailSwayHz;
            float sway = Mathf.Sin(Time.time * hz * Mathf.PI * 2f) * tailSwayAmpDeg;
            _tailBase.localEulerAngles = _tailBaseEuler0 + new Vector3(0f, sway, 0f);
        }

        if (_tailTip != null && moving)
        {
            // Tail bobs up slightly when running.
            float speedFrac = Mathf.Clamp01(_speed / runReferenceSpeed);
            float bob = Mathf.Abs(Mathf.Sin(Time.time * legCycleHzAtRun * Mathf.PI)) * tailBobAmpAtRun * speedFrac;
            _tailTip.localPosition = _tailTipPos0 + new Vector3(0f, bob, 0f);
        }
        else if (_tailTip != null)
        {
            _tailTip.localPosition = _tailTipPos0;
        }
    }

    void AnimateEars()
    {
        // Ear-perk blend: 0 = relaxed (use originals), 1 = perked (more upright)
        float target = _lookAround ? 1f : 0f;
        _earPerkBlend = Mathf.Lerp(_earPerkBlend, target, Time.deltaTime * earPerkLerp);

        // Idle ear flick — a small, occasional yaw tilt on the left ear.
        if (Time.time >= _nextEarFlickAt && !_lookAround)
        {
            _earFlickPhase = 1f;
            _nextEarFlickAt = Time.time + earFlickPeriod + Random.Range(-0.5f, 0.5f);
        }
        _earFlickPhase = Mathf.Max(0f, _earFlickPhase - Time.deltaTime * 4f);
        float flick = Mathf.Sin(_earFlickPhase * Mathf.PI) * 12f; // brief 12° pulse

        if (_earL != null)
            _earL.localEulerAngles = _earLEuler0
                + new Vector3(-_earPerkBlend * 8f, 0f, flick);
        if (_earR != null)
            _earR.localEulerAngles = _earREuler0
                + new Vector3(-_earPerkBlend * 8f, 0f, 0f);
    }

    void AnimateHead()
    {
        if (_head == null) return;

        if (_lookAround)
        {
            // Sweep head left/right with sin wave.
            float yaw = Mathf.Sin(Time.time * lookHz * Mathf.PI * 2f) * lookAroundAngleDeg;
            _head.localEulerAngles = _headEuler0 + new Vector3(0f, yaw, 0f);
        }
        else
        {
            // Smoothly return to forward. Use lerp on Quaternion to handle wrap.
            Quaternion target = Quaternion.Euler(_headEuler0);
            _head.localRotation = Quaternion.Slerp(_head.localRotation, target, Time.deltaTime * 6f);
        }
    }
}
