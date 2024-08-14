using Entities.Gun;
using UnityEngine;

namespace Entities.AI.Abilities.Gunner
{
    public class GunnerComponent : MonoBehaviour
    {
        public GunStateComponent gun;
        
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
            StartCoroutine(gun.StartReloading());
        }
    }
}
