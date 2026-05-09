using UnityEngine;

/// <summary>
/// Pure-C# procedural audio synthesis — generates AudioClip objects on the fly
/// using AudioClip.Create + SetData. No external sample assets needed.
///
/// Useful primitives:
///   • GenerateChord(frequencies, duration)  — bell-like sum-of-sines with
///     exponential decay envelope. Good for chimes, pickups, completion stings.
///   • GenerateWindLoop(duration)             — seamless low-frequency rumble
///     from LCG noise + slow gust LFO + edge cross-fade for loop continuity.
/// </summary>
public static class ProceduralAudio
{
    const float kDefaultSampleRate = 44100f;

    /// <summary>
    /// Sum-of-sines chord with exponential decay + tiny linear attack to avoid
    /// the click that comes from starting a sine at non-zero phase derivative.
    /// </summary>
    public static AudioClip GenerateChord(float[] frequencies, float duration, float sampleRate = kDefaultSampleRate)
    {
        int sampleCount = Mathf.RoundToInt(duration * sampleRate);
        float[] samples = new float[sampleCount];

        // Decay rate so envelope reaches ~0.0067 (≈ -43 dB) at the end.
        // exp(-5) ≈ 0.0067 → decay = 5 / duration
        float decayRate = 5f / Mathf.Max(0.01f, duration);
        const float kAmplitude = 0.4f; // peak -7 dB to leave headroom
        const int kAttackSamples = 64;

        int n = frequencies.Length;
        if (n == 0) n = 1;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / sampleRate;

            // Sum sine waves and average — keeps total amplitude bounded by 1.
            float sample = 0f;
            for (int f = 0; f < frequencies.Length; f++)
                sample += Mathf.Sin(2f * Mathf.PI * frequencies[f] * t);
            sample /= n;

            // Exponential decay envelope.
            float envelope = Mathf.Exp(-decayRate * t);

            // Linear ramp-in for the first ~64 samples to suppress the start click.
            if (i < kAttackSamples) envelope *= i / (float)kAttackSamples;

            samples[i] = sample * envelope * kAmplitude;
        }

        var clip = AudioClip.Create("ProcChord_" + duration.ToString("F2") + "s",
                                    sampleCount, 1, (int)sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Seamless low-frequency wind loop. Hold-and-interpolated LCG noise (cheap
    /// 1st-order low-pass) modulated by a 2-component gust LFO at 0.5 Hz + 1.7 Hz.
    /// End of buffer is cross-faded into the start so AudioSource.loop wraps cleanly.
    /// </summary>
    public static AudioClip GenerateWindLoop(float duration, float sampleRate = kDefaultSampleRate)
    {
        int sampleCount = Mathf.RoundToInt(duration * sampleRate);
        float[] samples = new float[sampleCount];

        // Linear-congruential generator state (seed chosen for reproducibility)
        uint lcgState = 0xC0FFEE42u;
        // Hold-rate: regenerate raw noise every N samples. Higher → lower-cutoff "wind".
        // 100 samples at 44.1 kHz ≈ 441 Hz cutoff — soft rumble.
        const int kHoldRate = 100;

        float prev = NextLcgUnit(ref lcgState);
        float next = NextLcgUnit(ref lcgState);
        int holdCounter = 0;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / sampleRate;

            // Two-LFO gust: 0.5 Hz dominant + 1.7 Hz texture.
            float lfoSlow = Mathf.Sin(2f * Mathf.PI * 0.5f * t) * 0.5f + 0.5f;
            float lfoFast = Mathf.Sin(2f * Mathf.PI * 1.7f * t) * 0.5f + 0.5f;
            float gust    = lfoSlow * 0.7f + lfoFast * 0.3f;

            // Smoothed (linearly interpolated) low-frequency noise.
            float k = holdCounter / (float)kHoldRate;
            float smoothed = Mathf.Lerp(prev, next, k);
            holdCounter++;
            if (holdCounter >= kHoldRate)
            {
                holdCounter = 0;
                prev = next;
                next = NextLcgUnit(ref lcgState);
            }

            samples[i] = smoothed * gust * 0.15f; // quiet floor — wind is ambient
        }

        // Loop-seam cross-fade: blend the last K samples with the first K so
        // the wrap from end → start is silent rather than a click.
        int crossfade = Mathf.Min(1000, sampleCount / 8);
        for (int i = 0; i < crossfade; i++)
        {
            float k = i / (float)crossfade;
            int tail = sampleCount - crossfade + i;
            samples[tail] = Mathf.Lerp(samples[tail], samples[i], k);
        }

        var clip = AudioClip.Create("ProcWindLoop_" + duration.ToString("F1") + "s",
                                    sampleCount, 1, (int)sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // -1..+1 unit float from a linear-congruential generator.
    static float NextLcgUnit(ref uint state)
    {
        state = state * 1664525u + 1013904223u;
        // Pull the high 24 bits to a [0..1) float, then scale to [-1..+1].
        return ((state >> 8) & 0xFFFFFFu) * (1f / 0x1000000) * 2f - 1f;
    }
}
