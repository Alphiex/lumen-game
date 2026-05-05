using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Fox AI driven by the WispController.
/// State machine: Idle → RunningToWisp → Decelerating → Looking.
/// Animator parameters: Speed (float), LookAround (trigger).
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class FoxController : MonoBehaviour
{
    public enum FoxState { Idle, RunningToWisp, Decelerating, Looking }

    [Header("Movement")]
    public float baseRunSpeed = 4f;
    public float decelerationTime = 0.4f;
    public float lookAroundDelay = 1.2f;    // seconds after stop before look-around anim

    [Header("Wisp Tracking")]
    [Tooltip("Set by WispController at runtime")]
    public Transform WispTarget;

    NavMeshAgent _agent;
    Animator _anim;
    FoxState _state = FoxState.Idle;
    float _stopTimer;
    static readonly int AnimSpeed = Animator.StringToHash("Speed");
    static readonly int AnimLook  = Animator.StringToHash("LookAround");

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _anim  = GetComponent<Animator>();
        _agent.speed = baseRunSpeed;
        _agent.stoppingDistance = 0.3f;
        _agent.updateRotation = true;
        _agent.updatePosition = true;
    }

    void Update()
    {
        float speed = _agent.velocity.magnitude;

        switch (_state)
        {
            case FoxState.Idle:
                if (WispTarget != null)
                    TransitionTo(FoxState.RunningToWisp);
                break;

            case FoxState.RunningToWisp:
                if (WispTarget != null)
                {
                    _agent.SetDestination(WispTarget.position);
                }
                else
                {
                    _agent.ResetPath();
                    TransitionTo(FoxState.Decelerating);
                }
                break;

            case FoxState.Decelerating:
                _stopTimer += Time.deltaTime;
                if (_stopTimer >= decelerationTime)
                    TransitionTo(FoxState.Looking);
                break;

            case FoxState.Looking:
                if (WispTarget != null)
                    TransitionTo(FoxState.RunningToWisp);
                break;
        }

        // Drive animator
        _anim.SetFloat(AnimSpeed, speed);
    }

    void TransitionTo(FoxState next)
    {
        _state = next;
        _stopTimer = 0f;

        if (next == FoxState.Looking)
            _anim.SetTrigger(AnimLook);
    }

    /// <summary>Called by DaylightManager when the loop ends — freeze fox.</summary>
    public void FreezeForLoopEnd()
    {
        _agent.isStopped = true;
        WispTarget = null;
        enabled = false;
    }
}
