using System;
using System.Collections;
using Player;
using UnityEngine;

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
        public string gunName;
        public float damage;
        public int rpm;
        private float fireInterval; // 600 rpm = 0.1s between each shot
        public int currentMagAmmo;
        public int magSize;
        public FireMode fireMode;
        public float reloadTime;
        // Sounds
        private AudioSource audioPlayer;
        public AudioClip fireSound;
        public AudioClip reloadSound;
        public AudioClip magEmptySound;
        // Tracer
        public GameObject tracerPrefab;
        public float tracerSpeed = 150f; // 150m/s
        public Transform muzzleTransform;
        // Game Manager
        public GameManager gameManager;
        // Holder
        public PlayerInputComponent holder;

        /*
         * States
         */
        private bool isTriggerDown = false;
        private bool isBoltInPosition = true;

        // Start is called before the first frame update
        private void Start()
        {
            audioPlayer = GetComponent<AudioSource>();
            SetCurrentMagAmmo(magSize);
            fireInterval = 60f / rpm;
        }

        // Update is called once per frame
        private void Update()
        {

        }

        private void SetCurrentMagAmmo(int ammo)
        {
            currentMagAmmo = ammo;
            gameManager.UpdateMagAmmoText(currentMagAmmo);
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
            if (isBoltInPosition) {
                if (isTriggerDown) {
                    FiringPinStruck();
                }
            } else {
                StartCoroutine(Cycle());
            }
        }

        private void FiringPinStruck()
        {
            if (currentMagAmmo != 0) {
                Fire(holder.LookAtPoint());
            } else {
                // Mag empty SFX
                audioPlayer.PlayOneShot(magEmptySound, 0.5f);
            }
        }

        private IEnumerator Cycle()
        {
            yield return new WaitForSeconds(fireInterval);
            setIsBoltInPosition(true);
        }

        // Shoot a raycast bullet horizontally from muzzle location
        private void Fire(Vector3 lookPoint)
        {
            // Fire SFX
            audioPlayer.PlayOneShot(fireSound, 0.4f);
            // Minus 1 bullet
            SetCurrentMagAmmo(currentMagAmmo - 1);
            setIsBoltInPosition(false);

            Vector3 muzzlePosition = muzzleTransform.position;
            // Align muzzle to lookPoint (on horizontal plane)
            // TODO: This is hacky, because we're only aligning muzzle's rotation, but not the gun
            muzzleTransform.LookAt(new Vector3(
                lookPoint.x,
                muzzlePosition.y,
                lookPoint.z
            ));
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
                    GetComponent<DamageDealerComponent>().DealDamage(hit.collider.gameObject, damage);
                }
            } else {
                // Tracer effect, endPoint is arbitrary (100m away, out of screen)
                StartCoroutine(SpawnTrail(tracerTrail, 100f));
            }
        }

        /*
         * Tracer Effect, referenced
         * TheKiwiCoder (2020) '[#05] Shooting a weapon using Projectile Raycasts (with effects)', Youtube, 17 May
         * https://www.youtube.com/watch?v=onpteKMsE84 (Accessed 3 June 2024)
         * BMo (2022) 'How to Add a TRAIL EFFECT to Anything in Unity', youtube, 2 May
         * https://www.youtube.com/watch?v=nLxvCRPJCKw (Accessed 3 June 2024)
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
            audioPlayer.PlayOneShot(reloadSound, 0.5f);
            SetCurrentMagAmmo(magSize);
        }
    }
}
