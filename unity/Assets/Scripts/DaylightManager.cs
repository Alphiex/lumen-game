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
    Image _sliderFillImage;
    float _urgencyPulsePhase = 0f;

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
        if (daylightSlider && daylightSlider.fillRect)
            _sliderFillImage = daylightSlider.fillRect.GetComponent<Image>();
        UpdateSlider();
    }

    void Update()
    {
        if (_loopEnded) return;

        _daylight -= Time.deltaTime;
        _daylight = Mathf.Max(0f, _daylight);
        UpdateSlider();

        // Urgency pulse — fade slider alpha at 2 Hz when below 20% daylight.
        if (!_loopEnded && _daylight / startingDaylight < 0.20f && daylightSlider)
        {
            _urgencyPulsePhase += Time.deltaTime * 2f * Mathf.PI * 2f; // 2 Hz
            float pulse = 0.7f + 0.3f * Mathf.Sin(_urgencyPulsePhase);
            var cg = daylightSlider.GetComponent<CanvasGroup>();
            if (cg) cg.alpha = pulse;
        }
        else if (daylightSlider)
        {
            var cg = daylightSlider.GetComponent<CanvasGroup>();
            if (cg) cg.alpha = 1f;
        }

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
        // Fade overlay in 0.8s
        float t = 0f;
        while (t < 0.8f) { t += Time.deltaTime; if (fadeOverlay) fadeOverlay.alpha = Mathf.Clamp01(t / 0.8f); yield return null; }
        if (fadeOverlay) fadeOverlay.alpha = 1f;

        // Show outcome text, fade in over 0.5s
        if (outcomeText)
        {
            outcomeText.text = message;
            outcomeText.gameObject.SetActive(true);
            var textCG = outcomeText.gameObject.GetComponent<CanvasGroup>() ?? outcomeText.gameObject.AddComponent<CanvasGroup>();
            textCG.alpha = 0f;
            t = 0f;
            while (t < 0.5f) { t += Time.deltaTime; textCG.alpha = Mathf.Clamp01(t / 0.5f); yield return null; }
            textCG.alpha = 1f;
        }

        // Hold 3s
        yield return new WaitForSeconds(3f);

        // Fade out 0.5s
        t = 0f;
        while (t < 0.5f) { t += Time.deltaTime; if (fadeOverlay) fadeOverlay.alpha = Mathf.Clamp01(1f - t / 0.5f); yield return null; }

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    void UpdateSlider()
    {
        if (daylightSlider)
            daylightSlider.value = _daylight / startingDaylight;

        // Color gradient — blue at 0% (cool/exhausted), amber at 100% (warm/full).
        float t = _daylight / startingDaylight;
        Color amber = new Color(0.961f, 0.651f, 0.137f); // #F5A623
        Color blue  = new Color(0.290f, 0.565f, 0.851f); // #4A90D9
        if (_sliderFillImage) _sliderFillImage.color = Color.Lerp(blue, amber, t);
    }
}
