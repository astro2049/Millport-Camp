using System.Collections.Generic;
using Abilities.Observer;
using UnityEngine;
using EventType = Abilities.Observer.EventType;

namespace Entities.Player
{
    public class PlayerInventoryComponent : MonoBehaviour
    {
        [SerializeField] private int size;
        private readonly HashSet<GameObject> items = new HashSet<GameObject>();

        public void Start()
        {
            GetComponent<SubjectComponent>().NotifyObservers(new MCEvent(EventType.InventoryReady));
        }

        public bool AddItem(GameObject entity)
        {
            if (items.Count == size) {
                return false;
            }
            return items.Add(entity);
        }

        public bool RemoveItem(GameObject entity)
        {
            return items.Remove(entity);
        }

        public bool EquipItem(GameObject entity)
        {
            GetComponent<PlayerStateComponent>().EquipGun(entity);
            return true;
        }
    }
}
