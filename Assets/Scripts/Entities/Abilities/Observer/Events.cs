using UnityEngine;

namespace Entities.Abilities.Observer
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
        EnteredBuildMode = 7,
        PlacingStructure = 8,
        ExitedBuildMode = 9,

        // Actor (Player, Zombie, Combat Robot, Vehicle, Turret)
        Dead = 13,

        // Gun
        AmmoChanged = 14,
        MagEmpty = 15,
        Reloaded = 16,

        // Structures
        CanPlace = 17,
        CannotPlace = 18,

        // AI: Target Tracker
        AcquiredFirstTarget = 19,
        AcquiredNewTarget = 20,
        LostAllTargets = 21
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
