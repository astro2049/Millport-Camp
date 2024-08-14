using System.Collections;
using Entities.Abilities.Health;
using Entities.Abilities.Observer;
using Managers;
using UnityEngine;
using EventType = Entities.Abilities.Observer.EventType;

namespace Entities.Gun
{
    public enum FireMode
    {
        Auto = 0,
        Burst = 1,
        Single = 2
    }

    public class GunStateComponent : MonoBehaviour
    {
        // Components
        private SubjectComponent subjectComponent;

        // Stats & Configuration
        public GunData stats;
        public int magAmmo;
        [SerializeField] private LayerMask raycastLayers;
        [SerializeField] private LayerMask damageLayers;
        // Muzzle
        private Transform muzzleTransform;
        // Sounds
        [SerializeField] private AudioClip fireSound;
        [SerializeField] private AudioClip releaseMagSound;
        [SerializeField] private AudioClip chargingBoltSound;
        private float chargingBoltSoundPlayTimestamp;
        [SerializeField] private AudioClip magEmptySound;
        // Tracer
        public GameObject tracerPrefab;
        public float tracerSpeed = 150f; // 150m/s

        public Vector3 lookPoint;

        // Part states
        private bool isTriggerDown = false;
        private bool isBoltInPosition = true;

        private void Awake()
        {
            subjectComponent = GetComponent<SubjectComponent>();

            Init();
        }

        public void Init()
        {
            // Initialize appearance (mesh and material) according to gunData (scriptable object)
            Transform meshTransform = transform.Find("Mesh");
            meshTransform.GetComponent<MeshFilter>().mesh = stats.mesh;
            meshTransform.GetComponent<MeshRenderer>().material = stats.material;

            // Initialize fields
            // magAmmo, muzzleTransform
            SetMagAmmo(stats.magSize);
            muzzleTransform = transform.Find("Muzzle").transform;

            // Calculate when to play chargingBoltSound
            chargingBoltSoundPlayTimestamp = stats.reloadTime - chargingBoltSound.length;
        }

        // Start is called before the first frame update
        private void Start()
        {

        }

        // Update is called once per frame
        private void Update()
        {

        }

        private void SetMagAmmo(int ammo)
        {
            magAmmo = ammo;

            // Tell UI manager / turret that ammo count has changes
            subjectComponent.NotifyObservers(new MCEventWInt(EventType.AmmoChanged, magAmmo));
        }

        public void SetIsTriggerDown(bool status)
        {
            isTriggerDown = status;
            // If the trigger is down, we'll have to see if a shot will be triggered
            if (isTriggerDown) {
                if (magAmmo > 0) {
                    if (isBoltInPosition) {
                        Fire();
                    } else {
                        // nothing happens, currently in cycle
                    }
                } else {
                    // Mag empty SFX
                    AudioManager.GetAudioSource().PlayOneShot(magEmptySound, 0.65f);
                }
            }
        }

        private void SetIsBoltInPosition(bool status)
        {
            isBoltInPosition = status;
            // A shot is fired
            if (isBoltInPosition) {
                // If on Auto, after shot cycle is completed, the trigger gets reset automatically after cycling,
                // so a next shot will be fired if trigger is held down 
                if (stats.fireMode == FireMode.Auto) {
                    if (isTriggerDown) {
                        Fire();
                    }
                }
            } else {
                // Just fired a shot
                // If there's still ammo, start cycle
                // When cycle is complete, the bolt return to ready position
                if (magAmmo > 0) {
                    StartCoroutine(Cycle());
                }
            }
        }

        private IEnumerator Cycle()
        {
            yield return new WaitForSeconds(stats.fireInterval);
            SetIsBoltInPosition(true);
        }

