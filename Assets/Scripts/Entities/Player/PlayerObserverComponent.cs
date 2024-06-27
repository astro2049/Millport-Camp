using Abilities.Observer;

namespace Entities.Player
{
    public class PlayerObserverComponent : ObserverComponent
    {
        private PlayerStateComponent playerStateComponent;

        private void Awake()
        {
            playerStateComponent = GetComponent<PlayerStateComponent>();
        }

        public override bool OnNotify(MCEvent mcEvent)
        {
            switch (mcEvent.type) {
                case EventType.Reloaded:
                    playerStateComponent.isReloading = false;
                    break;
            }
            return true;
        }
    }
}
