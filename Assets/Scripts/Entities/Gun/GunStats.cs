using UnityEngine;
using UnityEngine.UI;

namespace Entities.Gun
{
    [CreateAssetMenu(fileName = "Gun Data", menuName = "Scriptable Objects/Gun Data", order = 1)]
    public class GunStats : ScriptableObject
    {
        public new string name;
        public Image icon;
        public string description;
        public GameObject modelPrefab;
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
