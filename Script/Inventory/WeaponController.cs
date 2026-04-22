using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponController : MonoBehaviour
{
    [System.Serializable]
    public class WeaponSound
    {
        public AudioClip[] shot;
        public AudioClip reload;
        public AudioClip draw;
        public AudioClip down;
        public AudioClip empty;
    }

    [System.Serializable]
    public class AmmoConfig
    {
        public string ammoType = "ammotype";
        public int maxAmmunition = 30;
        public int indexAmmunition = 30;
    }

    [System.Serializable]
    public class WeaponConfig
    {
        public float shootRate = 0.1f;
        public float shootRange = 100f;
        public float reloadTime = 2f;
        public float spread = 0.01f;
    }

    [System.Serializable]
    public class RicochetConfig
    {
        public bool canRicochet = false;
        public AudioClip[] ricochetSounds;
        public float ricochetChanceMultiplier = 1.0f;
        public float minRicochetAngle = 15f;
        public float maxRicochetAngle = 75f;
    }

    [System.Serializable]
    public class WeaponStats
    {
        public float maxDurability = 100f;
        public float durabilityLossPerShot = 0.1f;
        public float currentDurability = 100f;
        public bool isBroken = false;
    }

    public WeaponSound Audio;
    public AmmoConfig Ammo;
    public WeaponConfig Config;
    public RicochetConfig Ricochet;
    public WeaponStats Stats;

    public Transform ActorCamera;
    public Transform decalPrefab;
    public ParticleSystem muzzleFlash;
    public Animator animator;
    public TrailRenderer BulletTrail;
    public Transform firePoint;
    public bool show = false;
    public float BulletSpeed = 100f;
    public float impactForce = 320f;
    public LayerMask ignoreLayer;

    private float nextFire = 0.0f;
    private bool reloader = false;
    private int weaponSlotIndex = -1;

    private Inventory inventory;
    private GameControl gameControl;

    private static Dictionary<string, int> savedAmmoData = new Dictionary<string, int>();
    private static Dictionary<string, float> savedDurabilityData = new Dictionary<string, float>();
    private string weaponID;

    void Start()
    {
        inventory = FindObjectOfType<Inventory>();
        gameControl = FindObjectOfType<GameControl>();

        weaponID = gameObject.name + "_" + transform.GetInstanceID();
        FindWeaponSlotIndex();

        if (savedAmmoData.ContainsKey(weaponID))
        {
            Ammo.indexAmmunition = savedAmmoData[weaponID];
        }
        else if (inventory != null && weaponSlotIndex != -1)
        {
            Ammo.indexAmmunition = inventory.APIGetActivesData(weaponSlotIndex, 1);
        }

        if (savedDurabilityData.ContainsKey(weaponID))
        {
            Stats.currentDurability = savedDurabilityData[weaponID];
        }
        else if (inventory != null && weaponSlotIndex != -1)
        {
            Inventory.Element weaponElement = GetWeaponElementFromInventory();
            if (weaponElement != null && weaponElement.option != null)
            {
                Stats.currentDurability = weaponElement.option.status;
            }
        }

        UpdateBrokenState();
        ActorCamera = Camera.main.transform;
        show = false;
        StartCoroutine(showhide());
    }

    private Inventory.Element GetWeaponElementFromInventory()
    {
        if (inventory == null) return null;

        System.Reflection.FieldInfo activesField = typeof(Inventory).GetField("Actives",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (activesField != null)
        {
            Inventory.Element[] actives = activesField.GetValue(inventory) as Inventory.Element[];
            if (actives != null && weaponSlotIndex >= 0 && weaponSlotIndex < actives.Length)
            {
                return actives[weaponSlotIndex];
            }
        }
        return null;
    }

    private void UpdateDurabilityInInventory()
    {
        if (inventory == null) return;

        Inventory.Element weaponElement = GetWeaponElementFromInventory();
        if (weaponElement != null && weaponElement.option != null)
        {
            weaponElement.option.status = Stats.currentDurability;
        }
    }

    private void FindWeaponSlotIndex()
    {
        if (inventory == null) return;

        System.Reflection.FieldInfo activesField = typeof(Inventory).GetField("Actives",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (activesField != null)
        {
            Inventory.Element[] actives = activesField.GetValue(inventory) as Inventory.Element[];
            if (actives != null)
            {
                for (int i = 0; i < actives.Length; i++)
                {
                    if (actives[i] != null && actives[i]._id != null &&
                        actives[i]._id.transform.IsChildOf(transform))
                    {
                        weaponSlotIndex = i;
                        break;
                    }
                }
            }
        }
    }

    void OnDestroy()
    {
        if (!string.IsNullOrEmpty(weaponID))
        {
            savedAmmoData[weaponID] = Ammo.indexAmmunition;
            savedDurabilityData[weaponID] = Stats.currentDurability;
        }
    }

    private void UpdateBrokenState()
    {
        Stats.isBroken = Stats.currentDurability <= 0;
    }

    IEnumerator reload()
    {
        if (inventory == null) yield break;

        if (Ammo.indexAmmunition >= Ammo.maxAmmunition)
        {
            yield break;
        }

        int baseAmmunition = inventory.APIgetItemCount(Ammo.ammoType);
        if (baseAmmunition > 0)
        {
            if (animator != null)
                animator.SetBool("Reloading", true);

            GetComponent<AudioSource>().clip = Audio.reload;
            reloader = true;

            GetComponent<AudioSource>().Play();
            yield return new WaitForSeconds(Config.reloadTime);

            int ammoNeeded = Ammo.maxAmmunition - Ammo.indexAmmunition;
            int ammoToTake = Mathf.Min(baseAmmunition, ammoNeeded);

            if (ammoToTake > 0)
            {
                Ammo.indexAmmunition += ammoToTake;
                inventory.APIeditItemCount(Ammo.ammoType, baseAmmunition - ammoToTake);

                if (!string.IsNullOrEmpty(weaponID))
                {
                    savedAmmoData[weaponID] = Ammo.indexAmmunition;
                }

                if (inventory != null && weaponSlotIndex != -1)
                {
                    inventory.APISetActivesData(weaponSlotIndex, 1, Ammo.indexAmmunition);
                }
            }

            if (animator != null)
                animator.SetBool("Reloading", false);

            reloader = false;
        }
    }

    public IEnumerator showhide()
    {
        if (inventory == null) yield break;

        if (!show)
        {
            GetComponent<AudioSource>().clip = Audio.down;
            GetComponent<AudioSource>().Play();
            yield return new WaitForSeconds(0.3f);
            GetComponent<AudioSource>().clip = Audio.draw;
            GetComponent<AudioSource>().Play();
            yield return new WaitForSeconds(0.5f);
            show = !show;
            if (inventory != null)
            {
                if (weaponSlotIndex != -1)
                {
                    int savedAmmo = inventory.APIGetActivesData(weaponSlotIndex, 1);
                    if (savedAmmo > 0)
                    {
                        Ammo.indexAmmunition = savedAmmo;
                    }

                    Inventory.Element weaponElement = GetWeaponElementFromInventory();
                    if (weaponElement != null && weaponElement.option != null)
                    {
                        Stats.currentDurability = weaponElement.option.status;
                        UpdateBrokenState();
                    }
                }
                else
                {
                    Ammo.indexAmmunition = inventory.APIGetActivesData(1, 1);
                }
            }
        }
        else
        {
            if (inventory != null)
            {
                if (weaponSlotIndex != -1)
                {
                    inventory.APISetActivesData(weaponSlotIndex, 1, Ammo.indexAmmunition);
                }
                else
                {
                    inventory.APISetActivesData(1, 1, Ammo.indexAmmunition);
                }
            }

            if (animator != null)
                animator.SetTrigger("holster");

            show = !show;
            GetComponent<AudioSource>().clip = Audio.down;
            GetComponent<AudioSource>().Play();
        }
    }

    bool Ammunition()
    {
        if (Stats.isBroken)
        {
            GetComponent<AudioSource>().clip = Audio.empty;
            GetComponent<AudioSource>().Play();
            return false;
        }

        if (Ammo.indexAmmunition > 0)
        {
            Ammo.indexAmmunition--;

            Stats.currentDurability -= Stats.durabilityLossPerShot;
            Stats.currentDurability = Mathf.Max(0, Stats.currentDurability);
            Stats.currentDurability = RoundToDecimals(Stats.currentDurability, 1);

            UpdateBrokenState();
            UpdateDurabilityInInventory();

            if (!string.IsNullOrEmpty(weaponID))
            {
                savedDurabilityData[weaponID] = Stats.currentDurability;
            }

            if (inventory != null)
            {
                if (weaponSlotIndex != -1)
                {
                    inventory.APISetActivesData(weaponSlotIndex, 1, Ammo.indexAmmunition);
                }
                else
                {
                    inventory.APISetActivesData(1, 1, Ammo.indexAmmunition);
                }
            }

            return true;
        }
        else
        {
            GetComponent<AudioSource>().clip = Audio.empty;
            GetComponent<AudioSource>().Play();
            return false;
        }
    }

    void shot()
    {
        if (inventory == null || Stats.isBroken || !show) return;

        RaycastHit hit;
        Vector3 dir = ActorCamera.TransformDirection(Vector3.forward);

        Vector3 spreadVector = new Vector3(
            Random.Range(-Config.spread, Config.spread),
            Random.Range(-Config.spread, Config.spread),
            Random.Range(-Config.spread, Config.spread)
        );
        dir += spreadVector;

        if (Input.GetButton("Fire1") && Time.time > nextFire && !reloader && show)
        {
            nextFire = Time.time + Config.shootRate;

            if (Physics.Raycast(ActorCamera.position, dir, out hit, Config.shootRange))
            {
                if (Ammunition())
                {
                    ProcessShot(hit, dir);
                }
                else
                {
                    StartCoroutine(reload());
                }
            }
            else
            {
                if (Ammunition())
                {
                    Vector3 endPoint = ActorCamera.position + dir * Config.shootRange;

                    if (BulletTrail != null && firePoint != null)
                    {
                        TrailRenderer trail = Instantiate(BulletTrail, firePoint.position, Quaternion.identity);
                        StartCoroutine(SpawnTrail(trail, endPoint, Vector3.zero, false, null));
                    }

                    PlayShotEffects();
                }
                else
                {
                    StartCoroutine(reload());
                }
            }
        }
    }

    private void ProcessShot(RaycastHit hit, Vector3 dir)
    {
        if (BulletTrail != null && firePoint != null)
        {
            TrailRenderer trail = Instantiate(BulletTrail, firePoint.position, Quaternion.identity);
            bool madeImpact = true;

            bool shouldRicochet = Ricochet.canRicochet && ShouldRicochet(hit.collider.tag, hit);
            RaycastHit? hitInfo = shouldRicochet ? (RaycastHit?)hit : null;

            StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, madeImpact, hitInfo));
        }
        else
        {
            DecalGen(hit.point, hit.normal, hit.collider, dir);
        }

        PlayShotEffects();
    }

    private void PlayShotEffects()
    {
        GetComponent<AudioSource>().clip = Audio.shot[Random.Range(0, Audio.shot.Length)];

        if (animator != null)
            animator.SetTrigger("Shoot");

        if (muzzleFlash != null)
            muzzleFlash.Play();

        GetComponent<AudioSource>().Play();
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 hitPoint, Vector3 hitNormal, bool madeImpact, RaycastHit? hitInfo)
    {
        if (trail == null) yield break;

        Vector3 startPosition = trail.transform.position;
        float distance = Vector3.Distance(trail.transform.position, hitPoint);
        float remainingDistance = distance;

        while (remainingDistance > 0)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hitPoint, 1 - (remainingDistance / distance));
            remainingDistance -= BulletSpeed * Time.deltaTime;
            yield return null;
        }

        trail.transform.position = hitPoint;

        if (madeImpact && hitInfo.HasValue)
        {
            RaycastHit hit = hitInfo.Value;

            if (hit.collider.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.AddForceAtPosition(transform.TransformDirection(Vector3.forward) * impactForce, hitPoint);
            }

            DecalGen(hit.point, hit.normal, hit.collider, (hit.point - firePoint.position).normalized);

            if (Ricochet.canRicochet && ShouldRicochet(hit.collider.tag, hit))
            {
                Vector3 incomingDirection = (hit.point - firePoint.position).normalized;
                Vector3 ricochetDirection = Vector3.Reflect(incomingDirection, hit.normal).normalized;

                float dot = Vector3.Dot(ricochetDirection, hit.normal);
                if (dot > -0.1f)
                {
                    ricochetDirection = Vector3.Reflect(ricochetDirection + hit.normal * 0.2f, hit.normal).normalized;
                }

                TrailRenderer ricochetTrail = Instantiate(BulletTrail, hitPoint, Quaternion.identity);

                RaycastHit ricochetHit;
                float ricochetRange = Config.shootRange * 0.5f;

                if (Physics.Raycast(hitPoint + ricochetDirection * 0.1f,
                                  ricochetDirection,
                                  out ricochetHit,
                                  ricochetRange,
                                  ~ignoreLayer))
                {
                    StartCoroutine(SpawnTrail(ricochetTrail, ricochetHit.point, ricochetHit.normal, true, ricochetHit));

                    if (Ricochet.ricochetSounds.Length > 0)
                    {
                        int randomIndex = Random.Range(0, Ricochet.ricochetSounds.Length);
                        AudioClip RicochetClip = Ricochet.ricochetSounds[randomIndex];
                        AudioSource.PlayClipAtPoint(RicochetClip, ricochetHit.point, 0.5f);
                    }
                }
                else
                {
                    Vector3 ricochetEndPoint = hitPoint + ricochetDirection * ricochetRange;
                    StartCoroutine(SpawnTrail(ricochetTrail, ricochetEndPoint, Vector3.zero, false, null));

                    if (Ricochet.ricochetSounds.Length > 0)
                    {
                        int randomIndex = Random.Range(0, Ricochet.ricochetSounds.Length);
                        AudioClip RicochetClip = Ricochet.ricochetSounds[randomIndex];
                        AudioSource.PlayClipAtPoint(RicochetClip, hitPoint + ricochetDirection * 2f, 0.3f);
                    }
                }
            }
        }

        Destroy(trail.gameObject, trail.time);
    }

    private bool ShouldRicochet(string surfaceTag, RaycastHit hit)
    {
        if (string.IsNullOrEmpty(surfaceTag))
            return false;

        float ricochetChance = 0f;

        switch (surfaceTag)
        {
            case "Metal":
                ricochetChance = 0.8f;
                break;
            case "Concrete":
                ricochetChance = 0.5f;
                break;
            case "Tile":
                ricochetChance = 0.6f;
                break;
            case "Untagged":
                ricochetChance = 0.45f;
                break;
            case "Wood":
                ricochetChance = 0.4f;
                break;
            case "Dirt":
                return false;
            case "Sand":
                return false;
            case "Enemy":
                return false;
            case "Water":
                return false;
            default:
                ricochetChance = 0.1f;
                break;
        }

        Vector3 bulletDirection = (hit.point - firePoint.position).normalized;
        float angle = Vector3.Angle(bulletDirection, -hit.normal);

        if (angle > Ricochet.maxRicochetAngle)
        {
            ricochetChance *= 0.1f;
        }
        else if (angle >= Ricochet.minRicochetAngle && angle <= 45f)
        {
            ricochetChance *= 1.5f;
        }
        else if (angle < Ricochet.minRicochetAngle)
        {
            ricochetChance *= 2.0f;
        }

        Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            ricochetChance *= 0.3f;
        }

        ricochetChance = Mathf.Clamp01(ricochetChance * Ricochet.ricochetChanceMultiplier);
        return Random.value < ricochetChance;
    }

    private float RoundToDecimals(float value, int decimals)
    {
        float scale = Mathf.Pow(10f, decimals);
        return Mathf.Round(value * scale) / scale;
    }

    void Update()
    {
        if (gameControl != null && !GameControl.ActivateGUI && show)
        {
            shot();
        }

        CharacterController controller = FindObjectOfType<CharacterController>();
        if (controller != null && show && !reloader)
        {
            if (controller.isGrounded && controller.velocity.magnitude > 3.8f && !Input.GetButton("Fire1"))
            {
            }
            else
            {
            }
        }

        if (Input.GetKeyUp(KeyCode.Alpha3) && show)
        {
            StartCoroutine(showhide());
        }

        if (Input.GetKeyDown(KeyCode.R) && show && !reloader && Ammo.indexAmmunition < Ammo.maxAmmunition)
        {
            StartCoroutine(reload());
        }
    }

    void DecalGen(Vector3 p, Vector3 n, Collider c, Vector3 dir)
    {
        if (decalPrefab == null) return;

        GameObject decalInst = Instantiate(decalPrefab.gameObject, p, Quaternion.FromToRotation(Vector3.up, n)) as GameObject;
        if (c.gameObject.tag == "wmDecal")
        {
            Destroy(c.gameObject);
        }

        Rigidbody rb = c.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(dir * impactForce);
        }
    }

    public void ForceShow()
    {
        if (!show)
        {
            StartCoroutine(showhide());
        }
    }

    public void ForceHide()
    {
        if (show)
        {
            StartCoroutine(showhide());
        }
    }

    public void RepairWeapon(float repairAmount)
    {
        Stats.currentDurability = Mathf.Min(Stats.currentDurability + repairAmount, Stats.maxDurability);
        Stats.currentDurability = RoundToDecimals(Stats.currentDurability, 1);

        UpdateBrokenState();
        UpdateDurabilityInInventory();

        if (!string.IsNullOrEmpty(weaponID))
        {
            savedDurabilityData[weaponID] = Stats.currentDurability;
        }
    }

    public void RepairWeaponCompletely()
    {
        Stats.currentDurability = Stats.maxDurability;
        Stats.isBroken = false;

        UpdateDurabilityInInventory();

        if (!string.IsNullOrEmpty(weaponID))
        {
            savedDurabilityData[weaponID] = Stats.currentDurability;
        }
    }

    public float GetCurrentDurability()
    {
        return Stats.currentDurability;
    }

    public float GetMaxDurability()
    {
        return Stats.maxDurability;
    }

    public bool IsWeaponBroken()
    {
        return Stats.isBroken;
    }

    public static void ClearAllSavedAmmoData()
    {
        savedAmmoData.Clear();
        savedDurabilityData.Clear();
    }

    public void LoadFromInventory(int slotIndex)
    {
        weaponSlotIndex = slotIndex;

        if (inventory != null)
        {
            int savedAmmo = inventory.APIGetActivesData(weaponSlotIndex, 1);
            if (savedAmmo >= 0)
            {
                Ammo.indexAmmunition = savedAmmo;
            }

            Inventory.Element weaponElement = GetWeaponElementFromInventory();
            if (weaponElement != null && weaponElement.option != null)
            {
                Stats.currentDurability = weaponElement.option.status;
                UpdateBrokenState();
            }
        }
    }

    public void SetWeaponSlotIndex(int slot)
    {
        weaponSlotIndex = slot;
    }
}