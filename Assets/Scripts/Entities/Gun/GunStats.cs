using Entities.Abilities.InventoryItem;
using UnityEngine;

namespace Entities.Gun
{
    [CreateAssetMenu(fileName = "Gun Data", menuName = "Scriptable Objects/Gun Data", order = 1)]
    public class GunStats : InventoryItem
    {
        public GameObject modelPrefab;
        public float damage;
        public int rpm;
        [HideInInspector] public float fireInterval; // 600 rpm = 0.1s between each shot
        public int magSize;
        public FireMode fireMode;
        public int shotsPerBurst;
        public float reloadTime;

        private void OnEnable()
        {
            fireInterval = 60f / rpm;
        }
    }
    
    public enum FireMode
    {
        Auto = 0,
        Burst = 1,
        Single = 2
    }
}
