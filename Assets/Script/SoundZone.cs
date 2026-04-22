using UnityEngine;
using System.Collections;

public class SoundZone : MonoBehaviour
{
    public AudioClip sound;
    public float maxVolume = 1.0f;
    public bool smoothly = true;
    public bool smoothlyExit = true;
    public bool loop = false;
    public bool destroyOnExit = false;
    public bool playToEnd = false;
    public GameObject objectStart;
    public string functionName = "objectStart";

    private AudioSource source;
    private bool down = false;
    private bool up = false;

    void Update()
    {
        SoundUpdate();
    }

    void SoundUpdate()
    {
        if (down)
        {
            if (!playToEnd)
            {
                if (smoothlyExit)
                    source.volume -= 0.5f * Time.deltaTime;
                else
                    source.volume = 0;

                if (source.volume <= 0)
                {
                    if (source != null)
                        Destroy(source);
                    down = !down;
                    if (destroyOnExit)
                        Destroy(gameObject);
                }
            }
            else
            {
                if (!source.isPlaying)
                    playToEnd = false;
            }
        }

        if (up)
        {
            if (smoothly)
                source.volume += 0.2f * Time.deltaTime;
            else
                source.volume = maxVolume;

            if (source.volume >= maxVolume)
                up = !up;
        }
    }

    void OnTriggerEnter(Collider sourceObj)
    {
        if (sourceObj.gameObject.tag == "Player")
        {
            if (down)
            {
                down = false;
                up = true;
            }
            else
            {
                if (source == null)
                    source = sourceObj.gameObject.AddComponent<AudioSource>();

                source.volume = 0;
                source.loop = loop;
                source.clip = sound;
                source.Play();
                down = false;
                up = true;

                if (objectStart != null)
                    objectStart.SendMessage(functionName);
            }
        }
    }

    void OnTriggerExit()
    {
        down = true;
        up = false;
    }
}