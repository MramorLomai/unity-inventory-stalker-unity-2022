using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
[AddComponentMenu("Audio/Collider")]
public class CollisionSound : MonoBehaviour
{
    public AudioClip[] Sounds;
    public float SoundRange = 0.5f;

    private float EmitTime;

    void Start()
    {
        EmitTime = Time.time;
    }

    void OnCollisionEnter()
    {
        if (Time.time > EmitTime)
        {
            EmitTime = Time.time + SoundRange;
            GetComponent<AudioSource>().PlayOneShot(Sounds[Random.Range(0, Sounds.Length)]);
        }
    }
}