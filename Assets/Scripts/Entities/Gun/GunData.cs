using UnityEngine;

namespace Entities.Gun
{
    [CreateAssetMenu(fileName = "GunData", menuName = "ScriptableObjects/GunData", order = 1)]
    public class GunData : ScriptableObject
    {
        public string gunName;
        public Mesh mesh;
        public Material material;
        public float damage;
        public int rpm;
        [HideInInspector] public float fireInterval; // 600 rpm = 0.1s between each shot
        public int magSize;
        public FireMode fireMode;
        public float reloadTime;

        private void OnEnable()
        {
            fireInterval = 60f / rpm;
        }
    }
}
