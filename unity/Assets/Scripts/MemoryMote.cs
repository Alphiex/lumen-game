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

        gameObject.SetActive(false);
    }
}
