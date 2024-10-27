using System.Collections;
using Entities.Abilities.Health;
using Entities.Abilities.Observer;
using UnityEngine;
using EventType = Entities.Abilities.Observer.EventType;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Entities.Gun
{
    public class GunStateComponent : MonoBehaviour
    {
        // Properties and Configurations
        public GunStats stats;
        public int magAmmo;
        private int shotNumInBurst = 0;
        [SerializeField] private LayerMask raycastLayers;
        [SerializeField] private LayerMask damageLayers;

        // Sounds
        [SerializeField] private AudioClip fireSound;
        [SerializeField] private AudioClip releaseMagSound;
        [SerializeField] private AudioClip chargingBoltSound;
        private float chargingBoltSoundPlayTimestamp;
        [SerializeField] private AudioClip magEmptySound;

        // Tracer
        public GameObject tracerPrefab;
        public float tracerSpeed = 150f; // 150m/s

        // Part states
        private bool isTriggerDown = false;
        private bool isBoltInPosition = true;

        // Muzzle
        [SerializeField] private Transform meshTransform;
        [SerializeField] private Transform muzzleTransform;

        // Components
        private SubjectComponent subjectComponent;
        private AudioSource audioSource;
        private DamageDealerComponent damageDealerComponent;

        private void Awake()
        {
            subjectComponent = GetComponent<SubjectComponent>();
            audioSource = GetComponent<AudioSource>();
            damageDealerComponent = GetComponent<DamageDealerComponent>();

            Init();
        }

        public void Init(GunStats stats = null)
        {
            if (stats != null) {
                this.stats = stats;
                Destroy(meshTransform.GetChild(0).gameObject);
            }

            // Initialize appearance (mesh and material) according to gunData (scriptable object)
            GameObject model = Instantiate(this.stats.modelPrefab, meshTransform);
            model.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            // Initialize fields
            // magAmmo
            SetMagAmmo(this.stats.magSize);

            // Calculate when to play chargingBoltSound
            chargingBoltSoundPlayTimestamp = this.stats.reloadTime - chargingBoltSound.length;
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
                        if (stats.fireMode == FireMode.Burst) {
                            shotNumInBurst = 1;
                        }
                    } else {
                        // nothing happens, currently in cycle
                    }
                } else {
                    // Mag empty SFX
                    audioSource.PlayOneShot(magEmptySound, 0.3f);
                }
            }
        }

        private void SetIsBoltInPosition(bool status)
        {
            isBoltInPosition = status;
            if (isBoltInPosition) {
                // A cycle just completed
                // If on Auto, after shot cycle is completed, the trigger gets reset automatically after cycling,
                // so a next shot will be fired if trigger is held down
                if (stats.fireMode == FireMode.Auto) {
                    if (isTriggerDown) {
                        Fire();
                    }
                } else if (stats.fireMode == FireMode.Burst) {
                    // If on Burst, after shot cycle is completed, the trigger gets reset if burst limit is not reached,
                    // and a next shot will be fired
                    if (shotNumInBurst < stats.shotsPerBurst) {
                        Fire();
                        shotNumInBurst++;
                    }
                }
            } else {
                // Just fired a shot
                // If there's still ammo, start cycle
                // When cycle is complete, the bolt returns to ready position
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
                audioSource.PlayOneShot(fireSound, 0.5f);
            }
            if (magAmmo == 0) {
                // Mag empty SFX
                audioSource.PlayOneShot(magEmptySound, 0.3f);
                // Additionally, tell UI manager / turret that mag is empty
                subjectComponent.NotifyObservers(new MCEvent(EventType.MagEmpty));
            }

            Vector3 muzzlePosition = muzzleTransform.position;
            Vector3 forward = transform.forward;
            Ray ray = new Ray(muzzlePosition, forward);
            TrailRenderer tracerTrail = Instantiate(tracerPrefab, muzzlePosition, Quaternion.LookRotation(forward)).GetComponent<TrailRenderer>();
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, raycastLayers, QueryTriggerInteraction.Ignore)) {
                GameObject hitGo = hit.collider.gameObject;
                // Debug: DrawLine - Green if hit NPC or Player, otherwise cyan
                Debug.DrawLine(muzzlePosition, hit.point, hitGo.layer == damageLayers ? Color.green : Color.cyan, 5f);
                // Tracer effect
                StartCoroutine(SpawnTrail(tracerTrail, hit.distance));
                LayerMask hitLayer = 1 << hitGo.layer;
                if ((hitLayer & damageLayers) == hitLayer) {
                    // Deal damage
                    damageDealerComponent.DealDamage(hitGo, stats.damage);
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
            audioSource.PlayOneShot(releaseMagSound, 0.2f);
            yield return new WaitForSeconds(stats.reloadTime);
            // TODO: Use this. However, need to trim down the audio length first...
            // yield return new WaitForSeconds(chargingBoltSoundPlayTimestamp);
            StartCoroutine(StartChargingBolt());
        }

        private IEnumerator StartChargingBolt()
        {
            audioSource.PlayOneShot(chargingBoltSound, 0.2f);
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
