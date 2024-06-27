using System.Collections;
using Managers;
using Observer;
using UnityEngine;
using EventType = Observer.EventType;

namespace Gun
{
    public enum FireMode
    {
        Auto = 0,
        Burst = 1,
        Single = 2
    }

    public class GunStateComponent : MonoBehaviour
    {
        /*
         * Public fields
         */
        // Stats
        public GunData gunData;
        public int currentMagAmmo;
        // Muzzle
        private Transform muzzleTransform;
        // Sounds
        public AudioClip fireSound;
        public AudioClip reloadSound;
        public AudioClip magEmptySound;
        // Tracer
        public GameObject tracerPrefab;
        public float tracerSpeed = 150f; // 150m/s

        public Vector3 lookPoint;

        private SubjectComponent subjectComponent;

        /*
         * States
         */
        private bool isTriggerDown = false;
        private bool isBoltInPosition = true;

        // Start is called before the first frame update
        private void Start()
        {
            subjectComponent = GetComponent<SubjectComponent>();

            Transform meshTransform = transform.Find("Mesh");
            meshTransform.GetComponent<MeshFilter>().mesh = gunData.mesh;
            meshTransform.GetComponent<MeshRenderer>().material = gunData.material;
            muzzleTransform = transform.Find("Muzzle").transform;
            SetCurrentMagAmmo(gunData.magSize);
        }

        // Update is called once per frame
        private void Update()
        {

        }

        private void SetCurrentMagAmmo(int ammo)
        {
            currentMagAmmo = ammo;

            // Broadcast event
            subjectComponent.NotifyObservers(new MCEventWInt(EventType.AmmoChanged, currentMagAmmo));
        }

        public void SetIsTriggerDown(bool status)
        {
            isTriggerDown = status;
            if (isTriggerDown) {
                if (isBoltInPosition) {
                    FiringPinStruck();
                }
            }
        }

        private void setIsBoltInPosition(bool status)
        {
            isBoltInPosition = status;
            // A shot is fired
            if (!isBoltInPosition) {
                StartCoroutine(Cycle());
            } else {
                // After shot cycle is completed
                // If on Auto, the trigger gets reset automatically after cycling,
                // so a next shot will be fired if trigger is held down 
                if (gunData.fireMode == FireMode.Auto) {
                    if (isTriggerDown) {
                        FiringPinStruck();
                    }
                }
            }
        }

        private void FiringPinStruck()
        {
            if (currentMagAmmo != 0) {
                Fire();
            } else {
                // Mag is empty
                // Broadcast event
                subjectComponent.NotifyObservers(new MCEvent(EventType.MagEmpty));
                // Mag empty SFX
                AudioManager.GetAudioSource().PlayOneShot(magEmptySound, 0.5f);
            }
        }

        private IEnumerator Cycle()
        {
            yield return new WaitForSeconds(gunData.fireInterval);
            setIsBoltInPosition(true);
        }

        // Shoot a raycast bullet horizontally towards lookPoint from muzzle location
        private void Fire()
        {
            // Fire SFX
            AudioManager.GetAudioSource().PlayOneShot(fireSound, 0.4f);
            // Minus 1 bullet
            SetCurrentMagAmmo(currentMagAmmo - 1);
            setIsBoltInPosition(false);

            Vector3 muzzlePosition = muzzleTransform.position;
            // Align muzzle to lookPoint (on horizontal plane)
            // TODO: This is hacky, because we're only aligning muzzle's rotation, but not the gun
            muzzleTransform.LookAt(lookPoint);
            Vector3 muzzleForward = muzzleTransform.forward;
            Ray ray = new Ray(muzzlePosition, muzzleForward);
            TrailRenderer tracerTrail = Instantiate(tracerPrefab, muzzlePosition, Quaternion.LookRotation(muzzleForward)).GetComponent<TrailRenderer>();
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Obstacle", "NPC", "Vehicle"))) {
                // Debug: DrawLine - Green if hit NPC, otherwise cyan
                Debug.DrawLine(muzzlePosition, hit.point, hit.collider.gameObject.layer == LayerMask.NameToLayer("NPC") ? Color.green : Color.cyan, 5f);
                // Tracer effect
                StartCoroutine(SpawnTrail(tracerTrail, hit.distance));
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("NPC")) {
                    // Deal damage
                    GetComponent<DamageDealerComponent>().DealDamage(hit.collider.gameObject, gunData.damage);
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

        public void Reload()
        {
            AudioManager.GetAudioSource().PlayOneShot(reloadSound, 0.5f);
            SetCurrentMagAmmo(gunData.magSize);
        }
    }
}
