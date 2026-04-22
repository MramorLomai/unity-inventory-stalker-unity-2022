using UnityEngine;

public class ImpactSound : MonoBehaviour
{
    [SerializeField] private AudioClip[] impactClips;
    [SerializeField] private float destroyDelay = 0.5f;
    [SerializeField] private float volume = 0.8f;

    void Start()
    {
        Destroy(gameObject, destroyDelay);

        if (impactClips != null && impactClips.Length > 0)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                AudioClip randomClip = impactClips[Random.Range(0, impactClips.Length)];
                audioSource.PlayOneShot(randomClip, volume);
            }
            else
            {
                Debug.LogWarning("AudioSource component not found!", this);
            }
        }
        else
        {
            Debug.LogWarning("No impact audio clips assigned!", this);
        }
    }
}