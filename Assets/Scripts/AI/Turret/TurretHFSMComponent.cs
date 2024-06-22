using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI.Turret.States;
using Observer;
using UnityEngine;

namespace AI.Turret
{
    public class TurretHFSMComponent : MonoBehaviour
    {
        private TurretStateComponent turret;
        public TurretGunHfsm gunHfsm;
        public TurretBaseFsm baseFsm;

        public GameObject target;
        private readonly HashSet<GameObject> targets = new HashSet<GameObject>();

        private TurretObserverComponent turretObserverComponent;

        private void Awake()
        {
            turret = GetComponent<TurretStateComponent>();
            gunHfsm = new TurretGunHfsm(gameObject, "Gun", null);
            baseFsm = new TurretBaseFsm(gameObject, "Base", null);

            turretObserverComponent = GetComponent<TurretObserverComponent>();
        }

        private void Update()
        {
            gunHfsm.Execute(Time.deltaTime);
            baseFsm.Execute(Time.deltaTime);
        }

        public void AddTarget(GameObject enemy)
        {
            // If there's no target lock-on, set target
            if (!target) {
                target = enemy;
            }

            // Add enemy to targets list (set), and subscribe to its events
            targets.Add(enemy);
            enemy.GetComponent<SubjectComponent>().AddObserver(turretObserverComponent);

            // Possible transition: Idle -> Track
            if (baseFsm.current.name == "Idle") {
                baseFsm.ChangeState("Track");
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
                if (baseFsm.current.name == "Track") {
                    baseFsm.ChangeState("Idle");
                }
            }
        }

        public bool TargetIsAligned()
        {
            if (targets.Count != 0) {
                Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - turret.baseTransform.position);
                return Mathf.Abs(Quaternion.Angle(targetRotation, turret.baseTransform.rotation)) <= 1f;
            } else {
                return false;
            }
        }

        public void TriggerDown()
        {
            turret.gun.SetIsTriggerDown(true);
        }

        public void TriggerUp()
        {
            turret.gun.SetIsTriggerDown(false);
        }

        public void Reload()
        {
            StartCoroutine(WaitForReloadTime(turret.gun.gunData.reloadTime));
        }

        private IEnumerator WaitForReloadTime(float reloadTime)
        {
            yield return new WaitForSeconds(reloadTime);
            turret.gun.Reload();
            gunHfsm.ChangeState("Trigger");
        }
    }
}
