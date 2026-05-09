using UnityEngine;

/// <summary>
/// Plays the procedurally generated gate-reach chime when the fox enters the
/// gate's trigger collider. Sibling-component to BiomeGate — that script handles
/// loop completion logic, this one handles the audio sting.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BiomeGateAudio : MonoBehaviour
{
    bool _played;

    void OnTriggerEnter(Collider other)
    {
        if (_played) return;
        if (!other.CompareTag("Fox")) return;

        _played = true;
        ProceduralAudioManager.Instance?.PlayGateChime();
    }
}
