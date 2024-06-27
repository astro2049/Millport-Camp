using UnityEngine;

namespace Abilities.Observer
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

        // Pawn (Player, Zombie, Combat Robot)
        PawnDead = 10,

        // Gun
        AmmoChanged = 11,
        MagEmpty = 12,
        Reloaded = 13,

        // Structures
        CanPlace = 14,
        CannotPlace = 15
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
