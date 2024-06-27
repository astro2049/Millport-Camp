using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gun;
using Observer;
using TMPro;
using UnityEngine;
using EventType = Observer.EventType;

namespace AI.Turret
{
    public class TurretStateComponent : StateComponent
    {
        public float turnSpeed = 360f; // 360 deg/s

        public float perceptionRadius = 5f;
        public LayerMask[] perceptionLayers;

        public Transform baseTransform;
        public GunStateComponent gun;

        public TextMeshPro ammoText;

        private TurretHFSMComponent turretHfsm;
        private TurretObserverComponent turretObserverComponent;

        private void Awake()
        {
            // Configure perception
            GetComponent<PerceptionComponent>().CreateSensorCollider(perceptionRadius, perceptionLayers);

            turretHfsm = GetComponent<TurretHFSMComponent>();
            turretObserverComponent = GetComponent<TurretObserverComponent>();
        }

        // HFSM Context
        public GameObject target;
        private readonly HashSet<GameObject> targets = new HashSet<GameObject>();

        public void AddTarget(GameObject enemy)
        {
            // If there's no target lock-on, set target
            if (!target) {
                target = enemy;
            }

            // Add enemy to targets list (set),
            // and subscribe to its event: PawnDead
            targets.Add(enemy);
            enemy.GetComponent<SubjectComponent>().AddObserver(turretObserverComponent, EventType.PawnDead);

            // Possible transition: Idle -> Track
            if (turretHfsm.baseFsm.current.name == "Idle") {
                turretHfsm.baseFsm.ChangeState("Track");
            }
        }

        public void RemoveTarget(GameObject enemy)
        {
            // Remove enemy lost track of off the target list (set)
            targets.Remove(enemy);

            // Assign new target
            if (targets.Count != 0) {
                // If the target list is not empty, assign first in set as new target
                // TODO: This can be more intelligent, like selecting based on distance
                target = targets.First();
            } else {
                // If the target list is empty, then there's no target at the moment
                target = null;
                // Possible transition: Track -> Idle
                if (turretHfsm.baseFsm.current.name == "Track") {
                    turretHfsm.baseFsm.ChangeState("Idle");
                }
            }
        }

        public bool TargetIsAligned()
        {
            if (targets.Count != 0) {
                Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - baseTransform.position);
                return Mathf.Abs(Quaternion.Angle(targetRotation, baseTransform.rotation)) <= 1f;
            } else {
                return false;
            }
        }

        public void TriggerDown()
        {
            gun.SetIsTriggerDown(true);
        }

        public void TriggerUp()
        {
            gun.SetIsTriggerDown(false);
        }

        public void Reload()
        {
            StartCoroutine(WaitForReloadTime(gun.gunData.reloadTime));
        }

        private IEnumerator WaitForReloadTime(float reloadTime)
        {
            yield return new WaitForSeconds(reloadTime);
            gun.Reload();
            turretHfsm.gunHfsm.ChangeState("Trigger");
        }
    }
}
