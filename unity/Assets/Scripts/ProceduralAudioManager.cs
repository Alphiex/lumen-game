using UnityEngine;

/// <summary>
/// Singleton that owns procedurally generated AudioClips for Lumen.
/// Generates clips lazily in Awake/OnEnable, then exposes PlayMoteChime,
/// PlayGateChime and StartWind to the rest of the codebase.
///
///   • moteChime  — bright C5–E5–G5 major triad, 0.4s exponential decay
///   • gateChime  — warm C4–E4–G4–C5 chord, 1.2s decay
///   • windLoop   — 8s seamless ambient rumble (looped via AudioSource.loop)
///
/// [ExecuteAlways] so the clips can be auditioned in edit mode.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class ProceduralAudioManager : MonoBehaviour
{
    public static ProceduralAudioManager Instance { get; private set; }

    [Header("Output sources")]
    [Tooltip("AudioSource used for one-shot SFX (chimes). Auto-added if missing.")]
    public AudioSource sfxSource;
    [Tooltip("AudioSource used for the looping ambient wind. Auto-added if missing.")]
    public AudioSource ambientSource;

    [Header("Volumes")]
    [Range(0f, 1f)] public float sfxVolume       = 0.9f;
    [Range(0f, 1f)] public float gateChimeVolume = 0.7f;
    [Range(0f, 1f)] public float ambientVolume   = 0.4f;

    [Header("Clips (generated at runtime)")]
    public AudioClip moteChime;
    public AudioClip gateChime;
    public AudioClip windLoop;

    void Awake()
    {
        Instance = this;
        EnsureSources();
        EnsureClips();
    }

    void OnEnable()
    {
        // Re-bind on script recompile / domain reload.
        Instance = this;
    }

    void Start()
    {
        if (Application.isPlaying) StartWind();
    }

    void EnsureSources()
    {
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f; // 2D — UI-style stinger
        }

        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.playOnAwake = false;
            ambientSource.loop = true;
            ambientSource.spatialBlend = 0f;
            ambientSource.volume = ambientVolume;
        }
    }

    void EnsureClips()
    {
        if (moteChime == null)
        {
            // C5, E5, G5 — bright major triad.
            moteChime = ProceduralAudio.GenerateChord(
                new[] { 523.25f, 659.25f, 783.99f }, 0.4f);
        }

        if (gateChime == null)
        {
            // C4, E4, G4, C5 — warmer voicing, longer decay.
            gateChime = ProceduralAudio.GenerateChord(
                new[] { 261.63f, 329.63f, 392.00f, 523.25f }, 1.2f);
        }

        if (windLoop == null)
        {
            windLoop = ProceduralAudio.GenerateWindLoop(8f);
        }
    }

    public void PlayMoteChime()
    {
        if (sfxSource == null || moteChime == null) return;
        sfxSource.PlayOneShot(moteChime, sfxVolume);
    }

    public void PlayGateChime()
    {
        if (sfxSource == null || gateChime == null) return;
        sfxSource.PlayOneShot(gateChime, gateChimeVolume);
    }

    public void StartWind()
    {
        if (ambientSource == null || windLoop == null) return;
        if (ambientSource.isPlaying && ambientSource.clip == windLoop) return;
        ambientSource.clip = windLoop;
        ambientSource.volume = ambientVolume;
        ambientSource.Play();
    }
}
