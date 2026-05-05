using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Singleton that tracks the daylight timer, drives the UI meter,
/// and fires loop-end events when the fox reaches the gate or time runs out.
/// </summary>
public class DaylightManager : MonoBehaviour
{
    public static DaylightManager Instance { get; private set; }

    [Header("Timer")]
    [Tooltip("Starting daylight in seconds")]
    public float startingDaylight = 120f;

    [Header("UI References")]
    public Slider daylightSlider;
    public TextMeshProUGUI outcomeText;
    public CanvasGroup fadeOverlay;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip moteCollectChime;
    public AudioClip gateReachChime;
    public AudioClip sighClip;

    public event Action OnBiomeComplete;
    public event Action OnDaylightExhausted;

    float _daylight;
    bool _loopEnded;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _daylight = startingDaylight;
        _loopEnded = false;
        if (outcomeText) outcomeText.gameObject.SetActive(false);
        if (fadeOverlay) fadeOverlay.alpha = 0f;
        UpdateSlider();
    }

    void Update()
    {
        if (_loopEnded) return;

        _daylight -= Time.deltaTime;
        _daylight = Mathf.Max(0f, _daylight);
        UpdateSlider();

        if (_daylight <= 0f)
        {
            TriggerExhausted();
        }
    }

    /// <summary>Memory mote collected — refill daylight by amount.</summary>
    public void AddTime(float seconds)
    {
        _daylight = Mathf.Min(_daylight + seconds, startingDaylight);
        UpdateSlider();
        if (audioSource && moteCollectChime)
            audioSource.PlayOneShot(moteCollectChime);
    }

    /// <summary>Fox reached the biome gate — success loop.</summary>
    public void CompleteLoop()
    {
        if (_loopEnded) return;
        _loopEnded = true;
        if (audioSource && gateReachChime)
            audioSource.PlayOneShot(gateReachChime);
        OnBiomeComplete?.Invoke();
        StartCoroutine(FadeAndShow("First biome — completed"));
    }

    void TriggerExhausted()
    {
        if (_loopEnded) return;
        _loopEnded = true;
        if (audioSource && sighClip)
            audioSource.PlayOneShot(sighClip);
        OnDaylightExhausted?.Invoke();
        StartCoroutine(FadeAndShow("The light was not enough yet"));
    }

    System.Collections.IEnumerator FadeAndShow(string message)
    {
        // Fade overlay in over 1.5s
        float t = 0f;
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            if (fadeOverlay) fadeOverlay.alpha = Mathf.Clamp01(t / 1.5f);
            yield return null;
        }

        // Show outcome text
        if (outcomeText)
        {
            outcomeText.text = message;
            outcomeText.gameObject.SetActive(true);
        }

        // Hold 3s then reload
        yield return new WaitForSeconds(3f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    void UpdateSlider()
    {
        if (daylightSlider)
            daylightSlider.value = _daylight / startingDaylight;
    }
}
