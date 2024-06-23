using UnityEngine;

namespace Observer
{
    public enum EventType
    {
        Default = 0,

        // Player
        WeaponChanged = 1,
        IsReloading = 2,
        InteractionStarted = 3,
        InteractionEnded = 4,
        EnteredVehicle = 5,
        ExitedVehicle = 6,

        // Pawn (Player, Zombie, Combat Robot)
        PawnDead = 7,

        // Gun
        AmmoChanged = 8,
        MagEmpty = 9,
    }

    public class MCEvent
    {
        public readonly EventType type;

        public MCEvent(EventType type)
        {
            this.type = type;
        }
    }

    public class MCEventWInt : MCEvent
    {
        public int value;

        public MCEventWInt(EventType type, int value) : base(type)
        {
            this.value = value;
        }
    }

    public class MCEventWEntity : MCEvent
    {
        public GameObject entity;

        public MCEventWEntity(EventType type, GameObject entity) : base(type)
        {
            this.entity = entity;
        }
    }
}
