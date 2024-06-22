using UnityEngine;

namespace Observer
{
    public enum EventType
    {
        Default = 0,
        InteractionStarted = 1,
        InteractionEnded = 2,
        EnteredVehicle = 3,
        ExitedVehicle = 4,
        AmmoChanged = 5,
        WeaponChanged = 6,
        MagEmpty = 7,
        IsReloading = 8,
        PawnDead = 9
    }

    public class MCEvent
    {
        public EventType type;

        public MCEvent(EventType type)
        {
            this.type = type;
        }
    }

    public class PawnDeadEvent : MCEvent
    {
        public GameObject pawn;

        public PawnDeadEvent(EventType type, GameObject pawn) : base(type)
        {
            this.pawn = pawn;
        }
    }
}
