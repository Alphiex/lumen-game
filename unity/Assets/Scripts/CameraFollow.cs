using UnityEngine;

/// <summary>
/// Smooth-damp camera that tracks the fox with a configurable world-space offset.
/// Adds a "look-back tilt" — when the fox decelerates below a speed threshold,
/// the camera pitches forward slightly so the framing reads as contemplative
/// rather than action-paced.
///
/// Designed to work without a NavMeshAgent or Rigidbody reference: speed is
/// estimated from per-frame Transform position deltas, smoothed via a small
/// exponential moving average to suppress single-frame noise.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Tracking target")]
    [SerializeField] public Transform foxTransform;

    [Header("Framing")]
    [Tooltip("World-space offset from fox to camera (default 0,4,-8 — slightly above + behind).")]
    [SerializeField] public Vector3 offset = new Vector3(0f, 4f, -8f);

    [Tooltip("SmoothDamp time-to-target for camera position. Higher = laggier follow.")]
    [SerializeField] public float smoothTime = 0.3f;

    [Header("Look-back tilt")]
    [Tooltip("Extra pitch (degrees, +X = look down) applied while the fox is nearly stopped.")]
    [SerializeField] public float lookBackTiltAngle = 5f;

    [Tooltip("Speed threshold (m/s) below which the camera blends into the look-back tilt.")]
    [SerializeField] public float decelerateSpeedThreshold = 0.5f;

    [Tooltip("Seconds for the tilt blend to complete (in either direction).")]
    [SerializeField] public float tiltBlendDuration = 0.5f;

    // Runtime state
    Vector3 _smoothDampVelocity;     // SmoothDamp internal velocity buffer
    Vector3 _foxPrevPosition;
    float   _foxSmoothedSpeed;
    bool    _hasFoxPrevPosition;
    Quaternion _baseRotation;
    float _tiltBlend;                // 0 = base orientation, 1 = fully tilted

    void Awake()
    {
        _baseRotation = transform.rotation;
        if (foxTransform != null)
        {
            _foxPrevPosition = foxTransform.position;
            _hasFoxPrevPosition = true;
        }
    }

    void LateUpdate()
    {
        if (foxTransform == null) return;

        // First-frame guard — if foxTransform was assigned after Awake,
        // initialize the position-delta tracker on this frame.
        if (!_hasFoxPrevPosition)
        {
            _foxPrevPosition = foxTransform.position;
            _hasFoxPrevPosition = true;
        }

        // ── Speed estimate ─────────────────────────────────────────────
        float dt = Mathf.Max(1e-4f, Time.deltaTime);
        Vector3 foxPosition = foxTransform.position;
        float instantSpeed = (foxPosition - _foxPrevPosition).magnitude / dt;
        _foxPrevPosition = foxPosition;
        // Exponential smoothing — ~0.2s time constant suppresses single-frame spikes.
        _foxSmoothedSpeed = Mathf.Lerp(_foxSmoothedSpeed, instantSpeed, dt * 5f);

        // ── Position: smooth-damp toward fox + offset ──────────────────
        Vector3 desiredPosition = foxPosition + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position, desiredPosition, ref _smoothDampVelocity, smoothTime);

        // ── Rotation: blend toward look-back tilt when fox is nearly stopped ──
        float wantTilt = (_foxSmoothedSpeed < decelerateSpeedThreshold) ? 1f : 0f;
        _tiltBlend = Mathf.MoveTowards(
            _tiltBlend, wantTilt, dt / Mathf.Max(1e-4f, tiltBlendDuration));

        Quaternion tiltedRotation = _baseRotation * Quaternion.Euler(lookBackTiltAngle, 0f, 0f);
        transform.rotation = Quaternion.Lerp(_baseRotation, tiltedRotation, _tiltBlend);
    }

    /// <summary>Re-capture the camera's current rotation as the new resting orientation.</summary>
    public void RebaseOrientation()
    {
        _baseRotation = transform.rotation;
    }
}