        // Shoot a raycast bullet horizontally towards lookPoint from muzzle location
        private void Fire()
        {
            // Minus 1 bullet
            SetMagAmmo(magAmmo - 1);
            // Bolt backing
            SetIsBoltInPosition(false);

            // SFX
            if (magAmmo > 0) {
                // Normal shot SFX
                AudioManager.GetAudioSource().PlayOneShot(fireSound, 0.4f);
            }
            if (magAmmo == 0) {
                // Mag empty SFX
                AudioManager.GetAudioSource().PlayOneShot(magEmptySound, 0.65f);
                // Additionally, tell UI manager / turret that mag is empty
                subjectComponent.NotifyObservers(new MCEvent(EventType.MagEmpty));
            }

            Vector3 muzzlePosition = muzzleTransform.position;
            // Align muzzle to lookPoint (on horizontal plane)
            // TODO: This is hacky, because we're only aligning muzzle's rotation, but not the gun
            muzzleTransform.LookAt(lookPoint);
            Vector3 muzzleForward = muzzleTransform.forward;
            Ray ray = new Ray(muzzlePosition, muzzleForward);
            TrailRenderer tracerTrail = Instantiate(tracerPrefab, muzzlePosition, Quaternion.LookRotation(muzzleForward)).GetComponent<TrailRenderer>();
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, raycastLayers)) {
                // Debug: DrawLine - Green if hit NPC or Player, otherwise cyan
                Debug.DrawLine(muzzlePosition, hit.point, hit.collider.gameObject.layer == damageLayers ? Color.green : Color.cyan, 5f);
                // Tracer effect
                StartCoroutine(SpawnTrail(tracerTrail, hit.distance));
                LayerMask hitLayer = 1 << hit.collider.gameObject.layer;
                if ((hitLayer & damageLayers) == hitLayer) {
                    // Deal damage
                    GetComponent<DamageDealerComponent>().DealDamage(hit.collider.gameObject, stats.damage);
                }
            } else {
                // Tracer effect, endPoint is arbitrary (100m away, out of screen)
                StartCoroutine(SpawnTrail(tracerTrail, 100f));
            }
        }

        /*
         * Tracer Effect, referenced:
         * TheKiwiCoder (2020) '[#05] Shooting a weapon using Projectile Raycasts (with effects)', YouTube, 17 May.
         * Available at: https://www.youtube.com/watch?v=onpteKMsE84 (Accessed 3 June 2024).
         * BMo (2022) 'How to Add a TRAIL EFFECT to Anything in Unity', YouTube, 2 May.
         * Available at: https://www.youtube.com/watch?v=nLxvCRPJCKw (Accessed 3 June 2024).
         */
        private IEnumerator SpawnTrail(TrailRenderer tracerTrail, float distance)
        {
            float time = 0f;
            float arrivalTime = distance / tracerSpeed;
            while (time <= arrivalTime) {
                tracerTrail.transform.Translate(Vector3.forward * (tracerSpeed * Time.deltaTime));
                time += Time.deltaTime;
                yield return null;
            }
            Destroy(tracerTrail.gameObject);
        }

        // In other approaches, playing audios might be handled within the animation. But in this project, it's handled here.
        // TODO: At the moment, this is uninterruptible
        public IEnumerator StartReloading()
        {
            // SFX
            AudioManager.GetAudioSource().PlayOneShot(releaseMagSound, 0.45f);
            yield return new WaitForSeconds(stats.reloadTime);
            // TODO: Use this. However, need to trim down the audio length first...
            // yield return new WaitForSeconds(chargingBoltSoundPlayTimestamp);
            StartCoroutine(StartChargingBolt());
        }

        private IEnumerator StartChargingBolt()
        {
            AudioManager.GetAudioSource().PlayOneShot(chargingBoltSound, 0.5f);
            yield return new WaitForSeconds(0f);
            // TODO: Use this. However, need to trim down the audio length first...
            // yield return new WaitForSeconds(chargingBoltSound.length);
            Reload();
        }

        private void Reload()
        {
            SetMagAmmo(stats.magSize);
            SetIsBoltInPosition(true);
            // Tell observers that the gun is reloaded
            subjectComponent.NotifyObservers(new MCEvent(EventType.Reloaded));
        }
    }
}
