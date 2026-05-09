using System.Collections;
using UnityEngine;

/// <summary>
/// Memory mote — a glowing orb the fox runs through to refill daylight.
/// Place a sphere trigger collider on this GameObject tagged "Mote".
/// Add a Point Light + ParticleSystem for glow.
/// </summary>
public class MemoryMote : MonoBehaviour
{
    [Header("Effect")]
    public float daylightRefill = 20f;
    public ParticleSystem collectBurst;
    public AudioClip collectChime;
    public float bobSpeed = 1.2f;
    public float bobHeight = 0.15f;

    Vector3 _startPos;
    bool _collected;
    AudioSource _audio;

    void Start()
    {
        _startPos = transform.position;
        _audio = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (_collected) return;
        // Idle bob
        float y = _startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(_startPos.x, y, _startPos.z);

        // Gentle spin
        transform.Rotate(Vector3.up, 60f * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (_collected) return;
        if (!other.CompareTag("Fox")) return;

        _collected = true;
        Collect();
    }

    void Collect()
    {
        DaylightManager.Instance?.AddTime(daylightRefill);

        // Wisp glow spike + screen flash — owned by other GameObjects, so they
        // keep running even after this mote is destroyed.
        WispController.Instance?.OnMoteCollected();
        DaylightManager.Instance?.FlashMoteCollect();

        // Procedural chime via the audio manager singleton.
        ProceduralAudioManager.Instance?.PlayMoteChime();

        // Play burst particles detached so they survive after we disable
        if (collectBurst)
        {
            var burst = Instantiate(collectBurst, transform.position, Quaternion.identity);
            burst.Play();
            Destroy(burst.gameObject, burst.main.duration + 0.5f);
        }

        // Play sound detached
        if (_audio && collectChime)
        {
            AudioSource.PlayClipAtPoint(collectChime, transform.position);
        }

        // Burst-scale the MoteOrb child, then destroy this GameObject.
        StartCoroutine(BurstAndDestroy());
    }

    IEnumerator BurstAndDestroy()
    {
        const float burstDuration = 0.1f;
        const float burstScalePeak = 2.5f;

        var orb = transform.Find("MoteOrb");
        Vector3 orbStartScale = (orb != null) ? orb.localScale : Vector3.one;

        float t = 0f;
        while (t < burstDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / burstDuration);
            if (orb != null) orb.localScale = orbStartScale * Mathf.Lerp(1f, burstScalePeak, k);
            yield return null;
        }

        Destroy(gameObject);
    }
}
