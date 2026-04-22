using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource), typeof(CharacterController))]
public class WalkSound : MonoBehaviour
{
    public AudioClip[] Metal;
    public AudioClip[] Wood;
    public AudioClip[] Brush;
    public AudioClip[] Grass;
    public AudioClip[] Asphalt;
    public AudioClip[] Concrete;

    private float range = 0.5f;
    private int nums = 0;
    private float nextTimeWalk = 0.0f;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        switch (hit.gameObject.tag)
        {
            case "Metal":
                nums = 0;
                break;
            case "Wood":
                nums = 1;
                break;
            case "Brush":
                nums = 2;
                break;
            case "Grass":
                nums = 3;
                break;
            case "Asphalt":
                nums = 4;
                break;
            case "Concrete":
                nums = 5;
                break;
        }
    }

    void Update()
    {
        CharacterController controller = GetComponent<CharacterController>();
        range = 0.7f - ((0.5f / 10) * controller.velocity.magnitude);

        if (controller.isGrounded && controller.velocity.magnitude > 0.2f && Time.time > nextTimeWalk)
        {
            nextTimeWalk = Time.time + range;
            AudioSource audioSource = GetComponent<AudioSource>();

            switch (nums)
            {
                case 0:
                    audioSource.clip = Metal[Random.Range(0, Metal.Length)];
                    break;
                case 1:
                    audioSource.clip = Wood[Random.Range(0, Wood.Length)];
                    break;
                case 2:
                    audioSource.clip = Brush[Random.Range(0, Brush.Length)];
                    break;
                case 3:
                    audioSource.clip = Grass[Random.Range(0, Grass.Length)];
                    break;
                case 4:
                    audioSource.clip = Asphalt[Random.Range(0, Asphalt.Length)];
                    break;
                case 5:
                    audioSource.clip = Concrete[Random.Range(0, Concrete.Length)];
                    break;
            }

            audioSource.Play();
        }
    }
}