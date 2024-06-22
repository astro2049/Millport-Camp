namespace Player
{
    public class PlayerPawnComponent : PawnComponent
    {
        public override void Die()
        {
            base.Die();
            GetComponent<PlayerInputComponent>().enabled = false;
        }
    }
}
