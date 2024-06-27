using Entities.AI.Turret.States;
using UnityEngine;

namespace Entities.AI.Turret
{
    public class TurretHFSMComponent : MonoBehaviour
    {
        public TurretGunHfsm gunHfsm;
        public TurretBaseFsm baseFsm;

        private void Awake()
        {
            TurretStateComponent turretStateComponent = GetComponent<TurretStateComponent>();

            gunHfsm = new TurretGunHfsm(turretStateComponent, HFSMStateType.Branch, "Gun", null);
            baseFsm = new TurretBaseFsm(turretStateComponent, HFSMStateType.Branch, "Base", null);
        }

        private void Update()
        {
            gunHfsm.ExecuteBranch(Time.deltaTime);
            baseFsm.ExecuteBranch(Time.deltaTime);
        }
    }
}
